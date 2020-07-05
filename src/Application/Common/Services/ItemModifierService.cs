using System;
using System.Collections.Generic;
using AutoMapper;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities;

namespace Crpg.Application.Common.Services
{
    /// <summary>
    /// Service for looming and repair <see cref="ItemCreation"/>.
    /// </summary>
    public class ItemModifierService
    {
        private static readonly ItemModifier[] ArmorModifiers =
        {
            new ArmorModifier { Name = "Damaged",    Value = 0.55f, Armor = 0.8f },
            new ArmorModifier { Name = "Battered",   Value = 0.75f, Armor = 0.9f },
            new ArmorModifier { Name = "Rusty",      Value = 0.83f, Armor = 0.95f },
            new ArmorModifier { Name = "Thick",      Value = 1.1f,  Armor = 1.05f },
            new ArmorModifier { Name = "Reinforced", Value = 1.2f,  Armor = 1.07f },
            new ArmorModifier { Name = "Lordly",     Value = 1.3f,  Armor = 1.09f },
        };

        private static readonly ItemModifier[] HorseModifiers =
        {
            new HorseModifier { Name = "Lame",      Value = 0.3f,  HitPoints = 0.75f, Speed = 0.7f,  Maneuver = 0.7f,  ChargeDamage = 0.8f },
            new HorseModifier { Name = "Swayback",  Value = 0.7f,  HitPoints = 0.85f, Speed = 0.8f,  Maneuver = 0.8f,  ChargeDamage = 1f },
            new HorseModifier { Name = "Stubborn",  Value = 0.85f, HitPoints = 0.95f, Speed = 0.9f,  Maneuver = 0.9f,  ChargeDamage = 1f },
            new HorseModifier { Name = "Well Bred", Value = 1.1f,  HitPoints = 1.05f, Speed = 1f,    Maneuver = 1.04f, ChargeDamage = 1.05f },
            new HorseModifier { Name = "Spirited",  Value = 1.2f,  HitPoints = 1.05f, Speed = 1.04f, Maneuver = 1.04f, ChargeDamage = 1.1f },
            new HorseModifier { Name = "Champion",  Value = 1.3f,  HitPoints = 1.07f, Speed = 1.07f, Maneuver = 1.07f, ChargeDamage = 1.15f },
        };

        private static readonly ItemModifier[] ShieldModifiers =
        {
            new ShieldModifier { Name = "Damaged",     Value = 0.3f,  Speed = 0.9f,  Durability = 0.3f,  Armor = 0.3f },
            new ShieldModifier { Name = "Battered",    Value = 0.6f,  Speed = 0.95f, Durability = 0.5f,  Armor = 0.5f },
            new ShieldModifier { Name = "Dented",      Value = 0.85f, Speed = 1f,    Durability = 0.7f,  Armor = 0.7f },
            new ShieldModifier { Name = "Balanced",    Value = 1.1f,  Speed = 1.03f, Durability = 1.07f, Armor = 1f },
            new ShieldModifier { Name = "Reinforced",  Value = 1.2f,  Speed = 1.03f, Durability = 1.15f, Armor = 1.1f },
            new ShieldModifier { Name = "Masterpiece", Value = 1.3f,  Speed = 1.04f, Durability = 1.2f,  Armor = 1.15f },
        };

        private static readonly ItemModifier[] BowModifiers =
        {
            new BowModifier { Name = "Cracked",    Value = 0.3f,  Damage = 0.7f,  FireRate = 0.75f, Accuracy = 0.94f },
            new BowModifier { Name = "Chipped",    Value = 0.5f,  Damage = 0.8f,  FireRate = 0.85f, Accuracy = 0.96f },
            new BowModifier { Name = "Bent",       Value = 0.65f, Damage = 0.9f,  FireRate = 0.95f, Accuracy = 0.98f },
            new BowModifier { Name = "Strong",     Value = 1.1f,  Damage = 1.05f, FireRate = 1.03f, Accuracy = 1.01f },
            new BowModifier { Name = "Fine",       Value = 1.2f,  Damage = 1.05f, FireRate = 1.06f, Accuracy = 1.02f },
            new BowModifier { Name = "Masterwork", Value = 1.3f,  Damage = 1.1f,  FireRate = 1.06f, Accuracy = 1.03f },
        };

