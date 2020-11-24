namespace Crpg.Domain.Entities.Characters
{
    /// <summary>
    /// Skills of a character.
    /// </summary>
    public class CharacterSkills
    {
        /// <summary>
        /// Points to spent.
        /// </summary>
        public int Points { get; set; }
        public int IronFlesh { get; set; }
        public int PowerStrike { get; set; }
        public int PowerDraw { get; set; }
        public int PowerThrow { get; set; }
        public int Athletics { get; set; }
        public int Riding { get; set; }
        public int WeaponMaster { get; set; }
        public int HorseArchery { get; set; }
        public int Shield { get; set; }
    }
}
