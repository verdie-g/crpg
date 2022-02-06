namespace Crpg.GameMod.Api.Models.Characters;

// Copy of Crpg.Application.Characters.Model.CharacterStatisticsViewModel
internal class CrpgCharacterStatistics
{
    public CrpgCharacterAttributes Attributes { get; set; } = new();
    public CrpgCharacterSkills Skills { get; set; } = new();
    public CrpgCharacterWeaponProficiencies WeaponProficiencies { get; set; } = new();
}
