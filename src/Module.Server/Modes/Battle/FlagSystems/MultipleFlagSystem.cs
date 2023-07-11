using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;
using MathF = TaleWorlds.Library.MathF;
using Timer = TaleWorlds.Core.Timer;

namespace Crpg.Module.Modes.Battle.FlagSystems;

internal class MultipleFlagSystem : FlagSystem
{
    private const float FlagCaptureRange = 6f;
    private const float FlagCaptureRangeSquared = FlagCaptureRange * FlagCaptureRange;

    private const float BattleMoraleGainOnTick = 0.00175f;
    private const float BattleMoraleGainMultiplierLastFlag = 2f;
    private const int BattleFlagsRemovalTime = 120;
    private const float SkirmishMoraleGainOnTick = 0.00125f;
    private const float SkirmishMoraleGainMultiplierLastFlag = 2f;
    private const int SkirmishFlagsRemovalTime = 120;

    /// <summary>Null on client-side.</summary>
    private readonly MultiplayerGameNotificationsComponent _notificationsComponent;
    private readonly bool _isSkirmish;

    /// <summary>True if captures points were removed and only one remains.</summary>
    private bool _wereFlagsRemoved;
    private Timer? _checkFlagRemovalTimer;

    /// <summary>A number between -1.0 and 1.0. Less than 0 means the defenders are winning. Greater than 0 for attackers.</summary>
    private float _morale;
    private FlagCapturePoint[] _flags = Array.Empty<FlagCapturePoint>();
    private Team?[] _flagOwners = Array.Empty<Team>();
    private int[,] _agentCountsAroundFlags = new int[0, 0];
    private float _remainingTimeForBellSoundToStop = float.MinValue;
    private SoundEvent? _bellSoundEvent;
    private bool _notifiedForFlagRemoval;

    public override IEnumerable<FlagCapturePoint> AllCapturePoints => _flags;

    public override event Action<BattleSideEnum, float>? OnMoraleChangedEvent;
    public override event Action? OnFlagNumberChangedEvent;
    public override event Action<FlagCapturePoint, Team>? OnCapturePointOwnerChangedEvent;

    public MultipleFlagSystem(Mission mission, MultiplayerGameNotificationsComponent notificationsComponent, bool isSkirmish)
        : base(mission)
    {
        _notificationsComponent = notificationsComponent;
        _isSkirmish = isSkirmish;
    }

    public override void Reset()
    {
        _morale = 0f;
        ResetFlags();

        if (GameNetwork.IsClient)
        {
            _notifiedForFlagRemoval = false;
            if (_bellSoundEvent != null)
            {
                _remainingTimeForBellSoundToStop = float.MinValue;
                _bellSoundEvent.Stop();
                _bellSoundEvent = null;
            }
        }
    }

    public override void Tick(float dt)
    {
        if (GameNetwork.IsServer)
        {
            CheckRemovalOfFlagsServer();
            CheckMoralesServer();
            TickFlagsServer();
        }
        else if (GameNetwork.IsClient)
        {
            TickBellClient(dt);
        }
    }

    public override Team? GetFlagOwner(FlagCapturePoint flag)
    {
        return _flagOwners[flag.FlagIndex];
    }

    public override bool HasRoundEnded()
    {
        if (Math.Abs(_morale) < 1.0)
        {
            return false;
        }

        if (!_wereFlagsRemoved)
        {
            return true;
        }

        var lastFlag = _flags.First(flag => !flag.IsDeactivated);
        var lastFlagOwner = GetFlagOwner(lastFlag);
        if (lastFlagOwner == null)
        {
            return true;
        }

        var winningSide = _morale > 0.0f
            ? BattleSideEnum.Attacker
            : BattleSideEnum.Defender;
        return lastFlagOwner.Side == winningSide
               && lastFlag.IsFullyRaised
               && GetNumberOfAttackersAroundFlagServer(lastFlag) == 0;
    }

