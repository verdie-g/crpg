namespace Crpg.Domain.Entities.Characters
{
    /// <summary>
    /// Weapon proficiencies of a character.
    /// </summary>
    public class CharacterWeaponProficiencies
    {
        /// <summary>
        /// Points to spent.
        /// </summary>
        public int Points { get; set; }
        public int OneHanded { get; set; }
        public int TwoHanded { get; set; }
        public int Polearm { get; set; }
        public int Bow { get; set; }
        public int Throwing { get; set; }
        public int Crossbow { get; set; }
    }
}
