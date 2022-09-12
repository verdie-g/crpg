using Crpg.Module.Battle;
using TaleWorlds.MountAndBlade;
using static TaleWorlds.MountAndBlade.MultiplayerWarmupComponent;

namespace Crpg.Module.Common.Warmup;

/// <summary>
/// Overrides the spawn behavior on cRPG Warmup startup. Unfortunately a bit hacky.
/// </summary>
internal class CrpgWarmupOverrideLogic : MissionBehavior
{
    private readonly CrpgConstants _constants;
    private bool _overrideSpawningBehavior;

    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

    public CrpgWarmupOverrideLogic(CrpgConstants constants)
    {
        _constants = constants;
    }

    public override void OnPreDisplayMissionTick(float dt)
    {
        if (_overrideSpawningBehavior)
        {
            SpawnComponent spawnComponent = Mission.GetMissionBehavior<SpawnComponent>();

            spawnComponent.SetNewSpawnFrameBehavior(new FFASpawnFrameBehavior());
            spawnComponent.SetNewSpawningBehavior(new CrpgBattleSpawningBehavior(_constants, null));
            _overrideSpawningBehavior = false;
        }
    }

    public override void OnClearScene()
    {
        MultiplayerWarmupComponent multiplayerWarmupComponent = Mission.GetMissionBehavior<MultiplayerWarmupComponent>();
        if (!_overrideSpawningBehavior && (multiplayerWarmupComponent == null || !multiplayerWarmupComponent.IsInWarmup))
        {
            return;
        }

        WarmupStates warmupState = (WarmupStates)typeof(MultiplayerWarmupComponent)
            .GetField("_warmupState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .GetValue(multiplayerWarmupComponent);
        SpawnComponent spawnComponent = Mission.GetMissionBehavior<SpawnComponent>();
        if (warmupState != WarmupStates.InProgress
            || (spawnComponent.SpawningBehavior.GetType() == typeof(CrpgBattleSpawningBehavior)
                && spawnComponent.SpawnFrameBehavior.GetType() == typeof(FFASpawnFrameBehavior)))
        {
            return;
        }

        _overrideSpawningBehavior = true;
    }
}
