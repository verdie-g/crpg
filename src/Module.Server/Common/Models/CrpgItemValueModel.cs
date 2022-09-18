﻿using TaleWorlds.Core;
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
    // It uses a degree 2 polynomial.
    // b is responsible for the linear part.
    // a is responsible for the quadratic part. a Linear fonction is not enough because it doesn't reflect how the best items are more
    // than just linearly better.
    public float GetAdjustedTier(float tier)
    {
        const float a = 300;
        const float b = 700;
        const float c =0;
        float tierPolynome = (float)(a * Math.Pow(tier, 2) + b * tier + c);
        float tierPolynomeScaler = 10 / ((float)(a * Math.Pow(10, 2) + b * 10 + c)); // this part will make sure that GetAdjustedTier(10)=10
        return tierPolynome * tierPolynomeScaler;
    }

    public float CrpgGetEquipmentValueFromTier(float tier, int desiredMaxPrice, int desiredTierZeroPrice)
    {
        return GetAdjustedTier(tier) * (desiredMaxPrice - desiredTierZeroPrice) / 10 + desiredStartingPrice;
    }

    public override float GetEquipmentValueFromTier(float tier) // i'd like to trash this function but since it's in ItemValueModel Implementation and it doesn't allow me to add more inputs (namidaka)
    {
        float whocares = 0;
        return whocares;
    }

    public override int CalculateValue(ItemObject item)
    {
        int desiredHeadArmorMaxPrice = 9754;
        int desiredCapeArmorMaxPrice = 11441;
        int desiredBodyArmorMaxPrice = 31632;
        int desiredHandArmorMaxPrice = 6000;
        int desiredLegArmorMaxPrice = 4662;
        int desiredHorseHarnessMaxPrice = 20000;
        int desiredHorseMaxPrice = 14000;
        int desiredShieldMaxPrice = 9235;
        int desiredBowMaxPrice = 12264;
        int desiredCrossbowMaxPrice = 18000;
        int desiredOneHandedWeaponMaxPrice = 9100;  // does not work as intended but may mean that onehanded are to be buffed
        int desiredTwoHandedWeaponMaxPrice = 14000; // kinda work as intended but no by design
        int desiredPolearmMaxPrice = 16175; // kinda work as intended but not by design
        int desiredThrownMaxPrice = 7385; // kinda work as intended but not by design
        int desiredArrowsMaxPrice = 3858; 
        int desiredBoltsMaxPrice = 16000; // doesn't work as intended yet
        int desiredBannerMaxPrice = 50;
        if (item!.ItemComponent?.Item?.ItemType == null)
        {
            return (int)(GetAdjustedTier(item.Tierf) * (desiredHorseMaxPrice - 50) / 10 + 50);
        }

        return item.ItemComponent.Item.ItemType switch
        {
            ItemObject.ItemTypeEnum.HeadArmor => (int)CrpgGetEquipmentValueFromTier(item.Tierf,desiredHeadArmorMaxPrice,50),
            ItemObject.ItemTypeEnum.Cape => (int)CrpgGetEquipmentValueFromTier(item.Tierf, desiredCapeArmorMaxPrice, 50),
            ItemObject.ItemTypeEnum.BodyArmor => (int)CrpgGetEquipmentValueFromTier(item.Tierf, desiredBodyArmorMaxPrice, 50),
            ItemObject.ItemTypeEnum.HandArmor => (int)CrpgGetEquipmentValueFromTier(item.Tierf, desiredHandArmorMaxPrice, 50),
            ItemObject.ItemTypeEnum.LegArmor => (int)CrpgGetEquipmentValueFromTier(item.Tierf, desiredLegArmorMaxPrice, 50),
            ItemObject.ItemTypeEnum.HorseHarness => (int)CrpgGetEquipmentValueFromTier(item.Tierf, desiredHorseHarnessMaxPrice, 50),
            ItemObject.ItemTypeEnum.Shield => (int)CrpgGetEquipmentValueFromTier(item.Tierf, desiredShieldMaxPrice, 50),
            ItemObject.ItemTypeEnum.Bow => (int)CrpgGetEquipmentValueFromTier(item.Tierf, desiredBowMaxPrice, 50),
            ItemObject.ItemTypeEnum.Crossbow =>(int)CrpgGetEquipmentValueFromTier(item.Tierf, desiredCrossbowMaxPrice, 50),
            ItemObject.ItemTypeEnum.OneHandedWeapon => (int)CrpgGetEquipmentValueFromTier(item.Tierf, desiredOneHandedWeaponMaxPrice, 50),
            ItemObject.ItemTypeEnum.TwoHandedWeapon => (int)CrpgGetEquipmentValueFromTier(item.Tierf, desiredTwoHandedWeaponMaxPrice, 50),
            ItemObject.ItemTypeEnum.Polearm => (int)CrpgGetEquipmentValueFromTier(item.Tierf, desiredPolearmMaxPrice, 50),
            ItemObject.ItemTypeEnum.Thrown => (int)CrpgGetEquipmentValueFromTier(item.Tierf, desiredThrownMaxPrice, 50),
            ItemObject.ItemTypeEnum.Arrows => (int)CrpgGetEquipmentValueFromTier(item.Tierf, desiredArrowsMaxPrice, 50),
            ItemObject.ItemTypeEnum.Bolts => (int)CrpgGetEquipmentValueFromTier(item.Tierf, desiredBoltsMaxPrice, 50),
            ItemObject.ItemTypeEnum.Banner => (int)CrpgGetEquipmentValueFromTier(item.Tierf, desiredBannerMaxPrice, 50),
            _ => 6969, //noice
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
