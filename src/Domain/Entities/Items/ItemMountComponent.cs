namespace Crpg.Domain.Entities.Items
{
    /// <summary>
    /// Mount component of an <see cref="Item"/>.
    /// </summary>
    public class ItemMountComponent
    {
        public int BodyLength { get; set; }
        public int ChargeDamage { get; set; }
        public int Maneuver { get; set; }
        public int Speed { get; set; }
        public int HitPoints { get; set; }
    }
}
