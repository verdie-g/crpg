using System;
using System.Collections.Generic;
using System.Text;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Helpers;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace Crpg.Module.Common.Models;

internal static class CrpgItemRequirementModel
{
    public static float ComputeArmorPieceStrengthRequirement(ItemObject item)
    {
        int strengthRequirementForTierTenArmor = 24;
        if (item.ArmorComponent == null)
        {
            throw new ArgumentException(item.ItemComponent.GetName().ToString(), "is not an armor item");
        }

        double doubleRequirement = item.Tierf * (float)(strengthRequirementForTierTenArmor / 10f);
        float halfIntRequirement = (int)(doubleRequirement * 2f) / 2f;
        return halfIntRequirement;
    }

    public static float ComputeArmorSetPieceStrengthRequirement(List<ItemObject> armors)
    {
        if (armors == null)
        {
            return 0;
        }

        float[] armorsrequirement = armors.Select(a => ComputeArmorPieceStrengthRequirement(a)).ToArray();
        int armorSetRequirementTimeTwo = (int)(MathHelper.GeneralizedMean(10, armorsrequirement) * 2f);
        return armorSetRequirementTimeTwo / 2f;
    }
}
