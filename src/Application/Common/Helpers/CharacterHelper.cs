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

        public static void SetDefaultValuesForNewCharacter(Character character)
        {
            character.Level = DefaultLevel;
            character.Experience = DefaultExperience;
            character.ExperienceMultiplier = DefaultExperienceMultiplier;
            character.Statistics.Attributes.Strength = DefaultStrength;
            character.Statistics.Attributes.Agility = DefaultAgility;
        }
    }
}