using Crpg.GameMod.Helpers;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crpg.GameMod.Common;

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

        if (WeaponClassesAffectedByPowerStrike.Contains(weapon.CurrentUsageItem.WeaponClass))
        {
            int powerStrike = attackerCharacter.GetSkillValue(CrpgSkills.PowerStrike);
            baseDamage *= MathHelper.ApplyPolynomialFunction(powerStrike, _constants.DamageFactorForPowerStrikeCoefs);
        }
        else if (WeaponClassesAffectedByPowerDraw.Contains(weapon.CurrentUsageItem.WeaponClass))
        {
            int powerDraw = attackerCharacter.GetSkillValue(CrpgSkills.PowerDraw);
            baseDamage *= MathHelper.ApplyPolynomialFunction(powerDraw, _constants.DamageFactorForPowerDrawCoefs);
        }
        else if (WeaponClassesAffectedByPowerThrow.Contains(weapon.CurrentUsageItem.WeaponClass))
        {
            int powerThrow = attackerCharacter.GetSkillValue(CrpgSkills.PowerThrow);
            baseDamage *= MathHelper.ApplyPolynomialFunction(powerThrow, _constants.DamageFactorForPowerThrowCoefs);
        }

        return baseDamage;
    }
}
