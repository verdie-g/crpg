using Crpg.Application.Characters.Models;
using Crpg.Domain.Entities.Characters;

namespace Crpg.Application.Common.Services;

/// <summary>Model to resolves the <see cref="CharacterClass"/> of a <see cref="Character"/>.</summary>
public interface ICharacterClassModel
{
    /// <summary>Resolves the character's class from its <see cref="CharacterCharacteristics"/>.</summary>
    CharacterClass ResolveCharacterClass(CharacterCharacteristics stats);
}

internal class CharacterClassModel : ICharacterClassModel
{
    private const int MinConsideredWeaponProficiency = 50;
    private const int MinConsideredSkills = 2;

    public CharacterClass ResolveCharacterClass(CharacterCharacteristics stats)
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

    private bool IsMounted(CharacterCharacteristics stats)
    {
        return stats.Skills.Riding >= MinConsideredSkills;
    }

    private bool IsArcher(CharacterCharacteristics stats)
    {
        return stats.WeaponProficiencies.Bow >= MinConsideredWeaponProficiency;
    }

    private bool IsCrossbowman(CharacterCharacteristics stats)
    {
        return stats.WeaponProficiencies.Crossbow >= MinConsideredWeaponProficiency;
    }

    private bool IsSkirmisher(CharacterCharacteristics stats)
    {
        return stats.WeaponProficiencies.Throwing >= MinConsideredWeaponProficiency;
    }

    private bool IsShielded(CharacterCharacteristics stats)
    {
        return stats.Skills.Shield >= MinConsideredSkills;
    }
}
