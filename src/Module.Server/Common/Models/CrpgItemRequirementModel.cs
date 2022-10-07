using Crpg.Module.Helpers;
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
            case ItemObject.ItemTypeEnum.HeadArmor:
            case ItemObject.ItemTypeEnum.BodyArmor:
            case ItemObject.ItemTypeEnum.Cape:
            case ItemObject.ItemTypeEnum.HandArmor:
            case ItemObject.ItemTypeEnum.LegArmor:
                return ComputeArmorPieceStrengthRequirement(item);
            case ItemObject.ItemTypeEnum.Crossbow:
                return ComputeCrossbowRequirement(item);
        }

        return 0;
    }

    // make sure this method does the same thing as the one in the webui.
    public int ComputeArmorSetPieceStrengthRequirement(List<ItemObject> armors)
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

        return (int)MathHelper.GeneralizedMean(_constants.ArmorSetRequirementPowerMeanPValue, armorsRequirements);
    }

    private static int ComputeArmorPieceStrengthRequirement(ItemObject item)
    {
        int strengthRequirementForTierTenArmor = 24; // Tiers are calulated in CrpgValueModel. 0<Tier=<10 . By design the best armor is always at Ten.
        if (item.ArmorComponent == null)
        {
            throw new ArgumentException(item.Name.ToString(), "is not an armor item");
        }

        return (int)(item.Tierf * (strengthRequirementForTierTenArmor / 10f));
    }

    private static int ComputeCrossbowRequirement(ItemObject item)
    {
        int strengthRequirementForTierTenCrossbow = 18; // Tiers are calulated in CrpgValueModel. 0<Tier=<10 . By design the best armor is always at Ten.
        if (item.ItemType != ItemObject.ItemTypeEnum.Crossbow)
        {
            throw new ArgumentException(item.Name.ToString(), "is not a crossbow");
        }

        return (int)(item.Tierf * (strengthRequirementForTierTenCrossbow / 10f));
    }
}
