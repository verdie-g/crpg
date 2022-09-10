namespace Crpg.Domain.Entities.Items;

/// <summary>
/// Armor component of an <see cref="Item"/>.
/// </summary>
public class ItemArmorComponent : ICloneable
{
    public int HeadArmor { get; set; }
    public int BodyArmor { get; set; }
    public int ArmArmor { get; set; }
    public int LegArmor { get; set; }
    public ArmorMaterialType MaterialType { get; set; }

    public object Clone() => new ItemArmorComponent
    {
        HeadArmor = HeadArmor,
        BodyArmor = BodyArmor,
        ArmArmor = ArmArmor,
        LegArmor = LegArmor,
        MaterialType = MaterialType,
    };
}
