using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;

namespace Crpg.Module.Modes.Conquest;

internal class CrpgConquestClient : MissionMultiplayerGameModeBaseClient, ICommanderInfo
{
    public const string StageTagPrefix = "crpg_conquest_stage_";
    public const int MaxStages = MissionMultiplayerSiege.NumberOfFlagsInGame;

    private FlagCapturePoint[] _flags = Array.Empty<FlagCapturePoint>();
    private Team?[] _flagOwners = Array.Empty<Team>();
    private SoundEvent? _bellSoundEvent;
    private MissionPeer? _myMissionPeer;

    public override bool IsGameModeUsingGold => false;
    public override bool IsGameModeTactical => false;
    public override bool IsGameModeUsingRoundCountdown => true;
    public override MissionLobbyComponent.MultiplayerGameType GameType => MissionLobbyComponent.MultiplayerGameType.Siege;
    public bool AreMoralesIndependent => false;

#pragma warning disable CS0067 // False positive
    public event Action<BattleSideEnum, float>? OnMoraleChangedEvent;
#pragma warning restore CS0067
    public event Action? OnFlagNumberChangedEvent;
    public event Action<FlagCapturePoint, Team>? OnCapturePointOwnerChangedEvent;

    public IEnumerable<FlagCapturePoint> AllCapturePoints => _flags;

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
        MissionNetworkComponent.OnMyClientSynchronized += OnMyClientSynchronized;
        _flags = Mission.MissionObjects.FindAllWithType<FlagCapturePoint>().ToArray();
        int maxNumberOfFlags = AllCapturePoints.Select(f => f.FlagIndex).Max() + 1;
        _flagOwners = new Team[maxNumberOfFlags];
    }

    public override void OnRemoveBehavior()
    {
        MissionNetworkComponent.OnMyClientSynchronized -= OnMyClientSynchronized;
        base.OnRemoveBehavior();
    }

    public override void AfterStart()
    {
        Mission.SetMissionMode(MissionMode.Battle, true);
    }

    public override int GetGoldAmount()
    {
        return 0;
    }

    public override void OnGoldAmountChangedForRepresentative(MissionRepresentativeBase representative, int goldAmount)
    {
    }

    public Team? GetFlagOwner(FlagCapturePoint flag)
    {
        return _flagOwners[flag.FlagIndex];
    }

    public override void OnMissionTick(float dt)
    {
        base.OnMissionTick(dt);

        if (MissionLobbyComponent.CurrentMultiplayerState != MissionLobbyComponent.MultiplayerGameState.Playing)
        {
            return;
        }

        bool attackerLosing = TimerComponent.GetRemainingTime(true) < 30;

        if (_bellSoundEvent != null && !attackerLosing)
        {
            _bellSoundEvent.Stop();
            _bellSoundEvent = null;
        }
        else if (_bellSoundEvent == null && attackerLosing && _myMissionPeer?.Team != null)
        {
            string bellSoundEventId = _myMissionPeer.Team.Side == BattleSideEnum.Defender
                ? "event:/multiplayer/warning_bells_defender"
                : "event:/multiplayer/warning_bells_attacker";
            _bellSoundEvent = SoundEvent.CreateEventFromString(bellSoundEventId, Mission.Scene);
            _bellSoundEvent.Play();
        }
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        if (!GameNetwork.IsClient)
        {
            return;
        }

        registerer.Register<FlagDominationCapturePointMessage>(HandleCapturePointMessage);
        registerer.Register<CrpgConquestStageStartMessage>(HandleConquestStageStartMessage);
    }

    private void OnMyClientSynchronized()
    {
        _myMissionPeer = GameNetwork.MyPeer.GetComponent<MissionPeer>();
    }

    private void OnCapturePointOwnerChanged(FlagCapturePoint flag, Team flagOwner)
    {
        _flagOwners[flag.FlagIndex] = flagOwner;
        OnCapturePointOwnerChangedEvent?.Invoke(flag, flagOwner);

        var myTeam = _myMissionPeer?.Team;
        if (myTeam == null)
        {
            return;
        }

        MatrixFrame cameraFrame = Mission.GetCameraFrame();
        Vec3 position = cameraFrame.origin + cameraFrame.rotation.u;
        string sound = myTeam == flagOwner ? "event:/alerts/report/flag_captured" : "event:/alerts/report/flag_lost";
        MBSoundEvent.PlaySound(SoundEvent.GetEventIdFromString(sound), position);
    }

    private void OnNumberOfFlagsChanged()
    {
        OnFlagNumberChangedEvent?.Invoke();
    }

    private void HandleCapturePointMessage(FlagDominationCapturePointMessage message)
    {
        foreach (FlagCapturePoint flag in AllCapturePoints)
        {
            if (flag.FlagIndex == message.FlagIndex)
            {
                OnCapturePointOwnerChanged(flag, message.OwnerTeam);
                break;
            }
        }
    }

    private void HandleConquestStageStartMessage(CrpgConquestStageStartMessage message)
    {
        bool flagsRemoved = message.StageIndex > 0;
        string? soundEventPath = null;
        if (flagsRemoved)
        {
            soundEventPath = "event:/ui/mission/multiplayer/pointsremoved";
            OnNumberOfFlagsChanged();
        }

        TimerComponent.StartTimerAsClient(message.StageStartTime, message.StageDuration);
        MBInformationManager.AddQuickInformation(new TextObject($"Stage {message.StageIndex + 1} started"), soundEventPath: soundEventPath);
    }
}
