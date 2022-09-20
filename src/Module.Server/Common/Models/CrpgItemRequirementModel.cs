using System;
using System.Collections.Generic;
using System.Text;
using Crpg.Module.Api.Models.Items;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Crpg.Module.Common.Models;
internal class CrpgItemRequirementModel
{
    internal float ComputeArmorStrengthRequirement(ItemObject item)
    {
        int strengthRequirementForTierTenArmor = 24;
        if (item.ArmorComponent == null)
        {
            throw new ArgumentException(item.ItemComponent.GetName().ToString(), "is not an armor item");
        }

        return item.Tierf * (float)(strengthRequirementForTierTenArmor / 10f); // is the cast too much?
    }

    internal float ComputeBowPowerDrawRequirement(ItemObject item)
    {
        int powerDrawRequirementForTierTenBow = 6;
        if (item.WeaponComponent == null)
        {
            throw new ArgumentException(item.ItemComponent.GetName().ToString(), "is not a Weapon");
        }

        if (item.ItemType != ItemObject.ItemTypeEnum.Bow)
        {
            throw new ArgumentException(item.ItemComponent.GetName().ToString(), "is not a Bow");
        }

        return item.Tierf * (float)(powerDrawRequirementForTierTenBow / 10f);
    }

    internal float ComputeCrossbowStrengthRequirement(ItemObject item)
    {
        int strengthRequirementForTierTenCrossbow = 18;
        if (item.WeaponComponent == null)
        {
            throw new ArgumentException(item.ItemComponent.GetName().ToString(), "is not a Weapon");
        }

        if (item.ItemType != ItemObject.ItemTypeEnum.Crossbow)
        {
            throw new ArgumentException(item.ItemComponent.GetName().ToString(), "is not a Crossbow");
        }

        return item.Tierf * (float)(strengthRequirementForTierTenCrossbow / 10f);
    }

    internal float ComputeMeleeWeaponStrengthRequirement(ItemObject item)
    {
        int weaponStrengthRequirementForTierTenMeleeWeapon = 18;
        if (item.WeaponComponent == null)
        {
            throw new ArgumentException(item.ItemComponent.GetName().ToString(), "is not a Weapon");
        }

        return item.Tierf * (float)(weaponStrengthRequirementForTierTenMeleeWeapon / 10f);
    }

    internal float ComputeHorseRidingSkillRequirement(ItemObject item)
    {
        int horseRidingSkillRequirement = 7;
        if (item.HorseComponent == null)
        {
            throw new ArgumentException(item.ItemComponent.GetName().ToString(), "is not a Horse");
        }

        return item.Tierf * (float)(horseRidingSkillRequirement / 10f);
    }

    internal float ComputeShieldSkillRequirement(ItemObject item)
    {
        int shieldSkillRequirement = 7;
        if (item.WeaponComponent == null)
        {
            throw new ArgumentException(item.ItemComponent.GetName().ToString(), "is not a shield , it's not even a weapon type");
        }

        if (item.ItemType != ItemObject.ItemTypeEnum.Crossbow)
        {
            throw new ArgumentException(item.ItemComponent.GetName().ToString(), "is not a shield");
        }

        return item.Tierf * (float)(shieldSkillRequirement / 10f);
    }

    internal float ComputePowerThrowRequirement(ItemObject item)
    {
        int powerThrowRequirement = 7;
        if (item.WeaponComponent == null)
        {
            throw new ArgumentException(item.ItemComponent.GetName().ToString(), "is not a throwing weapon , it's not even of weapon type");
        }

        if (item.ItemType != ItemObject.ItemTypeEnum.Thrown)
        {
            throw new ArgumentException(item.ItemComponent.GetName().ToString(), "is not a throwing weapon");
        }

        return item.Tierf * (float)(powerThrowRequirement / 10f);
    }
}
