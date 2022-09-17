using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Games.Models;
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
public record GetGameUserCommand : IMediatorRequest<GameUserViewModel>
{
    public Platform Platform { get; init; }
    public string PlatformUserId { get; init; } = default!;
    public string UserName { get; init; } = default!;

    internal class Handler : IMediatorRequestHandler<GetGameUserCommand, GameUserViewModel>
    {
        internal static readonly (string id, ItemSlot slot)[][] DefaultItemSets =
        {
            // aserai
            new[]
            {
                ("crpg_head_wrapped", ItemSlot.Head),
                ("crpg_long_desert_robe", ItemSlot.Body),
                ("crpg_southern_moccasins", ItemSlot.Leg),
                ("crpg_peasant_polearm_1_t1", ItemSlot.Weapon0),
                ("crpg_peasant_pickaxe_1_t1", ItemSlot.Weapon1),
                ("crpg_throwing_stone", ItemSlot.Weapon2),
            },
            // vlandia
            new[]
            {
                ("crpg_sackcloth_tunic", ItemSlot.Body),
                ("crpg_strapped_shoes", ItemSlot.Leg),
                ("crpg_peasant_pitchfork_1_t1", ItemSlot.Weapon0),
                ("crpg_peasant_hatchet_1_t1", ItemSlot.Weapon1),
                ("crpg_throwing_stone", ItemSlot.Weapon2),
            },
            // empire
            new[]
            {
                ("crpg_peasant_costume", ItemSlot.Body),
                ("crpg_folded_town_boots", ItemSlot.Leg),
                ("crpg_peasant_polearm_1_t1", ItemSlot.Weapon0),
                ("crpg_peasant_sickle_1_t1", ItemSlot.Weapon1),
                ("crpg_throwing_stone", ItemSlot.Weapon2),
            },
            // sturgia
            new[]
            {
                ("crpg_scarf", ItemSlot.Shoulder),
                ("crpg_light_tunic", ItemSlot.Body),
                ("crpg_peasant_2haxe_1_t1", ItemSlot.Weapon0),
                ("crpg_peasant_hammer_1_t1", ItemSlot.Weapon1),
                ("crpg_throwing_stone", ItemSlot.Weapon2),
            },
            // khuzait
            new[]
            {
                ("crpg_nomad_cap", ItemSlot.Head),
                ("crpg_khuzait_civil_coat", ItemSlot.Body),
                ("crpg_leather_boots", ItemSlot.Leg),
                ("crpg_peasant_pitchfork_2_t1", ItemSlot.Weapon0),
                ("crpg_peasant_hammer_2_t1", ItemSlot.Weapon1),
                ("crpg_throwing_stone", ItemSlot.Weapon2),
            },
            // battania
            new[]
            {
                ("crpg_baggy_trunks", ItemSlot.Body),
                ("crpg_armwraps", ItemSlot.Hand),
                ("crpg_ragged_boots", ItemSlot.Leg),
                ("crpg_peasant_2haxe_1_t1", ItemSlot.Weapon0),
                ("crpg_peasant_pickaxe_1_t1", ItemSlot.Weapon1),
                ("crpg_throwing_stone", ItemSlot.Weapon2),
            },
            // looters
            new[]
            {
                ("crpg_vlandia_bandit_c", ItemSlot.Body),
                ("crpg_rough_tied_boots", ItemSlot.Leg),
                ("crpg_hunting_bow", ItemSlot.Weapon0),
                ("crpg_default_arrows", ItemSlot.Weapon1),
                ("crpg_peasant_hatchet_1_t1", ItemSlot.Weapon2),
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

        public async Task<Result<GameUserViewModel>> Handle(GetGameUserCommand req, CancellationToken cancellationToken)
        {
            var user = await _db.Users
                .Include(u => u.Characters.Where(c => c.Name == req.UserName).Take(1))
                .Include(u => u.ClanMembership)
                .FirstOrDefaultAsync(u => u.Platform == req.Platform && u.PlatformUserId == req.PlatformUserId,
                    cancellationToken);

            if (user == null)
            {
                user = CreateUser(req.Platform, req.PlatformUserId, req.UserName);
                _db.Users.Add(user);
                Logger.LogInformation("{0} joined ({1}#{2})", req.UserName, req.Platform, req.PlatformUserId);
            }
            else
            {
                // Get the last restriction by type.
                await _db.Entry(user)
                    .Collection(u => u.Restrictions)
                    .Query()
                    .GroupBy(r => r.Type)
                    // ReSharper disable once SimplifyLinqExpressionUseMinByAndMaxBy (https://github.com/dotnet/efcore/issues/25566)
                    .Select(g => g.OrderByDescending(r => r.CreatedAt).FirstOrDefault())
                    .LoadAsync(cancellationToken);
            }

            Character? newCharacter = null;
            if (user.Characters.Count == 0)
            {
                var itemSet = await GiveUserRandomItemSet(user);
                newCharacter = CreateCharacter(req.UserName, itemSet);
                user.Characters.Add(newCharacter);
            }
            else
            {
                // Load items in separate query to avoid cartesian explosion if character has many items equipped.
                await _db.Entry(user.Characters[0])
                    .Collection(c => c.EquippedItems)
                    .Query()
                    .Include(ei => ei.UserItem)
                    .LoadAsync(cancellationToken);
            }

            await _db.SaveChangesAsync(cancellationToken);

            if (newCharacter != null)
            {
                Logger.LogInformation("User '{0}' created character '{1}'", user.Id, newCharacter.Id);
            }

            var gameUser = _mapper.Map<GameUserViewModel>(user);
            gameUser.Restrictions = gameUser.Restrictions
                .Where(r => _dateTime.UtcNow < r.CreatedAt + r.Duration)
                .ToArray();
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
                Statistics = new CharacterStatistics
                {
                    Kills = 0,
                    Deaths = 0,
                    Assists = 0,
                    PlayTime = TimeSpan.Zero,
                },
            };

            _characterService.SetDefaultValuesForCharacter(character);
            _characterService.ResetCharacterCharacteristics(character);
            return character;
        }

        private async Task<IList<EquippedItem>> GiveUserRandomItemSet(User user)
        {
            // Get a random set of items and check if the user already own some of them and add the others.
            var mbIdsWithSlot = DefaultItemSets[_random.Next(0, DefaultItemSets.Length)];
            string[] itemIds = mbIdsWithSlot.Select(i => i.id).ToArray();
            var items = await _db.Items
                .Include(i => i.UserItems.Where(oi => oi.UserId == user.Id))
                .Where(i => itemIds.Contains(i.Id))
                .ToDictionaryAsync(i => i.Id, StringComparer.Ordinal);

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
                        BaseItemId = item.Id,
                        User = user,
                        Rank = 0,
                    };

                    _db.UserItems.Add(userItem);
                }

                // Don't use Add method to avoid adding the item twice.
                EquippedItem equippedItem = new()
                {
                    Slot = slot,
                    UserItem = userItem,
                };

                equippedItems.Add(equippedItem);
            }

            return equippedItems;
        }
    }
}
