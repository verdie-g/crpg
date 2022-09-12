using Crpg.Module.Helpers;
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
        float finalDamage = baseDamage;
        if (collisionData.AttackBlockedWithShield)
        {
            int shieldSkill = 0;
            if (attackInformation.VictimAgentOrigin is CrpgBattleAgentOrigin crpgOrigin)
            {
                shieldSkill = crpgOrigin.Skills.GetPropertyValue(CrpgSkills.Shield);
            }

            finalDamage /= MathHelper.ApplyPolynomialFunction(shieldSkill, _constants.DurabilityFactorForShieldCoefs);
        }

        return finalDamage;
    }
}
