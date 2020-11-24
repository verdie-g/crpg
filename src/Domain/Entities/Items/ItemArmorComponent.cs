namespace Crpg.Domain.Entities.Items
{
    /// <summary>
    /// Armor component of an <see cref="Item"/>.
    /// </summary>
    public class ItemArmorComponent
    {
        public int HeadArmor { get; set; }
        public int BodyArmor { get; set; }
        public int ArmArmor { get; set; }
        public int LegArmor { get; set; }
    }
}
