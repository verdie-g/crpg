using Crpg.Module.Helpers;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.Models;

/// <summary>
/// Used to adjust dmg calculations.
/// </summary>
internal class CrpgAgentApplyDamageModel : MultiplayerAgentApplyDamageModel
{
    private readonly CrpgConstants _constants;

    public CrpgAgentApplyDamageModel(CrpgConstants constants)
    {
        _constants = constants;
    }

    public override float CalculateDamage(
        in AttackInformation attackInformation,
        in AttackCollisionData collisionData,
        in MissionWeapon weapon,
        float baseDamage)
    {
        List<WeaponClass> meleeClass = new()
        {
            WeaponClass.Dagger,
            WeaponClass.Mace,
            WeaponClass.TwoHandedMace,
            WeaponClass.OneHandedSword,
            WeaponClass.TwoHandedSword,
            WeaponClass.OneHandedAxe,
            WeaponClass.TwoHandedAxe,
            WeaponClass.Pick,
            WeaponClass.LowGripPolearm,
            WeaponClass.OneHandedPolearm,
            WeaponClass.TwoHandedPolearm,
        };
        List<WeaponClass> swordClass = new()
        {
            WeaponClass.Dagger,
            WeaponClass.OneHandedSword,
            WeaponClass.TwoHandedSword,
        };
        float finalDamage = base.CalculateDamage(attackInformation, collisionData, weapon, baseDamage);
        if (weapon.IsEmpty)
        {
            // Increase fist damage with strength.
            int strengthSkill = GetSkillValue(attackInformation.AttackerAgentOrigin, CrpgSkills.Strength);
            return finalDamage * (1 + 0.03f * strengthSkill);
        }

        // CalculateShieldDamage only has dmg as parameter. Therefore it cannot be used to get any Skill values.
        if (collisionData.AttackBlockedWithShield && finalDamage > 0)
        {
            int shieldSkill = GetSkillValue(attackInformation.VictimAgentOrigin, CrpgSkills.Shield);
            finalDamage /= MathHelper.RecursivePolynomialFunctionOfDegree2(shieldSkill, _constants.DurabilityFactorForShieldRecursiveCoefs);
            if (meleeClass.Contains(weapon.CurrentUsageItem.WeaponClass))
            {
                // in bannerlord/Src/TaleWorlds.MountAndBlade/MissionCombatMechanicsHelper.cs/GetAttackCollisionResults()
                // ComputeBlowDamageOnShield is fed the basemagnitude from ComputeBlowMagnitude() instead of the specialmagnitude
                // specialmagnitude takes in account the damagefactor which is the bladesharpness.
                //  specialmagnitude is the damage you deal to agents , while basemagnitude is the blow from strikemagnitudemodel
                //  basemagnitude only takes in account both sweetspots and speedbonus , but not the damage multiplicator that each weapon have
                finalDamage *=
                    collisionData.StrikeType == (int)StrikeType.Thrust
                        ? weapon.CurrentUsageItem.ThrustDamageFactor
                        : weapon.CurrentUsageItem.SwingDamageFactor;

                if (weapon.CurrentUsageItem.WeaponFlags.HasAnyFlag(WeaponFlags.BonusAgainstShield))
                {
                    // this bonus is on top of the native x2 in MissionCombatMechanicsHelper
                    // so the final bonus is 3.5 for axes and 3 for swords. We do this instead of nerfing the impact of shield skill so shield can stay virtually unbreakable against sword.
                    // it is the same logic as arrows not dealing a lot of damage to horse but spears dealing extra damage to horses
                    // As we want archer to fear cavs and cavs to fear spears, we want swords to fear shielders and shielders to fear axes.

                    finalDamage *= swordClass.Contains(weapon.CurrentUsageItem.WeaponClass) ? 1.5f : 1.75f;
                }
            }
        }

        // We want to decrease survivability of horses against melee weapon and especially against spears and pikes.
        // By doing that we ensure that cavalry stays an archer predator while punishing cav errors like running into a wall or an obstacle
        if (!attackInformation.IsVictimAgentHuman
            && !attackInformation.DoesAttackerHaveMountAgent
            && !weapon.CurrentUsageItem.IsConsumable
            && weapon.CurrentUsageItem.IsMeleeWeapon
            && !weapon.IsAnyConsumable())
        {
            if (
                collisionData.StrikeType == (int)StrikeType.Thrust
                && collisionData.DamageType == (int)DamageTypes.Pierce
                && weapon.CurrentUsageItem.IsPolearm)
            {
                finalDamage *= 1.85f;
            }
            else
            {
                finalDamage *= 1.4f;
            }
        }

        // For bashes (with and without shield) - Not for allies cause teamdmg might reduce the "finalDamage" below zero. That will break teamhits with bashes.
        else if (collisionData.IsAlternativeAttack && !attackInformation.IsFriendlyFire)
        {
            finalDamage = 1f;
        }

        if (attackInformation.DoesAttackerHaveMountAgent && attackInformation.IsAttackerAgentDoingPassiveAttack)
        {
            finalDamage *= 0.23f; // Decrease damage from couched lance.
        }

        return finalDamage;
    }

