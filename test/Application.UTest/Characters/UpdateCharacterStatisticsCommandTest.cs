using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Characters.Commands;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters
{
    public class UpdateCharacterStatisticsCommandTest : TestBase
    {
        private static readonly Constants Constants = new()
        {
            WeaponProficiencyPointsForAgilityCoefs = new[] { 14f, 0f }, // wpp = agi * 14
            WeaponProficiencyPointsForWeaponMasterCoefs = new[] { 10f, 0f }, // wpp = wm * 10
            WeaponProficiencyCostCoefs = new[] { 3f, 0f }, // wpp cost = wp * 3
        };

        [Test]
        public async Task ShouldUpdateIfEnoughPoints()
        {
            var character = new Character
            {
                Statistics = new CharacterStatistics
                {
                    Attributes = new CharacterAttributes
                    {
                        Points = 29 + 58,
                        Strength = 1,
                        Agility = 2,
                    },
                    Skills = new CharacterSkills
                    {
                        Points = 45,
                        IronFlesh = 1,
                        PowerStrike = 2,
                        PowerDraw = 3,
                        PowerThrow = 4,
                        Athletics = 5,
                        Riding = 6,
                        WeaponMaster = 7,
                        MountedArchery = 8,
                        Shield = 9,
                    },
                    WeaponProficiencies = new CharacterWeaponProficiencies
                    {
                        OneHanded = 1,
                        TwoHanded = 2,
                        Polearm = 3,
                        Bow = 4,
                        Throwing = 5,
                        Crossbow = 6,
                    },
                },
            };
            ArrangeDb.Add(character);
            await ArrangeDb.SaveChangesAsync();

            var result = await new UpdateCharacterStatisticsCommand.Handler(ActDb, Mapper, Constants).Handle(
                new UpdateCharacterStatisticsCommand
                {
                    UserId = character.UserId,
                    CharacterId = character.Id,
                    Statistics = new CharacterStatisticsViewModel
                    {
                        Attributes = new CharacterAttributesViewModel
                        {
                            Strength = 30,
                            Agility = 60,
                        },
                        Skills = new CharacterSkillsViewModel
                        {
                            IronFlesh = 10,
                            PowerStrike = 10,
                            PowerDraw = 10,
                            PowerThrow = 10,
                            Athletics = 10,
                            Riding = 10,
                            WeaponMaster = 10,
                            MountedArchery = 10,
                            Shield = 10,
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

            var stats = result.Data!;
            Assert.AreEqual(0, stats.Attributes.Points);
            Assert.AreEqual(30, stats.Attributes.Strength);
            Assert.AreEqual(60, stats.Attributes.Agility);
            Assert.AreEqual(0, stats.Skills.Points);
            Assert.AreEqual(10, stats.Skills.IronFlesh);
            Assert.AreEqual(10, stats.Skills.PowerStrike);
            Assert.AreEqual(10, stats.Skills.PowerDraw);
            Assert.AreEqual(10, stats.Skills.PowerThrow);
            Assert.AreEqual(10, stats.Skills.Athletics);
            Assert.AreEqual(10, stats.Skills.Riding);
            Assert.AreEqual(10, stats.Skills.WeaponMaster);
            Assert.AreEqual(10, stats.Skills.MountedArchery);
            Assert.AreEqual(10, stats.Skills.Shield);
            Assert.AreEqual(779, stats.WeaponProficiencies.Points);
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
            var character = ArrangeDb.Add(new Character
            {
                Statistics = new CharacterStatistics
                {
                    Attributes = new CharacterAttributes { Points = 3 },
                    WeaponProficiencies = new CharacterWeaponProficiencies { Points = 84 },
                },
            });
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateCharacterStatisticsCommand.Handler(ActDb, Mapper, Constants);

            var result = await handler.Handle(new UpdateCharacterStatisticsCommand
            {
                CharacterId = character.Entity.Id,
                UserId = character.Entity.UserId,
                Statistics = new CharacterStatisticsViewModel
                {
                    Attributes = new CharacterAttributesViewModel { Agility = 1 },
                }
            }, CancellationToken.None);

            var stats = result.Data!;
            Assert.AreEqual(1, stats.Attributes.Agility);
            Assert.AreEqual(98, stats.WeaponProficiencies.Points);

            result = await handler.Handle(new UpdateCharacterStatisticsCommand
            {
                CharacterId = character.Entity.Id,
                UserId = character.Entity.UserId,
                Statistics = new CharacterStatisticsViewModel
                {
                    Attributes = new CharacterAttributesViewModel { Agility = 3 },
                    WeaponProficiencies = new CharacterWeaponProficienciesViewModel { Bow = 42 },
                }
            }, CancellationToken.None);

            stats = result.Data!;
            Assert.AreEqual(3, stats.Attributes.Agility);
            Assert.AreEqual(0, stats.WeaponProficiencies.Points);
            Assert.AreEqual(42, stats.WeaponProficiencies.Bow);
        }

        [Test]
        public async Task IncreaseWeaponMasterShouldIncreaseWeaponProficienciesPoints()
        {
            var character = ArrangeDb.Add(new Character
            {
                Statistics = new CharacterStatistics
                {
                    Attributes = new CharacterAttributes { Agility = 9 },
                    Skills = new CharacterSkills { Points = 3 },
                    WeaponProficiencies = new CharacterWeaponProficiencies { Points = 270 },
                },
            });
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateCharacterStatisticsCommand.Handler(ActDb, Mapper, Constants);

            var result = await handler.Handle(new UpdateCharacterStatisticsCommand
            {
                CharacterId = character.Entity.Id,
                UserId = character.Entity.UserId,
                Statistics = new CharacterStatisticsViewModel
                {
                    Attributes = new CharacterAttributesViewModel { Agility = 9 },
                    Skills = new CharacterSkillsViewModel { WeaponMaster = 1 },
                }
            }, CancellationToken.None);

            var stats = result.Data!;
            Assert.AreEqual(1, stats.Skills.WeaponMaster);
            Assert.AreEqual(280, stats.WeaponProficiencies.Points);

            result = await handler.Handle(new UpdateCharacterStatisticsCommand
            {
                CharacterId = character.Entity.Id,
                UserId = character.Entity.UserId,
                Statistics = new CharacterStatisticsViewModel
                {
                    Attributes = new CharacterAttributesViewModel { Agility = 9 },
                    Skills = new CharacterSkillsViewModel { WeaponMaster = 3 },
                    WeaponProficiencies = new CharacterWeaponProficienciesViewModel { Bow = 100 },
                }
            }, CancellationToken.None);

            stats = result.Data!;
            Assert.AreEqual(3, stats.Skills.WeaponMaster);
            Assert.AreEqual(0, stats.WeaponProficiencies.Points);
            Assert.AreEqual(100, stats.WeaponProficiencies.Bow);
        }

        [Test]
        public async Task ShouldntUpdateIfInconsistentStats()
        {
            var character = new Character
            {
                Statistics = new CharacterStatistics
                {
                    Attributes = new CharacterAttributes { Points = 100 },
                    Skills = new CharacterSkills { Points = 100 },
                }
            };
            ArrangeDb.Add(character);
            await ArrangeDb.SaveChangesAsync();

            var statsObjects = new[]
            {
                new CharacterStatisticsViewModel
                {
                    Attributes = new CharacterAttributesViewModel { Strength = 2 },
                    Skills = new CharacterSkillsViewModel { IronFlesh = 1 },
                },
                new CharacterStatisticsViewModel
                {
                    Attributes = new CharacterAttributesViewModel { Strength = 2 },
                    Skills = new CharacterSkillsViewModel { PowerStrike = 1 },
                },
                new CharacterStatisticsViewModel
                {
                    Attributes = new CharacterAttributesViewModel { Strength = 2 },
                    Skills = new CharacterSkillsViewModel { PowerDraw = 1 },
                },
                new CharacterStatisticsViewModel
                {
                    Attributes = new CharacterAttributesViewModel { Strength = 2 },
                    Skills = new CharacterSkillsViewModel { PowerThrow = 1 },
                },
                new CharacterStatisticsViewModel
                {
                    Attributes = new CharacterAttributesViewModel { Agility = 2 },
                    Skills = new CharacterSkillsViewModel { Athletics = 1 },
                },
                new CharacterStatisticsViewModel
                {
                    Attributes = new CharacterAttributesViewModel { Agility = 2 },
                    Skills = new CharacterSkillsViewModel { Riding = 1 },
                },
                new CharacterStatisticsViewModel
                {
                    Attributes = new CharacterAttributesViewModel { Agility = 2 },
                    Skills = new CharacterSkillsViewModel { WeaponMaster = 1 },
                },
                new CharacterStatisticsViewModel
                {
                    Attributes = new CharacterAttributesViewModel { Agility = 5 },
                    Skills = new CharacterSkillsViewModel { MountedArchery = 1 },
                },
                new CharacterStatisticsViewModel
                {
                    Attributes = new CharacterAttributesViewModel { Agility = 5 },
                    Skills = new CharacterSkillsViewModel { Shield = 1 },
                },
            };

            var handler = new UpdateCharacterStatisticsCommand.Handler(ActDb, Mapper, Constants);
            foreach (var statObject in statsObjects)
            {
                var result = await handler.Handle(new UpdateCharacterStatisticsCommand
                {
                    UserId = character.UserId,
                    CharacterId = character.Id,
                    Statistics = statObject,
                }, CancellationToken.None);
                Assert.AreEqual(ErrorCode.SkillRequirementNotMet, result.Errors![0].Code);
            }
        }

        [Test]
        public async Task ShouldntUpdateIfNotEnoughAttributePoints()
        {
            var character = new Character();
            ArrangeDb.Add(character);
            await ArrangeDb.SaveChangesAsync();

            var statsObjects = new[]
            {
                new CharacterStatisticsViewModel { Attributes = new CharacterAttributesViewModel { Strength = 1 } },
                new CharacterStatisticsViewModel { Attributes = new CharacterAttributesViewModel { Agility = 1 } },
            };

            var handler = new UpdateCharacterStatisticsCommand.Handler(ActDb, Mapper, Constants);
            foreach (var statObject in statsObjects)
            {
                var result = await handler.Handle(new UpdateCharacterStatisticsCommand
                {
                    UserId = character.UserId,
                    CharacterId = character.Id,
                    Statistics = statObject,
                }, CancellationToken.None);
                Assert.AreEqual(ErrorCode.NotEnoughAttributePoints, result.Errors![0].Code);
            }
        }

        [Test]
        public async Task ShouldntUpdateIfNotEnoughSkillPoints()
        {
            var character = new Character();
            ArrangeDb.Add(character);
            await ArrangeDb.SaveChangesAsync();

            var statsObjects = new[]
            {
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { IronFlesh = 1 } },
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { PowerStrike = 1 } },
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { PowerDraw = 1 } },
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { PowerThrow = 1 } },
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { Athletics = 1 } },
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { WeaponMaster = 1 } },
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { Riding = 1 } },
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { MountedArchery = 1 } },
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { Shield = 1 } },
            };

            var handler = new UpdateCharacterStatisticsCommand.Handler(ActDb, Mapper, Constants);
            foreach (var statObject in statsObjects)
            {
                var result = await handler.Handle(new UpdateCharacterStatisticsCommand
                {
                    UserId = character.UserId,
                    CharacterId = character.Id,
                    Statistics = statObject,
                }, CancellationToken.None);
                Assert.AreEqual(ErrorCode.NotEnoughSkillPoints, result.Errors![0].Code);
            }
        }

        [Test]
        public async Task ShouldntUpdateIfNotEnoughWeaponProficiencyPoints()
        {
            var character = new Character();
            ArrangeDb.Add(character);
            await ArrangeDb.SaveChangesAsync();

            var statsObjects = new[]
            {
                new CharacterStatisticsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { OneHanded = 1 } },
                new CharacterStatisticsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { TwoHanded = 1 } },
                new CharacterStatisticsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { Polearm = 1 } },
                new CharacterStatisticsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { Bow = 1 } },
                new CharacterStatisticsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { Throwing = 1 } },
                new CharacterStatisticsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { Crossbow = 1 } },
            };

            var handler = new UpdateCharacterStatisticsCommand.Handler(ActDb, Mapper, Constants);
            foreach (var statObject in statsObjects)
            {
                var result = await handler.Handle(new UpdateCharacterStatisticsCommand
                {
                    UserId = character.UserId,
                    CharacterId = character.Id,
                    Statistics = statObject,
                }, CancellationToken.None);
                Assert.AreEqual(ErrorCode.NotEnoughWeaponProficiencyPoints, result.Errors![0].Code);
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
            ArrangeDb.Add(character);
            await ArrangeDb.SaveChangesAsync();

            var statsObjects = new[]
            {
                new CharacterStatisticsViewModel { Attributes = new CharacterAttributesViewModel { Agility = -1 } },
                new CharacterStatisticsViewModel { Attributes = new CharacterAttributesViewModel { Strength = -1 } },
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { IronFlesh = -1 } },
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { PowerStrike = -1 } },
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { PowerDraw = -1 } },
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { PowerThrow = -1 } },
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { Athletics = -1 } },
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { Riding = -1 } },
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { WeaponMaster = -1 } },
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { MountedArchery = -1 } },
                new CharacterStatisticsViewModel { Skills = new CharacterSkillsViewModel { Shield = -1 } },
                new CharacterStatisticsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { OneHanded = -1 } },
                new CharacterStatisticsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { TwoHanded = -1 } },
                new CharacterStatisticsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { Polearm = -1 } },
                new CharacterStatisticsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { Bow = -1 } },
                new CharacterStatisticsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { Throwing = -1 } },
                new CharacterStatisticsViewModel { WeaponProficiencies = new CharacterWeaponProficienciesViewModel { Crossbow = -1 } },
            };

            var handler = new UpdateCharacterStatisticsCommand.Handler(ActDb, Mapper, Constants);
            foreach (var statObject in statsObjects)
            {
                var result = await handler.Handle(new UpdateCharacterStatisticsCommand
                {
                    UserId = character.UserId,
                    CharacterId = character.Id,
                    Statistics = statObject,
                }, CancellationToken.None);
                Assert.AreEqual(ErrorCode.StatisticDecreased, result.Errors![0].Code);
            }
        }

        [Test]
        public async Task ShouldThrowNotFoundIfCharacterNotFound()
        {
             var user = ArrangeDb.Add(new User());
             await ArrangeDb.SaveChangesAsync();

             var handler = new UpdateCharacterStatisticsCommand.Handler(ActDb, Mapper, Constants);
             var result = await handler.Handle(new UpdateCharacterStatisticsCommand
             {
                 UserId = user.Entity.Id,
                 CharacterId = 1,
                 Statistics = new CharacterStatisticsViewModel(),
             }, CancellationToken.None);
             Assert.AreEqual(ErrorCode.CharacterNotFound, result.Errors![0].Code);
        }

        [Test]
        public async Task ShouldThrowNotFoundIfUserNotFound()
        {
             var handler = new UpdateCharacterStatisticsCommand.Handler(ActDb, Mapper, Constants);
             var result = await handler.Handle(new UpdateCharacterStatisticsCommand
             {
                 UserId = 1,
                 CharacterId = 1,
                 Statistics = new CharacterStatisticsViewModel(),
             }, CancellationToken.None);
             Assert.AreEqual(ErrorCode.CharacterNotFound, result.Errors![0].Code);
        }
    }
}
