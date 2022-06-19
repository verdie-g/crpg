using AutoMapper;
using Crpg.Application.Bans.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Games.Models;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Games.Commands;

/// <summary>
/// Get or create a user with its character.
/// </summary>
public record GetGameUserCommand : IMediatorRequest<GameUser>
{
    public Platform Platform { get; init; }
    public string PlatformUserId { get; init; } = default!;
    public string UserName { get; init; } = default!;

    internal class Handler : IMediatorRequestHandler<GetGameUserCommand, GameUser>
    {
        internal static readonly (string mbId, ItemSlot slot)[][] DefaultItemSets =
        {
            // aserai
            new[]
            {
                ("head_wrapped", ItemSlot.Head),
                ("long_desert_robe", ItemSlot.Body),
                ("southern_moccasins", ItemSlot.Leg),
                ("peasant_polearm_1_t1", ItemSlot.Weapon0),
                ("peasant_pickaxe_1_t1", ItemSlot.Weapon1),
                ("throwing_stone", ItemSlot.Weapon2),
            },
            // vlandia
            new[]
            {
                ("sackcloth_tunic", ItemSlot.Body),
                ("strapped_shoes", ItemSlot.Leg),
                ("peasant_pitchfork_1_t1", ItemSlot.Weapon0),
                ("peasant_hatchet_1_t1", ItemSlot.Weapon1),
                ("throwing_stone", ItemSlot.Weapon2),
            },
            // empire
            new[]
            {
                ("peasant_costume", ItemSlot.Body),
                ("folded_town_boots", ItemSlot.Leg),
                ("peasant_polearm_1_t1", ItemSlot.Weapon0),
                ("peasant_sickle_1_t1", ItemSlot.Weapon1),
                ("throwing_stone", ItemSlot.Weapon2),
            },
            // sturgia
            new[]
            {
                ("scarf", ItemSlot.Shoulder),
                ("light tunic", ItemSlot.Body),
                ("peasant_2haxe_1_t1", ItemSlot.Weapon0),
                ("peasant_hammer_1_t1", ItemSlot.Weapon1),
                ("throwing_stone", ItemSlot.Weapon2),
            },
            // khuzait
            new[]
            {
                ("nomad_cap", ItemSlot.Head),
                ("khuzait_civil_coat", ItemSlot.Body),
                ("leather_boots", ItemSlot.Leg),
                ("peasant_pitchfork_2_t1", ItemSlot.Weapon0),
                ("peasant_hammer_2_t1", ItemSlot.Weapon1),
                ("throwing_stone", ItemSlot.Weapon2),
            },
            // battania
            new[]
            {
                ("baggy_trunks", ItemSlot.Body),
                ("armwraps", ItemSlot.Hand),
                ("ragged_boots", ItemSlot.Leg),
                ("peasant_2haxe_1_t1", ItemSlot.Weapon0),
                ("peasant_pickaxe_1_t1", ItemSlot.Weapon1),
                ("throwing_stone", ItemSlot.Weapon2),
            },
            // looters
            new[]
            {
                ("vlandia_bandit_c", ItemSlot.Body),
                ("rough_tied_boots", ItemSlot.Leg),
                ("training_bow", ItemSlot.Weapon0),
                ("default_arrows", ItemSlot.Weapon1),
                ("peasant_hatchet_1_t1", ItemSlot.Weapon2),
            },
        };
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<GetGameUserCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly IDateTime _dateTime;
        private readonly IRandom _random;
        private readonly IUserService _userService;
        private readonly ICharacterService _characterService;

        public Handler(ICrpgDbContext db, IMapper mapper, IDateTime dateTime,
            IRandom random, IUserService userService, ICharacterService characterService)
        {
            _db = db;
            _mapper = mapper;
            _dateTime = dateTime;
            _random = random;
            _userService = userService;
            _characterService = characterService;
        }

        public async Task<Result<GameUser>> Handle(GetGameUserCommand req, CancellationToken cancellationToken)
        {
            var user = await _db.Users
                .Include(u => u.Characters.Where(c => c.Name == req.UserName).Take(1))
                .Include(u => u.Bans.OrderByDescending(b => b.Id).Take(1))
                .FirstOrDefaultAsync(u => u.Platform == req.Platform && u.PlatformUserId == req.PlatformUserId,
                    cancellationToken);

            if (user == null)
            {
                user = CreateUser(req.Platform, req.PlatformUserId, req.UserName);
                _db.Users.Add(user);
                Logger.LogInformation("{0} joined ({1}#{2})", req.UserName, req.Platform, req.PlatformUserId);
            }

            if (user.Characters.Count == 0)
            {
                var itemSet = await GiveUserRandomItemSet(user);
                var character = CreateCharacter(req.UserName, itemSet);
                user.Characters.Add(character);
            }
            else
            {
                // Load items in separate query to avoid cartesian explosion if character has many items equipped.
                await _db.Entry(user.Characters[0])
                    .Collection(c => c.EquippedItems)
                    .Query()
                    .Include(ei => ei.Item)
                    .LoadAsync(cancellationToken);
            }

            await _db.SaveChangesAsync(cancellationToken);

            var gameUser = _mapper.Map<GameUser>(user);
            gameUser.Ban = _mapper.Map<BanViewModel>(GetActiveBan(user.Bans.FirstOrDefault()));
            return new(gameUser);
        }

        private User CreateUser(Platform platform, string platformUserId, string name)
        {
            User user = new()
            {
                Platform = platform,
                PlatformUserId = platformUserId,
                Name = name,
            };

            _userService.SetDefaultValuesForUser(user);
            return user;
        }

        private Character CreateCharacter(string name, IList<EquippedItem> equippedItems)
        {
            Character character = new()
            {
                Name = name,
                EquippedItems = equippedItems,
            };

            _characterService.SetDefaultValuesForCharacter(character);
            _characterService.ResetCharacterStats(character);
            return character;
        }

        private async Task<IList<EquippedItem>> GiveUserRandomItemSet(User user)
        {
            // Get a random set of items and check if the user already own some of them and add the others.
            var mbIdsWithSlot = DefaultItemSets[_random.Next(0, DefaultItemSets.Length)];
            string[] itemMbIds = mbIdsWithSlot.Select(i => i.mbId).ToArray();
            var items = await _db.Items
                .Include(i => i.UserItems.Where(oi => oi.UserId == user.Id))
                .Where(i => itemMbIds.Contains(i.TemplateMbId) && i.Rank == 0)
                .ToDictionaryAsync(i => i.TemplateMbId);

            List<EquippedItem> equippedItems = new();
            foreach (var (newItemMbId, slot) in mbIdsWithSlot)
            {
                if (!items.TryGetValue(newItemMbId, out var item))
                {
                    Logger.LogWarning("Item '{0}' doesn't exist", newItemMbId);
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
                EquippedItem equippedItem = new()
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
            return lastBan != null && lastBan.CreatedAt + lastBan.Duration > _dateTime.UtcNow ? lastBan : null;
        }
    }
}
