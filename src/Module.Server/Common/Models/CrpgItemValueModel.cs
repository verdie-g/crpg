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
    public float AdjustedTier(float tier)
    {
        const float a = 300;
        const float b = 60;
        const float c = 50;
        float tierPolynome = (float)(a * Math.Pow(tier, 2) + b * tier + c);
        float tierPolynomeScaler = 10 / ((float)(a * Math.Pow(10, 2) + b * 10 + c));
        return tierPolynome * tierPolynomeScaler;

    }
    public override float GetEquipmentValueFromTier(float tier)
    {
        return tier;
    }

    public override int CalculateValue(ItemObject item)
    {
        int desiredArmorMaxPrice;
        int desiredHorseMaxPrice;
        int desiredBannerMaxPrice;
        int desiredArmorMaxPrice;
        return item.ItemComponent switch
        {
            ArmorComponent armorComponent => (int) GetEquipmentValueFromTier(item.Tierf),
            HorseComponent horseComponent => (int) GetEquipmentValueFromTier(item.Tierf),
            BannerComponent bannerComponent => (int) GetEquipmentValueFromTier(item.Tierf),
            WeaponComponent weaponComponent => (int) GetEquipmentValueFromTier(item.Tierf),
            _ => 0f,
        };
    }

    private void CalculateArmorTier(ArmorComponent armorComponent, out ItemObject.ItemTypeEnum armorType, out float armorTier)
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
        armorType = armorComponent.Item.ItemType;
        armorTier = 10 * armorValue / bestArmorValue;
    }

    private float CalculateHorseTier(HorseComponent horseComponent,out )
    {
            float horseValue =
            1.5f * horseComponent.ChargeDamage
            + 1.0f * horseComponent.Speed
            + 0.6f * horseComponent.Maneuver
            + 0.1f * horseComponent.HitPoints;
            float bestHorseValue = 125.5f;
            return 10 * horseValue / bestHorseValue;            
    }

    private float CalculateBannerTier(BannerComponent bannerComponent)
    {
        return GetBaseTierValueForBannerEffect(bannerComponent.BannerEffect)
            + bannerComponent.BannerLevel;
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
            return maxTier;
        }

        return maxTier * (float)Math.Pow(1f + (secondMaxTier + 1.5f) / (maxTier + 2.5f), 0.2f);
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
        float extra = 0f;
        if (weaponComponent.Item is { ItemType: ItemObject.ItemTypeEnum.Crossbow })
        {
            extra += -3.0f;
        }

        if (weapon.ItemUsage.Contains("light"))
        {
            extra += 1.25f;
        }

        if (!weaponComponent.PrimaryWeapon.ItemUsage.Contains("long_bow")
            && !weaponComponent.PrimaryWeapon.WeaponFlags.HasAnyFlag(WeaponFlags.CantReloadOnHorseback))
        {
            extra += 0.5f;
        }

        return 0.15f * weapon.ThrustDamage
            + 0.01f * weapon.SwingSpeed
            + 0.01f * weapon.MissileSpeed
            + 0.01f * weapon.Accuracy
            + extra - 3f;
    }

    private float CalculateShieldTier(WeaponComponent weaponComponent)
    {
        WeaponComponentData weapon = weaponComponent.Weapons[0];
        return (
                1.0f * weapon.MaxDataValue
                + 3.0f * weapon.BodyArmor
                + 1.0f * weapon.ThrustSpeed)
            / (6f + weaponComponent.Item.Weight) * 0.10f;
    }

    private float CalculateAmmoTier(WeaponComponent weaponComponent)
    {
        WeaponComponentData weapon = weaponComponent.Weapons[0];
        return weapon.MissileDamage * 1f
            + Math.Max(0, weapon.MaxDataValue - 20) * 0.1f;
    }
}
