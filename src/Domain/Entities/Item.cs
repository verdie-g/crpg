using System;
using System.Collections.Generic;
using Crpg.Domain.Common;

namespace Crpg.Domain.Entities
{
    public class Item : AuditableEntity
    {
        public int Id { get; set; }

        /// <summary>
        /// Item id in Mount and Blade.
        /// </summary>
        public string MbId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 256x120
        /// </summary>
        public Uri Image { get; set; } = default!;
        public ItemType Type { get; set; }
        public int Value { get; set; }
        public float Weight { get; set; }
        // TODO: Looming

        // Armor
        public int? HeadArmor { get; set; }
        public int? BodyArmor { get; set; }
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
        public WeaponFlags? PrimaryWeaponFlags { get; set; }

        public int? SecondaryThrustDamage { get; set; }
        public int? SecondaryThrustSpeed { get; set; }
        public int? SecondarySwingDamage { get; set; }
        public int? SecondarySwingSpeed { get; set; }
        public WeaponFlags? SecondaryWeaponFlags { get; set; }

        public List<UserItem> UserItems { get; set; } = new List<UserItem>();
    }
}