using Crpg.Application.Common;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Characters;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Services
{
    public class CharacterServiceTest
    {
        private static readonly Constants Constants = new()
        {
            MinimumLevel = 1,
            MaximumLevel = 38,
            ExperienceForLevelCoefs = new[] { 0f, 10f, -10f }, // xp = lvl * 10
            DefaultAttributePoints = 0,
            AttributePointsPerLevel = 1,
            DefaultSkillPoints = 2,
            SkillPointsPerLevel = 1,
            WeaponProficiencyPointsForLevelCoefs = new[] { 100f, -90f }, // wpp = lvl * 100 - 90
            DefaultStrength = 3,
            DefaultAgility = 3,
            DefaultGeneration = 1,
            DefaultExperienceMultiplier = 1.0f,
            DefaultAutoRepair = true,
        };

        private static readonly ExperienceTable ExperienceTable = new(Constants);

        [Test]
        public void GiveExperienceShouldGiveExperience()
        {
            var characterService = new CharacterService(ExperienceTable, Constants);
            var character = new Character { Level = 1, Experience = 2, ExperienceMultiplier = 2f, SkippedTheFun = false };
            characterService.GiveExperience(character, 3);

            Assert.AreEqual(1, character.Level);
            Assert.AreEqual(8, character.Experience);
        }

        [Test]
        public void GiveExperienceShouldntGiveExperienceIfSkippedTheFun()
        {
            var characterService = new CharacterService(ExperienceTable, Constants);
            var character = new Character { Level = 1, Experience = 2, ExperienceMultiplier = 2f, SkippedTheFun = true };
            characterService.GiveExperience(character, 3);

            Assert.AreEqual(1, character.Level);
            Assert.AreEqual(2, character.Experience);
        }

        [Test]
        public void GiveExperienceShouldMakeCharacterLevelUpIfEnoughExperience()
        {
            var characterService = new CharacterService(ExperienceTable, Constants);
            var character = new Character { Level = 1, Experience = 2, ExperienceMultiplier = 2f, SkippedTheFun = false };
            characterService.GiveExperience(character, 4);

            Assert.AreEqual(2, character.Level);
            Assert.AreEqual(10, character.Experience);
            Assert.AreEqual(1, character.Statistics.Attributes.Points);
            Assert.AreEqual(1, character.Statistics.Skills.Points);
            Assert.AreEqual(100, character.Statistics.WeaponProficiencies.Points);
        }

        [Test]
        [Theory]
        public void ResetCharacterStatsShouldResetStats(bool respecialization)
        {
            var characterService = new CharacterService(ExperienceTable, Constants);
            var character = new Character
            {
                Level = 5,
                Statistics = new CharacterStatistics
                {
                    Attributes = new CharacterAttributes
                    {
                        Points = 1,
                        Agility = 5,
                        Strength = 7,
                    },
                    Skills = new CharacterSkills
                    {
                        Points = 2,
                        IronFlesh = 3,
                        PowerStrike = 4,
                        PowerDraw = 5,
                        PowerThrow = 6,
                        Athletics = 7,
                        Riding = 8,
                        WeaponMaster = 9,
                        MountedArchery = 10,
                        Shield = 11,
                    },
                    WeaponProficiencies = new CharacterWeaponProficiencies
                    {
                        Points = 1,
                        OneHanded = 2,
                        TwoHanded = 3,
                        Polearm = 4,
                        Bow = 5,
                        Throwing = 6,
                        Crossbow = 7,
                    },
                },
            };

            characterService.ResetCharacterStats(character, respecialization);
            if (respecialization)
            {
                Assert.AreEqual(4, character.Statistics.Attributes.Points);
                Assert.AreEqual(4, character.Statistics.Skills.Points);
                Assert.AreEqual(410, character.Statistics.WeaponProficiencies.Points);
            }
            else
            {
                Assert.Zero(character.Statistics.Attributes.Points);
                Assert.AreEqual(2, character.Statistics.Skills.Points);
                Assert.AreEqual(10, character.Statistics.WeaponProficiencies.Points);
            }

            Assert.AreEqual(Constants.DefaultStrength, character.Statistics.Attributes.Strength);
            Assert.AreEqual(Constants.DefaultAgility, character.Statistics.Attributes.Agility);
            Assert.Zero(character.Statistics.Skills.IronFlesh);
            Assert.Zero(character.Statistics.Skills.PowerStrike);
            Assert.Zero(character.Statistics.Skills.PowerDraw);
            Assert.Zero(character.Statistics.Skills.PowerThrow);
            Assert.Zero(character.Statistics.Skills.Athletics);
            Assert.Zero(character.Statistics.Skills.Riding);
            Assert.Zero(character.Statistics.Skills.WeaponMaster);
            Assert.Zero(character.Statistics.Skills.MountedArchery);
            Assert.Zero(character.Statistics.Skills.Shield);
            Assert.Zero(character.Statistics.WeaponProficiencies.OneHanded);
            Assert.Zero(character.Statistics.WeaponProficiencies.TwoHanded);
            Assert.Zero(character.Statistics.WeaponProficiencies.Polearm);
            Assert.Zero(character.Statistics.WeaponProficiencies.Bow);
            Assert.Zero(character.Statistics.WeaponProficiencies.Throwing);
            Assert.Zero(character.Statistics.WeaponProficiencies.Crossbow);
        }

        [Test]
        public void SetDefaultValuesShouldSetDefaultValues()
        {
            var characterService = new CharacterService(ExperienceTable, Constants);
            var character = new Character { Level = 2, Experience = 2, ExperienceMultiplier = 2f, SkippedTheFun = false };
            characterService.SetDefaultValuesForCharacter(character);

            Assert.AreEqual(Constants.MinimumLevel, character.Level);
            Assert.AreEqual(0, character.Experience);
            Assert.AreEqual(Constants.DefaultExperienceMultiplier, character.ExperienceMultiplier);
            Assert.IsFalse(character.SkippedTheFun);
            Assert.AreEqual(Constants.DefaultAutoRepair, character.AutoRepair);
        }
    }
}
