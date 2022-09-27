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
    public static int ComputeArmorPieceStrengthRequirement(ItemObject item)
    {
        int strengthRequirementForTierTenArmor = 24; // Tiers are calulated in CrpgValueModel. 0<Tier=<10 . By design the best armor is always at Ten.
        if (item.ArmorComponent == null)
        {
            throw new ArgumentException(item.Name.ToString(), "is not an armor item");
        }

        int requirement = (int)(item.Tierf * (float)(strengthRequirementForTierTenArmor / 10f));
        return requirement;
    }

    public static int ComputeArmorSetPieceStrengthRequirement(List<ItemObject> armors)
    {
        if (armors == null)
        {
            return 0;
        }

        int[] armorsRequirement = armors.Select(a => ComputeArmorPieceStrengthRequirement(a)).ToArray();
        int armorSetRequirement = (int)MathHelper.GeneralizedMean(10, armorsRequirement.Select(value => (float)value).ToArray());
        return armorSetRequirement;
    }
}
