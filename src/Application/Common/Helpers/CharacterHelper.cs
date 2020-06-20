using System;
using Crpg.Domain.Entities;

namespace Crpg.Application.Common.Helpers
{
    internal static class CharacterHelper
    {
        public const int DefaultGeneration = 0;
        public const int DefaultLevel = 1;
        public const int DefaultExperience = 0;
        public const float DefaultExperienceMultiplier = 1.0f;
        public const bool DefaultAutoRepair = true;

        private const int DefaultStrength = 3;
        private const int DefaultAgility = 3;
        private const int AttributePointsPerLevel = 1;
        private const int SkillPointsPerLevel = 1;

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

        public static void UnequipCharacterItems(CharacterItems items)
        {
            items.HeadItemId = null;
            items.CapeItemId = null;
            items.BodyItemId = null;
            items.HandItemId = null;
            items.LegItemId = null;
            items.HorseHarnessItem = null;
            items.HorseItem = null;
            items.Weapon1Item = null;
            items.Weapon2Item = null;
            items.Weapon3Item = null;
            items.Weapon4Item = null;
        }

        public static void UnequipCharacterItem(CharacterItems items, int itemId)
        {
            if (items.HeadItemId == itemId)
            {
                items.HeadItemId = null;
            }
            else if (items.CapeItemId == itemId)
            {
                items.CapeItemId = null;
            }
            else if (items.BodyItemId == itemId)
            {
                items.BodyItemId = null;
            }
            else if (items.HandItemId == itemId)
            {
                items.HandItemId = null;
            }
            else if (items.LegItemId == itemId)
            {
                items.LegItemId = null;
            }
            else if (items.HorseHarnessItemId == itemId)
            {
                items.HorseHarnessItemId = null;
            }
            else if (items.HorseItemId == itemId)
            {
                items.HorseItemId = null;
            }
            else if (items.Weapon1ItemId == itemId)
            {
                items.Weapon1ItemId = null;
            }
            else if (items.Weapon2ItemId == itemId)
            {
                items.Weapon2ItemId = null;
            }
            else if (items.Weapon3ItemId == itemId)
            {
                items.Weapon3ItemId = null;
            }
            else if (items.Weapon4ItemId == itemId)
            {
                items.Weapon4ItemId = null;
            }
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