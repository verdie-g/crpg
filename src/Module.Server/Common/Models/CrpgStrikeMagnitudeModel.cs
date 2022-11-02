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

    public override float CalculateStrikeMagnitudeForSwing(
        BasicCharacterObject attackerCharacter,
        BasicCharacterObject attackerCaptainCharacter,
        float swingSpeed, float impactPointAsPercent,
        float weaponWeight, WeaponComponentData weaponUsageComponent,
        float weaponLength,
        float weaponInertia,
        float weaponCoM,
        float extraLinearSpeed,
        bool doesAttackerHaveMount)
    {
        float num = weaponLength * impactPointAsPercent - weaponCoM;
        float num2 = swingSpeed * (0.5f + weaponCoM) + extraLinearSpeed;
        float num3 = 0.5f * weaponWeight * num2 * num2;
        float num4 = 0.5f * weaponInertia * swingSpeed * swingSpeed;
        float num5 = num3 + num4;
        float num6 = (num2 + swingSpeed * num) / (1f / weaponWeight + num * num / weaponInertia);
        float num7 = num2 - num6 / weaponWeight;
        float num8 = swingSpeed - num6 * num / weaponInertia;
        float num9 = 0.5f * weaponWeight * num7 * num7;
        float num10 = 0.5f * weaponInertia * num8 * num8;
        float num11 = num9 + num10;
        float num12 = num5 - num11 + 0.5f;
        return 0.067f * num12;
    }

    public override float CalculateStrikeMagnitudeForThrust(
        BasicCharacterObject attackerCharacter,
        BasicCharacterObject attackerCaptainCharacter,
        float thrustWeaponSpeed, float weaponWeight,
        WeaponComponentData weaponUsageComponent,
        float extraLinearSpeed,
        bool doesAttackerHaveMount,
        bool isThrown = false)
    {
        float num = doesAttackerHaveMount
            ? thrustWeaponSpeed + (float)Math.Pow(extraLinearSpeed, 0.7f)
            : thrustWeaponSpeed + extraLinearSpeed;
        if (num > 0f)
        {
            if (!isThrown)
            {
                weaponWeight += 2.5f;
            }

            float num2 = 0.5f * weaponWeight * num * num;
            return 0.125f * num2;
        }

        return 0f;
    }

    public override float GetBluntDamageFactorByDamageType(DamageTypes damageType)
    {
        float result = 0f;
        switch (damageType)
        {
            case DamageTypes.Blunt:
                result = 1f; // Native 1f
                break;
            case DamageTypes.Cut:
                result = 0.4f; // Native .1f
                break;
            case DamageTypes.Pierce:
                result = .45f; // Native .25f
                break;
        }

        return result;
    }
}
