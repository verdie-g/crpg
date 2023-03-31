using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.MissionRepresentatives;
using TaleWorlds.MountAndBlade.Objects;
using MathF = TaleWorlds.Library.MathF;

namespace Crpg.Module.Modes.StrategusBattle;

internal class CrpgStrategusBattleClient : MissionMultiplayerGameModeBaseClient
{
    private const int BattleFlagsRemovalTime = 120;
    private const int SkirmishFlagsRemovalTime = 120;

    private readonly bool _isSkirmish;
    private FlagCapturePoint[] _flags = Array.Empty<FlagCapturePoint>();
    private Team?[] _flagOwners = Array.Empty<Team>();
    private bool _notifiedForFlagRemoval;
    private float _remainingTimeForBellSoundToStop = float.MinValue;
    private SoundEvent? _bellSoundEvent;
    private MissionPeer? _missionPeer;
    public event Action<BattleSideEnum, float>? OnMoraleChangedEvent;
    public event Action? OnFlagNumberChangedEvent;
    public event Action<FlagCapturePoint, Team?>? OnCapturePointOwnerChangedEvent;

    public CrpgStrategusBattleClient()
    {
    }

    public override bool IsGameModeUsingGold => false;
    public override bool IsGameModeTactical => _flags.Length != 0;
    public override bool IsGameModeUsingRoundCountdown => false;
    public override MissionLobbyComponent.MultiplayerGameType GameType => MissionLobbyComponent.MultiplayerGameType.TeamDeathmatch;
    public override bool IsGameModeUsingCasualGold => false;
    public IEnumerable<FlagCapturePoint> AllCapturePoints => _flags;
    public bool AreMoralesIndependent => false;
    public float FlagsRemovalTime => _isSkirmish ? SkirmishFlagsRemovalTime : BattleFlagsRemovalTime;

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
        MissionNetworkComponent.OnMyClientSynchronized += OnMyClientSynchronized;
    }

    public override void OnRemoveBehavior()
    {
        base.OnRemoveBehavior();
        MissionNetworkComponent.OnMyClientSynchronized -= OnMyClientSynchronized;
    }

    public override void OnGoldAmountChangedForRepresentative(MissionRepresentativeBase representative, int goldAmount)
    {
    }

    public override int GetGoldAmount()
    {
        return 0;
    }

    public override void OnMissionTick(float dt)
    {
        base.OnMissionTick(dt);
        if (_flags.Length == 0) // Protection against scene with no maps.
        {
            return;
        }
    }

    public override void AfterStart()
    {
        Mission.Current.SetMissionMode(MissionMode.Battle, true);
    }

    public override void OnClearScene()
    {
    }

    protected override void AddRemoveMessageHandlers(
        GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        base.AddRemoveMessageHandlers(registerer);
        if (GameNetwork.IsClientOrReplay)
        {
        }
    }

    private void OnPreparationEnded()
    {
    }

    private void OnMyClientSynchronized()
    {
        _missionPeer = GameNetwork.MyPeer.GetComponent<MissionPeer>();
    }

}
