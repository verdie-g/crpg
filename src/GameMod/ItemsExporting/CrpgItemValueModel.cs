using System;
using Crpg.GameMod.Api.Models.Items;

namespace Crpg.GameMod.ItemsExporting
{
    internal static class CrpgItemValueModel
    {
        public static int CalculateValue(CrpgItemCreation item)
        {
            float tier = CalculateTier(item);
            return (int)GetEquipmentValueFromTier(tier);
        }

        private static float CalculateTier(CrpgItemCreation item)
        {
            if (item.Armor != null)
            {
                return CalculateArmorTier(item.Armor, item.Type);
            }

            if (item.Mount != null)
            {
                return CalculateMountTier(item.Mount);
            }

            if (item.Weapons.Count != 0)
            {
                // TODO: what to do for weapon with several components?
                return CalculateWeaponTier(item.Weapons[0], item.Type, item.Weight);
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

        private static float CalculateArmorTier(CrpgItemArmorComponent armorComponent, CrpgItemType itemType)
        {
            float factor = 1.2f * armorComponent.HeadArmor
                           + 1.0f * armorComponent.BodyArmor
                           + 1.0f * armorComponent.ArmArmor
                           + 1.0f * armorComponent.LegArmor;
            factor *= itemType switch
            {
                CrpgItemType.HeadArmor => 1.2f,
                CrpgItemType.ShoulderArmor => 1.8f,
                CrpgItemType.BodyArmor => 1f,
                CrpgItemType.HandArmor => 1.7f,
                CrpgItemType.LegArmor => 1.6f,
                CrpgItemType.MountHarness => 1f,
                _ => throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null)
            };

            return 0.1f * factor;
        }

        private static float CalculateMountTier(CrpgItemMountComponent mountComponent)
        {
            return (0.6f * mountComponent.Maneuver
                   + 1.0f * mountComponent.Speed
                   + 1.5f * mountComponent.ChargeDamage
                   + 0.1f * mountComponent.HitPoints) * 0.07f;
        }

        private static float CalculateWeaponTier(CrpgItemWeaponComponent weaponComponent, CrpgItemType itemType, float weight)
        {
            switch (itemType)
            {
                case CrpgItemType.Shield:
                    return CalculateShieldTier(weaponComponent, weight);
                case CrpgItemType.Bow:
                case CrpgItemType.Crossbow:
                case CrpgItemType.Pistol:
                case CrpgItemType.Musket:
                    return CalculateRangedWeaponTier(weaponComponent, itemType);
                case CrpgItemType.OneHandedWeapon:
                case CrpgItemType.TwoHandedWeapon:
                case CrpgItemType.Polearm:
                case CrpgItemType.Thrown:
                    return CalculateTierMeleeWeapon(weaponComponent);
                case CrpgItemType.Arrows:
                case CrpgItemType.Bolts:
                case CrpgItemType.Bullets:
                    return CalculateAmmoTier(weaponComponent);
                default:
                    return 0f;
            }
        }

        private static float CalculateShieldTier(CrpgItemWeaponComponent weaponComponent, float weight)
        {
            return (1.0f * weaponComponent.StackAmount
                    + 3.0f * weaponComponent.BodyArmor
                    + 1.0f * weaponComponent.ThrustSpeed) / (6f + weight) * 0.13f;
        }

        private static float CalculateRangedWeaponTier(CrpgItemWeaponComponent weaponComponent, CrpgItemType itemType)
        {
            float rangeTypeFactor = itemType switch
            {
                CrpgItemType.Musket => 0.5f,
                CrpgItemType.Crossbow => 0.7f,
                _ => 1f
            };

            return rangeTypeFactor * 0.00001f * weaponComponent.ThrustDamage * weaponComponent.ThrustSpeed * weaponComponent.Accuracy;
        }

        private static float CalculateTierMeleeWeapon(CrpgItemWeaponComponent weaponComponent)
        {
            float tier = Math.Max(
                weaponComponent.ThrustDamage * GetDamageTypeFactor(weaponComponent.ThrustDamageType) * (float)Math.Pow(weaponComponent.ThrustSpeed * 0.01f, 1.5f),
                weaponComponent.SwingDamage * GetDamageTypeFactor(weaponComponent.SwingDamageType) * (float)Math.Pow(weaponComponent.SwingSpeed * 0.01f, 1.5f) * 1.1f);
            if (weaponComponent.Flags.HasFlag(CrpgWeaponFlags.NotUsableWithOneHand))
            {
                tier *= 0.8f;
            }

            if (weaponComponent.Class == CrpgWeaponClass.ThrowingKnife || weaponComponent.Class == CrpgWeaponClass.Javelin)
            {
                tier *= 1.2f;
            }

            return 0.06f * tier * (1f + 0.01f * weaponComponent.Length);
        }

        private static float GetDamageTypeFactor(CrpgDamageType damageType) => damageType switch
        {
            CrpgDamageType.Blunt => 1.3f,
            CrpgDamageType.Pierce => 1.15f,
            _ => 1f
        };

        private static float CalculateAmmoTier(CrpgItemWeaponComponent weaponComponent)
        {
            return 1 * weaponComponent.ThrustDamage
                   + 0.1f * weaponComponent.StackAmount;
        }
    }
}
