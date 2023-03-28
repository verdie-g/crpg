using System.Reflection;
using Crpg.Module.Common;
using Crpg.Module.Helpers;
using Crpg.Module.Modes.Battle;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Modes.Warmup;

/// <summary>
/// Custom warmup component so we can load the <see cref="CrpgBattleSpawningBehavior"/> as soon as warmup ends.
/// </summary>
internal class CrpgWarmupComponent : MultiplayerWarmupComponent
{
    private static readonly FieldInfo WarmupStateField = typeof(MultiplayerWarmupComponent)
        .GetField("_warmupState", BindingFlags.NonPublic | BindingFlags.Instance)!;
    private static readonly FieldInfo TimerComponentField = typeof(MultiplayerWarmupComponent)
        .GetField("_timerComponent", BindingFlags.NonPublic | BindingFlags.Instance)!;
    private static readonly FieldInfo GameModeField = typeof(MultiplayerWarmupComponent)
        .GetField("_gameMode", BindingFlags.NonPublic | BindingFlags.Instance)!;
    private static readonly FieldInfo LobbyComponentField = typeof(MultiplayerWarmupComponent)
        .GetField("_lobbyComponent", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private readonly CrpgConstants _constants;
    private readonly MultiplayerGameNotificationsComponent _notificationsComponent;
    private readonly Func<(SpawnFrameBehaviorBase, SpawningBehaviorBase)>? _createSpawnBehaviors;

    public CrpgWarmupComponent(CrpgConstants constants,
        MultiplayerGameNotificationsComponent notificationsComponent,
        Func<(SpawnFrameBehaviorBase, SpawningBehaviorBase)>? createSpawnBehaviors)
    {
        _constants = constants;
        _notificationsComponent = notificationsComponent;
        _createSpawnBehaviors = createSpawnBehaviors;
    }

    private WarmupStates WarmupStateReflection
    {
        get => (WarmupStates)WarmupStateField.GetValue(this)!;
        set => WarmupStateField.SetValue(this, value);
    }

    private MultiplayerTimerComponent TimerComponentReflection => (MultiplayerTimerComponent)TimerComponentField.GetValue(this)!;
    private MissionMultiplayerGameModeBase GameModeReflection => (MissionMultiplayerGameModeBase)GameModeField.GetValue(this)!;
    private MissionLobbyComponent LobbyComponentReflection => (MissionLobbyComponent)LobbyComponentField.GetValue(this)!;

    public override void OnPreDisplayMissionTick(float dt)
    {
        if (!GameNetwork.IsServer)
        {
            return;
        }

        var warmupState = WarmupStateReflection;
        switch (warmupState)
        {
            case WarmupStates.WaitingForPlayers:
                BeginWarmup();
                break;
            case WarmupStates.InProgress:
                if (CheckForWarmupProgressEnd())
                {
                    EndWarmupProgress();
                }

                break;
            case WarmupStates.Ending:
                if (TimerComponentReflection.CheckIfTimerPassed())
                {
                    EndWarmup();
                }

                break;
            case WarmupStates.Ended:
                if (TimerComponentReflection.CheckIfTimerPassed())
                {
                    Mission.RemoveMissionBehavior(this);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void BeginWarmup()
    {
        WarmupStateReflection = WarmupStates.InProgress;
        Mission.Current.ResetMission();
        GameModeReflection.MultiplayerTeamSelectComponent.BalanceTeams();
        TimerComponentReflection.StartTimerAsServer(TotalWarmupDuration);
        GameModeReflection.SpawnComponent.SpawningBehavior.Clear();
        SpawnComponent spawnComponent = Mission.GetMissionBehavior<SpawnComponent>();
        spawnComponent.SetNewSpawnFrameBehavior(new FFASpawnFrameBehavior());
        spawnComponent.SetNewSpawningBehavior(new CrpgWarmupSpawningBehavior(_constants));
    }

    private void EndWarmupProgress()
    {
        WarmupStateReflection = WarmupStates.Ending;
        TimerComponentReflection.StartTimerAsServer(30f);
        ReflectionHelper.RaiseEvent(this, nameof(OnWarmupEnding), Array.Empty<object>());
        if (!GameNetwork.IsDedicatedServer)
        {
            _notificationsComponent.WarmupEnding();
        }
    }

    private void EndWarmup()
    {
        WarmupStateReflection = WarmupStates.Ended;
        TimerComponentReflection.StartTimerAsServer(3f);
        ReflectionHelper.RaiseEvent(this, nameof(OnWarmupEnded), Array.Empty<object>());

        if (GameNetwork.NetworkPeers.Count() < MultiplayerOptions.OptionType.MinNumberOfPlayersForMatchStart.GetIntValue())
        {
            LobbyComponentReflection.SetStateEndingAsServer();
            return;
        }

        if (!GameNetwork.IsDedicatedServer)
        {
            ReflectionHelper.InvokeMethod(this, "PlayBattleStartingSound", Array.Empty<object>());
        }

        Mission.Current.ResetMission();
        GameModeReflection.MultiplayerTeamSelectComponent.BalanceTeams();

        GameModeReflection.SpawnComponent.SpawningBehavior.Clear();
        SpawnComponent spawnComponent = Mission.GetMissionBehavior<SpawnComponent>();
        (SpawnFrameBehaviorBase spawnFrame, SpawningBehaviorBase spawning) = _createSpawnBehaviors!();
        spawnComponent.SetNewSpawnFrameBehavior(spawnFrame);
        spawnComponent.SetNewSpawningBehavior(spawning);
    }
}
