using Crpg.Module.Helpers;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Crpg.Module.Common.Models;

internal class CrpgItemValueModel : ItemValueModel
{
    private static readonly float[] ItemPriceCoeffs = new float[] { 300f, 700f, 0f };
    private static readonly float[] ArmorPriceCoeffs = new float[] { 1f, 4f, 0f, 0f, 0f };
    private static readonly Dictionary<ItemObject.ItemTypeEnum, (int desiredMaxPrice, float[] priceCoeffs)> PricesAndCoeffs = new()
    {
        [ItemObject.ItemTypeEnum.HeadArmor] = (14500, ArmorPriceCoeffs),
        [ItemObject.ItemTypeEnum.Cape] = (10750, ArmorPriceCoeffs),
        [ItemObject.ItemTypeEnum.BodyArmor] = (34000, ArmorPriceCoeffs),
        [ItemObject.ItemTypeEnum.HandArmor] = (6750, ArmorPriceCoeffs),
        [ItemObject.ItemTypeEnum.LegArmor] = (5150, ArmorPriceCoeffs),
        [ItemObject.ItemTypeEnum.HorseHarness] = (27500, ItemPriceCoeffs),
        [ItemObject.ItemTypeEnum.Horse] = (15000, ItemPriceCoeffs),
        [ItemObject.ItemTypeEnum.Shield] = (9235, ItemPriceCoeffs),
        [ItemObject.ItemTypeEnum.Bow] = (14000, ItemPriceCoeffs),
        [ItemObject.ItemTypeEnum.Crossbow] = (18000, ItemPriceCoeffs),
        [ItemObject.ItemTypeEnum.OneHandedWeapon] = (7500, ItemPriceCoeffs),
        [ItemObject.ItemTypeEnum.TwoHandedWeapon] = (14000, ItemPriceCoeffs),
        [ItemObject.ItemTypeEnum.Polearm] = (14000, ItemPriceCoeffs),
        [ItemObject.ItemTypeEnum.Thrown] = (7385, ItemPriceCoeffs),
        [ItemObject.ItemTypeEnum.Arrows] = (4500, ItemPriceCoeffs),
        [ItemObject.ItemTypeEnum.Bolts] = (8200, ItemPriceCoeffs),
        [ItemObject.ItemTypeEnum.Banner] = (7000, ItemPriceCoeffs),
    };

    public override float CalculateTier(ItemObject item)
    {
        return item.ItemComponent switch
        {
            ArmorComponent armorComponent => CalculateArmorTier(armorComponent),
            HorseComponent horseComponent => CalculateHorseTier(horseComponent),
            BannerComponent bannerComponent => CalculateBannerTier(bannerComponent),
            WeaponComponent weaponComponent => CalculateWeaponTier(weaponComponent),
            _ => 0f,
        };
    }

    public override float GetEquipmentValueFromTier(float tier) // this method is never called
    {
        return 0;
    }

    public override int CalculateValue(ItemObject item)
    {
        return GetEquipmentValueFromTier(item.Tierf, PricesAndCoeffs[item.ItemType].desiredMaxPrice, PricesAndCoeffs[item.ItemType].priceCoeffs, 50);
    }

    private int GetEquipmentValueFromTier(float tier, int desiredMaxPrice, float[] priceCoeffs, int desiredTierZeroPrice)
    {
        // this method takes a value between 0 and 10 and outputs a value between 0 and 10
        // It uses a degree 2 polynomial.

        static float GetPriceTier(float tier, float[] coeffs)
        {
            float tierPolynome = MathHelper.ApplyPolynomialFunction(tier, coeffs);
            float tierPolynomeScaler = 10 / MathHelper.ApplyPolynomialFunction(10f, coeffs); // this part will make sure that GetAdjustedTier(10)=10
            return tierPolynome * tierPolynomeScaler;
        }

        return (int)(GetPriceTier(tier, priceCoeffs) * (desiredMaxPrice - desiredTierZeroPrice) / 10 + desiredTierZeroPrice);
    }

