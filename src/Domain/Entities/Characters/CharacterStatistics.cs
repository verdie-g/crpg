namespace Crpg.Domain.Entities.Characters;

/// <summary>
/// Statistics of a character.
/// </summary>
public class CharacterStatistics
{
    public CharacterAttributes Attributes { get; set; } = new();
    public CharacterSkills Skills { get; set; } = new();
    public CharacterWeaponProficiencies WeaponProficiencies { get; set; } = new();
}