        private static readonly ItemModifier[] CrossbowModifiers =
        {
            new CrossbowModifier { Name = "Cracked",    Value = 0.3f,  Damage = 0.4f,   FireRate = 1f,    Accuracy = 0.82f },
            new CrossbowModifier { Name = "Chipped",    Value = 0.5f,  Damage = 0.6f,   FireRate = 1f,    Accuracy = 0.88f },
            new CrossbowModifier { Name = "Bent",       Value = 0.65f, Damage = 0.8f,   FireRate = 1f,    Accuracy = 0.94f },
            new CrossbowModifier { Name = "Well Made",  Value = 1.1f,  Damage = 1.03f,  FireRate = 1.01f, Accuracy = 1.01f },
            new CrossbowModifier { Name = "Exquisite",  Value = 1.2f,  Damage = 1.05f,  FireRate = 1.01f, Accuracy = 1.02f },
            new CrossbowModifier { Name = "Masterwork", Value = 1.3f,  Damage = 1.06f,  FireRate = 1.02f, Accuracy = 1.04f },
        };

        private static readonly ItemModifier[] WeaponModifiers =
        {
            new WeaponModifier { Name = "Destroyed",  Value = 0.3f,  Damage = 0.7f,  Speed = 1f },
            new WeaponModifier { Name = "Cracked",    Value = 0.5f,  Damage = 0.8f,  Speed = 1f },
            new WeaponModifier { Name = "Chipped",    Value = 0.65f, Damage = 0.9f,  Speed = 1f },
            new WeaponModifier { Name = "Tempered",   Value = 1.1f,  Damage = 1.01f, Speed = 1f },
            new WeaponModifier { Name = "Balanced",   Value = 1.2f,  Damage = 1.04f, Speed = 1.1f },
            new WeaponModifier { Name = "Masterwork", Value = 1.3f,  Damage = 1.08f, Speed = 1.1f },
        };

        private static readonly ItemModifier[] PolearmModifiers =
        {
            new WeaponModifier { Name = "Cracked",    Value = 0.3f,  Damage = 0.7f,  Speed = 1f },
            new WeaponModifier { Name = "Damaged",    Value = 0.5f,  Damage = 0.8f,  Speed = 1f },
            new WeaponModifier { Name = "Bent",       Value = 0.65f, Damage = 0.9f,  Speed = 1f },
            new WeaponModifier { Name = "Deadly",     Value = 1.1f,  Damage = 1.01f, Speed = 1.1f },
            new WeaponModifier { Name = "Balanced",   Value = 1.2f,  Damage = 1.04f, Speed = 1.1f },
            new WeaponModifier { Name = "Masterwork", Value = 1.3f,  Damage = 1.07f, Speed = 1.1f },
        };

        private static readonly ItemModifier[] ThrownModifiers =
        {
            new ThrownModifier { Name = "Destroyed",  Value = 0.3f,  Damage = 0.6f,  FireRate = 0.9f,  Accuracy = 0.91f },
            new ThrownModifier { Name = "Damaged",    Value = 0.5f,  Damage = 0.75f, FireRate = 0.95f, Accuracy = 0.94f },
            new ThrownModifier { Name = "Worn",       Value = 0.65f, Damage = 0.82f, FireRate = 0.99f, Accuracy = 0.97f },
            new ThrownModifier { Name = "Heavy",      Value = 1.1f,  Damage = 1.05f, FireRate = 1.01f, Accuracy = 1.02f },
            new ThrownModifier { Name = "Balanced",   Value = 1.2f,  Damage = 1.07f, FireRate = 1.01f, Accuracy = 1.03f },
            new ThrownModifier { Name = "Masterwork", Value = 1.3f,  Damage = 1.1f,  FireRate = 1.02f, Accuracy = 1.04f },
        };

