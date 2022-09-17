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

    // this method takes a value between 0 and 10 and outputs a value between 0 and 10
    public float GetAdjustedTier(float tier)
    {
        const float a = 300;
        const float b = 700;
        const float c = 50;
        float tierPolynome = (float)(a * Math.Pow(tier, 2) + b * tier + c);
        float tierPolynomeScaler = 10 / ((float)(a * Math.Pow(10, 2) + b * 10 + c));
        return tierPolynome * tierPolynomeScaler;

    }
    public override float GetEquipmentValueFromTier(float tier) // i'd like to trash this function but since it's in ItemValueModel Implementation i don't know what do to with it (namidaka)
    {
        return GetAdjustedTier(tier);
    }

    public override int CalculateValue(ItemObject item)
    {
        int desiredHeadArmorMaxPrice = 9754; // work as intended
        int desiredCapeArmorMaxPrice = 11441; // work as intended
        int desiredBodyArmorMaxPrice = 31632; // work as intended
        int desiredHandArmorMaxPrice = 6000; // work as intended
        int desiredLegArmorMaxPrice = 4662; // work as intended
        int desiredHorseHarnessMaxPrice = 20000; // work as intended
        int desiredHorseMaxPrice = 14000; // work as intended
        int desiredShieldMaxPrice = 9235; // work as intended
        int desiredBowMaxPrice = 12264; // work as intended
        int desiredCrossbowMaxPrice = 18000; // work as intended
        int desiredOneHandedWeaponMaxPrice = 9100;  // does not work as intended but may mean that onehanded are to be buffed
        int desiredTwoHandedWeaponMaxPrice = 14000; // kinda work as intended but no by design
        int desiredPolearmMaxPrice = 16175; // kinda work as intended but not by design
        int desiredThrownMaxPrice = 7385; // kinda work as intended but not by design
        int desiredArrowsMaxPrice = 3858; // work as intended
        int desiredBoltsMaxPrice = 16000; // work as intended yet
        int desiredBannerMaxPrice = 50;
        if (item!.ItemComponent?.Item?.ItemType == null)
        {
            return (int)(GetAdjustedTier(item.Tierf) * (desiredHorseMaxPrice - 50) / 10 + 50);
        }

        return item.ItemComponent.Item.ItemType switch
        {
            ItemObject.ItemTypeEnum.HeadArmor => (int)(GetAdjustedTier(item.Tierf) * (desiredHeadArmorMaxPrice - 50) / 10 + 50),
            ItemObject.ItemTypeEnum.Cape => (int)(GetAdjustedTier(item.Tierf) * (desiredCapeArmorMaxPrice - 50 ) / 10 + 50),
            ItemObject.ItemTypeEnum.BodyArmor => (int)(GetAdjustedTier(item.Tierf) * (desiredBodyArmorMaxPrice - 50) / 10 + 50),
            ItemObject.ItemTypeEnum.HandArmor => (int)(GetAdjustedTier(item.Tierf) * (desiredHandArmorMaxPrice - 50) / 10 + 50),
            ItemObject.ItemTypeEnum.LegArmor => (int)(GetAdjustedTier(item.Tierf) * (desiredLegArmorMaxPrice - 50) / 10 + 50),
            ItemObject.ItemTypeEnum.HorseHarness => (int) (GetAdjustedTier(item.Tierf) * (desiredHorseHarnessMaxPrice - 50) / 10 + 50),
            ItemObject.ItemTypeEnum.Shield => (int)(GetAdjustedTier(item.Tierf) * (desiredShieldMaxPrice - 50) / 10 + 50),
            ItemObject.ItemTypeEnum.Bow => (int)(GetAdjustedTier(item.Tierf) * (desiredBowMaxPrice - 50) / 10 + 50),
            ItemObject.ItemTypeEnum.Crossbow =>(int)(GetAdjustedTier(item.Tierf) * (desiredCrossbowMaxPrice - 50) / 10 + 50),
            ItemObject.ItemTypeEnum.OneHandedWeapon => (int)(GetAdjustedTier(item.Tierf) * (desiredOneHandedWeaponMaxPrice - 50) / 10 + 50),
            ItemObject.ItemTypeEnum.TwoHandedWeapon => (int)(GetAdjustedTier(item.Tierf) * (desiredTwoHandedWeaponMaxPrice - 50) / 10 + 50),
            ItemObject.ItemTypeEnum.Polearm => (int)(GetAdjustedTier(item.Tierf) * (desiredPolearmMaxPrice - 50) / 10 + 50),
            ItemObject.ItemTypeEnum.Thrown => (int)(GetAdjustedTier(item.Tierf) * (desiredThrownMaxPrice - 50) / 10 + 50),
            ItemObject.ItemTypeEnum.Arrows => (int)(GetAdjustedTier(item.Tierf) * (desiredArrowsMaxPrice - 50) / 10 + 50),
            ItemObject.ItemTypeEnum.Bolts => (int)(GetAdjustedTier(item.Tierf) * (desiredBoltsMaxPrice - 50) / 10 + 50),
            ItemObject.ItemTypeEnum.Banner => (int)(GetAdjustedTier(item.Tierf) * (desiredBannerMaxPrice -50) / 10 + 50),
            _ => 6969,
        };
    }

    private float CalculateArmorTier(ArmorComponent armorComponent)
    {
        float armorValue = 1.2f * armorComponent.HeadArmor
            + 1.0f * armorComponent.BodyArmor
            + 1.0f * armorComponent.ArmArmor
            + 1.0f * armorComponent.LegArmor;
        float bestArmorValue = armorComponent.Item.ItemType switch
        {
            ItemObject.ItemTypeEnum.HeadArmor => 54 * 1.2f,
            ItemObject.ItemTypeEnum.Cape => 34f,
            ItemObject.ItemTypeEnum.BodyArmor => 94f,
            ItemObject.ItemTypeEnum.HandArmor => 25f,
            ItemObject.ItemTypeEnum.LegArmor => 26f,
            ItemObject.ItemTypeEnum.HorseHarness => 20f,
            _ => throw new ArgumentOutOfRangeException(),
        };
        float armorTier = 10 * armorValue / bestArmorValue;
        return armorTier;
    }

    private float CalculateHorseTier(HorseComponent horseComponent)
    {
            float horseValue =
            1.5f * horseComponent.ChargeDamage
            + 1.0f * horseComponent.Speed
            + 0.6f * horseComponent.Maneuver
            + 0.1f * horseComponent.HitPoints;
            float bestHorseValue = 125.5f;
            return 10f * horseValue / bestHorseValue;
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
        return weaponComponent.Item?.WeaponDesign == null
            ? CalculateTierNonCraftedWeapon(weaponComponent)
            : CalculateTierMeleeWeapon(weaponComponent);
    }

    private float CalculateTierMeleeWeapon(WeaponComponent weaponComponent)
    {
        float maxTier = float.MinValue;
        float secondMaxTier = float.MinValue;
        foreach (var weapon in weaponComponent.Weapons)
        {
            float thrustTier = weapon.ThrustDamage
                * CalculateDamageTypeFactor(weapon.ThrustDamageType)
                * (float)Math.Pow(weapon.ThrustSpeed * 0.01f, 1.5f)
                * 1f;
            float swingTier = weapon.SwingDamage
                * CalculateDamageTypeFactor(weapon.SwingDamageType)
                * (float)Math.Pow(weapon.SwingSpeed * 0.01f, 1.5f)
                * 1.1f;
            float tier = Math.Max(thrustTier, swingTier);

            if (weapon.WeaponFlags.HasAnyFlag(WeaponFlags.NotUsableWithOneHand))
            {
                tier *= 0.8f;
            }

            if (weapon.WeaponClass is WeaponClass.ThrowingKnife or WeaponClass.ThrowingAxe)
            {
                tier *= 1.2f;
            }

            if (weapon.WeaponClass == WeaponClass.Javelin)
            {
                tier *= 0.6f;
            }

            float lengthTier = weapon.WeaponLength * 0.01f;
            tier = 0.06f * (tier * (1f + lengthTier));
            if (tier > secondMaxTier)
            {
                if (tier >= maxTier)
                {
                    secondMaxTier = maxTier;
                    maxTier = tier;
                }
                else
                {
                    secondMaxTier = tier;
                }
            }
        }

        if (weaponComponent.Weapons.Count <= 1)
        {
            return maxTier * 1.5f;
        }

        return maxTier * (float)Math.Pow(1f + (secondMaxTier + 1.5f) / (maxTier + 2.5f), 0.2f) * 1.5f;
    }

    private float CalculateDamageTypeFactor(DamageTypes damageType)
    {
        return damageType switch
        {
            DamageTypes.Blunt => 1.45f,
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

    private float CalculateRangedWeaponTier(WeaponComponent weaponComponent)
    {
        WeaponComponentData weapon = weaponComponent.Weapons[0];
        float scaler = 1440257f;

        if (weaponComponent.Item is { ItemType: ItemObject.ItemTypeEnum.Crossbow })
        {
            scaler = 4094370f;
        }

        /*if (weapon.ItemUsage.Contains("light"))
        {
            extra += 1.25f;
        }

        if (!weaponComponent.PrimaryWeapon.ItemUsage.Contains("long_bow")
            && !weaponComponent.PrimaryWeapon.WeaponFlags.HasAnyFlag(WeaponFlags.CantReloadOnHorseback))
        {
            extra += 0.5f;
        }*/

        return weapon.ThrustDamage
            * weapon.SwingSpeed
            * weapon.MissileSpeed
            * weapon.Accuracy
            / scaler;
    }

    private float CalculateShieldTier(WeaponComponent weaponComponent)
    {
        WeaponComponentData weapon = weaponComponent.Weapons[0];
        return (
                1.0f * weapon.MaxDataValue
                + 3.0f * weapon.BodyArmor
                + 1.0f * weapon.ThrustSpeed)
            / (6f + weaponComponent.Item.Weight) / 48.6419f * 10f;
    }

    private float CalculateAmmoTier(WeaponComponent weaponComponent)
    {
        WeaponComponentData weapon = weaponComponent.Weapons[0];
        return 10f * weapon.MissileDamage * weapon.MissileDamage * weapon.MaxDataValue / 368f * CalculateDamageTypeFactor(weapon.ThrustDamageType);
        ;
    }
}
