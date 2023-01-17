using System.Reflection;
using Crpg.Module.Battle;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.Warmup;

/// <summary>
/// Custom warmup component so we can load the <see cref="CrpgBattleSpawningBehavior"/> as soon as warmup ends.
/// </summary>
internal class CrpgWarmupComponent : MultiplayerWarmupComponent
{
    private static readonly FieldInfo WarmupStateField = typeof(MultiplayerWarmupComponent)
        .GetField("_warmupState", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private readonly CrpgConstants _constants;
    private readonly MultiplayerGameNotificationsComponent _notificationsComponent;
    private readonly Func<(SpawnFrameBehaviorBase, SpawningBehaviorBase)>? _createSpawnBehaviors;
    private WarmupStates _lastTickWarmupState;

    public CrpgWarmupComponent(CrpgConstants constants,
        MultiplayerGameNotificationsComponent notificationsComponent,
        Func<(SpawnFrameBehaviorBase, SpawningBehaviorBase)>? createSpawnBehaviors)
    {
        _constants = constants;
        _notificationsComponent = notificationsComponent;
        _createSpawnBehaviors = createSpawnBehaviors;
        _lastTickWarmupState = WarmupStates.WaitingForPlayers;
    }

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
        base.OnWarmupEnding += OnWarmupEnding;
    }

    public override void OnPreDisplayMissionTick(float dt)
    {
        base.OnPreDisplayMissionTick(dt);
        if (!GameNetwork.IsServer)
        {
            return;
        }

        var warmupState = (WarmupStates)WarmupStateField.GetValue(this)!;

        if (_lastTickWarmupState == WarmupStates.WaitingForPlayers && warmupState == WarmupStates.InProgress)
        {
            SpawnComponent spawnComponent = Mission.GetMissionBehavior<SpawnComponent>();
            spawnComponent.SetNewSpawnFrameBehavior(new FFASpawnFrameBehavior());
            spawnComponent.SetNewSpawningBehavior(new CrpgWarmupSpawningBehavior(_constants));
        }
        else if (_lastTickWarmupState == WarmupStates.Ending && warmupState == WarmupStates.Ended)
        {
            SpawnComponent spawnComponent = Mission.GetMissionBehavior<SpawnComponent>();
            (SpawnFrameBehaviorBase spawnFrame, SpawningBehaviorBase spawning) = _createSpawnBehaviors!();
            spawnComponent.SetNewSpawnFrameBehavior(spawnFrame);
            spawnComponent.SetNewSpawningBehavior(spawning);
        }

        _lastTickWarmupState = warmupState;
    }

    private new void OnWarmupEnding()
    {
        _notificationsComponent.WarmupEnding();
    }
}
