using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Common.Services;

public class ItemModifiers
{
    public ArmorItemModifier[] Armor { get; init; } = default!;
    public MountItemModifier[] Mount { get; init; } = default!;
    public ShieldItemModifier[] Shield { get; init; } = default!;
    public BowItemModifier[] Bow { get; init; } = default!;
    public CrossbowItemModifier[] Crossbow { get; init; } = default!;
    public WeaponItemModifier[] Weapon { get; init; } = default!;
    public WeaponItemModifier[] Polearm { get; init; } = default!;
    public ThrownItemModifier[] Thrown { get; init; } = default!;
    public MissileItemModifier[] Missile { get; init; } = default!;
}

public class ItemModifier
{
    public string Name { get; init; } = string.Empty;
    public float Price { get; init; }

    public virtual void Apply(Item item)
    {
        item.Name = Name + " " + item.Name;
        item.Price = (int)Math.Round(item.Price * Price);
    }

    protected static int Scale(int val, float factor) => (int)Math.Round(val * factor);
    protected static float Scale(float val, float factor) => val * factor;
}

public class ArmorItemModifier : ItemModifier
{
    public float Armor { get; init; }

    public override void Apply(Item item)
    {
        base.Apply(item);
        item.Armor!.HeadArmor = Scale(item.Armor!.HeadArmor, Armor);
        item.Armor.BodyArmor = Scale(item.Armor.BodyArmor, Armor);
        item.Armor.ArmArmor = Scale(item.Armor.ArmArmor, Armor);
        item.Armor.LegArmor = Scale(item.Armor.LegArmor, Armor);
    }
}

public class MountItemModifier : ItemModifier
{
    public float ChargeDamage { get; init; }
    public float Maneuver { get; init; }
    public float Speed { get; init; }
    public float HitPoints { get; init; }

    public override void Apply(Item item)
    {
        base.Apply(item);
        item.Mount!.ChargeDamage = Scale(item.Mount.ChargeDamage, ChargeDamage);
        item.Mount!.Maneuver = Scale(item.Mount.Maneuver, Maneuver);
        item.Mount!.Speed = Scale(item.Mount.Speed, Speed);
        item.Mount!.HitPoints = Scale(item.Mount.HitPoints, HitPoints);
    }
}

public class ShieldItemModifier : ItemModifier
{
    public float Speed { get; init; }
    public float Durability { get; init; }
    public float Armor { get; init; }

    public override void Apply(Item item)
    {
        base.Apply(item);
        item.PrimaryWeapon!.SwingSpeed = Scale(item.PrimaryWeapon.SwingSpeed, Speed);
        item.PrimaryWeapon.StackAmount = Scale(item.PrimaryWeapon.StackAmount, Durability);
        item.PrimaryWeapon.BodyArmor = Scale(item.PrimaryWeapon.BodyArmor, Armor);
    }
}

public class BowItemModifier : ItemModifier
{
    public float Damage { get; init; }
    public float FireRate { get; init; }
    public float Accuracy { get; init; }

    public override void Apply(Item item)
    {
        base.Apply(item);
        item.PrimaryWeapon!.ThrustDamage = Scale(item.PrimaryWeapon.ThrustDamage, Damage);
        item.PrimaryWeapon.ThrustSpeed = Scale(item.PrimaryWeapon.ThrustSpeed, FireRate);
        item.PrimaryWeapon.Accuracy = Scale(item.PrimaryWeapon.Accuracy, Accuracy);
    }
}

public class CrossbowItemModifier : BowItemModifier
{
}

public class WeaponItemModifier : ItemModifier
{
    public float Damage { get; init; }
    public float Speed { get; init; }

    public override void Apply(Item item)
    {
        base.Apply(item);
        foreach (var weapon in item.GetWeapons())
        {
            weapon.SwingDamage = Scale(weapon.SwingDamage, Damage);
            weapon.SwingSpeed = Scale(weapon.SwingSpeed, Speed);
            weapon.ThrustDamage = Scale(weapon.ThrustDamage, Damage);
            weapon.ThrustSpeed = Scale(weapon.ThrustSpeed, Speed);
        }
    }
}

public class ThrownItemModifier : ItemModifier
{
    public float Damage { get; init; }
    public float FireRate { get; init; }
    public float Accuracy { get; init; }

    public override void Apply(Item item)
    {
        base.Apply(item);
        item.PrimaryWeapon!.ThrustDamage = Scale(item.PrimaryWeapon.ThrustDamage, Damage);
        item.PrimaryWeapon.MissileSpeed = Scale(item.PrimaryWeapon.ThrustDamage, FireRate);
        item.PrimaryWeapon.Accuracy = Scale(item.PrimaryWeapon.MissileSpeed, Accuracy);
    }
}

public class MissileItemModifier : ItemModifier
{
    public float Weight { get; init; }
    public float Damage { get; init; }
    public float StackAmount { get; init; }

    public override void Apply(Item item)
    {
        base.Apply(item);
        item.Weight = Scale(item.Weight, Weight);
        item.PrimaryWeapon!.ThrustDamage = Scale(item.PrimaryWeapon.ThrustDamage, Damage);
        item.PrimaryWeapon.StackAmount = Scale(item.PrimaryWeapon.StackAmount, StackAmount);
    }
}
