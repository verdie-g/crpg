﻿using Crpg.Module.Helpers;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.Models;

/// <summary>
/// Mostly copied from <see cref="MultiplayerAgentStatCalculateModel"/> but with the class division system removed.
/// </summary>
internal class CrpgAgentStatCalculateModel : AgentStatCalculateModel
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
        WeaponClass.Arrow,
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

    public CrpgAgentStatCalculateModel(CrpgConstants constants)
    {
        _constants = constants;
    }

    public override void InitializeAgentStats(
       Agent agent,
       Equipment spawnEquipment,
       AgentDrivenProperties agentDrivenProperties,
       AgentBuildData agentBuildData)
    {
        agentDrivenProperties.ArmorEncumbrance = spawnEquipment.GetTotalWeightOfArmor(agent.IsHuman);
        if (agent.IsHuman)
        {
            InitializeHumanAgentStats(agent, spawnEquipment, agentDrivenProperties);
        }
        else
        {
            InitializeMountAgentStats(agent, spawnEquipment, agentDrivenProperties);
        }
    }

    public override void UpdateAgentStats(Agent agent, AgentDrivenProperties agentDrivenProperties)
    {
        if (agent.IsHuman)
        {
            UpdateHumanAgentStats(agent, agentDrivenProperties);
        }
        else if (agent.IsMount)
        {
            UpdateMountAgentStats(agent, agentDrivenProperties);
        }
    }

    /// <summary>AI difficulty.</summary>
    public override float GetDifficultyModifier()
    {
        return 0.5f; // Same value as MultiplayerAgentStatCalculateModel.
    }

    public override bool CanAgentRideMount(Agent agent, Agent targetMount)
    {
        // TODO: check riding skills?
        return true;
    }

    public override float GetWeaponDamageMultiplier(BasicCharacterObject character, IAgentOriginBase agentOrigin,
        Formation agentFormation, WeaponComponentData weaponComponent)
    {
        if (WeaponClassesAffectedByPowerStrike.Contains(weaponComponent.WeaponClass))
        {
            int powerStrike = GetEffectiveSkill(character, agentOrigin, agentFormation, CrpgSkills.PowerStrike);
            return MathHelper.ApplyPolynomialFunction(powerStrike, _constants.DamageFactorForPowerStrikeCoefs);
        }

        if (WeaponClassesAffectedByPowerDraw.Contains(weaponComponent.WeaponClass))
        {
            int powerDraw = GetEffectiveSkill(character, agentOrigin, agentFormation, CrpgSkills.PowerDraw);
            return MathHelper.ApplyPolynomialFunction(powerDraw, _constants.DamageFactorForPowerDrawCoefs);
        }

        if (WeaponClassesAffectedByPowerThrow.Contains(weaponComponent.WeaponClass))
        {
            int powerThrow = GetEffectiveSkill(character, agentOrigin, agentFormation, CrpgSkills.PowerThrow);
            return MathHelper.ApplyPolynomialFunction(powerThrow, _constants.DamageFactorForPowerThrowCoefs);
        }

        return 1;
    }

    public override float GetKnockBackResistance(Agent agent)
    {
        return 0.25f;
    }

    public override float GetKnockDownResistance(Agent agent, StrikeType strikeType = StrikeType.Invalid)
    {
        float knockDownResistance = 0.5f;
        if (agent.HasMount)
        {
            knockDownResistance += 0.1f;
        }
        else if (strikeType == StrikeType.Thrust)
        {
            knockDownResistance += 0.25f;
        }

        return knockDownResistance;
    }

    public override float GetDismountResistance(Agent agent)
    {
        // TODO: depends on riding skills?
        return 0.5f;
    }

    private void InitializeHumanAgentStats(Agent agent, Equipment equipment, AgentDrivenProperties props)
    {
        props.SetStat(DrivenProperty.UseRealisticBlocking, MultiplayerOptions.OptionType.UseRealisticBlocking.GetBoolValue() ? 1f : 0.0f);
        props.ArmorHead = equipment.GetHeadArmorSum();
        props.ArmorTorso = equipment.GetHumanBodyArmorSum();
        props.ArmorLegs = equipment.GetLegArmorSum();
        props.ArmorArms = equipment.GetArmArmorSum();
        props.TopSpeedReachDuration = 2.3f; // Acceleration. TODO: should probably not be a constant.
        float bipedalCombatSpeedMinMultiplier = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.BipedalCombatSpeedMinMultiplier);
        float bipedalCombatSpeedMaxMultiplier = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.BipedalCombatSpeedMaxMultiplier);
        const float combatMovementSpeed = 0.8f; // TODO: should probably not be a constant.
        props.CombatMaxSpeedMultiplier = bipedalCombatSpeedMinMultiplier + (bipedalCombatSpeedMaxMultiplier - bipedalCombatSpeedMinMultiplier) * combatMovementSpeed;

        agent.BaseHealthLimit = 100f; // TODO: get character health.
        agent.HealthLimit = agent.BaseHealthLimit;
        agent.Health = agent.HealthLimit;
    }

    private void InitializeMountAgentStats(Agent agent, Equipment equipment, AgentDrivenProperties props)
    {
        EquipmentElement mount = equipment[EquipmentIndex.Horse];
        EquipmentElement mountHarness = equipment[EquipmentIndex.HorseHarness];

        props.AiSpeciesIndex = agent.Monster.FamilyType;
        props.AttributeRiding = 0.8f + (mountHarness.Item != null ? 0.2f : 0.0f);
        props.ArmorTorso = mountHarness.Item != null ? mountHarness.GetModifiedMountBodyArmor() : 0;
        props.MountChargeDamage = mount.GetModifiedMountCharge(in mountHarness) * 0.01f;
        props.MountDifficulty = mount.Item.Difficulty;
    }

    private void UpdateMountAgentStats(Agent agent, AgentDrivenProperties props)
    {
        EquipmentElement mount = agent.SpawnEquipment[EquipmentIndex.ArmorItemEndSlot];
        EquipmentElement mountHarness = agent.SpawnEquipment[EquipmentIndex.HorseHarness];
        int ridingSkill = agent.RiderAgent != null
            ? GetEffectiveSkill(agent.RiderAgent.Character, agent.RiderAgent.Origin, agent.RiderAgent.Formation, DefaultSkills.Riding)
            : 100;
        props.MountManeuver = mount.GetModifiedMountManeuver(in mountHarness) * (1.0f + ridingSkill * 0.0035f);
        props.MountSpeed = (mount.GetModifiedMountSpeed(in mountHarness) + 1) * 0.22f * (1.0f + ridingSkill * 0.0032f);
        props.TopSpeedReachDuration = Game.Current.BasicModels.RidingModel.CalculateAcceleration(in mount, in mountHarness, ridingSkill);
        float weightFactor = mount.Weight / 2.0f + (mountHarness.IsEmpty ? 0.0f : mountHarness.Weight);
        props.MountDashAccelerationMultiplier = weightFactor > 200.0
            ? weightFactor < 300.0 ? 1.0f - (weightFactor - 200.0f) / 111.0f : 0.1f
            : 1f;
    }

    private void UpdateHumanAgentStats(Agent agent, AgentDrivenProperties props)
    {
        BasicCharacterObject character = agent.Character;
        MissionEquipment equipment = agent.Equipment;
        float weaponsEncumbrance = equipment.GetTotalWeightOfWeapons();
        EquipmentIndex wieldedItemIndex1 = agent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
        EquipmentIndex wieldedItemIndex2 = agent.GetWieldedItemIndex(Agent.HandIndex.OffHand);
        if (wieldedItemIndex1 != EquipmentIndex.None)
        {
            ItemObject itemObject = equipment[wieldedItemIndex1].Item;
            WeaponComponent weaponComponent = itemObject.WeaponComponent;
            float realWeaponLength = weaponComponent.PrimaryWeapon.GetRealWeaponLength();
            weaponsEncumbrance += (weaponComponent.GetItemType() == ItemObject.ItemTypeEnum.Bow ? 4f : 1.5f)
                                  * itemObject.Weight * (float)Math.Sqrt(realWeaponLength);
        }

        if (wieldedItemIndex2 != EquipmentIndex.None)
        {
            weaponsEncumbrance += 1.5f * equipment[wieldedItemIndex2].Item.Weight;
        }

        props.WeaponsEncumbrance = weaponsEncumbrance;
        EquipmentIndex wieldedItemIndex3 = agent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
        WeaponComponentData? equippedItem = wieldedItemIndex3 != EquipmentIndex.None
            ? equipment[wieldedItemIndex3].CurrentUsageItem
            : null;
        ItemObject? primaryItem = wieldedItemIndex3 != EquipmentIndex.None
            ? equipment[wieldedItemIndex3].Item
            : null;
        EquipmentIndex wieldedItemIndex4 = agent.GetWieldedItemIndex(Agent.HandIndex.OffHand);
        WeaponComponentData? secondaryItem = wieldedItemIndex4 != EquipmentIndex.None
            ? equipment[wieldedItemIndex4].CurrentUsageItem
            : null;
        int itemSkill = GetEffectiveSkill(character, agent.Origin, agent.Formation, primaryItem?.RelevantSkill ?? DefaultSkills.Athletics);
        props.SwingSpeedMultiplier = 0.93f + 0.0007f * itemSkill;
        props.ThrustOrRangedReadySpeedMultiplier = props.SwingSpeedMultiplier;
        props.HandlingMultiplier = 1f;
        props.ShieldBashStunDurationMultiplier = 1f;
        props.KickStunDurationMultiplier = 1f;
        props.ReloadSpeed = props.SwingSpeedMultiplier;
        props.MissileSpeedMultiplier = 1f;
        props.ReloadMovementPenaltyFactor = 1f;
        SetAllWeaponInaccuracy(agent, props, (int)wieldedItemIndex3, equippedItem);
        const float movementSpeed = 0.8f; // TODO: should probably not be a constant.
        props.MaxSpeedMultiplier = 1.05f * movementSpeed * (100.0f / (100.0f + weaponsEncumbrance));
        int ridingSkill = GetEffectiveSkill(character, agent.Origin, agent.Formation, DefaultSkills.Riding);
        if (equippedItem != null)
        {
            int weaponSkill = GetEffectiveSkillForWeapon(agent, equippedItem);
            props.WeaponInaccuracy = GetWeaponInaccuracy(agent, equippedItem, weaponSkill);
            if (equippedItem.IsRangedWeapon)
            {
                if (!agent.HasMount)
                {
                    float num5 = Math.Max(0.0f, 1.0f - weaponSkill / 500.0f);
                    props.WeaponMaxMovementAccuracyPenalty = 0.125f * num5;
                    props.WeaponMaxUnsteadyAccuracyPenalty = 0.1f * num5;
                }
                else
                {
                    float num6 = Math.Max(0.0f, (1.0f - weaponSkill / 500.0f) * (1.0f - ridingSkill / 1800.0f));
                    props.WeaponMaxMovementAccuracyPenalty = 0.025f * num6;
                    props.WeaponMaxUnsteadyAccuracyPenalty = 0.06f * num6;
                }

                props.WeaponMaxMovementAccuracyPenalty = Math.Max(0.0f, props.WeaponMaxMovementAccuracyPenalty);
                props.WeaponMaxUnsteadyAccuracyPenalty = Math.Max(0.0f, props.WeaponMaxUnsteadyAccuracyPenalty);
                if (equippedItem.RelevantSkill == DefaultSkills.Bow)
                {
                    float amount = MBMath.ClampFloat((equippedItem.ThrustSpeed - 60.0f) / 75.0f, 0.0f, 1f);
                    props.WeaponMaxMovementAccuracyPenalty *= 6f;
                    props.WeaponMaxUnsteadyAccuracyPenalty *= 4.5f / MBMath.Lerp(0.75f, 2f, amount);
                }
                else if (equippedItem.RelevantSkill == DefaultSkills.Throwing)
                {
                    float amount = MBMath.ClampFloat((equippedItem.ThrustSpeed - 89.0f) / 13.0f, 0.0f, 1f);
                    props.WeaponMaxUnsteadyAccuracyPenalty *= 3.5f * MBMath.Lerp(1.5f, 0.8f, amount);
                }
                else if (equippedItem.RelevantSkill == DefaultSkills.Crossbow)
                {
                    props.WeaponMaxMovementAccuracyPenalty *= 2.5f;
                    props.WeaponMaxUnsteadyAccuracyPenalty *= 1.2f;
                }

                if (equippedItem.WeaponClass == WeaponClass.Bow)
                {
                    props.WeaponBestAccuracyWaitTime = 0.3f + (95.75f - equippedItem.ThrustSpeed) * 0.005f;
                    float amount = MBMath.ClampFloat((equippedItem.ThrustSpeed - 60.0f) / 75.0f, 0.0f, 1f);
                    props.WeaponUnsteadyBeginTime = 0.1f + weaponSkill * 0.001f * MBMath.Lerp(1f, 2f, amount);
                    if (agent.IsAIControlled)
                    {
                        props.WeaponUnsteadyBeginTime *= 4f;
                    }

                    props.WeaponUnsteadyEndTime = 2f + props.WeaponUnsteadyBeginTime;
                    props.WeaponRotationalAccuracyPenaltyInRadians = 0.1f;
                }
                else if (equippedItem.WeaponClass is WeaponClass.Javelin or WeaponClass.ThrowingAxe or WeaponClass.ThrowingKnife)
                {
                    props.WeaponBestAccuracyWaitTime = 0.4f + (89.0f - equippedItem.ThrustSpeed) * 0.03f;
                    props.WeaponUnsteadyBeginTime = 2.5f + weaponSkill * 0.01f;
                    props.WeaponUnsteadyEndTime = 10f + props.WeaponUnsteadyBeginTime;
                    props.WeaponRotationalAccuracyPenaltyInRadians = 0.025f;
                    if (equippedItem.WeaponClass == WeaponClass.ThrowingAxe)
                    {
                        props.WeaponInaccuracy *= 6.6f;
                    }
                }
                else
                {
                    props.WeaponBestAccuracyWaitTime = 0.1f;
                    props.WeaponUnsteadyBeginTime = 0.0f;
                    props.WeaponUnsteadyEndTime = 0.0f;
                    props.WeaponRotationalAccuracyPenaltyInRadians = 0.1f;
                }
            }
            else if (equippedItem.WeaponFlags.HasAllFlags(WeaponFlags.WideGrip))
            {
                props.WeaponUnsteadyBeginTime = 1.0f + weaponSkill * 0.005f;
                props.WeaponUnsteadyEndTime = 3.0f + weaponSkill * 0.01f;
            }
        }

        int shieldSkill = GetEffectiveSkill(character, agent.Origin, agent.Formation, CrpgSkills.Shield);
        props.AttributeShieldMissileCollisionBodySizeAdder = MathHelper.ApplyPolynomialFunction(shieldSkill, _constants.CoverageFactorForShieldCoefs);
        float ridingAttribute = agent.MountAgent?.GetAgentDrivenPropertyValue(DrivenProperty.AttributeRiding) ?? 1f;
        props.AttributeRiding = ridingSkill * ridingAttribute;
        // TODO: AttributeHorseArchery doesn't seem to have any effect for now.
        props.AttributeHorseArchery = Game.Current.BasicModels.StrikeMagnitudeModel.CalculateHorseArcheryFactor(character);
        props.BipedalRangedReadySpeedMultiplier = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.BipedalRangedReadySpeedMultiplier);
        props.BipedalRangedReloadSpeedMultiplier = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.BipedalRangedReloadSpeedMultiplier);

        SetAiRelatedProperties(agent, props, equippedItem, secondaryItem);
    }
}
