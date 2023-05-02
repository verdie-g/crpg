using TaleWorlds.Core;

namespace Crpg.Module.Common.Models;

internal class CrpgItemRequirementModel
{
    private readonly CrpgConstants _constants;

    public CrpgItemRequirementModel(CrpgConstants constants)
    {
        _constants = constants;
    }

    public static int ComputeItemRequirement(ItemObject item)
    {
        switch (item.ItemType)
        {
            case ItemObject.ItemTypeEnum.Crossbow:
                return ComputeCrossbowRequirement(item);
        }

        return 0;
    }

    private static int ComputeCrossbowRequirement(ItemObject item)
    {
        int baseStrengthRequirement = 9;
        int strengthRequirementForTierTenCrossbow = 18; // Tiers are calulated in CrpgValueModel. 0<Tier=<10 . By design the best armor is always at Ten.
        if (item.ItemType != ItemObject.ItemTypeEnum.Crossbow)
        {
            throw new ArgumentException(item.Name.ToString() + " is not a crossbow");
        }

        return (int)(baseStrengthRequirement + item.Tierf * (strengthRequirementForTierTenCrossbow - baseStrengthRequirement) / 10f);
    }
}
