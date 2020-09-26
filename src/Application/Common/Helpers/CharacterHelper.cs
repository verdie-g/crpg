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
            items.HorseHarnessItemId = null;
            items.HorseItemId = null;
            items.Weapon1ItemId = null;
            items.Weapon2ItemId = null;
            items.Weapon3ItemId = null;
            items.Weapon4ItemId = null;
        }

        public static void ReplaceCharacterItem(CharacterItems items, int itemToReplaceId, int? itemToReplaceWithId)
        {
            if (items.HeadItemId == itemToReplaceId)
            {
                items.HeadItemId = itemToReplaceWithId;
            }
            else if (items.CapeItemId == itemToReplaceId)
            {
                items.CapeItemId = itemToReplaceWithId;
            }
            else if (items.BodyItemId == itemToReplaceId)
            {
                items.BodyItemId = itemToReplaceWithId;
            }
            else if (items.HandItemId == itemToReplaceId)
            {
                items.HandItemId = itemToReplaceWithId;
            }
            else if (items.LegItemId == itemToReplaceId)
            {
                items.LegItemId = itemToReplaceWithId;
            }
            else if (items.HorseHarnessItemId == itemToReplaceId)
            {
                items.HorseHarnessItemId = itemToReplaceWithId;
            }
            else if (items.HorseItemId == itemToReplaceId)
            {
                items.HorseItemId = itemToReplaceWithId;
            }
            else if (items.Weapon1ItemId == itemToReplaceId)
            {
                items.Weapon1ItemId = itemToReplaceWithId;
            }
            else if (items.Weapon2ItemId == itemToReplaceId)
            {
                items.Weapon2ItemId = itemToReplaceWithId;
            }
            else if (items.Weapon3ItemId == itemToReplaceId)
            {
                items.Weapon3ItemId = itemToReplaceWithId;
            }
            else if (items.Weapon4ItemId == itemToReplaceId)
            {
                items.Weapon4ItemId = itemToReplaceWithId;
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
