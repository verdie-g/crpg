using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Common.Helpers;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Interfaces.Events;
using Crpg.Application.Games.Models;
using Crpg.Common;
using Crpg.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Games.Commands
{
    public class UpsertGameUserCommand : IRequest<GameUser>
    {
        public long SteamId { get; set; } = default!;
        public string CharacterName { get; set; } = default!;

        public class Handler : IRequestHandler<UpsertGameUserCommand, GameUser>
        {
            internal static readonly GameCharacterItems[] DefaultItemsSets =
            {
                // aserai
                new GameCharacterItems
                {
                    HeadItemMbId = "mp_wrapped_desert_cap",
                    BodyItemMbId = "mp_aserai_civil_e",
                    LegItemMbId = "mp_strapped_shoes",
                    Weapon1ItemMbId = "mp_aserai_axe",
                    Weapon2ItemMbId = "mp_throwing_stone",
                },
                // vlandia
                new GameCharacterItems
                {
                    HeadItemMbId = "mp_arming_coif",
                    BodyItemMbId = "mp_sackcloth_tunic",
                    LegItemMbId = "mp_strapped_shoes",
                    Weapon1ItemMbId = "mp_vlandian_billhook",
                    Weapon2ItemMbId = "mp_sling_stone",
                },
                // empire
                new GameCharacterItems
                {
                    HeadItemMbId = "mp_laced_cloth_coif",
                    BodyItemMbId = "mp_hemp_tunic",
                    LegItemMbId = "mp_leather_shoes",
                    Weapon1ItemMbId = "mp_empire_axe",
                    Weapon2ItemMbId = "mp_throwing_stone",
                },
                // sturgia
                new GameCharacterItems
                {
                    HeadItemMbId = "mp_nordic_fur_cap",
                    BodyItemMbId = "mp_northern_tunic",
                    LegItemMbId = "mp_wrapped_shoes",
                    Weapon1ItemMbId = "mp_sturgia_mace",
                    Weapon2ItemMbId = "mp_sling_stone",
                },
                // khuzait
                new GameCharacterItems
                {
                    HeadItemMbId = "mp_nomad_padded_hood",
                    BodyItemMbId = "mp_khuzait_civil_coat_b",
                    LegItemMbId = "mp_strapped_leather_boots",
                    Weapon1ItemMbId = "mp_khuzait_sichel",
                    Weapon2ItemMbId = "mp_throwing_stone",
                },
                // battania
                new GameCharacterItems
                {
                    HeadItemMbId = "mp_battania_civil_hood",
                    BodyItemMbId = "mp_battania_civil_a",
                    LegItemMbId = "mp_rough_tied_boots",
                    Weapon1ItemMbId = "mp_battania_axe",
                    Weapon2ItemMbId = "mp_sling_stone",
                },
                // looters
                new GameCharacterItems
                {
                    HeadItemMbId = "mp_vlandia_bandit_cape_a",
                    BodyItemMbId = "mp_burlap_sack_dress",
                    LegItemMbId = "mp_strapped_leather_boots",
                    Weapon1ItemMbId = "mp_empire_long_twohandedaxe",
                    Weapon2ItemMbId = "mp_throwing_stone",
                },
            };

            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;
            private readonly IEventRaiser _events;
            private readonly IDateTimeOffset _dateTime;
            private readonly IRandom _random;

            public Handler(ICrpgDbContext db, IMapper mapper, IEventRaiser events, IDateTimeOffset dateTime, IRandom random)
            {
                _db = db;
                _mapper = mapper;
                _events = events;
                _dateTime = dateTime;
                _random = random;
            }

            public async Task<GameUser> Handle(UpsertGameUserCommand request, CancellationToken cancellationToken)
            {
                var user = await _db.Users
                    .Where(u => u.SteamId == request.SteamId)
                    // https://github.com/dotnet/efcore/issues/1833#issuecomment-603543685
                    // .Include(u => u.Characters.Where(c => c.Name == request.CharacterName))
                    // .Include(u => u.Bans.OrderByDescending(b => b.Id).Take(1))
                    .Include(u => u.Characters).ThenInclude(c => c.Items.HeadItem)
                    .Include(u => u.Characters).ThenInclude(c => c.Items.CapeItem)
                    .Include(u => u.Characters).ThenInclude(c => c.Items.BodyItem)
                    .Include(u => u.Characters).ThenInclude(c => c.Items.HandItem)
                    .Include(u => u.Characters).ThenInclude(c => c.Items.LegItem)
                    .Include(u => u.Characters).ThenInclude(c => c.Items.HorseHarnessItem)
                    .Include(u => u.Characters).ThenInclude(c => c.Items.HorseItem)
                    .Include(u => u.Characters).ThenInclude(c => c.Items.Weapon1Item)
                    .Include(u => u.Characters).ThenInclude(c => c.Items.Weapon2Item)
                    .Include(u => u.Characters).ThenInclude(c => c.Items.Weapon3Item)
                    .Include(u => u.Characters).ThenInclude(c => c.Items.Weapon4Item)
                    .Include(u => u.Bans)
                    .FirstOrDefaultAsync(cancellationToken);

                // remove once above fixed
                var bans = user != null
                    ? user.Bans.OrderByDescending(b => b.Id).Take(1).ToList()
                    : new List<Ban>();

                if (user == null)
                {
                    user = new User { SteamId = request.SteamId };
                    UserHelper.SetDefaultValuesForUser(user);
                    await AddNewCharacterToUser(user, request.CharacterName, cancellationToken);
                    _db.Users.Add(user);
                    _events.Raise(EventLevel.Info, $"{request.CharacterName} joined ({request.SteamId})",
                        string.Empty, "new_user");
                }
                else if (user.Characters.Count == 0)
                {
                    await AddNewCharacterToUser(user, request.CharacterName, cancellationToken);
                }

                if (_db.Entry(user).State != EntityState.Unchanged
                    || _db.Entry(user.Characters[0]).State != EntityState.Unchanged)
                {
                    await _db.SaveChangesAsync(cancellationToken);
                }

                var gu = _mapper.Map<GameUser>(user);
                gu.Ban = bans.Count != 0 && bans[0].Until > _dateTime.Now
                    ? _mapper.Map<GameBan>(bans[0])
                    : null;
                return gu;
            }

            private async Task AddNewCharacterToUser(User user, string name, CancellationToken cancellationToken)
            {
                var c = new Character
                {
                    Name = name,
                    Level = CharacterHelper.DefaultLevel,
                    Experience = CharacterHelper.DefaultExperience,
                    ExperienceMultiplier = CharacterHelper.DefaultExperienceMultiplier,
                    Items = new CharacterItems
                    {
                        AutoRepair = true,
                    },
                };
                CharacterHelper.ResetCharacterStats(c);

                var items = DefaultItemsSets[_random.Next(DefaultItemsSets.Length - 1)];
                var itemsIdByMdId = await _db.Items
                    .Where(i => i.MbId == items.HeadItemMbId
                                || i.MbId == items.BodyItemMbId
                                || i.MbId == items.LegItemMbId
                                || i.MbId == items.Weapon1ItemMbId
                                || i.MbId == items.Weapon2ItemMbId)
                    .ToDictionaryAsync(i => i.MbId, i => i.Id, cancellationToken);

                c.Items.HeadItemId = itemsIdByMdId[items.HeadItemMbId!];
                c.Items.BodyItemId = itemsIdByMdId[items.BodyItemMbId!];
                c.Items.LegItemId = itemsIdByMdId[items.LegItemMbId!];
                c.Items.Weapon1ItemId = itemsIdByMdId[items.Weapon1ItemMbId!];
                c.Items.Weapon2ItemId = itemsIdByMdId[items.Weapon2ItemMbId!];

                // add character items to user inventory
                user.UserItems = itemsIdByMdId.Values.Select(itemId => new UserItem { ItemId = itemId }).ToList();
                user.Characters.Add(c);
            }
        }
    }
}