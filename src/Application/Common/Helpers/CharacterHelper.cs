using Crpg.Domain.Entities;

namespace Crpg.Application.Common.Helpers
{
    internal static class CharacterHelper
    {
        private const int DefaultLevel = 1;
        private const int DefaultExperience = 0;
        private const float DefaultExperienceMultiplier = 1.0f;
        private const int DefaultStrength = 3;
        private const int DefaultAgility = 3;
        private const int DefaultWeaponProficienciesPoint = 57;

        public static void SetDefaultValuesForCharacter(Character character)
        {
            character.Level = DefaultLevel;
            character.Experience = DefaultExperience;
            character.ExperienceMultiplier = DefaultExperienceMultiplier;

            character.Statistics = new CharacterStatistics
            {
                Attributes = new CharacterAttributes
                {
                    Strength = DefaultStrength,
                    Agility = DefaultAgility,
                },
                WeaponProficiencies = new CharacterWeaponProficiencies
                {
                    Points = DefaultWeaponProficienciesPoint,
                }
            };
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
    }
}