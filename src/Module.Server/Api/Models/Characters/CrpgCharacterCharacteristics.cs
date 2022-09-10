namespace Crpg.Module.Api.Models.Characters;

// Copy of Crpg.Application.Characters.Model.CharacterCharacteristicsViewModel
internal class CrpgCharacterCharacteristics
{
    public CrpgCharacterAttributes Attributes { get; set; } = new();
    public CrpgCharacterSkills Skills { get; set; } = new();
    public CrpgCharacterWeaponProficiencies WeaponProficiencies { get; set; } = new();
}
