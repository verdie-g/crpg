using System;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Characters;

namespace Crpg.Application.Common.Helpers
{
    internal static class CharacterHelper
    {
        public const int DefaultGeneration = 0;
        public const int DefaultLevel = 1;
        public const int DefaultExperience = 0;
        public const float DefaultExperienceMultiplier = 1.0f;
        public const bool DefaultAutoRepair = true;
        public const string DefaultBodyProperties = "0018880540003341783567B87A8C5A7791D94C672ABB9E8734775BD78C2B866900D776030D96978800000000000000000000000000000000000000002FA49042";
        public const CharacterGender DefaultGender = CharacterGender.Male;

        private const int DefaultStrength = 3;
        private const int DefaultAgility = 3;
        private const int AttributePointsPerLevel = 1;
        private const int SkillPointsPerLevel = 1;

        public static void SetDefaultValuesForCharacter(Character character)
        {
            character.Generation = DefaultGeneration;
            character.Level = DefaultLevel;
            character.Experience = DefaultExperience;
            character.ExperienceMultiplier = DefaultExperienceMultiplier;
            character.AutoRepair = DefaultAutoRepair;
            character.BodyProperties = DefaultBodyProperties;
            character.Gender = DefaultGender;
        }

        /// <summary>
        /// Reset character stats.
        /// </summary>
        /// <param name="character">Character to reset.</param>
        /// <param name="respecialization">If the stats points should be redistributed.</param>
        public static void ResetCharacterStats(Character character, bool respecialization = false)
        {
            character.Statistics = new CharacterStatistics
            {
                Attributes = new CharacterAttributes
                {
                    Points = respecialization ? (character.Level - 1) * AttributePointsPerLevel : 0,
                    Strength = DefaultStrength,
                    Agility = DefaultAgility,
                },
                Skills = new CharacterSkills
                {
                    Points = respecialization ? (character.Level - 1) * SkillPointsPerLevel : 0,
                },
                WeaponProficiencies = new CharacterWeaponProficiencies
                {
                    Points = WeaponProficiencyPointsForLevel(respecialization ? character.Level : DefaultLevel),
                }
            };
        }

        public static void LevelUp(Character character, int newLevel)
        {
            int levelDiff = newLevel - character.Level;
            character.Statistics.Attributes.Points += levelDiff * AttributePointsPerLevel;
            character.Statistics.Skills.Points += levelDiff * SkillPointsPerLevel;
            character.Statistics.WeaponProficiencies.Points += WeaponProficiencyPointsForLevel(newLevel)
                - WeaponProficiencyPointsForLevel(character.Level);
            character.Level = newLevel;
        }

        private static int WeaponProficiencyPointsForLevel(int lvl)
        {
            const float a = 0.1f;
            const float b = 4.9f;
            const float c = 52f;

            return (int)(a * Math.Pow(lvl, 2) + b * lvl + c);
        }
    }
}
