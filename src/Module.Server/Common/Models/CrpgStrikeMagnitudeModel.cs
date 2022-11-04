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
        float swingSpeed,
        float impactPointAsPercent,
        float weaponWeight,
        WeaponComponentData weaponUsageComponent,
        float weaponLength,
        float weaponInertia,
        float weaponCenterOfMass,
        float extraLinearSpeed,
        bool doesAttackerHaveMount)
    {
        float centerOfMassSpeed = swingSpeed * (0.5f + weaponCenterOfMass) + extraLinearSpeed;
        float translationalKineticEnergy1 = 0.5f * weaponWeight * centerOfMassSpeed * centerOfMassSpeed;
        float rotationalKineticEnergy1 = 0.5f * weaponInertia * swingSpeed * swingSpeed;
        float totalKineticEnergy1 = translationalKineticEnergy1 + rotationalKineticEnergy1;

        float num = weaponLength * impactPointAsPercent - weaponCenterOfMass; // distance to CM?
        float num6 = (centerOfMassSpeed + swingSpeed * num) / (1f / weaponWeight + num * num / weaponInertia);
        float num7 = centerOfMassSpeed - num6 / weaponWeight;
        float num8 = swingSpeed - num6 * num / weaponInertia;
        float translationalKineticEnergy2 = 0.5f * weaponWeight * num7 * num7;
        float rotationalKineticEnergy2 = 0.5f * weaponInertia * num8 * num8;
        float totalKineticEnergy2 = translationalKineticEnergy2 + rotationalKineticEnergy2;

        float num12 = totalKineticEnergy1 - totalKineticEnergy2 + 0.5f;
        return 0.067f * num12;
    }

    public override float CalculateStrikeMagnitudeForThrust(
        BasicCharacterObject attackerCharacter,
        BasicCharacterObject attackerCaptainCharacter,
        float thrustWeaponSpeed,
        float weaponWeight,
        WeaponComponentData weaponUsageComponent,
        float extraLinearSpeed,
        bool doesAttackerHaveMount,
        bool isThrown = false)
    {
        float speed = doesAttackerHaveMount
            ? thrustWeaponSpeed + (float)Math.Pow(extraLinearSpeed, 0.7f)
            : thrustWeaponSpeed + extraLinearSpeed;
        if (speed <= 0f)
        {
            return 0;
        }

        if (!isThrown) // Copied from native.
        {
            weaponWeight += 2.5f;
        }

        float translationalKineticEnergy = 0.5f * weaponWeight * speed * speed;
        return 0.125f * translationalKineticEnergy;
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
