using System;
using Crpg.Domain.Entities;

namespace Crpg.Application.Items.Models
{
    public class ItemCreation
    {
        public string MbId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
        public ItemType Type { get; set; }
        public float Weight { get; set; }

        // Armor
        public int? HeadArmor { get; set; }
        public int? BodyArmor { get; set; } // not null for shields
        public int? ArmArmor { get; set; }
        public int? LegArmor { get; set; }

        // Horse
        public int? BodyLength { get; set; }
        public int? ChargeDamage { get; set; }
        public int? Maneuver { get; set; }
        public int? Speed { get; set; }
        public int? HitPoints { get; set; }

        // Weapon
        public DamageType? ThrustDamageType { get; set; }
        public DamageType? SwingDamageType { get; set; }
        public int? Accuracy { get; set; }
        public int? MissileSpeed { get; set; }
        public int? StackAmount { get; set; }
        public int? WeaponLength { get; set; }

        public int? PrimaryThrustDamage { get; set; }
        public int? PrimaryThrustSpeed { get; set; }
        public int? PrimarySwingDamage { get; set; }
        public int? PrimarySwingSpeed { get; set; }
        public int? PrimaryHandling { get; set; }
        public WeaponFlags? PrimaryWeaponFlags { get; set; }

        public int? SecondaryThrustDamage { get; set; }
        public int? SecondaryThrustSpeed { get; set; }
        public int? SecondarySwingDamage { get; set; }
        public int? SecondarySwingSpeed { get; set; }
        public int? SecondaryHandling { get; set; }
        public WeaponFlags? SecondaryWeaponFlags { get; set; }
    }
}