    public override (BattleSideEnum side, RoundEndReason reason) GetRoundWinner(bool timedOut)
    {
        BattleSideEnum winnerSide = BattleSideEnum.None;
        for (BattleSideEnum side = BattleSideEnum.Defender; side < BattleSideEnum.NumSides; side += 1)
        {
            int moraleSide = side == BattleSideEnum.Defender ? -1 : 1;
            if ((timedOut && moraleSide * _morale > 0.0f) || (!timedOut && moraleSide * _morale >= 1.0f))
            {
                winnerSide = side;
                break;
            }
        }

        RoundEndReason endReason;
        if (winnerSide != BattleSideEnum.None)
        {
            endReason = timedOut ? RoundEndReason.RoundTimeEnded : RoundEndReason.GameModeSpecificEnded;
        }
        else
        {
            bool defenderTeamAlive = Mission.DefenderTeam.ActiveAgents.Count > 0;
            bool attackerTeamAlive = Mission.AttackerTeam.ActiveAgents.Count > 0;
            if (defenderTeamAlive && attackerTeamAlive)
            {
                if (_morale > 0.0f)
                {
                    winnerSide = BattleSideEnum.Attacker;
                }
                else if (_morale < 0.0)
                {
                    winnerSide = BattleSideEnum.Defender;
                }
                else
                {
                    winnerSide = BattleSideEnum.None;
                }

                endReason = RoundEndReason.RoundTimeEnded;
            }
            else if (defenderTeamAlive)
            {
                winnerSide = BattleSideEnum.Defender;
                endReason = RoundEndReason.SideDepleted;
            }
            else if (attackerTeamAlive)
            {
                winnerSide = BattleSideEnum.Attacker;
                endReason = RoundEndReason.SideDepleted;
            }
            else // Everyone ded
            {
                winnerSide = BattleSideEnum.None;
                endReason = RoundEndReason.SideDepleted;
            }
        }

        return (winnerSide, endReason);
    }

    public override bool ShouldOvertime()
    {
        if (!_wereFlagsRemoved)
        {
            return false;
        }

        var lastFlag = _flags.First(flag => !flag.IsDeactivated);
        var owner = GetFlagOwner(lastFlag);
        if (owner == null)
        {
            return false;
        }

        int moraleOwnerSide = owner.Side == BattleSideEnum.Defender ? -1 : 1;
        return moraleOwnerSide * _morale < 0.0 || GetNumberOfAttackersAroundFlagServer(lastFlag) > 0;
    }

    public override int GetWarningTimer(float remainingRoundTime, float roundTimeLimit)
    {
        int flagRemovalTime = _isSkirmish ? SkirmishFlagsRemovalTime : BattleFlagsRemovalTime;
        float timerStart = roundTimeLimit - flagRemovalTime;
        float timerEnd = timerStart + 30f;
        if (remainingRoundTime < timerStart || remainingRoundTime > timerEnd)
        {
            return 0;
        }

        int warningTimer = MathF.Ceiling(30.0f - timerEnd - remainingRoundTime);
        if (!_notifiedForFlagRemoval)
        {
            _notifiedForFlagRemoval = true;
            _notificationsComponent.FlagsWillBeRemovedInXSeconds(30);
        }

        return warningTimer;
    }

    public override void HandleNewClient(NetworkCommunicator networkPeer)
    {
        GameNetwork.BeginModuleEventAsServer(networkPeer);
        GameNetwork.WriteMessage(new FlagDominationMoraleChangeMessage(_morale));
        GameNetwork.EndModuleEventAsServer();
    }

