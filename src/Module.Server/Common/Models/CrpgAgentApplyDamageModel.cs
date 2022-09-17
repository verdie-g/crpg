using Crpg.Module.Helpers;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.Models;

/// <summary>
/// Used to adjust dmg calculations.
/// </summary>
internal class CrpgAgentApplyDamageModel : DefaultAgentApplyDamageModel
{
    private readonly CrpgConstants _constants;

    public CrpgAgentApplyDamageModel(CrpgConstants constants)
    {
        _constants = constants;
    }

    // public override float CalculateShieldDamage only has dmg as parameter. Therefore it cannot be used to get any Skill values.
    public override float CalculateDamage(in AttackInformation attackInformation, in AttackCollisionData collisionData, in MissionWeapon weapon, float baseDamage)
    {
        float finalDamage = base.CalculateDamage(attackInformation, collisionData, weapon, baseDamage);
        if (collisionData.AttackBlockedWithShield)
        {
            int shieldSkill = 0;
            if (attackInformation.VictimAgentOrigin is CrpgBattleAgentOrigin crpgOrigin)
            {
                shieldSkill = crpgOrigin.Skills.GetPropertyValue(CrpgSkills.Shield);
            }

            finalDamage /= MathHelper.ApplyPolynomialFunction(shieldSkill, _constants.DurabilityFactorForShieldCoefs);
        }

        // For bashes (with and without shield) - Not for allies cause teamdmg might reduce the "finalDamage" below zero. That will break teamhits with bashes.
        if (collisionData.IsAlternativeAttack && !weapon.IsEmpty && !attackInformation.IsFriendlyFire)
        {
            finalDamage = 1f;
        }

        return finalDamage;
    }

    public override void CalculateCollisionStunMultipliers(Agent attackerAgent, Agent defenderAgent, bool isAlternativeAttack, CombatCollisionResult collisionResult, WeaponComponentData attackerWeapon, WeaponComponentData defenderWeapon, out float attackerStunMultiplier, out float defenderStunMultiplier)
    {
        attackerStunMultiplier = 1f;
        if (collisionResult == CombatCollisionResult.Blocked && defenderAgent.WieldedOffhandWeapon.IsShield())
        {
            int shieldSkill = 0;
            if (defenderAgent.Origin is CrpgBattleAgentOrigin crpgOrigin)
            {
                shieldSkill = crpgOrigin.Skills.GetPropertyValue(CrpgSkills.Shield);
            }

            if (shieldSkill > _constants.ShieldDefendStunMultiplierForSkill.Length)
            {
                shieldSkill = _constants.ShieldDefendStunMultiplierForSkill.Length - 1;
            }

            defenderStunMultiplier = _constants.ShieldDefendStunMultiplierForSkill[shieldSkill];

            return;
        }

        defenderStunMultiplier = 1f;
    }

}
