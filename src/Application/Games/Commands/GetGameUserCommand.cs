using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Games.Models;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Limitations;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions;
using FluentValidation;
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
    public Region Region { get; init; }

    public class Validator : AbstractValidator<GetGameUserCommand>
    {
        public Validator()
        {
            RuleFor(c => c.Platform).IsInEnum();
            RuleFor(c => c.Region).IsInEnum();
        }
    }

    internal class Handler : IMediatorRequestHandler<GetGameUserCommand, GameUserViewModel>
    {
        internal static readonly (string id, ItemSlot slot)[][] DefaultItemSets =
        {
            // aserai
            new[]
            {
                ("crpg_turban_v2_h0", ItemSlot.Head),
                ("crpg_aserai_civil_e_v2_h0", ItemSlot.Body),
                ("crpg_southern_moccasins_v2_h0", ItemSlot.Leg),
                ("crpg_hoe_v1_h0", ItemSlot.Weapon0),
                ("crpg_throwing_stone_v2_h0", ItemSlot.Weapon1),
            },
            // vlandia
            new[]
            {
                ("crpg_leather_apron_v2_h0", ItemSlot.Body),
                ("crpg_leather_gloves_v2_h0", ItemSlot.Hand),
                ("crpg_leather_shoes_v2_h0", ItemSlot.Leg),
                ("crpg_iron_pitchfork_h0", ItemSlot.Weapon0),
                ("crpg_throwing_stone_v2_h0", ItemSlot.Weapon1),
            },
            // empire
            new[]
            {
                ("crpg_pilgrim_hood_v2_h0", ItemSlot.Head),
                ("crpg_tied_cloth_tunic_v2_h0", ItemSlot.Body),
                ("crpg_leather_shoes_v2_h0", ItemSlot.Leg),
                ("crpg_iron_pitchfork_h0", ItemSlot.Weapon0),
                ("crpg_throwing_stone_v2_h0", ItemSlot.Weapon1),
            },
            // sturgia
            new[]
            {
                ("crpg_scarf_v2_h0", ItemSlot.Shoulder),
                ("crpg_light_tunic_v2_h0", ItemSlot.Body),
                ("crpg_leather_shoes_v2_h0", ItemSlot.Leg),
                ("crpg_hoe_v1_h0", ItemSlot.Weapon0),
                ("crpg_bound_adarga_v3_h0", ItemSlot.Weapon1),
            },
            // khuzait
            new[]
            {
                ("crpg_nomad_cap_v2_h0", ItemSlot.Head),
                ("crpg_steppe_robe_v2_h0", ItemSlot.Body),
                ("crpg_leather_boots_v2_h0", ItemSlot.Leg),
                ("crpg_hatchet_h0", ItemSlot.Weapon0),
                ("crpg_throwing_stone_v2_h0", ItemSlot.Weapon1),
            },
            // battania
            new[]
            {
                ("crpg_baggy_trunks_v2_h0", ItemSlot.Body),
                ("crpg_armwraps_v2_h0", ItemSlot.Hand),
                ("crpg_ragged_boots_v2_h0", ItemSlot.Leg),
                ("crpg_makeshift_sledgehammer_h0", ItemSlot.Weapon0),
                ("crpg_throwing_stone_v2_h0", ItemSlot.Weapon1),
            },
            // looters
            new[]
            {
                ("crpg_vlandia_bandit_cape_b_v2_h0", ItemSlot.Head),
                ("crpg_vlandia_bandit_c_v2_h0", ItemSlot.Body),
                ("crpg_rough_tied_boots_v2_h0", ItemSlot.Leg),
                ("crpg_spiked_club_h0", ItemSlot.Weapon0),
                ("crpg_bound_adarga_v3_h0", ItemSlot.Weapon1),
            },
        };
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<GetGameUserCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly IDateTime _dateTime;
        private readonly IRandom _random;
        private readonly IUserService _userService;
        private readonly ICharacterService _characterService;
        private readonly IActivityLogService _activityLogService;

        public Handler(ICrpgDbContext db, IMapper mapper, IDateTime dateTime,
            IRandom random, IUserService userService, ICharacterService characterService,
            IActivityLogService activityLogService)
        {
            _db = db;
            _mapper = mapper;
            _dateTime = dateTime;
            _random = random;
            _userService = userService;
            _characterService = characterService;
            _activityLogService = activityLogService;
        }

        public async Task<Result<GameUserViewModel>> Handle(GetGameUserCommand req, CancellationToken cancellationToken)
        {
            var user = await _db.Users
                .Include(u => u.ActiveCharacter)
                .Include(u => u.ClanMembership)
                .FirstOrDefaultAsync(u => u.Platform == req.Platform && u.PlatformUserId == req.PlatformUserId,
                    cancellationToken);

            if (user == null)
            {
                user = CreateUser(req.Platform, req.PlatformUserId, req.Region);
                _db.Users.Add(user);

                await _db.SaveChangesAsync(cancellationToken);
                Logger.LogInformation("User joined ({1}#{2})", req.Platform, req.PlatformUserId);

                // No need to save here since a character will be created and a save will be needed.
                _db.ActivityLogs.Add(_activityLogService.CreateUserCreatedLog(user.Id));
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

            if (user.ActiveCharacter == null)
            {
                if (await HasRecentlyCreatedACharacter(user.Id))
                {
                    Logger.LogInformation("User '{0}' tried to create two characters in a short time window", user.Id);
                    return new(CommonErrors.CharacterRecentlyCreated(user.Id));
                }

                var itemSet = await GiveUserRandomItemSet(user);
                var newCharacter = CreateCharacter(itemSet);
                user.Characters.Add(newCharacter);
                user.ActiveCharacter = newCharacter;

                // Need to save new user and new character in two times because of the circular dependency
                // between the two (https://github.com/dotnet/efcore/issues/1699).
                await _db.SaveChangesAsync(cancellationToken);
                Logger.LogInformation("User '{0}' created character '{1}'", user.Id, newCharacter.Id);

                _db.ActivityLogs.Add(_activityLogService.CreateCharacterCreatedLog(user.Id, user.ActiveCharacter.Id));
                await _db.SaveChangesAsync(cancellationToken);
            }
            else
            {
                // Load items in separate query to avoid cartesian explosion if character has many items equipped.
                await _db.Entry(user.ActiveCharacter)
                    .Collection(c => c.EquippedItems)
                    .Query()
                    .Include(ei => ei.UserItem)
                    .LoadAsync(cancellationToken);
            }

            var gameUser = _mapper.Map<GameUserViewModel>(user);
            gameUser.Restrictions = gameUser.Restrictions
                .Where(r => _dateTime.UtcNow < r.CreatedAt + r.Duration)
                .ToArray();
            return new(gameUser);
        }

        private User CreateUser(Platform platform, string platformUserId, Region region)
        {
            User user = new()
            {
                Platform = platform,
                PlatformUserId = platformUserId,
                Region = region,
            };

            _userService.SetDefaultValuesForUser(user);
            return user;
        }

        /// <summary>
        /// To protect against players creating many characters to sell the peasant items, we check that no other
        /// character was created in the last hour.
        /// </summary>
        private Task<bool> HasRecentlyCreatedACharacter(int userId)
        {
            if (userId == default)
            {
                return Task.FromResult(false);
            }

            return _db.Characters
                .IgnoreQueryFilters()
                .AnyAsync(c =>
                c.UserId == userId && _dateTime.UtcNow < c.CreatedAt + TimeSpan.FromHours(1));
        }

        private Character CreateCharacter(IList<EquippedItem> equippedItems)
        {
            Character character = new()
            {
                Name = "Peasant",
                Class = CharacterClass.Peasant,
                EquippedItems = equippedItems,
                Statistics = new CharacterStatistics
                {
                    Kills = 0,
                    Deaths = 0,
                    Assists = 0,
                    PlayTime = TimeSpan.Zero,
                },
                Limitations = new CharacterLimitations
                {
                    LastRespecializeAt = _dateTime.UtcNow,
                },
            };

            _characterService.SetDefaultValuesForCharacter(character);
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
                        ItemId = item.Id,
                        User = user,
                        IsBroken = false,
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
