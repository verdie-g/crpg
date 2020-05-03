namespace Crpg.Domain.Entities
{
    public class CharacterStatistics
    {
        public CharacterAttributes Attributes { get; set; } = new CharacterAttributes();
        public CharacterSkills Skills { get; set; } = new CharacterSkills();
        public CharacterWeaponProficiencies WeaponProficiencies { get; set; } = new CharacterWeaponProficiencies();
    }
}