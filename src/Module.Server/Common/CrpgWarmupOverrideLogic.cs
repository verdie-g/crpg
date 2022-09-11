using Crpg.Module.Battle;
using TaleWorlds.MountAndBlade;
using static TaleWorlds.MountAndBlade.MultiplayerWarmupComponent;

namespace Crpg.Module.Common;

/// <summary>
/// Overrides the spawn behavior on cRPG Warmup startup. Unfortunately a bit hacky.
/// </summary>
internal class CrpgWarmupOverrideLogic : MissionBehavior
{
    private readonly CrpgConstants _constants = default!;

    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;
    private bool OverrideSpawningBehavior { get; set; } = false;

    public CrpgWarmupOverrideLogic(CrpgConstants constants)
    {
        _constants = constants;
    }

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
    }

    public override void OnPreDisplayMissionTick(float dt)
    {
        if (OverrideSpawningBehavior)
        {
            SpawnComponent spawnComponent = Mission.GetMissionBehavior<SpawnComponent>();

            spawnComponent.SetNewSpawnFrameBehavior(new FFASpawnFrameBehavior());
            spawnComponent.SetNewSpawningBehavior(new CrpgBattleSpawningBehavior(_constants, null));
            OverrideSpawningBehavior = false;
        }
    }

    public override void OnClearScene()
    {
        MultiplayerWarmupComponent multiplayerWarmupComponent = Mission.GetMissionBehavior<MultiplayerWarmupComponent>();
        if (!OverrideSpawningBehavior && (multiplayerWarmupComponent == null || !multiplayerWarmupComponent.IsInWarmup))
        {
            return;
        }

        WarmupStates warmupState = (WarmupStates)typeof(MultiplayerWarmupComponent).GetField("_warmupState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(multiplayerWarmupComponent);
        SpawnComponent spawnComponent = Mission.GetMissionBehavior<SpawnComponent>();
        if (warmupState != WarmupStates.InProgress || (spawnComponent.SpawningBehavior.GetType() == typeof(CrpgBattleSpawningBehavior) && spawnComponent.SpawnFrameBehavior.GetType() == typeof(FFASpawnFrameBehavior)))
        {
            return;
        }

        OverrideSpawningBehavior = true;
    }
}
