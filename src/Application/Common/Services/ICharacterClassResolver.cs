using Crpg.Domain.Entities.Characters;

namespace Crpg.Application.Common.Services;

/// <summary>Model to resolves the <see cref="CharacterClass"/> of a <see cref="Character"/>.</summary>
public interface ICharacterClassResolver
{
    /// <summary>Resolves the character's class from its <see cref="CharacterCharacteristics"/>.</summary>
    CharacterClass ResolveCharacterClass(CharacterCharacteristics characteristics);
}

internal class CharacterClassResolver : ICharacterClassResolver
{
    private const int MinConsideredWeaponProficiency = 50;
    private const int MinConsideredSkills = 2;

    public CharacterClass ResolveCharacterClass(CharacterCharacteristics characteristics)
    {
        if (characteristics.Attributes.Strength + characteristics.Attributes.Agility < 20)
        {
            return CharacterClass.Peasant;
        }

        if (IsMounted(characteristics))
        {
            return IsArcher(characteristics) || IsCrossbowman(characteristics) ? CharacterClass.MountedArcher : CharacterClass.Cavalry;
        }

        if (IsSkirmisher(characteristics))
        {
            return CharacterClass.Skirmisher;
        }

        if (IsCrossbowman(characteristics))
        {
            return CharacterClass.Crossbowman;
        }

        if (IsArcher(characteristics))
        {
            return CharacterClass.Archer;
        }

        return IsShielded(characteristics) ? CharacterClass.Infantry : CharacterClass.ShockInfantry;
    }

    private bool IsMounted(CharacterCharacteristics characteristics)
    {
        return characteristics.Skills.Riding >= MinConsideredSkills;
    }

    private bool IsArcher(CharacterCharacteristics characteristics)
    {
        return characteristics.WeaponProficiencies.Bow >= MinConsideredWeaponProficiency;
    }

    private bool IsCrossbowman(CharacterCharacteristics characteristics)
    {
        return characteristics.WeaponProficiencies.Crossbow >= MinConsideredWeaponProficiency;
    }

    private bool IsSkirmisher(CharacterCharacteristics characteristics)
    {
        return characteristics.WeaponProficiencies.Throwing >= MinConsideredWeaponProficiency;
    }

    private bool IsShielded(CharacterCharacteristics characteristics)
    {
        return characteristics.Skills.Shield >= MinConsideredSkills;
    }
}
