using Crpg.Domain.Entities.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crpg.Persistence.Configurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.HasIndex(i => new { ItemTemplateMbId = i.TemplateMbId, i.Rank }).IsUnique();
        builder.OwnsOne(i => i.Armor!, ConfigureItemArmorComponent);
        builder.OwnsOne(i => i.Mount!, ConfigureItemMountComponent);
        builder.OwnsOne(i => i.PrimaryWeapon!, b => ConfigureItemWeaponComponent(b, "primary_"));
        builder.OwnsOne(i => i.SecondaryWeapon!, b => ConfigureItemWeaponComponent(b, "secondary_"));
        builder.OwnsOne(i => i.TertiaryWeapon!, b => ConfigureItemWeaponComponent(b, "tertiary_"));
    }

    private static void ConfigureItemArmorComponent(OwnedNavigationBuilder<Item, ItemArmorComponent> builder)
    {
        // Default names are prefixed with item_armor_component_.
        builder.Property(ac => ac.HeadArmor).HasColumnName("armor_head");
        builder.Property(ac => ac.BodyArmor).HasColumnName("armor_body");
        builder.Property(ac => ac.ArmArmor).HasColumnName("armor_arm");
        builder.Property(ac => ac.LegArmor).HasColumnName("armor_leg");
    }

    private static void ConfigureItemMountComponent(OwnedNavigationBuilder<Item, ItemMountComponent> builder)
    {
        // Default names are prefixed with item_mount_component_.
        builder.Property(mc => mc.BodyLength).HasColumnName("mount_body_length");
        builder.Property(mc => mc.ChargeDamage).HasColumnName("mount_charge_damage");
        builder.Property(mc => mc.Maneuver).HasColumnName("mount_maneuver");
        builder.Property(mc => mc.Speed).HasColumnName("mount_speed");
        builder.Property(mc => mc.HitPoints).HasColumnName("mount_hit_points");
    }

    private static void ConfigureItemWeaponComponent(OwnedNavigationBuilder<Item, ItemWeaponComponent> builder, string prefix)
    {
        // Default names are prefixed with item_weapon_component_.
        builder.Property(wc => wc.Class).HasColumnName(prefix + "class");
        builder.Property(wc => wc.Accuracy).HasColumnName(prefix + "accuracy");
        builder.Property(wc => wc.MissileSpeed).HasColumnName(prefix + "missile_speed");
        builder.Property(wc => wc.StackAmount).HasColumnName(prefix + "stack_amount");
        builder.Property(wc => wc.Length).HasColumnName(prefix + "length");
        builder.Property(wc => wc.Balance).HasColumnName(prefix + "balance");
        builder.Property(wc => wc.Handling).HasColumnName(prefix + "handling");
        builder.Property(wc => wc.BodyArmor).HasColumnName(prefix + "body_armor");
        builder.Property(wc => wc.Flags).HasColumnName(prefix + "flags");
        builder.Property(wc => wc.ThrustDamage).HasColumnName(prefix + "thrust_damage");
        builder.Property(wc => wc.ThrustDamageType).HasColumnName(prefix + "thrust_damage_type");
        builder.Property(wc => wc.ThrustSpeed).HasColumnName(prefix + "thrust_speed");
        builder.Property(wc => wc.SwingDamage).HasColumnName(prefix + "swing_damage");
        builder.Property(wc => wc.SwingDamageType).HasColumnName(prefix + "swing_damage_type");
        builder.Property(wc => wc.SwingSpeed).HasColumnName(prefix + "swing_speed");
    }
}
