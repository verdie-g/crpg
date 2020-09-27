namespace Crpg.GameMod.Api.Models
{
    // Copy of Crpg.Application.Characters.Model.CharacterWeaponProficienciesViewModel
    internal class CrpgCharacterWeaponProficiencies
    {
        public int Points { get; set; }
        public int OneHanded { get; set; }
        public int TwoHanded { get; set; }
        public int Polearm { get; set; }
        public int Bow { get; set; }
        public int Throwing { get; set; }
        public int Crossbow { get; set; }
    }
}