    public override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        if (GameNetwork.IsClient)
        {
            registerer.Register<FlagDominationMoraleChangeMessage>(OnMoraleChangeClient);
            registerer.Register<FlagDominationCapturePointMessage>(OnCapturePointClient);
            registerer.Register<FlagDominationFlagsRemovedMessage>(OnFlagsRemovedClient);
        }
    }

    private void ResetFlags()
    {
        _flags = Mission.MissionObjects.FindAllWithType<FlagCapturePoint>().ToArray();
        _flagOwners = new Team[_flags.Length];

        if (GameNetwork.IsServer)
        {
            ThrowOnBadFlagIndexes(_flags);
            _agentCountsAroundFlags = new int[_flags.Length, (int)BattleSideEnum.NumSides];
            _checkFlagRemovalTimer = null;
            _wereFlagsRemoved = false;
            foreach (var flag in _flags)
            {
                flag.ResetPointAsServer(
                    TeammateColorsExtensions.NEUTRAL_COLOR,
                    TeammateColorsExtensions.NEUTRAL_COLOR2);
            }
        }
    }

    /// <summary>Checks the flag index are from 0 to N.</summary>
    private void ThrowOnBadFlagIndexes(FlagCapturePoint[] flags)
    {
        int expectedIndex = 0;
        foreach (var flag in flags.OrderBy(f => f.FlagIndex))
        {
            if (flag.FlagIndex != expectedIndex)
            {
                throw new Exception($"Invalid scene '{Mission.SceneName}': Flag indexes should be numbered from 0 to {flags.Length}");
            }

            expectedIndex += 1;
        }
    }

    private void CheckMoralesServer()
    {
        float moraleGain = GetMoraleGainServer();
        if (moraleGain == 0)
        {
            return;
        }

        _morale += moraleGain;
        _morale = MBMath.ClampFloat(_morale, -1f, 1f);

        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new FlagDominationMoraleChangeMessage(_morale));
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
    }

    private float GetMoraleGainServer()
    {
        FlagCapturePoint[] capturedFlags = _flags
            .Where(flag => !flag.IsDeactivated && GetFlagOwner(flag) != null && flag.IsFullyRaised)
            .ToArray();
        int teamFlagsDelta = capturedFlags.Count(flag => GetFlagOwner(flag)!.Side == BattleSideEnum.Attacker)
                             - capturedFlags.Count(flag => GetFlagOwner(flag)!.Side == BattleSideEnum.Defender);
        if (teamFlagsDelta == 0)
        {
            return 0f;
        }

        float moraleGainOnTick = _isSkirmish ? SkirmishMoraleGainOnTick : BattleMoraleGainOnTick;
        float moraleGainMultiplierLastFlag = _isSkirmish ? SkirmishMoraleGainMultiplierLastFlag : BattleMoraleGainMultiplierLastFlag;

        float moraleMultiplier = moraleGainOnTick * Math.Abs(teamFlagsDelta);
        float moraleGain = teamFlagsDelta <= 0
            ? MBMath.ClampFloat(-1 - _morale, -2f, -1f) * moraleMultiplier
            : MBMath.ClampFloat(1 - _morale, 1f, 2f) * moraleMultiplier;
        if (_wereFlagsRemoved) // For the last flag, the morale is moving faster.
        {
            moraleGain *= moraleGainMultiplierLastFlag;
        }

        return moraleGain;
    }

    private void ChangeMoraleClient(float morale)
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

        BattleSideEnum mySide = GameNetwork.MyPeer?.GetComponent<MissionPeer>()?.Team?.Side ?? BattleSideEnum.None;
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

    private void TickFlagsServer()
    {
        foreach (var flag in _flags)
        {
            if (flag.IsDeactivated)
            {
                continue;
            }

            int[] agentCountsAroundFlag = new int[(int)BattleSideEnum.NumSides];

            Agent? closestAgentToFlag = null;
            float closestAgentDistanceToFlagSquared = 16f; // Where does this number come from?
            var proximitySearch = AgentProximityMap.BeginSearch(Mission, flag.Position.AsVec2, FlagCaptureRange);
            for (; proximitySearch.LastFoundAgent != null; AgentProximityMap.FindNext(Mission, ref proximitySearch))
            {
                Agent agent = proximitySearch.LastFoundAgent;
                if (!agent.IsActive() || !agent.IsHuman || (!_isSkirmish && agent.HasMount))
                {
                    continue;
                }

                agentCountsAroundFlag[(int)agent.Team.Side] += 1;
                float distanceToFlagSquared = agent.Position.DistanceSquared(flag.Position);
                if (distanceToFlagSquared <= closestAgentDistanceToFlagSquared)
                {
                    closestAgentToFlag = agent;
                    closestAgentDistanceToFlagSquared = distanceToFlagSquared;
                }
            }

            bool agentCountsAroundFlagChanged =
                agentCountsAroundFlag[(int)BattleSideEnum.Defender] != _agentCountsAroundFlags[flag.FlagIndex, (int)BattleSideEnum.Defender]
                || agentCountsAroundFlag[(int)BattleSideEnum.Attacker] != _agentCountsAroundFlags[flag.FlagIndex, (int)BattleSideEnum.Attacker];
            _agentCountsAroundFlags[flag.FlagIndex, (int)BattleSideEnum.Defender] = agentCountsAroundFlag[(int)BattleSideEnum.Defender];
            _agentCountsAroundFlags[flag.FlagIndex, (int)BattleSideEnum.Attacker] = agentCountsAroundFlag[(int)BattleSideEnum.Attacker];
            float speedMultiplier = 1f;
            if (closestAgentToFlag != null)
            {
                BattleSideEnum side = closestAgentToFlag.Team.Side;
                BattleSideEnum oppositeSide = side.GetOppositeSide();
                if (agentCountsAroundFlag[(int)oppositeSide] != 0)
                {
                    int val1 = Math.Min(agentCountsAroundFlag[(int)side], 200);
                    int val2 = Math.Min(agentCountsAroundFlag[(int)oppositeSide], 200);
                    speedMultiplier = Math.Min(1f, (MathF.Log10(val1) + 1.0f) / (2.0f * (MathF.Log10(val2) + 1.0f)) - 0.09f);
                }
            }

            var flagOwner = GetFlagOwner(flag);
            if (flagOwner == null)
            {
                if (!flag.IsContested && closestAgentToFlag != null)
                {
                    flag.SetMoveFlag(CaptureTheFlagFlagDirection.Down, speedMultiplier);
                }
                else if (closestAgentToFlag == null & flag.IsContested)
                {
                    flag.SetMoveFlag(CaptureTheFlagFlagDirection.Up, speedMultiplier);
                }
                else if (agentCountsAroundFlagChanged)
                {
                    flag.ChangeMovementSpeed(speedMultiplier);
                }
            }
            else if (closestAgentToFlag != null)
            {
                if (closestAgentToFlag.Team != flagOwner && !flag.IsContested)
                {
                    flag.SetMoveFlag(CaptureTheFlagFlagDirection.Down, speedMultiplier);
                }
                else if (closestAgentToFlag.Team == flagOwner & flag.IsContested)
                {
                    flag.SetMoveFlag(CaptureTheFlagFlagDirection.Up, speedMultiplier);
                }
                else if (agentCountsAroundFlagChanged)
                {
                    flag.ChangeMovementSpeed(speedMultiplier);
                }
            }
            else if (flag.IsContested)
            {
                flag.SetMoveFlag(CaptureTheFlagFlagDirection.Up, speedMultiplier);
            }
            else if (agentCountsAroundFlagChanged)
            {
                flag.ChangeMovementSpeed(speedMultiplier);
            }

            flag.OnAfterTick(closestAgentToFlag != null, out bool flagOwnerChanged);
            if (flagOwnerChanged)
            {
                Team team = closestAgentToFlag!.Team!;
                flag.SetTeamColorsWithAllSynched(team.Color, team.Color2);
                _flagOwners[flag.FlagIndex] = team;
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new FlagDominationCapturePointMessage(flag.FlagIndex, team));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
                _notificationsComponent.FlagXCapturedByTeamX(flag, closestAgentToFlag.Team);
                MPPerkObject.RaiseEventForAllPeers(MPPerkCondition.PerkEventFlags.FlagCapture);
            }
        }
    }

    private void CheckRemovalOfFlagsServer()
    {
        if (_wereFlagsRemoved)
        {
            return;
        }

        _checkFlagRemovalTimer ??= new Timer(Mission.CurrentTime,
            _isSkirmish ? SkirmishFlagsRemovalTime : BattleFlagsRemovalTime);
        if (!_checkFlagRemovalTimer.Check(Mission.CurrentTime))
        {
            return;
        }

        var lastFlag = GetLastFlagServer();

        int[] flagIndexesToRemove = _flags
            .Where(f => f.FlagIndex != lastFlag.FlagIndex)
            .Select(RemoveFlagServer)
            .ToArray();

        _wereFlagsRemoved = true;

        if (flagIndexesToRemove.Length > 0) // In case there is only one flag on the map.
        {
            _notificationsComponent.FlagXRemaining(lastFlag);

            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(new FlagDominationFlagsRemovedMessage());
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);

            Debug.Print("Flags were removed");
        }
    }

    private int GetNumberOfAttackersAroundFlagServer(FlagCapturePoint flag)
    {
        var flagOwner = GetFlagOwner(flag);
        if (flagOwner == null)
        {
            return 0;
        }

        int count = 0;
        var proximitySearch = AgentProximityMap.BeginSearch(Mission, flag.Position.AsVec2, FlagCaptureRange);
        for (; proximitySearch.LastFoundAgent != null; AgentProximityMap.FindNext(Mission, ref proximitySearch))
        {
            Agent agent = proximitySearch.LastFoundAgent;
            if (agent.IsHuman
                && agent.IsActive()
                && agent.Position.DistanceSquared(flag.Position) <= FlagCaptureRangeSquared
                && agent.Team.Side != flagOwner.Side)
            {
                count += 1;
            }
        }

        return count;
    }

    private void TickBellClient(float dt)
    {
        if (_remainingTimeForBellSoundToStop > 0.0)
        {
            _remainingTimeForBellSoundToStop -= dt;
        }

        if (_bellSoundEvent == null || _remainingTimeForBellSoundToStop > 0.0)
        {
            return;
        }

        _remainingTimeForBellSoundToStop = float.MinValue;
        _bellSoundEvent.Stop();
        _bellSoundEvent = null;
    }

    /// <summary>Gets the last flag that should not be removed.</summary>
    private FlagCapturePoint GetLastFlagServer()
    {
        var uncapturedFlags = _flags.Where(f => GetFlagOwner(f) == null).ToArray();
        var defenderFlags = _flags.Where(f => GetFlagOwner(f)?.Side == BattleSideEnum.Defender).ToArray();
        var attackerFlags = _flags.Where(f => GetFlagOwner(f)?.Side == BattleSideEnum.Attacker).ToArray();

        if (uncapturedFlags.Length == _flags.Length)
        {
            Debug.Print("Last flag is a random uncaptured one");
            return uncapturedFlags.GetRandomElement();
        }

        if (defenderFlags.Length == attackerFlags.Length)
        {
            if (uncapturedFlags.Length != 0)
            {
                Debug.Print("Last flag is a random uncaptured one");
                return uncapturedFlags.GetRandomElement();
            }

            Debug.Print("Last flag is a random captured one");
            return _flags.GetRandomElement();
        }

        var dominatingTeamFlags = defenderFlags.Length > attackerFlags.Length ? defenderFlags : attackerFlags;

        var contestedFlags = dominatingTeamFlags.Where(f => GetNumberOfAttackersAroundFlagServer(f) > 0).ToArray();
        if (contestedFlags.Length > 0)
        {
            Debug.Print("Last flag is a contested one of the dominating team");
            return contestedFlags.GetRandomElement();
        }

        Debug.Print("Last flag is a random one of the dominating team");
        return dominatingTeamFlags.GetRandomElement();
    }

    private int RemoveFlagServer(FlagCapturePoint flag)
    {
        flag.RemovePointAsServer();
        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new FlagDominationCapturePointMessage(flag.FlagIndex, null));
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
        return flag.FlagIndex;
    }

    private void CaptureFlagClient(FlagCapturePoint flag, Team owner)
    {
        _flagOwners[flag.FlagIndex] = owner;
        OnCapturePointOwnerChangedEvent?.Invoke(flag, owner);

        var myTeam = GameNetwork.MyPeer?.GetComponent<MissionPeer>()?.Team;
        if (myTeam == null)
        {
            return;
        }

        MatrixFrame cameraFrame = Mission.GetCameraFrame();
        Vec3 position = cameraFrame.origin + cameraFrame.rotation.u;
        string eventStr = myTeam == owner
            ? "event:/alerts/report/flag_captured"
            : "event:/alerts/report/flag_lost";
        MBSoundEvent.PlaySound(SoundEvent.GetEventIdFromString(eventStr), position);
    }

    private void OnMoraleChangeClient(FlagDominationMoraleChangeMessage message)
    {
        ChangeMoraleClient(message.Morale);
    }

    private void OnCapturePointClient(FlagDominationCapturePointMessage message)
    {
        var capturedFlag = _flags.FirstOrDefault(flag => flag.FlagIndex == message.FlagIndex);
        if (capturedFlag == null)
        {
            return;
        }

        CaptureFlagClient(capturedFlag, message.OwnerTeam);
    }

    private void OnFlagsRemovedClient(FlagDominationFlagsRemovedMessage message)
    {
        OnFlagNumberChangedEvent?.Invoke();
    }
}
