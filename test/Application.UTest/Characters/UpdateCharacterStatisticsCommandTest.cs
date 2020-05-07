using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Characters.Commands;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Exceptions;
using Crpg.Domain.Entities;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters
{
    public class UpdateCharacterStatisticsCommandTest : TestBase
    {
        [Test]
        public async Task ShouldUpdateIfEnoughPoints()
        {
            var character = new Character
            {
                Statistics = new CharacterStatistics
                {
                    Attributes = new CharacterAttributes
                    {
                        Points = 3,
                        Strength = 1,
                        Agility = 2,
                    },
                    Skills = new CharacterSkills
                    {
                        Points = 45,
                        Athletics = 1,
                        HorseArchery = 2,
                        IronFlesh = 3,
                        PowerDraw = 4,
                        PowerStrike = 5,
                        PowerThrow = 6,
                        Riding = 7,
                        Shield = 8,
                        WeaponMaster = 9,
                    },
                    WeaponProficiencies = new CharacterWeaponProficiencies
                    {
                        Points = 21,
                        OneHanded = 1,
                        TwoHanded = 2,
                        Polearm = 3,
                        Bow = 4,
                        Throwing = 5,
                        Crossbow = 6,
                    },
                },
            };
            Db.Add(character);
            await Db.SaveChangesAsync();

            var stats = await new UpdateCharacterStatisticsCommand.Handler(Db, Mapper).Handle(
                new UpdateCharacterStatisticsCommand
                {
                    UserId = character.UserId,
                    CharacterId = character.Id,
                    Statistics = new CharacterStatisticsViewModel
                    {
                        Attributes = new CharacterAttributesViewModel
                        {
                            Strength = 3,
                            Agility = 3,
                        },
                        Skills = new CharacterSkillsViewModel
                        {
                            Athletics = 10,
                            HorseArchery = 10,
                            IronFlesh = 10,
                            PowerDraw = 10,
                            PowerStrike = 10,
                            PowerThrow = 10,
                            Riding = 10,
                            Shield = 10,
                            WeaponMaster = 10,
                        },
                        WeaponProficiencies = new CharacterWeaponProficienciesViewModel
                        {
                            OneHanded = 7,
                            TwoHanded = 7,
                            Polearm = 7,
                            Bow = 7,
                            Throwing = 7,
                            Crossbow = 7,
                        },
                    }
                }, CancellationToken.None);

            Assert.AreEqual(0, stats.Attributes.Points);
            Assert.AreEqual(3, stats.Attributes.Strength);
            Assert.AreEqual(3, stats.Attributes.Agility);
            Assert.AreEqual(0, stats.Skills.Points);
            Assert.AreEqual(10, stats.Skills.Athletics);
            Assert.AreEqual(10, stats.Skills.HorseArchery);
            Assert.AreEqual(10, stats.Skills.IronFlesh);
            Assert.AreEqual(10, stats.Skills.PowerDraw);
            Assert.AreEqual(10, stats.Skills.PowerStrike);
            Assert.AreEqual(10, stats.Skills.PowerThrow);
            Assert.AreEqual(10, stats.Skills.Riding);
            Assert.AreEqual(10, stats.Skills.Shield);
            Assert.AreEqual(10, stats.Skills.WeaponMaster);
            Assert.AreEqual(34, stats.WeaponProficiencies.Points); // 34 because +1 AGI and +1 WP
            Assert.AreEqual(7, stats.WeaponProficiencies.OneHanded);
            Assert.AreEqual(7, stats.WeaponProficiencies.TwoHanded);
            Assert.AreEqual(7, stats.WeaponProficiencies.Polearm);
            Assert.AreEqual(7, stats.WeaponProficiencies.Bow);
            Assert.AreEqual(7, stats.WeaponProficiencies.Throwing);
            Assert.AreEqual(7, stats.WeaponProficiencies.Crossbow);
        }

        [Test]
        public async Task IncreaseAgilityShouldIncreaseWeaponProficienciesPoints()
        {
            var character = Db.Add(new Character
            {
                Statistics = new CharacterStatistics { Attributes = new CharacterAttributes { Points = 3 } },
            });
            await Db.SaveChangesAsync();

            var handler = new UpdateCharacterStatisticsCommand.Handler(Db, Mapper);

            var stats = await handler.Handle(new UpdateCharacterStatisticsCommand
            {
                CharacterId = character.Entity.Id,
                UserId = character.Entity.UserId,
                Statistics = new CharacterStatisticsViewModel
                {
                    Attributes = new CharacterAttributesViewModel { Agility = 1 },
                }
            }, CancellationToken.None);

            Assert.AreEqual(1, stats.Attributes.Agility);
            Assert.AreEqual(14, stats.WeaponProficiencies.Points);

            stats = await handler.Handle(new UpdateCharacterStatisticsCommand
            {
                CharacterId = character.Entity.Id,
                UserId = character.Entity.UserId,
                Statistics = new CharacterStatisticsViewModel
                {
                    Attributes = new CharacterAttributesViewModel { Agility = 3 },
                    WeaponProficiencies = new CharacterWeaponProficienciesViewModel { Bow = 42 },
                }
            }, CancellationToken.None);

            Assert.AreEqual(3, stats.Attributes.Agility);
            Assert.AreEqual(0, stats.WeaponProficiencies.Points);
            Assert.AreEqual(42, stats.WeaponProficiencies.Bow);
        }

        [Test]
        public async Task IncreaseWeaponMasterShouldIncreaseWeaponProficienciesPoints()
        {
            var character = Db.Add(new Character
            {
                Statistics = new CharacterStatistics { Skills = new CharacterSkills { Points = 3 } },
            });
            await Db.SaveChangesAsync();

            var handler = new UpdateCharacterStatisticsCommand.Handler(Db, Mapper);

            var stats = await handler.Handle(new UpdateCharacterStatisticsCommand
            {
                CharacterId = character.Entity.Id,
                UserId = character.Entity.UserId,
                Statistics = new CharacterStatisticsViewModel
                {
                    Skills = new CharacterSkillsViewModel { WeaponMaster = 1 },
                }
            }, CancellationToken.None);

            Assert.AreEqual(1, stats.Skills.WeaponMaster);
            Assert.AreEqual(75, stats.WeaponProficiencies.Points);

            stats = await handler.Handle(new UpdateCharacterStatisticsCommand
            {
                CharacterId = character.Entity.Id,
                UserId = character.Entity.UserId,
                Statistics = new CharacterStatisticsViewModel
                {
                    Skills = new CharacterSkillsViewModel { WeaponMaster = 3 },
                    WeaponProficiencies = new CharacterWeaponProficienciesViewModel { Bow = 100 },
                }
            }, CancellationToken.None);

            Assert.AreEqual(3, stats.Skills.WeaponMaster);
            Assert.AreEqual(15, stats.WeaponProficiencies.Points);
            Assert.AreEqual(100, stats.WeaponProficiencies.Bow);
        }

        [Test]
        public async Task ShouldntUpdateIfNotEnoughPoints()
        {
            var character = new Character();
            Db.Add(character);
            await Db.SaveChangesAsync();

            var statsObjects = new[]
            {
                new CharacterStatisticsViewModel { Attributes = new CharacterAttributesViewModel { Agility = 1 } },
                new CharacterStatisticsViewModel { Attributes = new CharacterAttributesViewModel { Strength = 1 } },
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { Athletics = 1 } },
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { HorseArchery = 1 } },
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { IronFlesh = 1 } },
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { PowerDraw = 1 } },
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { PowerStrike = 1 } },
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { PowerThrow = 1 } },
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { Riding = 1 } },
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { Shield = 1 } },
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { WeaponMaster = 1 } },
                new CharacterStatisticsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { OneHanded = 1 } },
                new CharacterStatisticsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { TwoHanded = 1 } },
                new CharacterStatisticsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { Polearm = 1 } },
                new CharacterStatisticsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { Bow = 1 } },
                new CharacterStatisticsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { Throwing = 1 } },
                new CharacterStatisticsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { Crossbow = 1 } },
            };

            var handler = new UpdateCharacterStatisticsCommand.Handler(Db, Mapper);
            foreach (var statObject in statsObjects)
            {
                Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(new UpdateCharacterStatisticsCommand
                {
                    UserId = character.UserId,
                    CharacterId = character.Id,
                    Statistics = statObject,
                }, CancellationToken.None));
            }
        }

        [Test]
        public async Task StatShouldntBeDecreased()
        {
            var character = new Character
            {
                Statistics = new CharacterStatistics
                {
                    Attributes = new CharacterAttributes { Points = 1000 },
                    Skills = new CharacterSkills { Points = 1000 },
                    WeaponProficiencies = new CharacterWeaponProficiencies { Points = 1000 },
                }
            };
            Db.Add(character);
            await Db.SaveChangesAsync();

            var statsObjects = new[]
            {
                new CharacterStatisticsViewModel { Attributes = new CharacterAttributesViewModel { Agility = -1 } },
                new CharacterStatisticsViewModel { Attributes = new CharacterAttributesViewModel { Strength = -1 } },
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { Athletics = -1 } },
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { HorseArchery = -1 } },
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { IronFlesh = -1 } },
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { PowerDraw = -1 } },
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { PowerStrike = -1 } },
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { PowerThrow = -1 } },
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { Riding = -1 } },
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { Shield = -1 } },
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { WeaponMaster = -1 } },
                new CharacterStatisticsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { OneHanded = -1 } },
                new CharacterStatisticsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { TwoHanded = -1 } },
                new CharacterStatisticsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { Polearm = -1 } },
                new CharacterStatisticsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { Bow = -1 } },
                new CharacterStatisticsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { Throwing = -1 } },
                new CharacterStatisticsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { Crossbow = -1 } },
            };

            var handler = new UpdateCharacterStatisticsCommand.Handler(Db, Mapper);
            foreach (var statObject in statsObjects)
            {
                Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(new UpdateCharacterStatisticsCommand
                {
                    UserId = character.UserId,
                    CharacterId = character.Id,
                    Statistics = statObject,
                }, CancellationToken.None));
            }
        }

        [Test]
        public async Task StatShouldntBeIncreasedIfNotEnoughPoints()
        {
             var character = new Character
             {
                 Statistics = new CharacterStatistics
                 {
                     Attributes = new CharacterAttributes { Points = 0 },
                     Skills = new CharacterSkills { Points = 0 },
                     WeaponProficiencies = new CharacterWeaponProficiencies { Points = 0 },
                 }
             };
             Db.Add(character);
             await Db.SaveChangesAsync();
 
             var statsObjects = new[]
             {
                 new CharacterStatisticsViewModel { Attributes = new CharacterAttributesViewModel { Agility = 1 } },
                 new CharacterStatisticsViewModel { Attributes = new CharacterAttributesViewModel { Strength = 1 } },
                 new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { Athletics = 1 } },
                 new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { HorseArchery = 1 } },
                 new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { IronFlesh = 1 } },
                 new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { PowerDraw = 1 } },
                 new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { PowerStrike = 1 } },
                 new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { PowerThrow = 1 } },
                 new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { Riding = 1 } },
                 new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { Shield = 1 } },
                 new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { WeaponMaster = 1 } },
                 new CharacterStatisticsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { OneHanded = 1 } },
                 new CharacterStatisticsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { TwoHanded = 1 } },
                 new CharacterStatisticsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { Polearm = 1 } },
                 new CharacterStatisticsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { Bow = 1 } },
                 new CharacterStatisticsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { Throwing = 1 } },
                 new CharacterStatisticsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { Crossbow = 1 } },
             };

             var handler = new UpdateCharacterStatisticsCommand.Handler(Db, Mapper);
             foreach (var statObject in statsObjects)
             {
                 Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(new UpdateCharacterStatisticsCommand
                 {
                     UserId = character.UserId,
                     CharacterId = character.Id,
                     Statistics = statObject,
                 }, CancellationToken.None));
             }
        }

        [Test]
        public async Task ShouldThrowNotFoundIfCharacterNotFound()
        {
             var user = Db.Add(new User());
             await Db.SaveChangesAsync();

             var handler = new UpdateCharacterStatisticsCommand.Handler(Db, Mapper);
             Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(new UpdateCharacterStatisticsCommand
             {
                 UserId = user.Entity.Id,
                 CharacterId = 1,
                 Statistics = new CharacterStatisticsViewModel(),
             }, CancellationToken.None));
        }

        [Test]
        public void ShouldThrowNotFoundIfUserNotFound()
        {
             var handler = new UpdateCharacterStatisticsCommand.Handler(Db, Mapper);
             Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(new UpdateCharacterStatisticsCommand
             {
                 UserId = 1,
                 CharacterId = 1,
                 Statistics = new CharacterStatisticsViewModel(),
             }, CancellationToken.None));
        }
    }
}