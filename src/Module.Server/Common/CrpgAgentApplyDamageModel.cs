using Crpg.Module.Helpers;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

internal class CrpgAgentApplyDamageModel : MultiplayerAgentApplyDamageModel
{
    private static readonly HashSet<WeaponClass> WeaponClassesAffectedByPowerStrike = new()
    {
        WeaponClass.Dagger,
        WeaponClass.OneHandedSword,
        WeaponClass.TwoHandedSword,
        WeaponClass.OneHandedAxe,
        WeaponClass.TwoHandedAxe,
        WeaponClass.Mace,
        WeaponClass.Pick,
        WeaponClass.TwoHandedMace,
        WeaponClass.OneHandedPolearm,
        WeaponClass.TwoHandedPolearm,
        WeaponClass.LowGripPolearm,
    };

    private static readonly HashSet<WeaponClass> WeaponClassesAffectedByPowerDraw = new()
    {
        WeaponClass.Bow,
    };

    private static readonly HashSet<WeaponClass> WeaponClassesAffectedByPowerThrow = new()
    {
        WeaponClass.Stone,
        WeaponClass.Boulder,
        WeaponClass.ThrowingAxe,
        WeaponClass.ThrowingKnife,
        WeaponClass.Javelin,
    };

    private readonly CrpgConstants _constants;

    public CrpgAgentApplyDamageModel(CrpgConstants constants)
    {
        _constants = constants;
    }

    public override float CalculateDamage(ref AttackInformation attackInformation,
        ref AttackCollisionData collisionData,
        in MissionWeapon weapon, float baseDamage)
    {
        var attackerCharacter = attackInformation.AttackerAgentCharacter;
        if (attackerCharacter == null)
        {
            return baseDamage;
        }

        WeaponComponentData? weaponComponent = weapon.CurrentUsageItem;
        if (weaponComponent == null) // Basically the guy is using his fists.
        {
            return baseDamage;
        }

        // Because for now cRPG doesn't have its own items but use Native ones without changing the stats, increasing
        // their power with skills make them too powerful. To compensate that we had a penalty of power skills equal
        // to the average a lvl 30 character has. In the future, cRPG should have its own items on which we can control
        // their stats.
        const int averageCharacterStrength = 18;
        const int averagePowerSkills = averageCharacterStrength / 3;

        if (WeaponClassesAffectedByPowerStrike.Contains(weapon.CurrentUsageItem.WeaponClass))
        {
            int powerStrike = attackerCharacter.GetSkillValue(CrpgSkills.PowerStrike) - averagePowerSkills;
            baseDamage *= MathHelper.ApplyPolynomialFunction(powerStrike, _constants.DamageFactorForPowerStrikeCoefs);
        }
        else if (WeaponClassesAffectedByPowerDraw.Contains(weapon.CurrentUsageItem.WeaponClass))
        {
            int powerDraw = attackerCharacter.GetSkillValue(CrpgSkills.PowerDraw) - averagePowerSkills;
            baseDamage *= MathHelper.ApplyPolynomialFunction(powerDraw, _constants.DamageFactorForPowerDrawCoefs);
        }
        else if (WeaponClassesAffectedByPowerThrow.Contains(weapon.CurrentUsageItem.WeaponClass))
        {
            int powerThrow = attackerCharacter.GetSkillValue(CrpgSkills.PowerThrow) - averagePowerSkills;
            baseDamage *= MathHelper.ApplyPolynomialFunction(powerThrow, _constants.DamageFactorForPowerThrowCoefs);
        }

        return baseDamage;
    }
}