        private static readonly ItemModifier[] MissileModifiers =
        {
            new MissileModifier { Name = "Cracked",      Value = 0.3f,  Weight = 1f,   Damage = 0.25f, StackAmount = 0.55f },
            new MissileModifier { Name = "Damaged",      Value = 0.5f,  Weight = 1f,   Damage = 0.5f,  StackAmount = 0.7f },
            new MissileModifier { Name = "Worn",         Value = 0.65f, Weight = 1f,   Damage = 0.75f, StackAmount = 0.85f },
            new MissileModifier { Name = "Light Bag of", Value = 1.1f,  Weight = 0.8f, Damage = 1f,    StackAmount = 1f },
            new MissileModifier { Name = "Sharp",        Value = 1.2f,  Weight = 0.8f, Damage = 1.1f,  StackAmount = 1f },
            new MissileModifier { Name = "Masterwork",   Value = 1.3f,  Weight = 0.7f, Damage = 1.2f,  StackAmount = 1.1f },
        };

        private static readonly Dictionary<ItemType, ItemModifier[]> ItemModifiers = new Dictionary<ItemType, ItemModifier[]>
        {
            [ItemType.HeadArmor] = ArmorModifiers,
            [ItemType.Cape] = ArmorModifiers,
            [ItemType.BodyArmor] = ArmorModifiers,
            [ItemType.HandArmor] = ArmorModifiers,
            [ItemType.LegArmor] = ArmorModifiers,
            [ItemType.HorseHarness] = ArmorModifiers,
            [ItemType.Horse] = HorseModifiers,
            [ItemType.Shield] = ShieldModifiers,
            [ItemType.Bow] = BowModifiers,
            [ItemType.Crossbow] = CrossbowModifiers,
            [ItemType.OneHandedWeapon] = WeaponModifiers,
            [ItemType.TwoHandedWeapon] = WeaponModifiers,
            [ItemType.Polearm] = PolearmModifiers,
            [ItemType.Thrown] = ThrownModifiers,
            [ItemType.Arrows] = MissileModifiers,
            [ItemType.Bolts] = MissileModifiers,
        };

        private readonly IMapper _mapper;

        public ItemModifierService(IMapper mapper)
        {
            _mapper = mapper;
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

            var clone = _mapper.Map<ItemCreation>(baseItem);

            Index idx = rank < 0 ? rank + 3 : rank + 2;
            ItemModifier modifier = ItemModifiers[baseItem.Type][idx];
            modifier.Apply(clone);
            return clone;
        }

        private abstract class ItemModifier
        {
            public string Name { get; set; } = default!;
            public float Value { get; set; }

            public virtual void Apply(ItemCreation item)
            {
                item.MbId = item.MbId.StartsWith("mp_", StringComparison.Ordinal)
                    ? "mp_" + Name.ToLower() + "_" + item.MbId.Substring(3)
                    : Name.ToLower() + "_" + item.MbId;
                item.Name = Name + " " + item.Name;
                item.Value = (int)Math.Round(item.Value * Value);
            }

            protected static int Scale(int val, float factor) => (int)Math.Round(val * factor);
            protected static float Scale(float val, float factor) => val * factor;
        }

        private class ArmorModifier : ItemModifier
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

        private class HorseModifier : ItemModifier
        {
            public float ChargeDamage { get; set; }
            public float Maneuver { get; set; }
            public float Speed { get; set; }
            public float HitPoints { get; set; }

            public override void Apply(ItemCreation item)
            {
                base.Apply(item);
                item.Horse!.ChargeDamage = Scale(item.Horse.ChargeDamage, ChargeDamage);
                item.Horse!.Maneuver = Scale(item.Horse.Maneuver, Maneuver);
                item.Horse!.Speed = Scale(item.Horse.Speed, Speed);
                item.Horse!.HitPoints = Scale(item.Horse.HitPoints, HitPoints);
            }
        }

        private class ShieldModifier : ItemModifier
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

        private class BowModifier : ItemModifier
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

        private class CrossbowModifier : BowModifier
        {
        }

        private class WeaponModifier : ItemModifier
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

        private class ThrownModifier : ItemModifier
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

        private class MissileModifier : ItemModifier
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
}
