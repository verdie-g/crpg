using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using Crpg.Module.Common.Network;

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
                return BladeDamageFactorToDamageRatio * (0.4f + 0.6f * impactPointFactor) * (1f * (float)Math.Pow(swingSpeedPercentage, 5f) + extraLinearSpeed / 10f);

            default: // Weapon that do not have a wooden handle
                impactPointFactor = (float)Math.Pow(10f, -4f * Math.Pow(impactPoint - 0.75, 2f));
                return BladeDamageFactorToDamageRatio * (0.8f + 0.2f * impactPointFactor) * (1f * (float)Math.Pow(swingSpeedPercentage, 5f) + extraLinearSpeed / 10f);

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
        return BladeDamageFactorToDamageRatio * (1f * (float)Math.Pow(thrustSpeedPercentage, 8f) + extraLinearSpeed / 10f);

    }

    public void ServerSendServerMessageToEveryone(Color color, string message)
    {
        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new CrpgServerMessage
        {
            Message = message,
            Red = color.Red,
            Green = color.Green,
            Blue = color.Blue,
            Alpha = color.Alpha,
            IsMessageTextId = false,
        });
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.IncludeUnsynchronizedClients);
    }
}
