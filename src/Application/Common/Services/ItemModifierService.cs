using System;
using System.Collections.Generic;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Common.Services
{
    /// <summary>
    /// Service for looming and repair <see cref="ItemCreation"/>.
    /// </summary>
    public class ItemModifierService
    {
        private readonly Dictionary<ItemType, ItemModifier[]> _itemModifiers;

        public ItemModifierService(ItemModifiers itemModifiers)
        {
            _itemModifiers = new Dictionary<ItemType, ItemModifier[]>
            {
                [ItemType.HeadArmor] = itemModifiers.Armor,
                [ItemType.ShoulderArmor] = itemModifiers.Armor,
                [ItemType.BodyArmor] = itemModifiers.Armor,
                [ItemType.HandArmor] = itemModifiers.Armor,
                [ItemType.LegArmor] = itemModifiers.Armor,
                [ItemType.MountHarness] = itemModifiers.Armor,
                [ItemType.Mount] = itemModifiers.Mount,
                [ItemType.Shield] = itemModifiers.Shield,
                [ItemType.Bow] = itemModifiers.Bow,
                [ItemType.Crossbow] = itemModifiers.Crossbow,
                [ItemType.OneHandedWeapon] = itemModifiers.Weapon,
                [ItemType.TwoHandedWeapon] = itemModifiers.Weapon,
                [ItemType.Polearm] = itemModifiers.Polearm,
                [ItemType.Thrown] = itemModifiers.Thrown,
                [ItemType.Arrows] = itemModifiers.Missile,
                [ItemType.Bolts] = itemModifiers.Missile,
            };
        }

        /// <summary>
        /// Create a new item from an rank 0 item and given rank.
        /// </summary>
        /// <param name="baseItem">Rank 0 item.</param>
        /// <param name="rank">Result rank.</param>
        /// <returns>A new item instance of rank <paramref name="rank"/>.</returns>
        public ItemCreation ModifyItem(ItemCreation baseItem, int rank)
        {
            if (rank == 0 || rank < -3 || rank > 3)
            {
                throw new ArgumentException("Rank should be one of { -3, -2, -1, 1, 2, 3 }");
            }

            var clone = (ItemCreation)baseItem.Clone();
            clone.Rank = rank;
            if (!_itemModifiers.TryGetValue(baseItem.Type, out ItemModifier[]? typeItemModifiers))
            {
                // For banners and firearms use a random type for now.
                typeItemModifiers = _itemModifiers[ItemType.OneHandedWeapon];
            }

            Index idx = rank < 0 ? rank + 3 : rank + 2;
            ItemModifier modifier = typeItemModifiers[idx];

            modifier.Apply(clone);
            return clone;
        }
    }

    public class ItemModifiers
    {
        public ArmorItemModifier[] Armor { get; set; } = default!;
        public MountItemModifier[] Mount { get; set; } = default!;
        public ShieldItemModifier[] Shield { get; set; } = default!;
        public BowItemModifier[] Bow { get; set; } = default!;
        public CrossbowItemModifier[] Crossbow { get; set; } = default!;
        public WeaponItemModifier[] Weapon { get; set; } = default!;
        public WeaponItemModifier[] Polearm { get; set; } = default!;
        public ThrownItemModifier[] Thrown { get; set; } = default!;
        public MissileItemModifier[] Missile { get; set; } = default!;
    }

    public class ItemModifier
    {
        public string Name { get; set; } = string.Empty;
        public float Value { get; set; }

        public virtual void Apply(ItemCreation item)
        {
            item.Name = Name + " " + item.Name;
            item.Value = (int)Math.Round(item.Value * Value);
        }

        protected static int Scale(int val, float factor) => (int)Math.Round(val * factor);
        protected static float Scale(float val, float factor) => val * factor;
    }

    public class ArmorItemModifier : ItemModifier
    {
        public float Armor { get; set; }

        public override void Apply(ItemCreation item)
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
        public float ChargeDamage { get; set; }
        public float Maneuver { get; set; }
        public float Speed { get; set; }
        public float HitPoints { get; set; }

        public override void Apply(ItemCreation item)
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
        public float Speed { get; set; }
        public float Durability { get; set; }
        public float Armor { get; set; }

        public override void Apply(ItemCreation item)
        {
            base.Apply(item);
            item.Weapons[0].SwingSpeed = Scale(item.Weapons[0].SwingSpeed, Speed);
            item.Weapons[0].StackAmount = Scale(item.Weapons[0].StackAmount, Durability);
            item.Weapons[0].BodyArmor = Scale(item.Weapons[0].BodyArmor, Armor);
        }
    }

    public class BowItemModifier : ItemModifier
    {
        public float Damage { get; set; }
        public float FireRate { get; set; }
        public float Accuracy { get; set; }

        public override void Apply(ItemCreation item)
        {
            base.Apply(item);
            item.Weapons[0].ThrustDamage = Scale(item.Weapons[0].ThrustDamage, Damage);
            item.Weapons[0].ThrustSpeed = Scale(item.Weapons[0].ThrustSpeed, FireRate);
            item.Weapons[0].Accuracy = Scale(item.Weapons[0].Accuracy, Accuracy);
        }
    }

    public class CrossbowItemModifier : BowItemModifier
    {
    }

    public class WeaponItemModifier : ItemModifier
    {
        public float Damage { get; set; }
        public float Speed { get; set; }

        public override void Apply(ItemCreation item)
        {
            base.Apply(item);
            foreach (var weapon in item.Weapons)
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
        public float Damage { get; set; }
        public float FireRate { get; set; }
        public float Accuracy { get; set; }

        public override void Apply(ItemCreation item)
        {
            base.Apply(item);
            item.Weapons[0].ThrustDamage = Scale(item.Weapons[0].ThrustDamage, Damage);
            item.Weapons[0].MissileSpeed = Scale(item.Weapons[0].ThrustDamage, FireRate);
            item.Weapons[0].Accuracy = Scale(item.Weapons[0].MissileSpeed, Accuracy);
        }
    }

    public class MissileItemModifier : ItemModifier
    {
        public float Weight { get; set; }
        public float Damage { get; set; }
        public float StackAmount { get; set; }

        public override void Apply(ItemCreation item)
        {
            base.Apply(item);
            item.Weight = Scale(item.Weight, Weight);
            item.Weapons[0].ThrustDamage = Scale(item.Weapons[0].ThrustDamage, Damage);
            item.Weapons[0].StackAmount = Scale(item.Weapons[0].StackAmount, StackAmount);
        }
    }
}
