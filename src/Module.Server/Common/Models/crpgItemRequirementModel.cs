using System;
using System.Collections.Generic;
using System.Text;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Helpers;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.Models;

internal static class CrpgItemRequirementModel
{
    public static int ComputeItemRequirement(ItemObject item)
    {
        return item.ItemType switch
        {
            ItemObject.ItemTypeEnum.HeadArmor => ComputeArmorPieceStrengthRequirement(item),
            ItemObject.ItemTypeEnum.BodyArmor => ComputeArmorPieceStrengthRequirement(item),
            ItemObject.ItemTypeEnum.Cape => ComputeArmorPieceStrengthRequirement(item),
            ItemObject.ItemTypeEnum.HandArmor => ComputeArmorPieceStrengthRequirement(item),
            ItemObject.ItemTypeEnum.LegArmor => ComputeArmorPieceStrengthRequirement(item),
            ItemObject.ItemTypeEnum.Crossbow => ComputeCrossbowRequirement(item),
            _ => 0,
        };
    }

    public static int ComputeArmorPieceStrengthRequirement(ItemObject item)
    {
        int strengthRequirementForTierTenArmor = 24; // Tiers are calulated in CrpgValueModel. 0<Tier=<10 . By design the best armor is always at Ten.
        if (item.ArmorComponent == null)
        {
            throw new ArgumentException(item.Name.ToString(), "is not an armor item");
        }

        return (int)(item.Tierf * (strengthRequirementForTierTenArmor / 10f));
    }

    // make sure this method does the same thing as the one in the webui.
    public static int ComputeArmorSetPieceStrengthRequirement(List<ItemObject> armors)
    {
        if (armors == null)
        {
            return 0;
        }

        const int numberOfArmorItemTypes = 5;
        float[] armorsRequirements = new float[numberOfArmorItemTypes];
        for (int i = 0; i < armors.Count; i++)
        {
            armorsRequirements[i] = ComputeArmorPieceStrengthRequirement(armors[i]);
        }

        return (int)MathHelper.GeneralizedMean(10, armorsRequirements);
    }

    public static int ComputeCrossbowRequirement(ItemObject item)
    {
        int strengthRequirementForTierTenCrossbow = 18; // Tiers are calulated in CrpgValueModel. 0<Tier=<10 . By design the best armor is always at Ten.
        if (item.ItemType != ItemObject.ItemTypeEnum.Crossbow)
        {
            throw new ArgumentException(item.Name.ToString(), "is not a crossbow");
        }

        return (int)(item.Tierf * (strengthRequirementForTierTenCrossbow / 10f));
    }
}
