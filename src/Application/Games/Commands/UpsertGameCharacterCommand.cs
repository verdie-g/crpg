using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Interfaces.Events;
using Crpg.Application.Games.Models;
using Crpg.Common;
using Crpg.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Games.Commands
{
    public class UpsertGameCharacterCommand : IRequest<GameCharacter>
    {
        public long SteamId { get; set; } = default!;
        public string CharacterName { get; set; } = default!;

        public class Handler : IRequestHandler<UpsertGameCharacterCommand, GameCharacter>
        {
            internal static readonly GameCharacter[] DefaultCharacterItems =
            {
                // aserai
                new GameCharacter
                {
                    HeadItemMbId = "mp_wrapped_desert_cap",
                    BodyItemMbId = "mp_aserai_civil_e",
                    LegItemMbId = "mp_strapped_shoes",
                    Weapon1ItemMbId = "mp_aserai_axe",
                    Weapon2ItemMbId = "mp_throwing_stone",
                },
                // vlandia
                new GameCharacter
                {
                    HeadItemMbId = "mp_arming_coif",
                    BodyItemMbId = "mp_sackcloth_tunic",
                    LegItemMbId = "mp_strapped_shoes",
                    Weapon1ItemMbId = "mp_vlandian_billhook",
                    Weapon2ItemMbId = "mp_sling_stone",
                },
                // empire
                new GameCharacter
                {
                    HeadItemMbId = "mp_laced_cloth_coif",
                    BodyItemMbId = "mp_hemp_tunic",
                    LegItemMbId = "mp_leather_shoes",
                    Weapon1ItemMbId = "mp_empire_axe",
                    Weapon2ItemMbId = "mp_throwing_stone",
                },
                // sturgia
                new GameCharacter
                {
                    HeadItemMbId = "mp_nordic_fur_cap",
                    BodyItemMbId = "mp_northern_tunic",
                    LegItemMbId = "mp_wrapped_shoes",
                    Weapon1ItemMbId = "mp_sturgia_mace",
                    Weapon2ItemMbId = "mp_sling_stone",
                },
                // khuzait
                new GameCharacter
                {
                    HeadItemMbId = "mp_nomad_padded_hood",
                    BodyItemMbId = "mp_khuzait_civil_coat_b",
                    LegItemMbId = "mp_strapped_leather_boots",
                    Weapon1ItemMbId = "mp_khuzait_sichel",
                    Weapon2ItemMbId = "mp_throwing_stone",
                },
                // battania
                new GameCharacter
                {
                    HeadItemMbId = "mp_battania_civil_hood",
                    BodyItemMbId = "mp_battania_civil_a",
                    LegItemMbId = "mp_rough_tied_boots",
                    Weapon1ItemMbId = "mp_battania_axe",
                    Weapon2ItemMbId = "mp_sling_stone",
                },
                // looters
                new GameCharacter
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

            public Handler(ICrpgDbContext db, IMapper mapper, IEventRaiser events)
            {
                _db = db;
                _mapper = mapper;
                _events = events;
            }

            public async Task<GameCharacter> Handle(UpsertGameCharacterCommand request, CancellationToken cancellationToken)
            {
                var user = await _db.Users
                    .Where(u => u.SteamId == request.SteamId)
                    // https://github.com/dotnet/efcore/issues/1833#issuecomment-603543685
                    // .Include(u => u.Characters.Where(c => c.Name == request.CharacterName))
                    .Include(u => u.Characters)
                    .FirstOrDefaultAsync(cancellationToken);

                if (user == null)
                {
                    user = new User
                    {
                        SteamId = request.SteamId,
                        Gold = Constants.StartingGold,
                        Role = Constants.DefaultRole,
                        Characters = new List<Character>(),
                    };

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

                return _mapper.Map<GameCharacter>(user.Characters[0]);
            }

            private async Task AddNewCharacterToUser(User user, string name, CancellationToken cancellationToken)
            {
                var c = new Character
                {
                    Name = name,
                    Level = 1,
                    Experience = 0,
                };

                var items = DefaultCharacterItems[ThreadSafeRandom.Instance.Value.Next(DefaultCharacterItems.Length - 1)];
                var itemsIdByMdId = await _db.Items
                    .Where(i => i.MbId == items.HeadItemMbId
                                || i.MbId == items.BodyItemMbId
                                || i.MbId == items.LegItemMbId
                                || i.MbId == items.Weapon1ItemMbId
                                || i.MbId == items.Weapon2ItemMbId)
                    .ToDictionaryAsync(i => i.MbId, i => i.Id, cancellationToken);

                c.HeadItemId = itemsIdByMdId[items.HeadItemMbId];
                c.BodyItemId = itemsIdByMdId[items.BodyItemMbId];
                c.LegItemId = itemsIdByMdId[items.LegItemMbId];
                c.Weapon1ItemId = itemsIdByMdId[items.Weapon1ItemMbId];
                c.Weapon2ItemId = itemsIdByMdId[items.Weapon2ItemMbId];

                // add character items to user inventory
                user.UserItems = itemsIdByMdId.Values.Select(itemId => new UserItem { ItemId = itemId }).ToList();
                user.Characters.Add(c);
            }
        }
    }
}