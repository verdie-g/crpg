using System;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;

namespace Crpg.Application.Items.Models
{
    public class ItemWeaponComponentViewModel : IMapFrom<ItemWeaponComponent>, ICloneable
    {
        public WeaponClass Class { get; set; }
        public int Accuracy { get; set; }
        public int MissileSpeed { get; set; }
        public int StackAmount { get; set; }
        public int Length { get; set; }
        public float Balance { get; set; }
        public int Handling { get; set; }
        public int BodyArmor { get; set; }
        public WeaponFlags Flags { get; set; }

        public int ThrustDamage { get; set; }
        public DamageType ThrustDamageType { get; set; }
        public int ThrustSpeed { get; set; }

        public int SwingDamage { get; set; }
        public DamageType SwingDamageType { get; set; }
        public int SwingSpeed { get; set; }

        public object Clone() => new ItemWeaponComponentViewModel
        {
            Class = Class,
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
            ThrustSpeed = ThrustDamage,
            SwingDamage = SwingDamage,
            SwingDamageType = SwingDamageType,
            SwingSpeed = SwingDamage,
        };
    }
}
