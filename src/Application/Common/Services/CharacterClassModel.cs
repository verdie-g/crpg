using Crpg.Application.Characters.Models;
using Crpg.Domain.Entities.Characters;

namespace Crpg.Application.Common.Services
{
    /// <summary>Model to resolves the <see cref="CharacterClass"/> of a <see cref="Character"/>.</summary>
    public interface ICharacterClassModel
    {
        /// <summary>Resolves the character's class from its <see cref="CharacterStatistics"/>.</summary>
        CharacterClass ResolveCharacterClass(CharacterStatistics stats);
    }

    internal class CharacterClassModel : ICharacterClassModel
    {
        private const int MinConsideredWeaponProficiency = 50;
        private const int MinConsideredSkills = 2;

        public CharacterClass ResolveCharacterClass(CharacterStatistics stats)
        {
            if (IsMounted(stats))
            {
                return IsArcher(stats) || IsCrossbowman(stats) ? CharacterClass.MountedArcher : CharacterClass.Cavalry;
            }

            if (IsSkirmisher(stats))
            {
                return CharacterClass.Skirmisher;
            }

            if (IsCrossbowman(stats))
            {
                return CharacterClass.Crossbowman;
            }

            if (IsArcher(stats))
            {
                return CharacterClass.Archer;
            }

            return IsShielded(stats) ? CharacterClass.Infantry : CharacterClass.ShockInfantry;
        }

        private bool IsMounted(CharacterStatistics stats)
        {
            return stats.Skills.Riding >= MinConsideredSkills;
        }

        private bool IsArcher(CharacterStatistics stats)
        {
            return stats.WeaponProficiencies.Bow >= MinConsideredWeaponProficiency;
        }

        private bool IsCrossbowman(CharacterStatistics stats)
        {
            return stats.WeaponProficiencies.Crossbow >= MinConsideredWeaponProficiency;
        }

        private bool IsSkirmisher(CharacterStatistics stats)
        {
            return stats.WeaponProficiencies.Throwing >= MinConsideredWeaponProficiency;
        }

        private bool IsShielded(CharacterStatistics stats)
        {
            return stats.Skills.Shield >= MinConsideredSkills;
        }
    }
}