    private float CalculateArmorTier(ArmorComponent armorComponent)
    {
        float armorPower =
              1.2f * armorComponent.HeadArmor
            + 1.1f * armorComponent.BodyArmor
            + 1.15f * armorComponent.ArmArmor
            + 0.75f * armorComponent.LegArmor;
        float bestArmorPower = armorComponent.Item.ItemType switch
        {
            ItemObject.ItemTypeEnum.HeadArmor => 21.25f,
            ItemObject.ItemTypeEnum.Cape => 11.36f,
            ItemObject.ItemTypeEnum.BodyArmor => 19.492481f,
            ItemObject.ItemTypeEnum.HandArmor => 10.1992f,
            ItemObject.ItemTypeEnum.LegArmor => 6.94f,
            ItemObject.ItemTypeEnum.HorseHarness => 60f,
            _ => throw new ArgumentOutOfRangeException(),
        };

        return armorComponent.Item.ItemType switch
        {
            ItemObject.ItemTypeEnum.HorseHarness => 10 * armorPower / bestArmorPower,
            _ => 10 * armorPower / (bestArmorPower * (float)Math.Pow(armorComponent.Item.Weight + 8, 0.5f)),
        };
    }

    private float CalculateHorseTier(HorseComponent horseComponent)
    {
        float horsePower =
          (float)Math.Pow(horseComponent.Speed, 1.60f)
        * (float)Math.Pow(horseComponent.Maneuver, 1.65f)
        * (float)Math.Pow(horseComponent.HitPoints + horseComponent.HitPointBonus, 1.26f)
        + 300f * (float)Math.Pow(horseComponent.ChargeDamage, 5f) + 2500000f * horseComponent.ChargeDamage;
        float bestHorsePower = 487438929.1f;
        float horseTier = 10f * horsePower / bestHorsePower;
        return horseTier * horseTier / 12.2f;
    }

    private float CalculateBannerTier(BannerComponent bannerComponent)
    {
        return 10f;
    }

    private float CalculateWeaponTier(WeaponComponent weaponComponent)
    {
        bool isAThrowingWeapon = weaponComponent.Weapons.Max(a => a.MaxDataValue) >= 1;
        if (weaponComponent.Item?.WeaponDesign == null)
        {
            return CalculateTierNonCraftedWeapon(weaponComponent);
        }
        else
        {
            if (isAThrowingWeapon && CalculateThrownWeaponTier(weaponComponent) > CalculateTierMeleeWeapon(weaponComponent))
            {
                return CalculateThrownWeaponTier(weaponComponent);
            }
            else
            {
                return CalculateTierMeleeWeapon(weaponComponent);
            }
        }
    }

