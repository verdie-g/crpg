using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.System.Commands
{
    public record SeedDataCommand : IMediatorRequest
    {
        internal class Handler : IMediatorRequestHandler<SeedDataCommand>
        {
            private static readonly int[] ItemRanks = { -3, -2, -1, 1, 2, 3 };
            private readonly ICrpgDbContext _db;
            private readonly IItemsSource _itemsSource;
            private readonly IApplicationEnvironment _appEnv;
            private readonly ICharacterService _characterService;
            private readonly IExperienceTable _experienceTable;
            private readonly ItemValueModel _itemValueModel;
            private readonly ItemModifierService _itemModifierService;

            public Handler(ICrpgDbContext db, IItemsSource itemsSource, IApplicationEnvironment appEnv,
                ICharacterService characterService, IExperienceTable experienceTable, ItemValueModel itemValueModel,
                ItemModifierService itemModifierService)
            {
                _db = db;
                _itemsSource = itemsSource;
                _appEnv = appEnv;
                _characterService = characterService;
                _experienceTable = experienceTable;
                _itemValueModel = itemValueModel;
                _itemModifierService = itemModifierService;
            }

            public async Task<Result> Handle(SeedDataCommand request, CancellationToken cancellationToken)
            {
                if (_appEnv.Environment == HostingEnvironment.Development)
                {
                    await AddDevelopmentData();
                }

                await CreateOrUpdateItems(cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);
                return new Result();
            }

            private async Task AddDevelopmentData()
            {
                var users = new[]
                {
                    new User
                    {
                        PlatformUserId = "76561197987525637",
                        Name = "takeoshigeru",
                        Gold = 30000,
                        HeirloomPoints = 2,
                        Role = Role.Admin,
                        AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/2c/2ce4694f06523a2ffad501f5dc30ec7a8008e90e.jpg"),
                        AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/2c/2ce4694f06523a2ffad501f5dc30ec7a8008e90e_full.jpg"),
                        AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/2c/2ce4694f06523a2ffad501f5dc30ec7a8008e90e_medium.jpg"),
                        Characters = new List<Character>
                        {
                            new()
                            {
                                Name = "takeoshigeru",
                                Generation = 2,
                                Level = 23,
                                Experience = _experienceTable.GetExperienceForLevel(23),
                            },
                            new()
                            {
                                Name = "totoalala",
                                Level = 12,
                                Experience = _experienceTable.GetExperienceForLevel(12),
                            },
                            new()
                            {
                                Name = "Retire me",
                                Level = 31,
                                Experience = _experienceTable.GetExperienceForLevel(31) + 100,
                            },
                        },
                        Bans = new List<Ban>
                        {
                            new()
                            {
                                Duration = TimeSpan.FromDays(2),
                                Reason = "Did shit",
                                BannedByUser = new User { PlatformUserId = "123", Name = "toto" },
                            },
                            new()
                            {
                                Duration = TimeSpan.FromMinutes(5),
                                Reason = "Did shot",
                                BannedByUser = new User { PlatformUserId = "456", Name = "titi" },
                            },
                        },
                    },
                };

                var existingUsers = (await _db.Users.ToArrayAsync())
                    .Select(u => (u.Platform, u.PlatformUserId))
                    .ToHashSet();
                foreach (var user in users)
                {
                    foreach (var character in user.Characters)
                    {
                        _characterService.ResetCharacterStats(character, respecialization: true);
                    }

                    if (!existingUsers.Contains((user.Platform, user.PlatformUserId)))
                    {
                        _db.Users.Add(user);
                    }
                }
            }

            private async Task CreateOrUpdateItems(CancellationToken cancellationToken)
            {
                var itemsByMdId = (await _itemsSource.LoadItems())
                    .ToDictionary(i => i.TemplateMbId);
                var dbItemsByMbId = await _db.Items
                    .ToDictionaryAsync(di => (di.TemplateMbId, di.Rank), cancellationToken);

                var baseItems = new List<Item>();

                foreach (ItemCreation item in itemsByMdId.Values)
                {
                    Item baseItem = ItemCreationToItem(item);
                    baseItem.Value = _itemValueModel.ComputeItemValue(baseItem);
                    // EF Core doesn't support creating an entity referencing itself, which is needed for items with
                    // rank = 0. Workaround is to set BaseItemId to null and replace with the reference to the item
                    // once it was created. This is the only reason why BaseItemId is nullable.
                    baseItem.BaseItemId = null;
                    CreateOrUpdateItem(dbItemsByMbId, baseItem);
                    baseItems.Add(baseItem);

                    foreach (int rank in ItemRanks)
                    {
                        var modifiedItem = _itemModifierService.ModifyItem(baseItem, rank);
                        modifiedItem.BaseItem = baseItem;
                        CreateOrUpdateItem(dbItemsByMbId, modifiedItem);
                    }
                }

                // Remove items that were deleted from the item source
                foreach (Item dbItem in dbItemsByMbId.Values)
                {
                    if (dbItem.Rank != 0 || itemsByMdId.ContainsKey(dbItem.TemplateMbId))
                    {
                        continue;
                    }

                    var ownedItems = await _db.OwnedItems
                        .Include(oi => oi.User)
                        .Include(oi => oi.Item)
                        .Where(oi => oi.Item!.BaseItemId == dbItem.BaseItemId)
                        .ToArrayAsync(cancellationToken);
                    foreach (var ownedItem in ownedItems)
                    {
                        ownedItem.User!.Gold += ownedItem.Item!.Value;
                        if (ownedItem.Item.Rank > 0)
                        {
                            ownedItem.User.HeirloomPoints += ownedItem.Item.Rank;
                        }

                        _db.OwnedItems.Remove(ownedItem);
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

            private void CreateOrUpdateItem(Dictionary<(string mbId, int rank), Item> dbItemsByMbId, Item item)
            {
                if (dbItemsByMbId.TryGetValue((item.TemplateMbId, item.Rank), out Item? dbItem))
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

            private static Item ItemCreationToItem(ItemCreation item)
            {
                var res = new Item
                {
                    TemplateMbId = item.TemplateMbId,
                    Name = item.Name,
                    Culture = item.Culture,
                    Type = item.Type,
                    Weight = item.Weight,
                    Rank = item.Rank,
                };

                if (item.Armor != null)
                {
                    res.Armor = new ItemArmorComponent
                    {
                        HeadArmor = item.Armor!.HeadArmor,
                        BodyArmor = item.Armor!.BodyArmor,
                        ArmArmor = item.Armor!.ArmArmor,
                        LegArmor = item.Armor!.LegArmor,
                    };
                }

                if (item.Mount != null)
                {
                    res.Mount = new ItemMountComponent
                    {
                        BodyLength = item.Mount!.BodyLength,
                        ChargeDamage = item.Mount!.ChargeDamage,
                        Maneuver = item.Mount!.Maneuver,
                        Speed = item.Mount!.Speed,
                        HitPoints = item.Mount!.HitPoints,
                    };
                }

                if (item.Weapons.Count > 0)
                {
                    res.PrimaryWeapon = IteamWeaponComponentFromViewModel(item.Weapons[0]);
                }

                if (item.Weapons.Count > 1)
                {
                    res.SecondaryWeapon = IteamWeaponComponentFromViewModel(item.Weapons[1]);
                }

                if (item.Weapons.Count > 2)
                {
                    res.TertiaryWeapon = IteamWeaponComponentFromViewModel(item.Weapons[2]);
                }

                return res;
            }

            private static ItemWeaponComponent IteamWeaponComponentFromViewModel(ItemWeaponComponentViewModel weaponComponent)
            {
                return new()
                {
                    Class = weaponComponent.Class,
                    Accuracy = weaponComponent.Accuracy,
                    MissileSpeed = weaponComponent.MissileSpeed,
                    StackAmount = weaponComponent.StackAmount,
                    Length = weaponComponent.Length,
                    Balance = weaponComponent.Balance,
                    Handling = weaponComponent.Handling,
                    BodyArmor = weaponComponent.BodyArmor,
                    Flags = weaponComponent.Flags,
                    ThrustDamage = weaponComponent.ThrustDamage,
                    ThrustDamageType = weaponComponent.ThrustDamageType,
                    ThrustSpeed = weaponComponent.ThrustSpeed,
                    SwingDamage = weaponComponent.SwingDamage,
                    SwingDamageType = weaponComponent.SwingDamageType,
                    SwingSpeed = weaponComponent.SwingSpeed,
                };
            }
        }
    }
}
