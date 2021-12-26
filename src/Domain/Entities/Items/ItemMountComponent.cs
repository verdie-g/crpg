namespace Crpg.Domain.Entities.Items;

/// <summary>
/// Mount component of an <see cref="Item"/>.
/// </summary>
public class ItemMountComponent : ICloneable
{
    public int BodyLength { get; set; }
    public int ChargeDamage { get; set; }
    public int Maneuver { get; set; }
    public int Speed { get; set; }
    public int HitPoints { get; set; }

    public object Clone() => new ItemMountComponent
    {
        BodyLength = BodyLength,
        ChargeDamage = ChargeDamage,
        Maneuver = Maneuver,
        Speed = Speed,
        HitPoints = HitPoints,
    };
}
