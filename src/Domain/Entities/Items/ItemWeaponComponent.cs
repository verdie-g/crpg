namespace Crpg.Domain.Entities.Items;

/// <summary>
/// Represents a weapon mode.
/// </summary>
public class ItemWeaponComponent : ICloneable
{
    public WeaponClass Class { get; set; }
    public string ItemUsage { get; set; } = string.Empty;
    public int Accuracy { get; set; }
    public int MissileSpeed { get; set; }

    /// <summary>
    /// Number of ammo for <see cref="ItemType.Bow"/> or <see cref="ItemType.Crossbow"/> or <see cref="ItemType.Thrown"/>.
    /// Durability for <see cref="ItemType.Shield"/>.
    /// </summary>
    public int StackAmount { get; set; }
    public int Length { get; set; }
    public float Balance { get; set; }
    public int Handling { get; set; }

    /// <summary>
    /// Armor for <see cref="ItemType.Shield"/>.
    /// </summary>
    public int BodyArmor { get; set; }
    public WeaponFlags Flags { get; set; }

    public int ThrustDamage { get; set; }
    public DamageType ThrustDamageType { get; set; }
    public int ThrustSpeed { get; set; }

    public int SwingDamage { get; set; }
    public DamageType SwingDamageType { get; set; }
    public int SwingSpeed { get; set; }

    public object Clone() => new ItemWeaponComponent
    {
        Class = Class,
        ItemUsage = ItemUsage,
        Accuracy = Accuracy,
        MissileSpeed = MissileSpeed,
        StackAmount = StackAmount,
        Length = Length,
        Balance = Balance,
        Handling = Handling,
        BodyArmor = BodyArmor,
        Flags = Flags,
        ThrustDamage = ThrustDamage,
        ThrustDamageType = ThrustDamageType,
        ThrustSpeed = ThrustSpeed,
        SwingDamage = SwingDamage,
        SwingDamageType = SwingDamageType,
        SwingSpeed = SwingSpeed,
    };
}
