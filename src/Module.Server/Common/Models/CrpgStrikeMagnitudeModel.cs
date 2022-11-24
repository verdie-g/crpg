using TaleWorlds.Core;
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

    public override float CalculateStrikeMagnitudeForSwing(BasicCharacterObject attackerCharacter, BasicCharacterObject attackerCaptainCharacter, float swingSpeed, float impactPoint, float weaponWeight, WeaponComponentData weaponUsageComponent, float weaponLength, float weaponInertia, float weaponCoM, float extraLinearSpeed, bool doesAttackerHaveMount)
    {
        float impactPointFactor;
        if (weaponUsageComponent.WeaponClass == WeaponClass.OneHandedAxe
        || weaponUsageComponent.WeaponClass == WeaponClass.TwoHandedAxe
        || weaponUsageComponent.WeaponClass == WeaponClass.Mace
        || weaponUsageComponent.WeaponClass == WeaponClass.TwoHandedMace
        || weaponUsageComponent.WeaponClass == WeaponClass.OneHandedPolearm
        || weaponUsageComponent.WeaponClass == WeaponClass.TwoHandedPolearm
        || weaponUsageComponent.WeaponClass == WeaponClass.LowGripPolearm)
        {
            impactPointFactor = (float)Math.Pow(10f, -4f * Math.Pow(impactPoint - 0.93, 2f));
            Console.WriteLine();
            return 10f * (0.4f + 0.6f * impactPointFactor) * (1f + extraLinearSpeed / 15f);
        }
        else
        {
            impactPointFactor = (float)Math.Pow(10f, -4f * Math.Pow(impactPoint - 0.75, 2f));
            Console.WriteLine("others");
            return 10f * (0.8f + 0.2f * impactPointFactor) * (1f + extraLinearSpeed / 15f);
            ;
        }
    }
    public override float CalculateStrikeMagnitudeForThrust(BasicCharacterObject attackerCharacter, BasicCharacterObject attackerCaptainCharacter, float thrustWeaponSpeed, float weaponWeight, WeaponComponentData weaponUsageComponent, float extraLinearSpeed, bool doesAttackerHaveMount, bool isThrown = false)
    {
        Console.WriteLine("thrust");
        return 10f * (1f + extraLinearSpeed / 15f);
    }
}
