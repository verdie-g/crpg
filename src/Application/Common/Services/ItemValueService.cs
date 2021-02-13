using System;
using Crpg.Domain.Entities.Items;

namespace Crpg.Application.Common.Services
{
    /// <summary>
    /// Service to compute the value of an <see cref="Item"/>.
    /// </summary>
    internal class ItemValueService
    {
        public int ComputeItemValue(Item item)
        {
            float tier = CalculateTier(item);
            return (int)GetEquipmentValueFromTier(tier);
        }

        private static float CalculateTier(Item item)
        {
            if (item.Armor != null)
            {
                return CalculateArmorTier(item.Armor, item.Type);
            }

            if (item.Mount != null)
            {
                return CalculateMountTier(item.Mount);
            }

            if (item.PrimaryWeapon != null)
            {
                // TODO: what to do for weapon with several components?
                return CalculateWeaponTier(item.PrimaryWeapon, item.Type, item.Weight);
            }

            return 0f;
        }

        private static float GetEquipmentValueFromTier(float tier)
        {
            const float a = 300;
            const float b = 60;
            const float c = 50;
            return (float)(a * Math.Pow(tier, 2) + b * tier + c);
        }

        private static float CalculateArmorTier(ItemArmorComponent armorComponent, ItemType itemType)
        {
            float factor = 1.2f * armorComponent.HeadArmor
                           + 1.0f * armorComponent.BodyArmor
                           + 1.0f * armorComponent.ArmArmor
                           + 1.0f * armorComponent.LegArmor;
            factor *= itemType switch
            {
                ItemType.HeadArmor => 1.2f,
                ItemType.ShoulderArmor => 1.8f,
                ItemType.BodyArmor => 1f,
                ItemType.HandArmor => 1.7f,
                ItemType.LegArmor => 1.6f,
                ItemType.MountHarness => 1f,
                _ => throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null)
            };

            return 0.1f * factor;
        }

        private static float CalculateMountTier(ItemMountComponent mountComponent)
        {
            return (0.6f * mountComponent.Maneuver
                   + 1.0f * mountComponent.Speed
                   + 1.5f * mountComponent.ChargeDamage
                   + 0.1f * mountComponent.HitPoints) * 0.06f;
        }

        private static float CalculateWeaponTier(ItemWeaponComponent weaponComponent, ItemType itemType, float weight)
        {
            switch (itemType)
            {
                case ItemType.Shield:
                    return CalculateShieldTier(weaponComponent, weight);
                case ItemType.Bow:
                case ItemType.Crossbow:
                case ItemType.Pistol:
                case ItemType.Musket:
                    return CalculateRangedWeaponTier(weaponComponent, itemType);
                case ItemType.OneHandedWeapon:
                case ItemType.TwoHandedWeapon:
                case ItemType.Polearm:
                case ItemType.Thrown:
                    return CalculateTierMeleeWeapon(weaponComponent);
                case ItemType.Arrows:
                case ItemType.Bolts:
                case ItemType.Bullets:
                    return CalculateAmmoTier(weaponComponent);
                default:
                    return 0f;
            }
        }

        private static float CalculateShieldTier(ItemWeaponComponent weaponComponent, float weight)
        {
            return (1.0f * weaponComponent.StackAmount
                    + 3.0f * weaponComponent.BodyArmor
                    + 1.0f * weaponComponent.ThrustSpeed) / (6f + weight) * 0.09f;
        }

        private static float CalculateRangedWeaponTier(ItemWeaponComponent weaponComponent, ItemType itemType)
        {
            float rangeTypeFactor = itemType switch
            {
                ItemType.Musket => 0.5f,
                ItemType.Crossbow => 0.7f,
                _ => 1f
            };

            return rangeTypeFactor * 0.00001f * weaponComponent.ThrustDamage * weaponComponent.ThrustSpeed * weaponComponent.Accuracy;
        }

        private static float CalculateTierMeleeWeapon(ItemWeaponComponent weaponComponent)
        {
            float tier = Math.Max(
                weaponComponent.ThrustDamage * GetDamageTypeFactor(weaponComponent.ThrustDamageType) * (float)Math.Pow(weaponComponent.ThrustSpeed * 0.01f, 1.5f),
                weaponComponent.SwingDamage * GetDamageTypeFactor(weaponComponent.SwingDamageType) * (float)Math.Pow(weaponComponent.SwingSpeed * 0.01f, 1.5f) * 1.1f);
            if (weaponComponent.Flags.HasFlag(WeaponFlags.NotUsableWithOneHand))
            {
                tier *= 0.8f;
            }

            if (weaponComponent.Class == WeaponClass.ThrowingKnife || weaponComponent.Class == WeaponClass.Javelin)
            {
                tier *= 1.2f;
            }

            return 0.06f * tier * (1f + 0.01f * weaponComponent.Length);
        }

        private static float GetDamageTypeFactor(DamageType damageType) => damageType switch
        {
            DamageType.Blunt => 1.3f,
            DamageType.Pierce => 1.15f,
            _ => 1f
        };

        private static float CalculateAmmoTier(ItemWeaponComponent weaponComponent)
        {
            return 1 * weaponComponent.ThrustDamage
                   + 0.1f * weaponComponent.StackAmount;
        }
    }
}
