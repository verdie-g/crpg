using Crpg.Common.Helpers;
using Crpg.Domain.Entities.Characters;

namespace Crpg.Application.Common.Services
{
    /// <summary>
    /// Common logic for characters.
    /// </summary>
    internal interface ICharacterService
    {
        void SetDefaultValuesForCharacter(Character character);

        /// <summary>
        /// Reset character stats.
        /// </summary>
        /// <param name="character">Character to reset.</param>
        /// <param name="respecialization">If the stats points should be redistributed.</param>
        void ResetCharacterStats(Character character, bool respecialization = false);

        void GiveExperience(Character character, int experience);
    }

    /// <inheritdoc />
    internal class CharacterService : ICharacterService
    {
        private readonly IExperienceTable _experienceTable;
        private readonly Constants _constants;

        public CharacterService(IExperienceTable experienceTable, Constants constants)
        {
            _experienceTable = experienceTable;
            _constants = constants;
        }

        public void SetDefaultValuesForCharacter(Character character)
        {
            character.Generation = _constants.DefaultGeneration;
            character.Level = _constants.MinimumLevel;
            character.Experience = _experienceTable.GetExperienceForLevel(character.Level);
            character.ExperienceMultiplier = _constants.DefaultExperienceMultiplier;
            character.SkippedTheFun = false;
            character.AutoRepair = _constants.DefaultAutoRepair;
            character.BodyProperties = _constants.DefaultCharacterBodyProperties;
            character.Gender = _constants.DefaultCharacterGender;
        }

        /// <inheritdoc />
        public void ResetCharacterStats(Character character, bool respecialization = false)
        {
            character.Statistics = new CharacterStatistics
            {
                Attributes = new CharacterAttributes
                {
                    Points = respecialization ? (character.Level - 1) * _constants.AttributePointsPerLevel : 0,
                    Strength = _constants.DefaultStrength,
                    Agility = _constants.DefaultAgility,
                },
                Skills = new CharacterSkills
                {
                    Points = respecialization ? (character.Level - 1) * _constants.SkillPointsPerLevel : 0,
                },
                WeaponProficiencies = new CharacterWeaponProficiencies
                {
                    Points = WeaponProficiencyPointsForLevel(respecialization ? character.Level : 1),
                }
            };
        }

        public void GiveExperience(Character character, int experience)
        {
            if (character.SkippedTheFun)
            {
                return;
            }

            character.Experience += (int)(character.ExperienceMultiplier * experience);
            int newLevel = _experienceTable.GetLevelForExperience(character.Experience);
            if (character.Level != newLevel) // if character leveled up
            {
                int levelDiff = newLevel - character.Level;
                character.Statistics.Attributes.Points += levelDiff * _constants.AttributePointsPerLevel;
                character.Statistics.Skills.Points += levelDiff * _constants.SkillPointsPerLevel;
                character.Statistics.WeaponProficiencies.Points += WeaponProficiencyPointsForLevel(newLevel) - WeaponProficiencyPointsForLevel(character.Level);
                character.Level = newLevel;
            }
        }

        private int WeaponProficiencyPointsForLevel(int lvl) =>
            (int)MathHelper.ApplyPolynomialFunction(lvl, _constants.WeaponProficiencyPointsForLevelCoefs);
    }
}
