namespace Crpg.Domain.Entities.Items
{
    /// <summary>
    /// Horse component of an <see cref="Item"/>.
    /// </summary>
    public class ItemHorseComponent
    {
        public int BodyLength { get; set; }
        public int ChargeDamage { get; set; }
        public int Maneuver { get; set; }
        public int Speed { get; set; }
        public int HitPoints { get; set; }
    }
}
