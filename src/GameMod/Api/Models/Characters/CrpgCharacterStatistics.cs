namespace Crpg.GameMod.Api.Models.Characters
{
    // Copy of Crpg.Application.Characters.Model.CharacterStatisticsViewModel
    internal class CrpgCharacterStatistics
    {
        public CrpgCharacterAttributes Attributes { get; set; } = new CrpgCharacterAttributes();
        public CrpgCharacterSkills Skills { get; set; } = new CrpgCharacterSkills();
        public CrpgCharacterWeaponProficiencies WeaponProficiencies { get; set; } = new CrpgCharacterWeaponProficiencies();
    }
}
