using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;

namespace Crpg.Module.Modes.Siege;

internal class CrpgSiegeClient : MissionMultiplayerGameModeBaseClient, ICommanderInfo
{
    public const string MasterFlagTag = "keep_capture_point";
    public const int StartingMorale = 360;

    private const float DefenderMoraleDropThresholdIncrement = 0.2f;
    private const float DefenderMoraleDropThresholdLow = 0.4f;
    private const float DefenderMoraleDropThresholdMedium = 0.6f;
    private const float DefenderMoraleDropThresholdHigh = 0.8f;
    private const float DefenderMoraleDropMediumDuration = 8f;
    private const float DefenderMoraleDropHighDuration = 4f;
    private const float BattleWinLoseAlertThreshold = 0.25f;
    private const float BattleWinLoseLateAlertThreshold = 0.15f;
    private const string BattleWinningSoundEventString = "event:/alerts/report/battle_winning";
    private const string BattleLosingSoundEventString = "event:/alerts/report/battle_losing";
    private const float IndefiniteDurationThreshold = 8f;

    private FlagCapturePoint[] _flags = Array.Empty<FlagCapturePoint>();
    private Team?[] _flagOwners = Array.Empty<Team>();
    private FlagCapturePoint _masterFlag = default!;
    private SoundEvent? _bellSoundEvent;
    private float _remainingTimeForBellSoundToStop = float.MinValue;
    private float _lastBellSoundPercentage = 1f;
    private bool _battleEndingNotificationGiven;
    private bool _battleEndingLateNotificationGiven;
    private Vec3 _retreatHornPosition;
    private MissionPeer? _myMissionPeer;

    public override bool IsGameModeUsingGold => false;
    public override bool IsGameModeTactical => true;
    public override bool IsGameModeUsingRoundCountdown => true;
    public override MultiplayerGameType GameType => MultiplayerGameType.Siege;
    public bool AreMoralesIndependent => true;

