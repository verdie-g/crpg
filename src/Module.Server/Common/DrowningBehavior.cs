using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

/// <summary>Inflict damages to agents under water level.</summary>
internal class DrowningBehavior : MissionLogic
{
    private const float TickPeriod = 0.5f;
    private const int DamagePerTick = 5;

    private MissionTimer? _tickTimer;
    private float _waterLevel = float.MinValue;

    public override void OnBehaviorInitialize()
    {
        _waterLevel = Mission.Scene.GetWaterLevel();
    }

    public override void OnMissionTick(float dt)
    {
        _tickTimer ??= new MissionTimer(TickPeriod);
        if (!_tickTimer.Check(reset: true))
        {
            return;
        }

        // Killing an agent removes it from the Mission.Agents list which breaks its enumerator. So a temporary buffer
        // need to be used.
        List<Agent>? drowningAgents = null;
        foreach (var agent in Mission.Agents)
        {
            if (!agent.IsActive())
            {
                continue;
            }

            if (agent.GetChestGlobalPosition().Z < _waterLevel)
            {
                drowningAgents ??= new List<Agent>();
                drowningAgents.Add(agent);
            }
        }

        if (drowningAgents == null)
        {
            return;
        }

        foreach (var agent in drowningAgents)
        {
            Blow blow = new()
            {
                WeaponRecord = new BlowWeaponRecord { AffectorWeaponSlotOrMissileIndex = -1 },
                Position = agent.Position,
                Direction = Vec3.Zero,
                SwingDirection = Vec3.Zero,
                InflictedDamage = DamagePerTick,
                SelfInflictedDamage = 0,
                BaseMagnitude = 0,
                DefenderStunPeriod = 0,
                AttackerStunPeriod = 0,
                AbsorbedByArmor = 0,
                MovementSpeedDamageModifier = 0,
                StrikeType = StrikeType.Invalid,
                AttackType = AgentAttackType.Standard,
                BlowFlag = BlowFlags.None,
                OwnerId = agent.Index,
                BoneIndex = 0,
                VictimBodyPart = BoneBodyPartType.None,
                DamageType = DamageTypes.Invalid,
                NoIgnore = false,
                DamageCalculated = true,
                IsFallDamage = true,
                DamagedPercentage = 0,
            };
            var attackCollisionData = AttackCollisionData.GetAttackCollisionDataForDebugPurpose(
                _attackBlockedWithShield: false,
                _correctSideShieldBlock: false,
                _isAlternativeAttack: false,
                _isColliderAgent: false,
                _collidedWithShieldOnBack: false,
                _isMissile: false,
                _isMissileBlockedWithWeapon: false,
                _missileHasPhysics: false,
                _entityExists: false,
                _thrustTipHit: false,
                _missileGoneUnderWater: true,
                _missileGoneOutOfBorder: false,
                collisionResult: CombatCollisionResult.None,
                affectorWeaponSlotOrMissileIndex: blow.WeaponRecord.AffectorWeaponSlotOrMissileIndex,
                StrikeType: (int)blow.StrikeType,
                DamageType: (int)blow.DamageType,
                CollisionBoneIndex: blow.BoneIndex,
                VictimHitBodyPart: blow.VictimBodyPart,
                AttackBoneIndex: blow.BoneIndex,
                AttackDirection: Agent.UsageDirection.None,
                PhysicsMaterialIndex: -1,
                CollisionHitResultFlags: CombatHitResultFlags.NormalHit,
                AttackProgress: 0f,
                CollisionDistanceOnWeapon: 0f,
                AttackerStunPeriod: blow.AttackerStunPeriod,
                DefenderStunPeriod: blow.DefenderStunPeriod,
                MissileTotalDamage: 0f,
                MissileInitialSpeed: 0f,
                ChargeVelocity: 0f,
                FallSpeed: 0f,
                WeaponRotUp: Vec3.Zero,
                _weaponBlowDir: Vec3.Zero,
                CollisionGlobalPosition: blow.Position,
                MissileVelocity: Vec3.Zero,
                MissileStartingPosition: Vec3.Zero,
                VictimAgentCurVelocity: Vec3.Zero,
                GroundNormal: Vec3.Zero);
            agent.RegisterBlow(blow, in attackCollisionData);
        }
    }
}
