using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Crpg.Module.Common.Models;

internal class CrpgItemValueModel : ItemValueModel
{
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
        int desiredHeadArmorMaxPrice = 9754;
        int desiredCapeArmorMaxPrice = 11441;
        int desiredBodyArmorMaxPrice = 31632;
        int desiredHandArmorMaxPrice = 6000;
        int desiredLegArmorMaxPrice = 4662;
        int desiredHorseHarnessMaxPrice = 26000;
        int desiredHorseMaxPrice = 18000;
        int desiredShieldMaxPrice = 9235;
        int desiredBowMaxPrice = 12264;
        int desiredCrossbowMaxPrice = 18000;
        int desiredOneHandedWeaponMaxPrice = 7500;
        int desiredTwoHandedWeaponMaxPrice = 14000;
        int desiredPolearmMaxPrice = 20000;
        int desiredThrownMaxPrice = 7385;
        int desiredArrowsMaxPrice = 4500;
        int desiredBoltsMaxPrice = 8200;
        int desiredBannerMaxPrice = 50;

        return item.ItemType switch
        {
            ItemObject.ItemTypeEnum.HeadArmor => GetEquipmentValueFromTier(item.Tierf, desiredHeadArmorMaxPrice, 50),
            ItemObject.ItemTypeEnum.Cape => GetEquipmentValueFromTier(item.Tierf, desiredCapeArmorMaxPrice, 50),
            ItemObject.ItemTypeEnum.BodyArmor => GetEquipmentValueFromTier(item.Tierf, desiredBodyArmorMaxPrice, 50),
            ItemObject.ItemTypeEnum.HandArmor => GetEquipmentValueFromTier(item.Tierf, desiredHandArmorMaxPrice, 50),
            ItemObject.ItemTypeEnum.LegArmor => GetEquipmentValueFromTier(item.Tierf, desiredLegArmorMaxPrice, 50),
            ItemObject.ItemTypeEnum.HorseHarness => GetEquipmentValueFromTier(item.Tierf, desiredHorseHarnessMaxPrice, 50),
            ItemObject.ItemTypeEnum.Shield => GetEquipmentValueFromTier(item.Tierf, desiredShieldMaxPrice, 50),
            ItemObject.ItemTypeEnum.Bow => GetEquipmentValueFromTier(item.Tierf, desiredBowMaxPrice, 50),
            ItemObject.ItemTypeEnum.Crossbow =>GetEquipmentValueFromTier(item.Tierf, desiredCrossbowMaxPrice, 50),
            ItemObject.ItemTypeEnum.OneHandedWeapon => GetEquipmentValueFromTier(item.Tierf, desiredOneHandedWeaponMaxPrice, 50),
            ItemObject.ItemTypeEnum.TwoHandedWeapon => GetEquipmentValueFromTier(item.Tierf, desiredTwoHandedWeaponMaxPrice, 50),
            ItemObject.ItemTypeEnum.Polearm => GetEquipmentValueFromTier(item.Tierf, desiredPolearmMaxPrice, 50),
            ItemObject.ItemTypeEnum.Thrown => GetEquipmentValueFromTier(item.Tierf, desiredThrownMaxPrice, 50),
            ItemObject.ItemTypeEnum.Arrows => GetEquipmentValueFromTier(item.Tierf, desiredArrowsMaxPrice, 50),
            ItemObject.ItemTypeEnum.Bolts => GetEquipmentValueFromTier(item.Tierf, desiredBoltsMaxPrice, 50),
            ItemObject.ItemTypeEnum.Banner => GetEquipmentValueFromTier(item.Tierf, desiredBannerMaxPrice, 50),
            ItemObject.ItemTypeEnum.Horse => GetEquipmentValueFromTier(item.Tierf, desiredHorseMaxPrice, 50),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    private int GetEquipmentValueFromTier(float tier, int desiredMaxPrice, int desiredTierZeroPrice)
    {
        // this method takes a value between 0 and 10 and outputs a value between 0 and 10
        // It uses a degree 2 polynomial.
        // b is responsible for the linear part.
        // a is responsible for the quadratic part. a Linear fonction is not enough because it doesn't reflect how the best items are more
        // than just linearly better.
        static float GetAdjustedTier(float tier)
        {
            const float a = 300;
            const float b = 700;
            const float c = 0;
            float tierPolynome = (float)(a * Math.Pow(tier, 2) + b * tier + c);
            float tierPolynomeScaler = 10 / ((float)(a * Math.Pow(10, 2) + b * 10 + c)); // this part will make sure that GetAdjustedTier(10)=10
            return tierPolynome * tierPolynomeScaler;
        }

        return (int)(GetAdjustedTier(tier) * (desiredMaxPrice - desiredTierZeroPrice) / 10 + desiredTierZeroPrice);
    }

    private float CalculateArmorTier(ArmorComponent armorComponent)
    {
        float armorPower =
              1.2f * armorComponent.HeadArmor
            + 1.0f * armorComponent.BodyArmor
            + 1.0f * armorComponent.ArmArmor
            + 0.8f * armorComponent.LegArmor;
        float bestArmorPower = armorComponent.Item.ItemType switch
        {
            ItemObject.ItemTypeEnum.HeadArmor => 50.7206461f,
            ItemObject.ItemTypeEnum.Cape => 26.6184727f,
            ItemObject.ItemTypeEnum.BodyArmor => 48.2288f,
            ItemObject.ItemTypeEnum.HandArmor => 23.0526978f,
            ItemObject.ItemTypeEnum.LegArmor => 17.1818002f,
            ItemObject.ItemTypeEnum.HorseHarness => 20f,
            _ => throw new ArgumentOutOfRangeException(),
        };

        return armorComponent.Item.ItemType switch
        {
            ItemObject.ItemTypeEnum.HorseHarness => 10 * armorPower / bestArmorPower,
            _ => 10 * armorPower / (bestArmorPower * (float)Math.Pow(armorComponent.Item.Weight, 0.2f)),
        };
    }

    private float CalculateHorseTier(HorseComponent horseComponent)
    {
            float horsePower =
              1.5f * horseComponent.ChargeDamage
            + 1.0f * horseComponent.Speed
            + 0.6f * horseComponent.Maneuver
            + 0.1f * horseComponent.HitPoints;
            float bestHorsePower = 125.5f;
            return 10f * horsePower / bestHorsePower;
    }

    private float CalculateBannerTier(BannerComponent bannerComponent)
    {
        // return GetBaseTierValueForBannerEffect(bannerComponent.BannerEffect)
        // *bannerComponent.BannerLevel;
        return 10f;
    }

    private float GetBaseTierValueForBannerEffect(BannerComponent.BannerItemEffects bannerEffect)
    {
        return bannerEffect switch
        {
            BannerComponent.BannerItemEffects.IncreasedDamageAgainstMountedTroops => 1f,
            BannerComponent.BannerItemEffects.IncreasedRangedDamage => 1f,
            BannerComponent.BannerItemEffects.IncreasedRangedWeaponAccuracy => 1f,
            BannerComponent.BannerItemEffects.IncreasedChargeDamage => 1f,
            BannerComponent.BannerItemEffects.DecreasedMoraleShock => 1f,
            BannerComponent.BannerItemEffects.DecreasedMeleeAttackDamage => 1f,
            BannerComponent.BannerItemEffects.DecreasedRangedAttackDamage => 1f,
            BannerComponent.BannerItemEffects.DecreasedShieldDamage => 1f,
            BannerComponent.BannerItemEffects.IncreasedTroopMovementSpeed => 1f,
            BannerComponent.BannerItemEffects.IncreasedMountMovementSpeed => 1f,
            _ => 1f,
        };
    }

    private float CalculateWeaponTier(WeaponComponent weaponComponent)
    {
        bool isAThrowingWeapon = weaponComponent.Weapons.Max(a => a.MaxDataValue) >= 1;

        return weaponComponent.Item?.WeaponDesign == null
            ? CalculateTierNonCraftedWeapon(weaponComponent)
            :
                isAThrowingWeapon
                    ? CalculateThrownWeaponTier(weaponComponent)
                    : CalculateTierMeleeWeapon(weaponComponent);
    }

    private float CalculateTierMeleeWeapon(WeaponComponent weaponComponent)
    {
        float weaponScaler = weaponComponent.Item.ItemType switch
        {
            ItemObject.ItemTypeEnum.OneHandedWeapon => 58.854553f,
            ItemObject.ItemTypeEnum.TwoHandedWeapon => 116.82765f,
            ItemObject.ItemTypeEnum.Polearm => 59.05128f,
            _ => 1f,
        };
        float maxTier = float.MinValue;

        foreach (var weapon in weaponComponent.Weapons)
        {
            float thrustTier =
                  (float)Math.Pow(weapon.ThrustDamage, 2.25f)
                * CalculateDamageTypeFactor(weapon.ThrustDamageType)
                * (float)Math.Pow(weapon.ThrustSpeed * 0.01f, 3f)
                * 1f;
            float swingTier =
                  (float)Math.Pow(weapon.SwingDamage, 2.25f)
                * CalculateDamageTypeFactor(weapon.SwingDamageType)
                * (float)Math.Pow(weapon.SwingSpeed * 0.01f, 3f)
                * 1.1f;
            float tier = Math.Max(thrustTier, swingTier);

            if (weapon.WeaponFlags.HasAnyFlag(WeaponFlags.NotUsableWithOneHand) | weapon.WeaponFlags.HasAnyFlag(WeaponFlags.NotUsableWithTwoHand))
            {
                tier *= 0.95f;
            }

            if (weapon.WeaponFlags.HasAnyFlag(WeaponFlags.BonusAgainstShield))
            {
                tier *= 1.1f;
            }

            if (weapon.WeaponFlags.HasAnyFlag(WeaponFlags.CanCrushThrough))
            {
                tier *= 1.2f;
            }

            if (weapon.ThrustDamage > 0)
            {
                tier *= 1.2f;
            }

            float lengthTier = weapon.WeaponLength * 0.01f;
            float handlingFactor = weapon.Handling / 100f;
            tier =
                  0.06f
                * (tier * (float)Math.Pow(1f + lengthTier, 1.75f))
                * (float)Math.Pow(handlingFactor, 3f);

            if (tier >= maxTier)
            {
                maxTier = tier;
            }
        }

        return maxTier / weaponScaler;
    }

    private float CalculateDamageTypeFactor(DamageTypes damageType)
    {
        return damageType switch
        {
            DamageTypes.Blunt => 1.8f,
            DamageTypes.Pierce => 1f,
            _ => 1.15f,
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
            default:
                return 0f;
        }
    }

    private float CalculateThrownWeaponTier(WeaponComponent weaponComponent)
    {
        WeaponComponentData weapon = weaponComponent.Weapons.MaxBy(a => a.MaxDataValue);
        float scaler = 125416166.4f;
        return
              weapon.ThrustDamage
            * weapon.ThrustDamage
            * weapon.SwingSpeed
            * weapon.MissileSpeed
            * weapon.Accuracy
            * weapon.MaxDataValue
            * CalculateDamageTypeFactor(weapon.ThrustDamageType)
            / scaler;
    }

    private float CalculateRangedWeaponTier(WeaponComponent weaponComponent)
    {
        WeaponComponentData weapon = weaponComponent.Weapons[0];
        float scaler = 0.27364886f;

        if (weaponComponent.Item is { ItemType: ItemObject.ItemTypeEnum.Crossbow })
        {
            scaler = 3.8178947156399532384f;
        }

        return
              weapon.ThrustDamage / 100f
            * weapon.ThrustDamage / 100f
            * weapon.SwingSpeed / 100f
            * weapon.MissileSpeed / 10f
            * weapon.Accuracy / 10f
            / scaler;
    }

    private float CalculateShieldTier(WeaponComponent weaponComponent)
    {
        WeaponComponentData weapon = weaponComponent.Weapons[0];
        return
                 (1.0f * weapon.MaxDataValue
                + 3.0f * weapon.BodyArmor
                + 1.0f * weapon.ThrustSpeed)
                / (6f + weaponComponent.Item.Weight)
                / 48.6419f
                * 10f;
    }

    private float CalculateAmmoTier(WeaponComponent weaponComponent)
    {
        WeaponComponentData weapon = weaponComponent.Weapons[0];
        float scaler = weaponComponent.Item.ItemType switch
        {
            ItemObject.ItemTypeEnum.Arrows => 388.7999856f,
            ItemObject.ItemTypeEnum.Bolts => 225f,
            _ => 10f,
        };
        return
            10f
          * CalculateDamageTypeFactor(weapon.ThrustDamageType)
          * weapon.MissileDamage * weapon.MissileDamage
          * weapon.MaxDataValue
          * CalculateDamageTypeFactor(weapon.ThrustDamageType)
          / scaler;
    }
}
