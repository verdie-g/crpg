using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.MissionRepresentatives;
using TaleWorlds.MountAndBlade.Objects;
using MathF = TaleWorlds.Library.MathF;

namespace Crpg.Module.Modes.Battle;

internal class CrpgBattleClient : MissionMultiplayerGameModeBaseClient, ICommanderInfo
{
    private const int BattleFlagSpawnTime = 150;
    private const int SkirmishFlagsRemovalTime = 120;

    private readonly bool _isSkirmish;
    private FlagCapturePoint[] _flags = Array.Empty<FlagCapturePoint>();
    private Team?[] _flagOwners = Array.Empty<Team>();
    private bool _notifiedForFlagRemoval;
    private float _remainingTimeForBellSoundToStop = float.MinValue;
    private SoundEvent? _bellSoundEvent;
    private MissionPeer? _missionPeer;
    private FlagDominationMissionRepresentative? _myRepresentative;

    public event Action<BattleSideEnum, float>? OnMoraleChangedEvent;
    public event Action? OnFlagNumberChangedEvent;
    public event Action<FlagCapturePoint, Team?>? OnCapturePointOwnerChangedEvent;

    public CrpgBattleClient(bool isSkirmish)
    {
        _isSkirmish = isSkirmish;
    }

    public override bool IsGameModeUsingGold => false;
    public override bool IsGameModeTactical => _flags.Length != 0;
    public override bool IsGameModeUsingRoundCountdown => true;
    public override MissionLobbyComponent.MultiplayerGameType GameType => _isSkirmish
        ? MissionLobbyComponent.MultiplayerGameType.Skirmish
        : MissionLobbyComponent.MultiplayerGameType.Battle;
    public override bool IsGameModeUsingCasualGold => false;
    public IEnumerable<FlagCapturePoint> AllCapturePoints => _flags;
    public bool AreMoralesIndependent => false;
    public float FlagManipulationTime => _isSkirmish ? SkirmishFlagsRemovalTime : BattleFlagSpawnTime;

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
        RoundComponent.OnPreparationEnded += OnPreparationEnded;
        MissionNetworkComponent.OnMyClientSynchronized += OnMyClientSynchronized;
        ResetFlags(); // Get the flags early so it's available for the HUD.
    }

    public override void OnRemoveBehavior()
    {
        base.OnRemoveBehavior();
        RoundComponent.OnPreparationEnded -= OnPreparationEnded;
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

        if (_remainingTimeForBellSoundToStop > 0.0)
        {
            _remainingTimeForBellSoundToStop -= dt;
        }

        if (_bellSoundEvent == null
            || (_remainingTimeForBellSoundToStop > 0.0
                && MissionLobbyComponent.CurrentMultiplayerState == MissionLobbyComponent.MultiplayerGameState.Playing))
        {
            return;
        }

        _remainingTimeForBellSoundToStop = float.MinValue;
        _bellSoundEvent.Stop();
        _bellSoundEvent = null;
    }

    public override void AfterStart()
    {
        Mission.Current.SetMissionMode(MissionMode.Battle, true);
    }

    public override void OnClearScene()
    {
        _notifiedForFlagRemoval = false;
        ResetFlags();

        if (_bellSoundEvent != null)
        {
            _remainingTimeForBellSoundToStop = float.MinValue;
            _bellSoundEvent.Stop();
            _bellSoundEvent = null;
        }
    }

    public void ChangeMorale(float morale)
    {
        for (BattleSideEnum side = BattleSideEnum.Defender; side < BattleSideEnum.NumSides; side += 1)
        {
            float num = (morale + 1.0f) / 2.0f;
            if (side == BattleSideEnum.Defender)
            {
                OnMoraleChangedEvent?.Invoke(BattleSideEnum.Defender, 1f - num);
            }
            else if (side == BattleSideEnum.Attacker)
            {
                OnMoraleChangedEvent?.Invoke(BattleSideEnum.Attacker, num);
            }
        }

        BattleSideEnum mySide = _missionPeer?.Team?.Side ?? BattleSideEnum.None;
        if (mySide == BattleSideEnum.None)
        {
            return;
        }

        float absMorale = MathF.Abs(morale);
        if (_remainingTimeForBellSoundToStop < 0.0)
        {
            _remainingTimeForBellSoundToStop = absMorale < 0.6 || absMorale >= 1.0
                ? float.MinValue
                : float.MaxValue;
            if (_remainingTimeForBellSoundToStop <= 0.0)
            {
                return;
            }

            _bellSoundEvent =
                (mySide == BattleSideEnum.Defender && morale >= 0.6f) ||
                (mySide == BattleSideEnum.Attacker && morale <= -0.6f)
                    ? SoundEvent.CreateEventFromString("event:/multiplayer/warning_bells_defender", Mission.Scene)
                    : SoundEvent.CreateEventFromString("event:/multiplayer/warning_bells_attacker", Mission.Scene);
            MatrixFrame flagGlobalFrame = _flags
                .Where(flag => !flag.IsDeactivated)
                .GetRandomElementInefficiently()
                .GameEntity.GetGlobalFrame();
            _bellSoundEvent.PlayInPosition(flagGlobalFrame.origin + flagGlobalFrame.rotation.u * 3f);
        }
        else
        {
            if (absMorale < 1.0 && absMorale >= 0.6)
            {
                return;
            }

            _remainingTimeForBellSoundToStop = float.MinValue;
        }
    }

    public void CaptureFlag(FlagCapturePoint flag, Team owner)
    {
        _flagOwners[flag.FlagIndex] = owner;
        OnCapturePointOwnerChangedEvent?.Invoke(flag, owner);

        var myTeam = _missionPeer?.Team;
        if (myTeam == null)
        {
            return;
        }

        MatrixFrame cameraFrame = Mission.Current.GetCameraFrame();
        Vec3 position = cameraFrame.origin + cameraFrame.rotation.u;
        string eventStr = myTeam == owner
            ? "event:/alerts/report/flag_captured"
            : "event:/alerts/report/flag_lost";
        MBSoundEvent.PlaySound(SoundEvent.GetEventIdFromString(eventStr), position);
    }

    public void ChangeNumberOfFlags()
    {
        OnFlagNumberChangedEvent?.Invoke();
    }

    public Team? GetFlagOwner(FlagCapturePoint flag)
    {
        return _flagOwners[flag.FlagIndex];
    }

    protected override void AddRemoveMessageHandlers(
        GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        base.AddRemoveMessageHandlers(registerer);
        if (GameNetwork.IsClientOrReplay)
        {
            registerer.Register<FlagDominationMoraleChangeMessage>(OnMoraleChange);
            registerer.Register<FlagDominationCapturePointMessage>(OnCapturePoint);
            if (_isSkirmish)
            {
                registerer.Register<FlagDominationFlagsRemovedMessage>(OnFlagsRemovedSkirmish);
            }
            else
            {
                registerer.Register<CrpgBattleSpawnFlagMessage>(OnFlagsSpawnedBattle);
            }
        }
    }

    protected override int GetWarningTimer()
    {
        if (!IsRoundInProgress || _flags.Length < 2)
        {
            return 0;
        }

        float timerStart = MultiplayerOptions.OptionType.RoundTimeLimit.GetIntValue() - FlagManipulationTime;
        float timerEnd = timerStart + 30f;
        if (RoundComponent.RemainingRoundTime < timerStart
            || RoundComponent.RemainingRoundTime > timerEnd)
        {
            return 0;
        }

        int warningTimer = MathF.Ceiling(30.0f - timerEnd - RoundComponent.RemainingRoundTime);
        if (!_notifiedForFlagRemoval)
        {
            _notifiedForFlagRemoval = true;
            NotifyForFlagManipulation();
        }

        return warningTimer;
    }

    private void NotifyForFlagManipulation()
    {
        if (!_isSkirmish)
        {
            TextObject textObject = new("{=nbOZ9BNX}A flag will spawn in {TIMER} seconds.",
            new Dictionary<string, object> { ["TIMER"] = 30 });
            string soundEventPath = "event:/ui/mission/multiplayer/pointwarning";
            MBInformationManager.AddQuickInformation(textObject, 0, null, soundEventPath);
        }
        else
        {
            NotificationsComponent.FlagsWillBeRemovedInXSeconds(30);
        }
    }

    private void OnPreparationEnded()
    {
        ResetFlags();
        if (_flags.Length == 0)
        {
            return;
        }

        OnFlagNumberChangedEvent?.Invoke();
        foreach (var flag in _flags)
        {
            OnCapturePointOwnerChangedEvent?.Invoke(flag, null);
        }
    }

    private void OnMyClientSynchronized()
    {
        _missionPeer = GameNetwork.MyPeer.GetComponent<MissionPeer>();
        _myRepresentative = GameNetwork.MyPeer.GetComponent<FlagDominationMissionRepresentative>();
    }

    private void OnMoraleChange(FlagDominationMoraleChangeMessage message)
    {
        ChangeMorale(message.Morale);
    }

    private void OnCapturePoint(FlagDominationCapturePointMessage message)
    {
        var capturedFlag = _flags.FirstOrDefault(flag => flag.FlagIndex == message.FlagIndex);
        if (capturedFlag == null)
        {
            return;
        }

        CaptureFlag(capturedFlag, message.OwnerTeam);
    }

    private void OnFlagsRemovedSkirmish(FlagDominationFlagsRemovedMessage message)
    {
        ChangeNumberOfFlags();
    }

    private void OnFlagsSpawnedBattle(CrpgBattleSpawnFlagMessage message)
    {
        TextObject textObject = new("{=nbOZ9BNX}Flag {NAME} has spawned.",
                new Dictionary<string, object> { ["NAME"] = char.ConvertFromUtf32(message.FlagChar) });
        string soundEventPath = "event:/ui/mission/multiplayer/pointsremoved";
        MBInformationManager.AddQuickInformation(textObject, 0, null, soundEventPath);

        ChangeNumberOfFlags();
    }

    private void ResetFlags()
    {
        _flags = Mission.Current.MissionObjects.FindAllWithType<FlagCapturePoint>().ToArray();
        _flagOwners = new Team[_flags.Length];
    }
}
