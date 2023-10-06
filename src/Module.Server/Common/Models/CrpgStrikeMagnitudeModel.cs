using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.Models;

/// <summary>
/// Used to adjust raw dmg calculations.
/// </summary>
internal class CrpgStrikeMagnitudeModel : MultiplayerStrikeMagnitudeModel
{
    /// <summary>
    /// This constants was introduced to decorelate damage from the physics system.
    /// Now damage dealts by a weapon only depends on the blade damage factor and where the blade hit the defender.
    /// </summary>
    public const float BladeDamageFactorToDamageRatio = 10f;

    private readonly CrpgConstants _constants;

    public CrpgStrikeMagnitudeModel(CrpgConstants constants)
    {
        _constants = constants;
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
                result = 0.35f; // Native .1f
                break;
            case DamageTypes.Pierce:
                result = .45f; // Native .25f
                break;
        }

        return result;
    }

    public override float CalculateStrikeMagnitudeForSwing(
        BasicCharacterObject attackerCharacter,
        BasicCharacterObject attackerCaptainCharacter,
        float swingSpeed,
        float impactPoint,
        float weaponWeight,
        WeaponComponentData weaponUsageComponent,
        float weaponLength,
        float weaponInertia,
        float weaponCoM,
        float extraLinearSpeed,
        bool doesAttackerHaveMount)
    {
        float impactPointFactor;
        float swingSpeedPercentage = swingSpeed * 4.5454545f / weaponUsageComponent.SwingSpeed;
        float extraLinearSpeedSign = Math.Sign(extraLinearSpeed);
        float magnitudeBonusFromExtraSpeed = extraLinearSpeedSign * (float)(Math.Pow(extraLinearSpeedSign * extraLinearSpeed / 20f, 0.7f) + Math.Pow(extraLinearSpeed / 22f, 4f));
        switch (weaponUsageComponent.WeaponClass)
        {
            case WeaponClass.OneHandedAxe:
            case WeaponClass.TwoHandedAxe:
            case WeaponClass.Mace:
            case WeaponClass.TwoHandedMace:
            case WeaponClass.OneHandedPolearm:
            case WeaponClass.TwoHandedPolearm:
            case WeaponClass.LowGripPolearm:
                impactPointFactor = (float)Math.Pow(10f, -4f * Math.Pow(impactPoint - 0.93, 2f));
                return BladeDamageFactorToDamageRatio *
                    (0.4f + 0.6f * impactPointFactor) *
                    (float)(Math.Pow(swingSpeedPercentage, 5f) + magnitudeBonusFromExtraSpeed);

            default: // Weapon that do not have a wooden handle
                impactPointFactor = (float)Math.Pow(10f, -4f * Math.Pow(impactPoint - 0.75, 2f));
                return BladeDamageFactorToDamageRatio
                    * (0.8f + 0.2f * impactPointFactor)
                    * (float)(Math.Pow(swingSpeedPercentage, 5f) + magnitudeBonusFromExtraSpeed);
        }
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
        float thrustSpeedPercentage = thrustWeaponSpeed * 11.7647057f / weaponUsageComponent.ThrustSpeed;
        float extraLinearSpeedSign = Math.Sign(extraLinearSpeed);
        float magnitudeBonusFromExtraSpeed = extraLinearSpeedSign * (float)(Math.Pow(extraLinearSpeedSign * extraLinearSpeed / 20f, 0.7f) + Math.Pow(extraLinearSpeed / 22f, 4f));
        switch (weaponUsageComponent.WeaponClass)
        {
            case WeaponClass.OneHandedSword:
            case WeaponClass.Dagger:
                 return
                    BladeDamageFactorToDamageRatio
                    * (float)(Math.Pow(thrustSpeedPercentage, 2f) + magnitudeBonusFromExtraSpeed);
            default:
                 return BladeDamageFactorToDamageRatio *
                    (float)(Math.Pow(thrustSpeedPercentage, 2f) + magnitudeBonusFromExtraSpeed);
        }
    }

    public override float CalculateAdjustedArmorForBlow(
        float baseArmor,
        BasicCharacterObject attackerCharacter,
        BasicCharacterObject attackerCaptainCharacter,
        BasicCharacterObject victimCharacter,
        BasicCharacterObject victimCaptainCharacter,
        WeaponComponentData weaponComponent)
    {
        if (weaponComponent == null)
        {
            return baseArmor;
        }

        return baseArmor * weaponComponent.WeaponClass switch
        {
            WeaponClass.Arrow => 1.2f,
            WeaponClass.Bolt => 0.9f,
            WeaponClass.Stone => 0.95f,
            WeaponClass.Boulder => 0.9f,
            WeaponClass.ThrowingAxe => 1.3f,
            WeaponClass.ThrowingKnife => 1.2f,
            WeaponClass.Javelin => 1.1f,
            _ => 1f,
        };
    }
}
