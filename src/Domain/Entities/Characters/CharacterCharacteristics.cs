namespace Crpg.Domain.Entities.Characters;

/// <summary>
/// Characteristics of a character.
/// </summary>
public class CharacterCharacteristics
{
    public CharacterAttributes Attributes { get; set; } = new();
    public CharacterSkills Skills { get; set; } = new();
    public CharacterWeaponProficiencies WeaponProficiencies { get; set; } = new();
}
