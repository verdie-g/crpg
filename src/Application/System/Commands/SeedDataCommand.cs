using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Helpers;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Games;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.System.Commands
{
    public class SeedDataCommand : IRequest
    {
        public bool IsDevelopment { get; set; }

        public class Handler : IRequestHandler<SeedDataCommand>
        {
            private readonly ICrpgDbContext _db;
            private readonly IItemsSource _itemsSource;

            public Handler(ICrpgDbContext db, IItemsSource itemsSource)
            {
                _db = db;
                _itemsSource = itemsSource;
            }

            public async Task<Unit> Handle(SeedDataCommand request, CancellationToken cancellationToken)
            {
                if (request.IsDevelopment)
                {
                    AddDevelopperUsers();
                }

                await CreateOrUpdateItems(cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }

            private void AddDevelopperUsers()
            {
                var users = new[]
                {
                    new User
                    {
                        SteamId = 76561197987525637,
                        UserName = "takeoshigeru",
                        Gold = 30000,
                        Role = Role.SuperAdmin,
                        AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/2c/2ce4694f06523a2ffad501f5dc30ec7a8008e90e.jpg"),
                        AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/2c/2ce4694f06523a2ffad501f5dc30ec7a8008e90e_full.jpg"),
                        AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/2c/2ce4694f06523a2ffad501f5dc30ec7a8008e90e_medium.jpg"),
                        Characters = new List<Character>
                        {
                            new Character
                            {
                                Name = "takeoshigeru",
                                Generation = 2,
                                Level = 23,
                                Experience = ExperienceTable.GetExperienceForLevel(23),
                            },
                            new Character
                            {
                                Name = "totoalala",
                                Level = 12,
                                Experience = ExperienceTable.GetExperienceForLevel(12),
                            },
                            new Character
                            {
                                Name = "Retire me",
                                Level = 31,
                                Experience = ExperienceTable.GetExperienceForLevel(31) + 100,
                            },
                        },
                        Bans = new List<Ban>
                        {
                            new Ban
                            {
                                Duration = TimeSpan.FromDays(2),
                                Reason = "Did shit",
                                BannedByUser = new User { SteamId = 123, UserName = "toto" },
                            },
                            new Ban
                            {
                                Duration = TimeSpan.FromMinutes(5),
                                Reason = "Did shot",
                                BannedByUser = new User { SteamId = 456, UserName = "titi" },
                            },
                        },
                    },
                };

                foreach (var user in users)
                {
                    foreach (var character in user.Characters)
                    {
                        CharacterHelper.ResetCharacterStats(character, respecialization: true);
                    }

                    _db.Users.Add(user);
                }
            }

            private async Task CreateOrUpdateItems(CancellationToken cancellationToken)
            {
                var itemsByMdId = (await _itemsSource.LoadItems())
                    .ToDictionary(i => i.MbId, i => i);
                var dbItemsByMdId = await _db.Items
                    .ToDictionaryAsync(di => di.MbId, di => di, cancellationToken);

                foreach (ItemCreation item in itemsByMdId.Values)
                {
                    var newDbItem = ItemHelper.ToItem(item);
                    if (dbItemsByMdId.TryGetValue(item.MbId, out Item? dbItem))
                    {
                        newDbItem.Id = dbItem.Id;
                        _db.Entry(dbItem).CurrentValues.SetValues(newDbItem);
                    }
                    else
                    {
                        _db.Items.Add(newDbItem);
                    }
                }

                foreach (Item dbItem in dbItemsByMdId.Values)
                {
                    if (!itemsByMdId.ContainsKey(dbItem.MbId))
                    {
                        _db.Items.Remove(dbItem);
                    }
                }
            }
        }
    }
}