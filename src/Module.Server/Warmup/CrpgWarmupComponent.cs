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
    private bool _overridenSpawningBehavior;

    public CrpgWarmupComponent(CrpgConstants constants, MultiplayerGameNotificationsComponent notificationsComponent)
    {
        _constants = constants;
        _notificationsComponent = notificationsComponent;
    }

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
        base.OnWarmupEnding += OnWarmupEnding;
    }

    public override void OnPreDisplayMissionTick(float dt)
    {
        base.OnPreDisplayMissionTick(dt);
        if (!_overridenSpawningBehavior)
        {
            return;
        }

        SpawnComponent spawnComponent = Mission.GetMissionBehavior<SpawnComponent>();
        spawnComponent.SetNewSpawnFrameBehavior(new FFASpawnFrameBehavior());
        spawnComponent.SetNewSpawningBehavior(new CrpgWarmupSpawningBehavior(_constants));
        _overridenSpawningBehavior = false;
    }

    public override void OnClearScene()
    {
        base.OnClearScene();
        if (!_overridenSpawningBehavior && !IsInWarmup)
        {
            return;
        }

        SpawnComponent spawnComponent = Mission.GetMissionBehavior<SpawnComponent>();
        if ((WarmupStates)WarmupStateField.GetValue(this) != WarmupStates.InProgress
            || (spawnComponent.SpawningBehavior.GetType() == typeof(CrpgWarmupSpawningBehavior)
                && spawnComponent.SpawnFrameBehavior.GetType() == typeof(FFASpawnFrameBehavior)))
        {
            return;
        }

        _overridenSpawningBehavior = true;
    }

    public override void OnRemoveBehavior()
    {
        base.OnWarmupEnding -= OnWarmupEnding;
        base.OnRemoveBehavior();
        if (!GameNetwork.IsServer)
        {
            return;
        }

        // When this behavior is being removed, it it means the game is about to start and the hardcoded spawn behaviors
        // were set. It's the moment to replace them.
        SpawnComponent spawnComponent = Mission.GetMissionBehavior<SpawnComponent>();
        MultiplayerRoundController multiplayerRoundController = Mission.GetMissionBehavior<MultiplayerRoundController>();
        spawnComponent.SetNewSpawnFrameBehavior(new FlagDominationSpawnFrameBehavior());
        spawnComponent.SetNewSpawningBehavior(new CrpgBattleSpawningBehavior(_constants, multiplayerRoundController));
    }

    private new void OnWarmupEnding()
    {
        _notificationsComponent.WarmupEnding();
    }
}
