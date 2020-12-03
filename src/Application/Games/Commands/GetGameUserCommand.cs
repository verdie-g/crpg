using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Bans.Models;
using Crpg.Application.Common.Helpers;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Games.Models;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions;
using Crpg.Sdk.Abstractions.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Crpg.Application.Games.Commands
{
    /// <summary>
    /// Get or create a user with its character.
    /// </summary>
    public class GetGameUserCommand : IMediatorRequest<GameUser>
    {
        public Platform Platform { get; set; }
        public string PlatformUserId { get; set; } = default!;
        public string UserName { get; set; } = default!;

        public class Handler : IMediatorRequestHandler<GetGameUserCommand, GameUser>
        {
            internal static readonly (string mbId, ItemSlot slot)[][] DefaultItemSets =
            {
                // aserai
                new[]
                {
                    ("mp_wrapped_desert_cap", ItemSlot.Head),
                    ("mp_aserai_civil_e", ItemSlot.Body),
                    ("mp_strapped_shoes", ItemSlot.Leg),
                    ("mp_aserai_axe", ItemSlot.Weapon1),
                    ("mp_throwing_stone", ItemSlot.Weapon2),
                },
                // vlandia
                new[]
                {
                    ("mp_arming_coif", ItemSlot.Head),
                    ("mp_sackcloth_tunic", ItemSlot.Body),
                    ("mp_strapped_shoes", ItemSlot.Leg),
                    ("mp_vlandian_billhook", ItemSlot.Weapon1),
                    ("mp_sling_stone", ItemSlot.Weapon2),
                },
                // empire
                new[]
                {
                    ("mp_laced_cloth_coif", ItemSlot.Head),
                    ("mp_hemp_tunic", ItemSlot.Body),
                    ("mp_leather_shoes", ItemSlot.Leg),
                    ("mp_empire_axe", ItemSlot.Weapon1),
                    ("mp_throwing_stone", ItemSlot.Weapon2),
                },
                // sturgia
                new[]
                {
                    ("mp_nordic_fur_cap", ItemSlot.Head),
                    ("mp_northern_tunic", ItemSlot.Body),
                    ("mp_wrapped_shoes", ItemSlot.Leg),
                    ("mp_sturgia_mace", ItemSlot.Weapon1),
                    ("mp_sling_stone", ItemSlot.Weapon2),
                },
                // khuzait
                new[]
                {
                    ("mp_nomad_padded_hood", ItemSlot.Head),
                    ("mp_khuzait_civil_coat_b", ItemSlot.Body),
                    ("mp_strapped_leather_boots", ItemSlot.Leg),
                    ("mp_khuzait_sichel", ItemSlot.Weapon1),
                    ("mp_throwing_stone", ItemSlot.Weapon2),
                },
                // battania
                new[]
                {
                    ("mp_battania_civil_hood", ItemSlot.Head),
                    ("mp_battania_civil_a", ItemSlot.Body),
                    ("mp_rough_tied_boots", ItemSlot.Leg),
                    ("mp_battania_axe", ItemSlot.Weapon1),
                    ("mp_sling_stone", ItemSlot.Weapon2),
                },
                // looters
                new[]
                {
                    ("mp_vlandia_bandit_cape_a", ItemSlot.Head),
                    ("mp_burlap_sack_dress", ItemSlot.Body),
                    ("mp_strapped_leather_boots", ItemSlot.Leg),
                    ("mp_empire_long_twohandedaxe", ItemSlot.Weapon1),
                    ("mp_throwing_stone", ItemSlot.Weapon2),
                },
            };

            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;
            private readonly IEventService _events;
            private readonly IDateTimeOffset _dateTime;
            private readonly IRandom _random;
            private readonly ILogger<GetGameUserCommand> _logger;

            public Handler(ICrpgDbContext db, IMapper mapper, IEventService events, IDateTimeOffset dateTime,
                IRandom random, ILogger<GetGameUserCommand> logger)
            {
                _db = db;
                _mapper = mapper;
                _events = events;
                _dateTime = dateTime;
                _random = random;
                _logger = logger;
            }

            public async Task<Result<GameUser>> Handle(GetGameUserCommand req, CancellationToken cancellationToken)
            {
                var user = await _db.Users
                    .Include(u => u.Characters.Where(c => c.Name == req.UserName).Take(1))
                    .ThenInclude(c => c!.EquippedItems).ThenInclude(ei => ei.Item)
                    .Include(u => u.Bans.OrderByDescending(b => b.Id).Take(1))
                    .FirstOrDefaultAsync(u => u.Platform == req.Platform && u.PlatformUserId == req.PlatformUserId,
                        cancellationToken);

                if (user == null)
                {
                    user = CreateUser(req.Platform, req.PlatformUserId, req.UserName);
                    _db.Users.Add(user);
                    _events.Raise(EventLevel.Info, $"{req.UserName} joined ({req.Platform}#{req.PlatformUserId})", string.Empty, "user_created");
                }

                if (user.Characters.Count == 0)
                {
                    var itemSet = await GiveUserRandomItemSet(user);
                    var character = CreateCharacter(req.UserName, itemSet);
                    user.Characters.Add(character);
                }

                await _db.SaveChangesAsync(cancellationToken);

                var gameUser = _mapper.Map<GameUser>(user);
                gameUser.Ban = _mapper.Map<BanViewModel>(GetActiveBan(user.Bans.FirstOrDefault()));
                return new Result<GameUser>(gameUser);
            }

            private User CreateUser(Platform platform, string platformUserId, string name)
            {
                var user = new User
                {
                    Platform = platform,
                    PlatformUserId = platformUserId,
                    Name = name,
                };

                UserHelper.SetDefaultValuesForUser(user);
                return user;
            }

            private Character CreateCharacter(string name, IList<EquippedItem> equippedItems)
            {
                var character = new Character
                {
                    Name = name,
                    EquippedItems = equippedItems,
                };

                CharacterHelper.SetDefaultValuesForCharacter(character);
                CharacterHelper.ResetCharacterStats(character);
                return character;
            }

            private async Task<IList<EquippedItem>> GiveUserRandomItemSet(User user)
            {
                // Get a random set of items and check if the user already own some of them and add the others.
                var mbIdsWithSlot = DefaultItemSets[_random.Next(0, DefaultItemSets.Length - 1)];
                string[] itemMbIds = mbIdsWithSlot.Select(i => i.mbId).ToArray();
                var items = await _db.Items
                    .Include(i => i.UserItems.Where(ui => ui.UserId == user.Id))
                    .Where(i => itemMbIds.Contains(i.MbId))
                    .ToDictionaryAsync(i => i.MbId);

                var equippedItems = new List<EquippedItem>();
                foreach (var (newItemMbId, slot) in mbIdsWithSlot)
                {
                    if (!items.TryGetValue(newItemMbId, out var item))
                    {
                        _logger.LogWarning("Item '{0}' doesn't exist", newItemMbId);
                        continue;
                    }

                    UserItem userItem;
                    if (item.UserItems.Count != 0)
                    {
                        userItem = item.UserItems[0];
                    }
                    else
                    {
                        userItem = new UserItem
                        {
                            ItemId = item.Id,
                            User = user,
                        };

                        _db.UserItems.Add(userItem);
                    }

                    // Don't use Add method to avoid adding the item twice.
                    var equippedItem = new EquippedItem
                    {
                        Slot = slot,
                        Item = item,
                        UserItem = userItem,
                    };

                    equippedItems.Add(equippedItem);
                }

                return equippedItems;
            }

            private Ban? GetActiveBan(Ban? lastBan)
            {
                return lastBan != null && lastBan.CreatedAt + lastBan.Duration > _dateTime.Now ? lastBan : null;
            }
        }
    }
}
