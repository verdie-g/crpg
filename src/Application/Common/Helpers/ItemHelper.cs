using Crpg.Application.Items.Models;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Common.Helpers
{
    internal static class ItemHelper
    {
        public static Item ToItem(ItemCreation item)
        {
            var res = new Item
            {
                MbId = item.MbId,
                Name = item.Name,
                Type = item.Type,
                Value = item.Value,
                Weight = item.Weight,
                Rank = 0,
            };

            if (item.Armor != null)
            {
                res.Armor = new ItemArmorComponent
                {
                    HeadArmor = item.Armor!.HeadArmor,
                    BodyArmor = item.Armor!.BodyArmor,
                    ArmArmor = item.Armor!.ArmArmor,
                    LegArmor = item.Armor!.LegArmor,
                };
            }

            if (item.Mount != null)
            {
                res.Mount = new ItemMountComponent
                {
                    BodyLength = item.Mount!.BodyLength,
                    ChargeDamage = item.Mount!.ChargeDamage,
                    Maneuver = item.Mount!.Maneuver,
                    Speed = item.Mount!.Speed,
                    HitPoints = item.Mount!.HitPoints,
                };
            }

            if (item.Weapons.Count > 0)
            {
                res.PrimaryWeapon = ToItemWeaponComponent(item.Weapons[0]);
            }

            if (item.Weapons.Count > 1)
            {
                res.SecondaryWeapon = ToItemWeaponComponent(item.Weapons[1]);
            }

            if (item.Weapons.Count > 2)
            {
                res.TertiaryWeapon = ToItemWeaponComponent(item.Weapons[2]);
            }

            return res;
        }

        private static ItemWeaponComponent ToItemWeaponComponent(ItemWeaponComponentViewModel weaponComponent)
        {
            return new ItemWeaponComponent
            {
                Class = weaponComponent.Class,
                Accuracy = weaponComponent.Accuracy,
                MissileSpeed = weaponComponent.MissileSpeed,
                StackAmount = weaponComponent.StackAmount,
                Length = weaponComponent.Length,
                Balance = weaponComponent.Balance,
                Handling = weaponComponent.Handling,
                BodyArmor = weaponComponent.BodyArmor,
                Flags = weaponComponent.Flags,
                ThrustDamage = weaponComponent.ThrustDamage,
                ThrustDamageType = weaponComponent.ThrustDamageType,
                ThrustSpeed = weaponComponent.ThrustSpeed,
                SwingDamage = weaponComponent.SwingDamage,
                SwingDamageType = weaponComponent.SwingDamageType,
                SwingSpeed = weaponComponent.SwingSpeed,
            };
        }
    }
}