    private float CalculateTierMeleeWeapon(WeaponComponent weaponComponent)
    {
        float maxTier = float.MinValue;

        foreach (var weapon in weaponComponent.Weapons)
        {
            float weaponSwingScaler = weapon.WeaponClass switch
            {
                WeaponClass.OneHandedSword => 20.9f,
                WeaponClass.OneHandedAxe => 25.3f,
                WeaponClass.Mace => 25.3f,
                WeaponClass.Dagger => 27f,
                WeaponClass.TwoHandedSword => 27.5f,
                WeaponClass.TwoHandedMace => 28.5f,
                WeaponClass.TwoHandedAxe => 29.5f,
                WeaponClass.TwoHandedPolearm => 25f,
                WeaponClass.OneHandedPolearm => 25f,
                _ => float.MaxValue,
            };
            float weaponThrustScaler = weapon.WeaponClass switch
            {
                WeaponClass.OneHandedSword => 30.8f,
                WeaponClass.OneHandedAxe => 30.8f,
                WeaponClass.Mace => 30.8f,
                WeaponClass.Dagger => 34f,
                WeaponClass.TwoHandedSword => 30f,
                WeaponClass.TwoHandedMace => 30f,
                WeaponClass.TwoHandedAxe => 30f,
                WeaponClass.TwoHandedPolearm => 33f,
                WeaponClass.OneHandedPolearm => 33f,
                _ => float.MaxValue,
            };
            float thrustTier =
                  (float)Math.Pow(CrpgStrikeMagnitudeModel.BladeDamageFactorToDamageRatio * weapon.ThrustDamageFactor, 4.27f)
                * CalculateDamageTypeFactor(weapon.ThrustDamageType)
                * (float)Math.Pow(weapon.ThrustSpeed * 0.1f, 2f)
                / 62000f;
            float swingTier =
                  (float)Math.Pow(CrpgStrikeMagnitudeModel.BladeDamageFactorToDamageRatio * weapon.SwingDamageFactor, 2.15f)
                * CalculateDamageTypeFactor(weapon.SwingDamageType)
                * (float)Math.Pow(weapon.SwingSpeed, 4.4f)
                / 390000000f;

            if (!weapon.WeaponFlags.HasAnyFlag(WeaponFlags.NotUsableWithOneHand))
            {
                swingTier *= 1.1f;
            }

            if (weapon.WeaponFlags.HasAnyFlag(WeaponFlags.BonusAgainstShield))
            {
                swingTier *= 1.1f;
            }

            if (weapon.WeaponFlags.HasAnyFlag(WeaponFlags.CanCrushThrough))
            {
                swingTier *= 1.5f;
            }

            if (weapon.WeaponFlags.HasAnyFlag(WeaponFlags.CanKnockDown))
            {
                swingTier *= 1.6f;
            }

            if (weapon.WeaponFlags.HasAnyFlag(WeaponFlags.MultiplePenetration))
            {
                swingTier *= 1.10f;
                thrustTier *= 1.10f;
            }

            if (!weapon.ItemUsage.Contains("block")
                && !weapon.ItemUsage.Contains("pike")
                && !weapon.ItemUsage.Contains("axe")
                && !weapon.ItemUsage.Contains("couch")
                && !weapon.ItemUsage.Contains("bracing")
                && !weapon.ItemUsage.Contains("throwing"))
            {
                swingTier *= 0.7f;
                thrustTier *= 0.7f;
            }

            float swingLengthTier;
            float thrustLengthTier = 0.55f * (float)Math.Pow(0.4f + weapon.WeaponLength * 0.01f, 2f);
            switch (weapon.WeaponClass)
            {
                case WeaponClass.OneHandedSword:
                case WeaponClass.Dagger:
                case WeaponClass.OneHandedAxe:
                case WeaponClass.Mace:
                    swingLengthTier = 0.455f * (float)Math.Pow(0.8f + weapon.WeaponLength * 0.01f, 2f);
                    break;
                case WeaponClass.TwoHandedSword:
                case WeaponClass.TwoHandedMace:
                case WeaponClass.TwoHandedAxe:
                    swingLengthTier = 0.55f * (float)Math.Pow(0.4f + weapon.WeaponLength * 0.01f, 2f);
                    break;
                case WeaponClass.OneHandedPolearm:
                case WeaponClass.TwoHandedPolearm:
                    swingLengthTier = 0.82f * (float)Math.Pow(0.8f + weapon.WeaponLength * 0.00409f, 2f);
                    break;
                case WeaponClass.LowGripPolearm:
                case WeaponClass.Pick:
                    swingLengthTier = 0.4f * (float)Math.Pow(0.8f + weapon.WeaponLength * 0.01f, 2f);
                    break;
                case WeaponClass.ThrowingAxe:
                case WeaponClass.Javelin:
                case WeaponClass.ThrowingKnife:
                    swingLengthTier = 0f;
                    thrustLengthTier = 0f;
                    break;
                default:
                    throw new Exception(weapon.WeaponClass.ToString() + " has no swingTierAssociated");
            }

            float swinghandlingFactor = weapon.Handling / 10000f;
            float thrustHandlingFactor = weapon.Handling / 10000f;
            swingTier =
                  12f
                * (swingTier * swingLengthTier)
                * (float)Math.Pow(swinghandlingFactor, 1f)
                / weaponSwingScaler;
            thrustTier =
                        5f
                        * (thrustTier * thrustLengthTier)
                        * (float)Math.Pow(thrustHandlingFactor, 1f)
                        / weaponThrustScaler;
            float tier = 0.8f * Math.Max(swingTier, thrustTier) + 0.2f * Math.Min(swingTier, thrustTier);
            if (tier >= maxTier)
            {
                maxTier = tier;
            }
        }

        return maxTier * maxTier / 10f; // makes weapon of lower Tier Better
    }

    private float CalculateDamageTypeFactor(DamageTypes damageType)
    {
        return damageType switch
        {
            DamageTypes.Blunt => 3f,
            DamageTypes.Pierce => 2.2f,
            _ => 1.0f,
        };
    }

    private float CalculateDamageTypeFactorForAmmo(DamageTypes damageType)
    {
        return damageType switch
        {
            DamageTypes.Blunt => 2f,
            DamageTypes.Pierce => 1.75f,
            _ => 1.0f,
        };
    }

    private float CalculateDamageTypeFactorForThrown(DamageTypes damageType)
    {
        return damageType switch
        {
            DamageTypes.Blunt => 2.4f,
            DamageTypes.Pierce => 1.75f,
            _ => 1.25f,
        };
    }

