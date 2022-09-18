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

        if (!weapon.IsEmpty)
        {
            // Bonus dmg with spears against horses (does only work with "main" spears - not javelins etc)
            if (!attackInformation.IsVictimAgentHuman && !attackInformation.DoesAttackerHaveMountAgent && weapon.CurrentUsageItem.IsPolearm && !weapon.CurrentUsageItem.IsConsumable && weapon.CurrentUsageItem.IsMeleeWeapon && collisionData.StrikeType == (int)StrikeType.Thrust && collisionData.DamageType == (int)DamageTypes.Pierce && !weapon.GetConsumableIfAny(out var consumableWeapon))
            {
                finalDamage *= 1.75f; // 75% bonus dmg against horses
            }

            // For bashes (with and without shield) - Not for allies cause teamdmg might reduce the "finalDamage" below zero. That will break teamhits with bashes.
            else if (collisionData.IsAlternativeAttack && !attackInformation.IsFriendlyFire)
            {
                finalDamage = 1f;
            }
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
