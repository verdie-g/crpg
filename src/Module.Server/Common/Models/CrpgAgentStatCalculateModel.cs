using Crpg.Module.Api.Models.Users;
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
    // Hack to workaround not being able to spawn custom character. In the client this property is set so the
    // StatCalculateModel has access to the cRPG user.
    public static CrpgUser? MyUser { get; set; }

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

        const float accuracyPointA = 45; // Abscissa of the lowest accuracy (point A) that corresponds to stones at the moment. It's our lower calibration bound.
        const float valueAtAccuracyPointA = 100; // Inaccuracy at point A abscissa which corresponds to equipping stones.
        const float parabolOffset = 20; // Inaccuracy for the most accurate weapon.
        const float parabolMinAbscissa = 140; // Set at 100 so the weapon component is strictly monotonous.

        const float a = valueAtAccuracyPointA - parabolOffset; // Parameter for the polynomial, do not change.

        float skillComponentMultiplier = weapon.WeaponClass == WeaponClass.Bow ? 0.4f : 0.2f;

        if (weapon.IsRangedWeapon)
        {
            float weaponComponent = (parabolMinAbscissa - weapon.Accuracy)
                * (parabolMinAbscissa - weapon.Accuracy)
                * a
                / ((parabolMinAbscissa - accuracyPointA) * (100 - accuracyPointA))
                + parabolOffset;
            float skillComponent = skillComponentMultiplier * (float)Math.Pow(10.0, (200.0 - weaponSkill) / 200.0);
            inaccuracy = (weaponComponent * skillComponent + (100 - weapon.Accuracy)) * 0.001f;
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
        // Dirty hack, part of the work-around to have skills without spawning custom characters.
        var crpgUser = GameNetwork.IsClientOrReplay
            ? MyUser
            : null; // For the server, the origin is set in CrpgBattleSpawningBehavior.
        if (crpgUser != null)
        {
            var characteristics = crpgUser.Character.Characteristics;
            var mbSkills = CrpgBattleSpawningBehavior.CreateCharacterSkills(characteristics);
            agent.Origin = new CrpgBattleAgentOrigin(agent.Origin?.Troop, mbSkills);
        }

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
                    int mountedArcherySkill = GetEffectiveSkill(character, agent.Origin, agent.Formation, CrpgSkills.MountedArchery);
                    // float num6 = Math.Max(0.0f, (1.0f - weaponSkill / 500.0f) * (1.0f - ridingSkill / 1800.0f));
                    float num6 = 1;
                    props.WeaponMaxMovementAccuracyPenalty = 0.025f * num6;
                    props.WeaponMaxUnsteadyAccuracyPenalty = 0.06f * num6;
                    props.WeaponInaccuracy /= _constants.MountedRangedSkillInaccurary[mountedArcherySkill];
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
                    props.WeaponInaccuracy *= 1.5f;
                }
                else if (equippedItem.RelevantSkill == DefaultSkills.Crossbow)
                {
                    props.WeaponMaxMovementAccuracyPenalty *= 1f;
                    props.WeaponMaxUnsteadyAccuracyPenalty = 0.5f;
                    props.WeaponRotationalAccuracyPenaltyInRadians *= 1f;
                    props.ThrustOrRangedReadySpeedMultiplier *= 0.75f * (float)Math.Pow(2, weaponSkill / 191);
                    props.WeaponInaccuracy /= 2;
                    props.ReloadSpeed *= 0.65f;
                    props.MissileSpeedMultiplier *= 1.4f;

                if (equippedItem.WeaponClass == WeaponClass.Bow)
                {
                    int powerDraw = GetEffectiveSkill(character, agent.Origin, agent.Formation, CrpgSkills.PowerDraw);
                    props.WeaponBestAccuracyWaitTime = 0.3f + (95.75f - equippedItem.ThrustSpeed) * 0.005f;
                    float amount = MBMath.ClampFloat((equippedItem.ThrustSpeed - 60.0f) / 75.0f, 0.0f, 1f);

                    props.WeaponUnsteadyBeginTime = 0.06f + weaponSkill * 0.001f * MBMath.Lerp(1f, 2f, amount) + powerDraw * powerDraw / 10f * 0.4f;
                    if (agent.IsAIControlled)
                    {
                        props.WeaponUnsteadyBeginTime *= 4f;
                    }

                    props.WeaponUnsteadyEndTime = 2f + props.WeaponUnsteadyBeginTime;
                    props.WeaponRotationalAccuracyPenaltyInRadians = 0.1f;
                }
                else if (equippedItem.WeaponClass is WeaponClass.Javelin or WeaponClass.ThrowingAxe or WeaponClass.ThrowingKnife or WeaponClass.Stone)
                {
                    int powerThrow = GetEffectiveSkill(character, agent.Origin, agent.Formation, CrpgSkills.PowerThrow);
                    props.WeaponBestAccuracyWaitTime = 0.4f + (89.0f - equippedItem.ThrustSpeed) * 0.03f;
                    props.WeaponUnsteadyBeginTime = 1.0f + weaponSkill * 0.006f + powerThrow * powerThrow / 10f * 0.4f;
                    props.WeaponUnsteadyEndTime = 10f + props.WeaponUnsteadyBeginTime;
                    props.WeaponRotationalAccuracyPenaltyInRadians = 0.025f;
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
        float setRequirement = CrpgItemRequirementModel.ComputeArmorSetPieceStrengthRequirement(equippedArmors);
        float distanceToStrRequirement = Math.Max(setRequirement - strengthAttribute, 0);
        float impactOfStrReqOnSpeedFactor = 0.2f; // tweak here
        return 1 / (1 + distanceToStrRequirement * impactOfStrReqOnSpeedFactor);
    }
}
