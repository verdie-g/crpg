using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Helpers;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Services;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.System.Commands
{
    public class SeedDataCommand : IRequest
    {
        public class Handler : IRequestHandler<SeedDataCommand>
        {
            private static readonly int[] ItemRanks = { -3, -2, -1, 1, 2, 3 };
            private readonly ICrpgDbContext _db;
            private readonly IItemsSource _itemsSource;
            private readonly IApplicationEnvironment _appEnv;
            private readonly ItemModifierService _itemModifier;

            public Handler(ICrpgDbContext db, IItemsSource itemsSource, IApplicationEnvironment appEnv,
                ItemModifierService itemModifier)
            {
                _db = db;
                _itemsSource = itemsSource;
                _appEnv = appEnv;
                _itemModifier = itemModifier;
            }

            public async Task<Unit> Handle(SeedDataCommand request, CancellationToken cancellationToken)
            {
                if (_appEnv.Environment == HostingEnvironment.Development)
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
                        PlatformId = "76561197987525637",
                        Name = "takeoshigeru",
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
                                BannedByUser = new User { PlatformId = "123", Name = "toto" },
                            },
                            new Ban
                            {
                                Duration = TimeSpan.FromMinutes(5),
                                Reason = "Did shot",
                                BannedByUser = new User { PlatformId = "456", Name = "titi" },
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
                var itemsByMdId = (await _itemsSource.LoadItems()).ToDictionary(i => i.MbId);
                var dbItemsByMbId = await _db.Items.ToDictionaryAsync(di => di.MbId, cancellationToken);

                var baseItems = new List<Item>();

                foreach (ItemCreation item in itemsByMdId.Values)
                {
                    var baseItem = ItemHelper.ToItem(item);
                    baseItem.Rank = 0;
                    // EF Core doesn't support creating an entity referencing itself, which is needed for items with
                    // rank = 0. Workaround is to set BaseItemId to null and replace with the reference to the item
                    // once it was created. This is the only reason why BaseItemId is nullable.
                    baseItem.BaseItemId = null;
                    CreateOrUpdateItem(dbItemsByMbId, baseItem);
                    baseItems.Add(baseItem);

                    foreach (int rank in ItemRanks)
                    {
                        var modifiedItem = ItemHelper.ToItem(_itemModifier.ModifyItem(item, rank));
                        modifiedItem.Rank = rank;
                        modifiedItem.BaseItem = baseItem;
                        CreateOrUpdateItem(dbItemsByMbId, modifiedItem);
                    }
                }

                // Remove items that were deleted from the item source
                foreach (Item dbItem in dbItemsByMbId.Values)
                {
                    if (dbItem.Rank != 0 || itemsByMdId.ContainsKey(dbItem.MbId))
                    {
                        continue;
                    }

                    var itemsToDelete = dbItemsByMbId.Values.Where(i => i.BaseItemId == dbItem.BaseItemId).ToArray();
                    foreach (var i in itemsToDelete)
                    {
                        _db.Entry(i).State = EntityState.Deleted;
                    }
                }

                await _db.SaveChangesAsync(cancellationToken);

                // Fix BaseItem for items of rank = 0
                foreach (Item baseItem in baseItems)
                {
                    baseItem.BaseItem = baseItem;
                }
            }

            private void CreateOrUpdateItem(Dictionary<string, Item> dbItemsByMbId, Item item)
            {
                if (dbItemsByMbId.TryGetValue(item.MbId, out Item? dbItem))
                {
                    // replace item in context
                    _db.Entry(dbItem).State = EntityState.Detached;

                    item.Id = dbItem.Id;
                    _db.Items.Update(item);
                }
                else
                {
                    _db.Items.Add(item);
                }
            }
        }
    }
}