    private float CalculateTierNonCraftedWeapon(WeaponComponent weaponComponent)
    {
        ItemObject.ItemTypeEnum itemType = weaponComponent.Item?.ItemType ?? ItemObject.ItemTypeEnum.Invalid;
        switch (itemType)
        {
            case ItemObject.ItemTypeEnum.Crossbow:
            case ItemObject.ItemTypeEnum.Bow:
            case ItemObject.ItemTypeEnum.Musket:
            case ItemObject.ItemTypeEnum.Pistol:
                return CalculateRangedWeaponTier(weaponComponent);
            case ItemObject.ItemTypeEnum.Arrows:
            case ItemObject.ItemTypeEnum.Bolts:
            case ItemObject.ItemTypeEnum.Bullets:
                return CalculateAmmoTier(weaponComponent);
            case ItemObject.ItemTypeEnum.Shield:
                return CalculateShieldTier(weaponComponent);
            case ItemObject.ItemTypeEnum.Thrown:
                return CalculateThrownWeaponTier(weaponComponent);
            default:
                return 0f;
        }
    }

    private float CalculateThrownWeaponTier(WeaponComponent weaponComponent)
    {
        WeaponComponentData weapon = weaponComponent.Weapons.MaxBy(a => a.MaxDataValue);
        float scaler = 1600000f;
        float bonusVsShield = weapon.WeaponFlags.HasFlag(WeaponFlags.BonusAgainstShield) ? 1.15f : 1f;
        float tier = weapon.ThrustDamage
            * weapon.ThrustDamage
            * weapon.MissileSpeed
            * weapon.Accuracy
            * weapon.MaxDataValue
            * CalculateDamageTypeFactorForThrown(weapon.ThrustDamageType == DamageTypes.Invalid ? weapon.SwingDamageType : weapon.ThrustDamageType)
            * bonusVsShield
            / scaler;
        return tier * tier / 10f;
    }

    private float CalculateRangedWeaponTier(WeaponComponent weaponComponent)
    {
        WeaponComponentData weapon = weaponComponent.Weapons[0];
        float scaler = 1.560284f;

        if (weaponComponent.Item is { ItemType: ItemObject.ItemTypeEnum.Crossbow })
        {
            float crossbowscaler = 2.479958463230f;
            float crossbowTier = weapon.ThrustDamage / 100f * weapon.ThrustDamage / 100f
                * weapon.SwingSpeed / 100f
                * weapon.MissileSpeed / 10f
                * weapon.Accuracy / 10f
                * (float)Math.Pow(weapon.ThrustSpeed, 0.5f) / 10f
                * (weapon.ItemUsage == "crossbow_light" ? 2f : 1f)
                / crossbowscaler;
            return crossbowTier * crossbowTier / 10f;
        }

        float bowTier = weapon.ThrustDamage / 100f * weapon.ThrustDamage / 100f
            * weapon.SwingSpeed / 100f
            * weapon.MissileSpeed / 10f
            * weapon.Accuracy / 10f
            * weapon.ThrustSpeed / 10f
            * (weapon.ItemUsage == "long_bow" ? 0.668f : 1f)
            / scaler;
        return bowTier * bowTier / 10f;
    }

    private float CalculateShieldTier(WeaponComponent weaponComponent)
    {
        WeaponComponentData weapon = weaponComponent.Weapons[0];
        float shieldTier = (1.0f * weapon.MaxDataValue
                * (1.0f + weapon.BodyArmor / 10f)
                + 1.0f * weapon.ThrustSpeed)
                * (0.14f + weapon.GetRealWeaponLength())
                / (6f + weaponComponent.Item.Weight)
                / 25f
                * 10f;
        return shieldTier * shieldTier / 10f;
    }

    private float CalculateAmmoTier(WeaponComponent weaponComponent)
    {
        WeaponComponentData weapon = weaponComponent.Weapons[0];
        float arrowsTier = 10f
          * CalculateDamageTypeFactorForAmmo(weapon.ThrustDamageType) * CalculateDamageTypeFactorForAmmo(weapon.ThrustDamageType)
          * (10 + weapon.MissileDamage) * (10 + weapon.MissileDamage)
          * (float)Math.Pow(weapon.MaxDataValue, 0.3f)
          / 1537.6f;
        float boltsTier = 10f
          * CalculateDamageTypeFactorForAmmo(weapon.ThrustDamageType) * CalculateDamageTypeFactorForAmmo(weapon.ThrustDamageType)
          * (22 + weapon.MissileDamage) * (22 + weapon.MissileDamage)
          * (float)Math.Pow(weapon.MaxDataValue, 0.3f)
          / 6606.34f;
        return weaponComponent.Item.ItemType switch
        {
            ItemObject.ItemTypeEnum.Arrows => arrowsTier * arrowsTier / 10f,

            ItemObject.ItemTypeEnum.Bolts => boltsTier * boltsTier / 10f,
            _ => 10f,
        };
    }
}
