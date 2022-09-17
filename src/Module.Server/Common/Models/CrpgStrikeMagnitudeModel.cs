using Crpg.Module.Helpers;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.Models;

/// <summary>
/// Used to adjust raw dmg calculations.
/// </summary>
internal class CrpgStrikeMagnitudeModel : MultiplayerStrikeMagnitudeModel
{
    private readonly CrpgConstants _constants;

    public CrpgStrikeMagnitudeModel(CrpgConstants constants)
    {
        _constants = constants;
    }

    public override float ComputeRawDamage(DamageTypes damageType, float magnitude, float armorEffectiveness, float absorbedDamageRatio)
    {
        float bluntDamageFactorByDamageType = GetBluntDamageFactorByDamageType(damageType);
        float bluntDamageFactorByDamageTypeNew = GetBluntDamageFactorByDamageTypeNew(damageType);
        float num = 100f / (100f + armorEffectiveness);
        float num2 = magnitude * num;
        float num3 = bluntDamageFactorByDamageType * num2;
        float num3New = bluntDamageFactorByDamageTypeNew * num2;
        Console.WriteLine("Dmgtype: "+damageType.ToString());
        if (damageType != DamageTypes.Blunt)
        {
            float num4;
            switch (damageType)
            {
                case DamageTypes.Cut:
                    num4 = MathF.Max(0f, magnitude * (1f - 0.6f * armorEffectiveness / (20f + 0.4f * armorEffectiveness)));
                    break;
                case DamageTypes.Pierce:
                    num4 = MathF.Max(0f, magnitude * (45f / (45f + armorEffectiveness)));
                    break;
                default:
                    Debug.FailedAssert("Given damage type is invalid.", "C:\\Develop2\\mb3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\ComponentInterfaces\\MultiplayerStrikeMagnitudeModel.cs", "ComputeRawDamage", 45);
                    return 0f;
            }

            num3 += (1f - bluntDamageFactorByDamageType) * num4;
            num3New += (1f - bluntDamageFactorByDamageTypeNew) * num4;
        }
        float finalRawDmg = num3 * absorbedDamageRatio;
        float newFinalRawDmg = num3New * absorbedDamageRatio;
        Console.WriteLine("Old raw dmg: " + finalRawDmg);
        Console.WriteLine("New raw dmg: " + newFinalRawDmg);
        Console.WriteLine("Name: "+this.GetType().ToString());
        return finalRawDmg;
    }

    public override float GetBluntDamageFactorByDamageType(DamageTypes damageType)
    {
        float result = 0f;
        switch (damageType)
        {
            case DamageTypes.Blunt:
                result = 1f;
                break;
            case DamageTypes.Cut:
                result = 0.1f;
                break;
            case DamageTypes.Pierce:
                result = 0.25f;
                break;
        }

        return result;
    }
    public float GetBluntDamageFactorByDamageTypeNew(DamageTypes damageType)
    {
        float result = 0f;
        switch (damageType)
        {
            case DamageTypes.Blunt:
                result = 1f;
                break;
            case DamageTypes.Cut:
                result = 0.2f;
                break;
            case DamageTypes.Pierce:
                result = 0.3f;
                break;
        }

        return result;
    }


}
