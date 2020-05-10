using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Games;
using Crpg.Application.Games.Commands;
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
            private readonly ICrpgDbContext _db;
            private readonly IItemsSource _itemsSource;

            public Handler(ICrpgDbContext db, IItemsSource itemsSource)
            {
                _db = db;
                _itemsSource = itemsSource;
            }

            public async Task<Unit> Handle(SeedDataCommand request, CancellationToken cancellationToken)
            {
                await CreateOrUpdateItems(cancellationToken);

                _db.Users.AddRange(
                    new User
                    {
                        SteamId = 76561197987525637,
                        UserName = "takeoshigeru",
                        Gold = 3000,
                        Role = Role.SuperAdmin,
                        AvatarSmall = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/2c/2ce4694f06523a2ffad501f5dc30ec7a8008e90e.jpg"),
                        AvatarFull = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/2c/2ce4694f06523a2ffad501f5dc30ec7a8008e90e_full.jpg"),
                        AvatarMedium = new Uri("https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/2c/2ce4694f06523a2ffad501f5dc30ec7a8008e90e_medium.jpg"),
                        Characters = new List<Character>
                        {
                            new Character
                            {
                                Name = "takeoshigeru",
                                Level = 23,
                                Experience = ExperienceTable.GetExperienceForLevel(23),
                                Statistics = new CharacterStatistics
                                {
                                    Attributes = new CharacterAttributes
                                    {
                                        Points = 4,
                                        Strength = 3,
                                        Agility = 3,
                                    },
                                    Skills = new CharacterSkills { Points = 7 },
                                    WeaponProficiencies = new CharacterWeaponProficiencies { Points = 43 },
                                },
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
                        }
                    });

                await _db.SaveChangesAsync(cancellationToken);

                return Unit.Value;
            }

            private async Task CreateOrUpdateItems(CancellationToken cancellationToken)
            {
                var itemsByMdId = (await _itemsSource.LoadItems())
                    .ToDictionary(i => i.MbId, i => i);
                var dbItemsByMdId = await _db.Items
                    .ToDictionaryAsync(di => di.MbId, di => di, cancellationToken);

                foreach (ItemCreation item in itemsByMdId.Values)
                {
                    bool exists = true;
                    if (!dbItemsByMdId.TryGetValue(item.MbId, out Item? dbItem))
                    {
                        dbItem = new Item();
                        exists = false;
                    }

                    dbItem.MbId = item.MbId;
                    dbItem.Name = item.Name;
                    dbItem.Image = new Uri("https://via.placeholder.com/256x120");
                    dbItem.Type = item.Type;
                    dbItem.Value = item.Value;
                    dbItem.Weight = item.Weight;
                    dbItem.HeadArmor = item.HeadArmor;
                    dbItem.BodyArmor = item.BodyArmor;
                    dbItem.ArmArmor = item.ArmArmor;
                    dbItem.LegArmor = item.LegArmor;
                    dbItem.BodyLength = item.BodyLength;
                    dbItem.ChargeDamage = item.ChargeDamage;
                    dbItem.Maneuver = item.Maneuver;
                    dbItem.Speed = item.Speed;
                    dbItem.HitPoints = item.HitPoints;
                    dbItem.ThrustDamageType = item.ThrustDamageType;
                    dbItem.SwingDamageType = item.SwingDamageType;
                    dbItem.Accuracy = item.Accuracy;
                    dbItem.MissileSpeed = item.MissileSpeed;
                    dbItem.StackAmount = item.StackAmount;
                    dbItem.WeaponLength = item.WeaponLength;
                    dbItem.PrimaryThrustDamage = item.PrimaryThrustDamage;
                    dbItem.PrimaryThrustSpeed = item.PrimaryThrustSpeed;
                    dbItem.PrimarySwingDamage = item.PrimarySwingDamage;
                    dbItem.PrimarySwingSpeed = item.PrimarySwingSpeed;
                    dbItem.PrimaryHandling = item.PrimaryHandling;
                    dbItem.PrimaryWeaponFlags = item.PrimaryWeaponFlags;
                    dbItem.SecondaryThrustDamage = item.SecondaryThrustDamage;
                    dbItem.SecondaryThrustSpeed = item.SecondaryThrustSpeed;
                    dbItem.SecondarySwingDamage = item.SecondarySwingDamage;
                    dbItem.SecondarySwingSpeed = item.SecondarySwingSpeed;
                    dbItem.SecondaryHandling = item.SecondaryHandling;
                    dbItem.SecondaryWeaponFlags = item.SecondaryWeaponFlags;

                    if (!exists)
                    {
                        _db.Items.Add(dbItem);
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