    public event Action<BattleSideEnum, float>? OnMoraleChangedEvent;
    public event Action? OnFlagNumberChangedEvent;
    public event Action<FlagCapturePoint, Team>? OnCapturePointOwnerChangedEvent;
    public event Action<int[]>? OnCapturePointRemainingMoraleGainsChangedEvent;

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
        foreach (FlagCapturePoint flag in AllCapturePoints)
        {
            if (flag.GameEntity.HasTag(MasterFlagTag))
            {
                _masterFlag = flag;
            }
            else if (flag.FlagIndex == 0)
            {
                MatrixFrame globalFrame = flag.GameEntity.GetGlobalFrame();
                _retreatHornPosition = globalFrame.origin + globalFrame.rotation.u * 3f;
            }
        }
    }

    public override int GetGoldAmount()
    {
        return 0;
    }

    public override void OnGoldAmountChangedForRepresentative(MissionRepresentativeBase representative, int goldAmount)
    {
    }

    public void OnCapturePointOwnerChanged(FlagCapturePoint flag, Team flagOwner)
    {
        _flagOwners[flag.FlagIndex] = flagOwner;
        OnCapturePointOwnerChangedEvent?.Invoke(flag, flagOwner);

        if (flagOwner is { Side: BattleSideEnum.Defender }
            && _remainingTimeForBellSoundToStop > IndefiniteDurationThreshold
            && flag == _masterFlag)
        {
            _bellSoundEvent!.Stop();
            _bellSoundEvent = null;
            _remainingTimeForBellSoundToStop = float.MinValue;
            _lastBellSoundPercentage += DefenderMoraleDropThresholdIncrement;
        }

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

    public void OnNumberOfFlagsChanged()
    {
        OnFlagNumberChangedEvent?.Invoke();
    }

    public void OnMoraleChanged(
        int attackerMorale,
        int defenderMorale,
        int[] capturePointRemainingMoraleGains)
    {
        float attackerMoraleRatio = attackerMorale / (float)StartingMorale;
        float defenderMoraleRatio = defenderMorale / (float)StartingMorale;
        if (_myMissionPeer?.Team != null && _myMissionPeer?.Team.Side != BattleSideEnum.None)
        {
            var masterFlagOwner = _flagOwners[_masterFlag.FlagIndex];
            if (masterFlagOwner is not { Side: BattleSideEnum.Defender } && _remainingTimeForBellSoundToStop < 0.0)
            {
                if (defenderMoraleRatio > (double)_lastBellSoundPercentage)
                {
                    _lastBellSoundPercentage += DefenderMoraleDropThresholdIncrement;
                }

                if (defenderMoraleRatio <= DefenderMoraleDropThresholdLow)
                {
                    if (_lastBellSoundPercentage > DefenderMoraleDropThresholdLow)
                    {
                        _remainingTimeForBellSoundToStop = float.MaxValue;
                        _lastBellSoundPercentage = DefenderMoraleDropThresholdLow;
                    }
                }
                else if (defenderMoraleRatio <= DefenderMoraleDropThresholdMedium)
                {
                    if (_lastBellSoundPercentage > DefenderMoraleDropThresholdMedium)
                    {
                        _remainingTimeForBellSoundToStop = DefenderMoraleDropMediumDuration;
                        _lastBellSoundPercentage = DefenderMoraleDropThresholdMedium;
                    }
                }
                else if (defenderMoraleRatio <= DefenderMoraleDropThresholdHigh)
                {
                    if (_lastBellSoundPercentage > DefenderMoraleDropThresholdHigh)
                    {
                        _remainingTimeForBellSoundToStop = DefenderMoraleDropHighDuration;
                        _lastBellSoundPercentage = DefenderMoraleDropThresholdHigh;
                    }
                }

                if (_remainingTimeForBellSoundToStop > 0.0)
                {
                    MatrixFrame masterFlagFrame = _masterFlag.GameEntity.GetGlobalFrame();
                    string bellSoundEventId = _myMissionPeer!.Team.Side == BattleSideEnum.Defender
                        ? "event:/multiplayer/warning_bells_defender"
                        : "event:/multiplayer/warning_bells_attacker";
                    _bellSoundEvent = SoundEvent.CreateEventFromString(bellSoundEventId, Mission.Scene);
                    _bellSoundEvent.PlayInPosition(masterFlagFrame.origin + masterFlagFrame.rotation.u * 3f);
                }
            }

            if (!_battleEndingNotificationGiven || !_battleEndingLateNotificationGiven)
            {
                float battleEndingNotificationThreshold = !_battleEndingNotificationGiven ? BattleWinLoseAlertThreshold : BattleWinLoseLateAlertThreshold;
                MatrixFrame cameraFrame = Mission.GetCameraFrame();
                Vec3 position = cameraFrame.origin + cameraFrame.rotation.u;
                if (attackerMoraleRatio <= battleEndingNotificationThreshold && defenderMoraleRatio > battleEndingNotificationThreshold)
                {
                    if (_myMissionPeer!.Team.Side == BattleSideEnum.Attacker)
                    {
                        MBSoundEvent.PlaySound(
                            SoundEvent.GetEventIdFromString(BattleLosingSoundEventString),
                            position);
                        MBSoundEvent.PlaySound(
                            SoundEvent.GetEventIdFromString("event:/multiplayer/retreat_horn_attacker"),
                            _retreatHornPosition);
                    }
                    else if (_myMissionPeer!.Team.Side == BattleSideEnum.Defender)
                    {
                        MBSoundEvent.PlaySound(
                            SoundEvent.GetEventIdFromString(BattleWinningSoundEventString),
                            position);
                        MBSoundEvent.PlaySound(
                            SoundEvent.GetEventIdFromString("event:/multiplayer/retreat_horn_defender"),
                            _retreatHornPosition);
                    }

                    if (_battleEndingNotificationGiven)
                    {
                        _battleEndingLateNotificationGiven = true;
                    }

                    _battleEndingNotificationGiven = true;
                }

                if (defenderMoraleRatio <= battleEndingNotificationThreshold && attackerMoraleRatio > battleEndingNotificationThreshold)
                {
                    string soundEventId = _myMissionPeer!.Team.Side == BattleSideEnum.Defender
                        ? BattleLosingSoundEventString
                        : BattleWinningSoundEventString;
                    MBSoundEvent.PlaySound(SoundEvent.GetEventIdFromString(soundEventId), position);
                    if (_battleEndingNotificationGiven)
                    {
                        _battleEndingLateNotificationGiven = true;
                    }

                    _battleEndingNotificationGiven = true;
                }
            }
        }

        OnMoraleChangedEvent?.Invoke(BattleSideEnum.Attacker, attackerMoraleRatio);
        OnMoraleChangedEvent?.Invoke(BattleSideEnum.Defender, defenderMoraleRatio);
        OnCapturePointRemainingMoraleGainsChangedEvent?.Invoke(capturePointRemainingMoraleGains);
    }

    public Team? GetFlagOwner(FlagCapturePoint flag)
    {
        return _flagOwners[flag.FlagIndex];
    }

    public override void OnMissionTick(float dt)
    {
        base.OnMissionTick(dt);
        if (_remainingTimeForBellSoundToStop <= 0.0)
        {
            return;
        }

        _remainingTimeForBellSoundToStop -= dt;
        if (_remainingTimeForBellSoundToStop > 0.0
            && MissionLobbyComponent.CurrentMultiplayerState == MissionLobbyComponent.MultiplayerGameState.Playing)
        {
            return;
        }

        _remainingTimeForBellSoundToStop = float.MinValue;
        _bellSoundEvent!.Stop();
        _bellSoundEvent = null;
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        if (!GameNetwork.IsClient)
        {
            return;
        }

        registerer.Register<SiegeMoraleChangeMessage>(HandleMoraleChangedMessage);
        registerer.Register<FlagDominationCapturePointMessage>(HandleCapturePointMessage);
        registerer.Register<FlagDominationFlagsRemovedMessage>(HandleFlagsRemovedMessage);
    }

    private void OnMyClientSynchronized()
    {
        _myMissionPeer = GameNetwork.MyPeer.GetComponent<MissionPeer>();
    }

    private void HandleMoraleChangedMessage(SiegeMoraleChangeMessage message)
    {
        OnMoraleChanged(message.AttackerMorale, message.DefenderMorale, message.CapturePointRemainingMoraleGains);
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

    private void HandleFlagsRemovedMessage(FlagDominationFlagsRemovedMessage message)
    {
        OnNumberOfFlagsChanged();
    }
}
