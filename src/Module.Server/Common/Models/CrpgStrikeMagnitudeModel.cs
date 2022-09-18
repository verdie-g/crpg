﻿using Crpg.Module.Helpers;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.Models;

/// <summary>
/// Used to adjust raw dmg calculations.
/// </summary>
internal class CrpgStrikeMagnitudeModel : StrikeMagnitudeCalculationModel
{
    private readonly CrpgConstants _constants;

    public CrpgStrikeMagnitudeModel(CrpgConstants constants)
    {
        _constants = constants;
    }

    public override float ComputeRawDamage(DamageTypes damageType, float magnitude, float armorEffectiveness, float absorbedDamageRatio)
    {
        float bluntDamageFactorByDamageType = GetBluntDamageFactorByDamageType(damageType);
        float num = 100f / (100f + armorEffectiveness);
        float num2 = magnitude * num;
        float num3 = bluntDamageFactorByDamageType * num2;
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
        }

        return num3 * absorbedDamageRatio;
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
                result = 0.25f; // Native .1f
                break;
            case DamageTypes.Pierce:
                result = 0.30f; // Native .25f
                break;
        }

        return result;
    }

    public override float CalculateStrikeMagnitudeForSwing(BasicCharacterObject attackerCharacter, BasicCharacterObject attackerCaptainCharacter, float swingSpeed, float impactPoint, float weaponWeight, WeaponComponentData weaponUsageComponent, float weaponLength, float weaponInertia, float weaponCoM, float extraLinearSpeed, bool doesAttackerHaveMount)
    {
        return CombatStatCalculator.CalculateStrikeMagnitudeForSwing(swingSpeed, impactPoint, weaponWeight, weaponLength, weaponInertia, weaponCoM, extraLinearSpeed);
    }

    public override float CalculateStrikeMagnitudeForThrust(BasicCharacterObject attackerCharacter, BasicCharacterObject attackerCaptainCharacter, float thrustWeaponSpeed, float weaponWeight, WeaponComponentData weaponUsageComponent, float extraLinearSpeed, bool doesAttackerHaveMount, bool isThrown = false)
    {
        return CombatStatCalculator.CalculateStrikeMagnitudeForThrust(thrustWeaponSpeed, weaponWeight, extraLinearSpeed, isThrown);
    }

    public override float CalculateSpeedBonusMultiplierForMissile(BasicCharacterObject attackerCharacter, WeaponClass ammoClass)
    {
        return 0f;
    }

    public override float CalculateHorseArcheryFactor(BasicCharacterObject characterObject)
    {
        return 100f;
    }
}