    public override float GetDamageMultiplierForBodyPart(BoneBodyPartType bodyPart, DamageTypes type, bool isHuman)
    {
        float result = 1f;
        switch (bodyPart)
        {
            case BoneBodyPartType.None:
                result = 1f;
                break;
            case BoneBodyPartType.Head:
                switch (type)
                {
                    case DamageTypes.Invalid:
                        result = 2f;
                        break;
                    case DamageTypes.Cut:
                        result = 1.2f;
                        break;
                    case DamageTypes.Pierce:
                        result = !isHuman ? 1.2f : 1.6f;
                        break;
                    case DamageTypes.Blunt:
                        result = 1.2f;
                        break;
                }

                break;
            case BoneBodyPartType.Neck:
                switch (type)
                {
                    case DamageTypes.Invalid:
                        result = 2f;
                        break;
                    case DamageTypes.Cut:
                        result = 1.2f;
                        break;
                    case DamageTypes.Pierce:
                        result = !isHuman ? 1.2f : 1.6f;
                        break;
                    case DamageTypes.Blunt:
                        result = 1.2f;
                        break;
                }

                break;
            case BoneBodyPartType.Chest:
            case BoneBodyPartType.Abdomen:
            case BoneBodyPartType.ShoulderLeft:
            case BoneBodyPartType.ShoulderRight:
            case BoneBodyPartType.ArmLeft:
            case BoneBodyPartType.ArmRight:
                result = !isHuman ? 0.8f : 1f;
                break;
            case BoneBodyPartType.Legs:
                result = 0.8f;
                break;
        }

        return result;
    }

    public override void CalculateCollisionStunMultipliers(
        Agent attackerAgent,
        Agent defenderAgent,
        bool isAlternativeAttack,
        CombatCollisionResult collisionResult,
        WeaponComponentData attackerWeapon,
        WeaponComponentData defenderWeapon,
        out float attackerStunMultiplier,
        out float defenderStunMultiplier)
    {
        attackerStunMultiplier = 1f;
        if (collisionResult == CombatCollisionResult.Blocked && defenderAgent.WieldedOffhandWeapon.IsShield())
        {
            int shieldSkill = 0;
            if (defenderAgent.Origin is CrpgBattleAgentOrigin crpgOrigin)
            {
                shieldSkill = crpgOrigin.Skills.GetPropertyValue(CrpgSkills.Shield);
            }

            defenderStunMultiplier = 1 / MathHelper.RecursivePolynomialFunctionOfDegree2(shieldSkill, _constants.ShieldDefendStunMultiplierForSkillRecursiveCoefs);

            return;
        }

        defenderStunMultiplier = 1f;
    }

    // TODO : Consider reworking once https://forums.taleworlds.com/index.php?threads/missioncombatmechanicshelper-getdefendcollisionresults-bypass-strikemagnitudecalculationmodel.459379 is fixed
    public override bool DecideCrushedThrough(
        Agent attackerAgent,
        Agent defenderAgent,
        float totalAttackEnergy,
        Agent.UsageDirection attackDirection,
        StrikeType strikeType,
        WeaponComponentData defendItem,
        bool isPassiveUsage)
    {
        EquipmentIndex wieldedItemIndex = attackerAgent.GetWieldedItemIndex(Agent.HandIndex.OffHand);
        if (wieldedItemIndex == EquipmentIndex.None)
        {
            wieldedItemIndex = attackerAgent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
        }

        var weaponComponentData = wieldedItemIndex != EquipmentIndex.None
            ? attackerAgent.Equipment[wieldedItemIndex].CurrentUsageItem
            : null;
        if (weaponComponentData == null
            || isPassiveUsage
            || !weaponComponentData.WeaponFlags.HasAnyFlag(WeaponFlags.CanCrushThrough)
            || strikeType != StrikeType.Swing
            || attackDirection != Agent.UsageDirection.AttackUp)
        {
            return false;
        }

        float attackerPower = 3f * GetSkillValue(attackerAgent.Origin, CrpgSkills.PowerStrike);

        float defenderStrengthSkill = GetSkillValue(defenderAgent.Origin, CrpgSkills.Strength);
        float defenderShieldSkill = GetSkillValue(defenderAgent.Origin, CrpgSkills.Shield);
        float defenderDefendPower = defendItem != null && defendItem.IsShield
            ? Math.Max(defenderShieldSkill * 6 + 3, defenderStrengthSkill)
            : defenderStrengthSkill;
        int randomNumber = MBRandom.RandomInt(0, 1000);
        return randomNumber / 10f < Math.Pow(attackerPower / defenderDefendPower / 2.5f, 1.8f) * 100f;
    }

    private int GetSkillValue(IAgentOriginBase agentOrigin, SkillObject skill)
    {
        if (agentOrigin is CrpgBattleAgentOrigin crpgOrigin)
        {
            return crpgOrigin.Skills.GetPropertyValue(skill);
        }

        return 0;
    }
}
