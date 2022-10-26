using Crpg.Module.Battle;
using Crpg.Module.Helpers;
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
    private readonly CrpgItemRequirementModel reqModel;

    public CrpgAgentStatCalculateModel(CrpgConstants constants)
    {
        _constants = constants;
        reqModel = new CrpgItemRequirementModel(_constants);
    }

    public override int GetEffectiveSkill(
        BasicCharacterObject agentCharacter,
        IAgentOriginBase agentOrigin,
        Formation agentFormation,
        SkillObject skill)
    {
        // Current (1.8.0), only predefined characters can be spawned so we can't use the character to carry the crpg
        // skills. To hack around that the skills are carried by a custom implementation of the IAgentOriginBase.
        if (agentOrigin is CrpgBattleAgentOrigin crpgOrigin)
        {
            return crpgOrigin.Skills.GetPropertyValue(skill);
        }

        return base.GetEffectiveSkill(agentCharacter, agentOrigin, agentFormation, skill);
    }

    public override float GetWeaponInaccuracy(
        Agent agent,
        WeaponComponentData weapon,
        int weaponSkill)
    {
        float inaccuracy = 0.0f;
        float skillComponentMultiplier = 1f;
        float weaponClassMultiplier = weapon.WeaponClass switch
        {
            WeaponClass.Bow => 1.4f,
            WeaponClass.Crossbow => 0.5f,
            WeaponClass.Stone => 1f,
            WeaponClass.ThrowingAxe => 1.5f,
            WeaponClass.ThrowingKnife => 1.5f,
            WeaponClass.Javelin => 1.5f,
            _ => 1f,
        };

        if (weapon.IsRangedWeapon)
        {
            float weaponComponent = 0.1f / ((float)Math.Pow(weapon.Accuracy / 100f, 5f));
            float skillComponent = skillComponentMultiplier * 1000000f / (1000000f + 0.01f * (float)Math.Pow(weaponSkill, 4));
            inaccuracy = weaponComponent * skillComponent;
            inaccuracy *= weaponClassMultiplier;
        }
        else if (weapon.WeaponFlags.HasAllFlags(WeaponFlags.WideGrip))
        {
            inaccuracy = 1.0f - weaponSkill * 0.01f;
        }

        return MathF.Max(inaccuracy, 0.0f);
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
            // For human players, this is now done once in UpdateAgentStats (part of the big characteristic hack).
            if (agent.IsAIControlled)
            {
                InitializeHumanAgentStats(agent, spawnEquipment, agentDrivenProperties);
            }
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
        return 0.25f; // Same value as MultiplayerAgentStatCalculateModel.
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
        int ridingSkills = GetEffectiveSkill(agent.Character, agent.Origin, agent.Formation, DefaultSkills.Riding);
        return 0.0025f * ridingSkills;
    }

    private void InitializeHumanAgentStats(Agent agent, Equipment equipment, AgentDrivenProperties props)
    {
        props.SetStat(DrivenProperty.UseRealisticBlocking, MultiplayerOptions.OptionType.UseRealisticBlocking.GetBoolValue() ? 1f : 0.0f);
        props.ArmorHead = equipment.GetHeadArmorSum();
        props.ArmorTorso = equipment.GetHumanBodyArmorSum();
        props.ArmorLegs = equipment.GetLegArmorSum();
        props.ArmorArms = equipment.GetArmArmorSum();

        int strengthAttribute = GetEffectiveSkill(agent.Character, agent.Origin, agent.Formation, CrpgSkills.Strength);
        int ironFleshSkill = GetEffectiveSkill(agent.Character, agent.Origin, agent.Formation, CrpgSkills.IronFlesh);
        agent.BaseHealthLimit = (int)(_constants.DefaultHealthPoints
                                      + MathHelper.ApplyPolynomialFunction(strengthAttribute, _constants.HealthPointsForStrengthCoefs)
                                      + MathHelper.ApplyPolynomialFunction(ironFleshSkill, _constants.HealthPointsForIronFleshCoefs));
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
        props.MountManeuver = mount.GetModifiedMountManeuver(in mountHarness) * (0.5f + ridingSkill * 0.0025f);
        props.MountSpeed = (mount.GetModifiedMountSpeed(in mountHarness) + 1) * 0.22f * (1.0f + ridingSkill * 0.00275f);
        props.TopSpeedReachDuration = Game.Current.BasicModels.RidingModel.CalculateAcceleration(in mount, in mountHarness, ridingSkill);
        float weightFactor = mount.Weight / 2.0f + (mountHarness.IsEmpty ? 0.0f : mountHarness.Weight);
        props.MountDashAccelerationMultiplier = weightFactor > 200.0
            ? weightFactor < 300.0 ? 1.0f - (weightFactor - 200.0f) / 111.0f : 0.1f
            : 1f;
    }

    private void UpdateHumanAgentStats(Agent agent, AgentDrivenProperties props)
    {
        // Dirty hack, part of the work-around to have skills without spawning custom characters. This hack should be
        // be performed in InitializeHumanAgentStats but the MissionPeer is not available there.
        if (GameNetwork.IsClientOrReplay) // Server-side the hacky AgentOrigin is directly passed to the AgentBuildData.
        {
            var crpgUser = agent.MissionPeer?.GetComponent<CrpgRepresentative>()?.User;
            if (crpgUser != null && agent.Origin is not CrpgBattleAgentOrigin)
            {
                var characteristics = crpgUser.Character.Characteristics;
                var mbSkills = CrpgBattleSpawningBehavior.CreateCharacterSkills(characteristics);
                agent.Origin = new CrpgBattleAgentOrigin(agent.Origin?.Troop, mbSkills);
                InitializeAgentStats(agent, agent.SpawnEquipment, props, null!);
            }
        }

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

        float totalEncumbrance = props.ArmorEncumbrance + props.WeaponsEncumbrance;
        float agentWeight = agent.Monster.Weight;
        int athleticsSkill = GetEffectiveSkill(agent.Character, agent.Origin, agent.Formation, DefaultSkills.Athletics);
        float impactOfStrReqOnSpeed = ImpactOfStrReqOnSpeed(agent);
        props.TopSpeedReachDuration = 2f / MathF.Max((200f + athleticsSkill) / 300f * (agentWeight / (agentWeight + totalEncumbrance)) * impactOfStrReqOnSpeed, 0.3f);
        float speed = 0.7f + 0.00070000015f * athleticsSkill;
        float weightSpeedPenalty = MathF.Max(0.2f * (1f - athleticsSkill * 0.001f), 0f) * totalEncumbrance / agentWeight / impactOfStrReqOnSpeed;
        float maxSpeedMultiplier = MBMath.ClampFloat(speed - weightSpeedPenalty, 0f, 0.91f);
        float atmosphereSpeedPenalty = agent.Mission.Scene.IsAtmosphereIndoor && agent.Mission.Scene.GetRainDensity() > 0
            ? 0.9f
            : 1f;
        props.MaxSpeedMultiplier = atmosphereSpeedPenalty * maxSpeedMultiplier;
        float bipedalCombatSpeedMinMultiplier = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.BipedalCombatSpeedMinMultiplier);
        float bipedalCombatSpeedMaxMultiplier = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.BipedalCombatSpeedMaxMultiplier);
        props.CombatMaxSpeedMultiplier =
            MathF.Min(
                MBMath.Lerp(
                    bipedalCombatSpeedMaxMultiplier,
                    bipedalCombatSpeedMinMultiplier,
                    MathF.Min(totalEncumbrance / agentWeight, 1f)),
                1f);

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
        int itemSkill = GetEffectiveSkill(character, agent.Origin, agent.Formation, equippedItem?.RelevantSkill ?? DefaultSkills.Athletics);
        props.SwingSpeedMultiplier = 0.93f + 0.0007f * itemSkill;
        props.ThrustOrRangedReadySpeedMultiplier = props.SwingSpeedMultiplier;
        props.HandlingMultiplier = 1f;
        props.ShieldBashStunDurationMultiplier = 1f;
        props.KickStunDurationMultiplier = 1f;
        props.ReloadSpeed = equippedItem == null ? props.SwingSpeedMultiplier : (equippedItem.SwingSpeed / 100f) * (0.6f + 0.0001f * itemSkill + 0.0000125f * itemSkill * itemSkill);
        props.MissileSpeedMultiplier = 1f;
        props.ReloadMovementPenaltyFactor = 1f;
        SetAllWeaponInaccuracy(agent, props, (int)wieldedItemIndex3, equippedItem);
        int ridingSkill = GetEffectiveSkill(character, agent.Origin, agent.Formation, DefaultSkills.Riding);
        if (equippedItem != null)
        {
            int weaponSkill = GetEffectiveSkillForWeapon(agent, equippedItem);
            props.WeaponInaccuracy = GetWeaponInaccuracy(agent, equippedItem, weaponSkill);
            if (agent.HasMount)
            {
                // SwingSpeed Nerf on Horseback
                float swingSpeedFactor = 1f / Math.Max(equippedItem.WeaponLength / 100f, 1f);
                props.SwingSpeedMultiplier *= HasSwingDamage(primaryItem) ? swingSpeedFactor : 1f;
            }

            // Ranged Behavior
            if (equippedItem.IsRangedWeapon)
            {
                props.ThrustOrRangedReadySpeedMultiplier = equippedItem.ThrustSpeed / 220f + 0.0035f * itemSkill;
                float maxMovementAccuracyPenaltyMultiplier = Math.Max(0.0f, 1.0f - weaponSkill / 500.0f);
                float weaponMaxMovementAccuracyPenalty = 0.125f * maxMovementAccuracyPenaltyMultiplier;
                float weaponMaxUnsteadyAccuracyPenalty = 0.1f * maxMovementAccuracyPenaltyMultiplier;
                props.WeaponMaxMovementAccuracyPenalty = Math.Max(0.0f, weaponMaxMovementAccuracyPenalty);
                props.WeaponMaxUnsteadyAccuracyPenalty = Math.Max(0.0f, weaponMaxUnsteadyAccuracyPenalty);

                // Crossbows
                if (equippedItem.RelevantSkill == DefaultSkills.Crossbow)
                {
                    props.WeaponInaccuracy /= ImpactOfStrReqOnCrossbows(agent, 1f, primaryItem);
                    props.WeaponMaxMovementAccuracyPenalty *= ImpactOfStrReqOnCrossbows(agent, 0.2f, primaryItem);
                    props.WeaponMaxUnsteadyAccuracyPenalty = 0.5f / ImpactOfStrReqOnCrossbows(agent, 0.05f, primaryItem); // override to remove impact of wpf on this property
                    props.WeaponRotationalAccuracyPenaltyInRadians /= ImpactOfStrReqOnCrossbows(agent, 0.3f, primaryItem);
                    props.ThrustOrRangedReadySpeedMultiplier *= 0.4f * (float)Math.Pow(2, weaponSkill / 191f) * ImpactOfStrReqOnCrossbows(agent, 0.3f, primaryItem); // Multiplying make windup time slower a 0 wpf, faster at 80 wpf
                    props.ReloadSpeed *= ImpactOfStrReqOnCrossbows(agent, 0.15f, primaryItem);
                    props.ReloadMovementPenaltyFactor = 100f * ImpactOfStrReqOnCrossbows(agent, 1f, primaryItem);
                    InformationManager.DisplayMessage(new InformationMessage(props.ReloadMovementPenaltyFactor.ToString()));
                }

                // Bows
                if (equippedItem.WeaponClass == WeaponClass.Bow)
                {
                    // Movement Penalty
                    float scale = MBMath.ClampFloat((equippedItem.ThrustSpeed - 60.0f) / 75.0f, 0.0f, 1f);
                    props.WeaponMaxMovementAccuracyPenalty *= 6f;
                    props.WeaponMaxUnsteadyAccuracyPenalty *= 4.5f / MBMath.Lerp(0.75f, 2f, scale);

                    // Aim Speed
                    props.WeaponBestAccuracyWaitTime = 0.3f + (95.75f - equippedItem.ThrustSpeed) * 0.005f;
                    float amount = MBMath.ClampFloat((equippedItem.ThrustSpeed - 60.0f) / 75.0f, 0.0f, 1f);

                    // Hold Time
                    int powerDraw = GetEffectiveSkill(character, agent.Origin, agent.Formation, CrpgSkills.PowerDraw);
                    props.WeaponUnsteadyBeginTime = 0.06f + weaponSkill * 0.00175f * MBMath.Lerp(1f, 2f, amount) + powerDraw * powerDraw / 10f * 0.35f;
                    props.WeaponUnsteadyEndTime = 2f + props.WeaponUnsteadyBeginTime;

                    // Rotation Penalty
                    props.WeaponRotationalAccuracyPenaltyInRadians = 0.1f;
                }

                // Throwing
                else if (equippedItem.WeaponClass is WeaponClass.Javelin or WeaponClass.ThrowingAxe or WeaponClass.ThrowingKnife or WeaponClass.Stone)
                {
                    float unsteadyAccuracyPenaltyScaler = MBMath.ClampFloat((equippedItem.ThrustSpeed - 89.0f) / 13.0f, 0.0f, 1f);
                    props.WeaponMaxUnsteadyAccuracyPenalty *= 3.5f * MBMath.Lerp(1.5f, 0.8f, unsteadyAccuracyPenaltyScaler);
                    int powerThrow = GetEffectiveSkill(character, agent.Origin, agent.Formation, CrpgSkills.PowerThrow);
                    props.WeaponBestAccuracyWaitTime = 0.4f + (89.0f - equippedItem.ThrustSpeed) * 0.03f;
                    props.WeaponUnsteadyBeginTime = 1.0f + weaponSkill * 0.006f + powerThrow * powerThrow / 10f * 0.4f;
                    props.WeaponUnsteadyEndTime = 10f + props.WeaponUnsteadyBeginTime;
                    props.WeaponRotationalAccuracyPenaltyInRadians = 0.025f;
                }

                // Rest? Will not touch. It may affect other mechanics like Catapults etc...
                else
                {
                    props.WeaponBestAccuracyWaitTime = 0.1f;
                    props.WeaponUnsteadyBeginTime = 0.0f;
                    props.WeaponUnsteadyEndTime = 0.0f;
                    props.WeaponRotationalAccuracyPenaltyInRadians = 0.1f;
                }
            }

            // does this govern couching?
            else if (equippedItem.WeaponFlags.HasAllFlags(WeaponFlags.WideGrip))
            {
                props.WeaponUnsteadyBeginTime = 1.0f + weaponSkill * 0.005f;
                props.WeaponUnsteadyEndTime = 3.0f + weaponSkill * 0.01f;
            }

            // Mounted Archery

            if (agent.HasMount)
            {
                int mountedArcherySkill = GetEffectiveSkill(character, agent.Origin, agent.Formation, CrpgSkills.MountedArchery);

                float weaponMaxMovementAccuracyPenalty = 0.03f / _constants.MountedRangedSkillInaccurary[mountedArcherySkill];
                float weaponMaxUnsteadyAccuracyPenalty = 0.15f / _constants.MountedRangedSkillInaccurary[mountedArcherySkill];
                if (equippedItem.RelevantSkill == DefaultSkills.Crossbow)
                {
                    weaponMaxUnsteadyAccuracyPenalty /= ImpactOfStrReqOnCrossbows(agent, 0.2f, primaryItem);
                    weaponMaxMovementAccuracyPenalty /= ImpactOfStrReqOnCrossbows(agent, 0.2f, primaryItem);
                }

                props.WeaponMaxMovementAccuracyPenalty = Math.Min(weaponMaxMovementAccuracyPenalty, 1f);
                props.WeaponMaxUnsteadyAccuracyPenalty = Math.Min(weaponMaxUnsteadyAccuracyPenalty, 1f);
                props.WeaponInaccuracy /= _constants.MountedRangedSkillInaccurary[mountedArcherySkill];
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

    private List<ItemObject> GetArmorItemObjectList(Equipment equipment)
    {
        List<ItemObject> armorItemObjectList = new();
        for (EquipmentIndex equipmentIndex = EquipmentIndex.NumAllWeaponSlots; equipmentIndex < EquipmentIndex.ArmorItemEndSlot; equipmentIndex++)
        {
            EquipmentElement equipmentElement = equipment[equipmentIndex];
            if (equipmentElement.Item != null)
            {
                armorItemObjectList.Add(equipmentElement.Item);
            }
        }

        return armorItemObjectList;
    }

    private float ImpactOfStrReqOnSpeed(Agent agent)
    {
        int strengthAttribute = GetEffectiveSkill(agent.Character, agent.Origin, agent.Formation, CrpgSkills.Strength);
        var equippedArmors = GetArmorItemObjectList(agent.SpawnEquipment);
        float setRequirement = reqModel.ComputeArmorSetPieceStrengthRequirement(equippedArmors);
        float distanceToStrRequirement = Math.Max(setRequirement - strengthAttribute, 0);
        float impactOfStrReqOnSpeedFactor = 0.2f; // tweak here
        return 1 / (1 + distanceToStrRequirement * impactOfStrReqOnSpeedFactor);
    }

    private float ImpactOfStrReqOnCrossbows(Agent agent, float impact, ItemObject? equippedItem)
    {
        if (equippedItem == null)
        {
            return 1;
        }

        float distanceToStrRequirement = CrossbowDistanceToStrRequirement(agent, equippedItem);
        return 1 / (1 + distanceToStrRequirement * impact);
    }

    private float CrossbowDistanceToStrRequirement(Agent agent, ItemObject? equippedItem)
    {
        if (equippedItem == null)
        {
            return 0;
        }

        int strengthAttribute = GetEffectiveSkill(agent.Character, agent.Origin, agent.Formation, CrpgSkills.Strength);
        float setRequirement = CrpgItemRequirementModel.ComputeItemRequirement(equippedItem);
        return Math.Max(setRequirement - strengthAttribute, 0);
    }

    private bool HasSwingDamage(ItemObject? equippedItem)
    {
        if (equippedItem == null)
        {
            return false;
        }

        return equippedItem.WeaponComponent.Weapons.Any(a => a.SwingDamage > 0);
    }
}
