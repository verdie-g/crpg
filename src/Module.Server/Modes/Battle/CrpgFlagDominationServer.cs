using Crpg.Module.Modes.Skirmish;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.MissionRepresentatives;
using TaleWorlds.MountAndBlade.Objects;
using TaleWorlds.ObjectSystem;
using MathF = TaleWorlds.Library.MathF;
using Timer = TaleWorlds.Core.Timer;

namespace Crpg.Module.Modes.Battle;

internal class CrpgFlagDominationServer : MissionMultiplayerGameModeBase
{
    private const float FlagCaptureRange = 6f;
    private const float FlagCaptureRangeSquared = FlagCaptureRange * FlagCaptureRange;

    private const float BattleMoraleGainOnTick = 0.00175f;
    private const float BattleMoraleGainMultiplierLastFlag = 2f;
    private const float SkirmishMoraleGainOnTick = 0.00125f;
    private const float SkirmishMoraleGainMultiplierLastFlag = 2f;

    private readonly CrpgFlagDominationClient _flagDominationClient;
    private readonly bool _isSkirmish;

    /// <summary>A number between -1.0 and 1.0. Less than 0 means the defenders are winning. Greater than 0 for attackers.</summary>
    private float _morale;
    private FlagCapturePoint[] _flags = Array.Empty<FlagCapturePoint>();
    private Team?[] _flagOwners = Array.Empty<Team>();
    private int[,] _agentCountsAroundFlags = new int[0, 0];

    /// <summary>True if captures points were removed and only one remains.</summary>
    private bool _wereFlagsRemoved;
    private Timer? _checkFlagRemovalTimer;

    public override bool IsGameModeHidingAllAgentVisuals => true;
    public override bool IsGameModeUsingOpposingTeams => true;
    public override bool AllowCustomPlayerBanners() => false;
    public override bool UseRoundController() => true;

    public CrpgFlagDominationServer(CrpgFlagDominationClient flagDominationClient, bool isSkirmish)
    {
        _flagDominationClient = flagDominationClient;
        _isSkirmish = isSkirmish;
    }

    public override MissionLobbyComponent.MultiplayerGameType GetMissionType()
    {
        return MissionLobbyComponent.MultiplayerGameType.Battle;
    }

    public override void AfterStart()
    {
        base.AfterStart();
        RoundController.OnPreRoundEnding += OnPreRoundEnding;

        AddTeams();
    }

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();

        ResetFlags();
        _morale = 0f;
        // TODO: SetTeamColorsWithAllSynched
    }

    public override void OnRemoveBehavior()
    {
        RoundController.OnPreRoundEnding -= OnPreRoundEnding;
        base.OnRemoveBehavior();
    }

    public override void OnClearScene()
    {
        ResetFlags();
        _morale = 0.0f;
        _checkFlagRemovalTimer = null;
        _wereFlagsRemoved = false;
    }

    public override bool CheckForWarmupEnd()
    {
        int playersInTeam = 0;
        foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
        {
            MissionPeer component = networkPeer.GetComponent<MissionPeer>();
            if (networkPeer.IsSynchronized && component?.Team != null && component.Team.Side != BattleSideEnum.None)
            {
                playersInTeam += 1;
            }
        }

        return playersInTeam >= MultiplayerOptions.OptionType.MaxNumberOfPlayers.GetIntValue();
    }

    public override void OnMissionTick(float dt)
    {
        base.OnMissionTick(dt);
        if (MissionLobbyComponent.CurrentMultiplayerState != MissionLobbyComponent.MultiplayerGameState.Playing
            || !RoundController.IsRoundInProgress
            || !CanGameModeSystemsTickThisFrame
            || _flags.Length == 0) // Protection against scene with no flags.
        {
            return;
        }

        CheckRemovalOfFlags();
        CheckMorales();
        TickFlags();
    }

    public override bool CheckForRoundEnd()
    {
        if (!CanGameModeSystemsTickThisFrame)
        {
            return false;
        }

        if (_flags.Length != 0 && Math.Abs(_morale) >= 1.0)
        {
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
                   && GetNumberOfAttackersAroundFlag(lastFlag) == 0;
        }

        if (SpawnComponent.SpawningBehavior is CrpgBattleSpawningBehavior s && !s.SpawnDelayEnded())
        {
            return false;
        }

        bool defenderTeamDepleted = Mission.DefenderTeam.ActiveAgents.Count == 0;
        bool attackerTeamDepleted = Mission.AttackerTeam.ActiveAgents.Count == 0;
        if (!_isSkirmish)
        {
            return defenderTeamDepleted || attackerTeamDepleted;
        }

        bool defenderCanSpawn = false;
        bool attackerCanSpawn = false;
        foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
        {
            MissionPeer missionPeer = networkPeer.GetComponent<MissionPeer>();
            if (missionPeer?.Team == null || missionPeer.SpawnCountThisRound >= CrpgSkirmishSpawningBehavior.MaxSpawns)
            {
                continue;
            }

            if (missionPeer.Team.Side == BattleSideEnum.Defender)
            {
                defenderCanSpawn = true;
            }
            else if (missionPeer.Team.Side == BattleSideEnum.Attacker)
            {
                attackerCanSpawn = true;
            }
        }

        return (defenderTeamDepleted && !defenderCanSpawn) || (attackerTeamDepleted && !attackerCanSpawn);
    }

    public override void OnAgentBuild(Agent agent, Banner banner)
    {
        agent.UpdateSyncHealthToAllClients(true); // Why is that needed
    }

    public override bool CheckIfOvertime()
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
        return moraleOwnerSide * _morale < 0.0 || GetNumberOfAttackersAroundFlag(lastFlag) > 0;
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
    }

    protected override void HandleNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
    {
        GameNetwork.BeginModuleEventAsServer(networkPeer);
        GameNetwork.WriteMessage(new FlagDominationMoraleChangeMessage(_morale));
        GameNetwork.EndModuleEventAsServer();
    }

    protected override void HandleEarlyNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
    {
        networkPeer.AddComponent<FlagDominationMissionRepresentative>();
    }

    private void AddTeams()
    {
        BasicCultureObject cultureTeam1 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue());
        Banner bannerTeam1 = new(cultureTeam1.BannerKey, cultureTeam1.BackgroundColor1, cultureTeam1.ForegroundColor1);
        Mission.Teams.Add(BattleSideEnum.Attacker, cultureTeam1.BackgroundColor1, cultureTeam1.ForegroundColor1, bannerTeam1, false, true);
        BasicCultureObject cultureTeam2 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue());
        Banner bannerTeam2 = new(cultureTeam2.BannerKey, cultureTeam2.BackgroundColor2, cultureTeam2.ForegroundColor2);
        Mission.Teams.Add(BattleSideEnum.Defender, cultureTeam2.BackgroundColor2, cultureTeam2.ForegroundColor2, bannerTeam2, false, true);
    }

    private void ResetFlags()
    {
        _flags = Mission.Current.MissionObjects.FindAllWithType<FlagCapturePoint>().ToArray();
        ThrowOnBadFlagIndexes(_flags);
        _flagOwners = new Team[_flags.Length];
        _agentCountsAroundFlags = new int[_flags.Length, (int)BattleSideEnum.NumSides];
        foreach (var flag in _flags)
        {
            flag.ResetPointAsServer(TeammateColorsExtensions.NEUTRAL_COLOR, TeammateColorsExtensions.NEUTRAL_COLOR2);
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
                throw new Exception($"Invalid scene '{Mission.Current?.SceneName}': Flag indexes should be numbered from 0 to {flags.Length}");
            }

            expectedIndex += 1;
        }
    }

    private void CheckMorales()
    {
        float moraleGain = GetMoraleGain();
        if (moraleGain == 0)
        {
            return;
        }

        _morale += moraleGain;
        _morale = MBMath.ClampFloat(_morale, -1f, 1f);

        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new FlagDominationMoraleChangeMessage(_morale));
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);

        _flagDominationClient.ChangeMorale(_morale);
    }

    private float GetMoraleGain()
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

    private void TickFlags()
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
            var proximitySearch = AgentProximityMap.BeginSearch(Mission.Current, flag.Position.AsVec2, FlagCaptureRange);
            for (; proximitySearch.LastFoundAgent != null; AgentProximityMap.FindNext(Mission.Current, ref proximitySearch))
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
                _flagDominationClient.CaptureFlag(flag, team);
                NotificationsComponent.FlagXCapturedByTeamX(flag, closestAgentToFlag.Team);
                MPPerkObject.RaiseEventForAllPeers(MPPerkCondition.PerkEventFlags.FlagCapture);
            }
        }
    }

    private void CheckRemovalOfFlags()
    {
        if (_wereFlagsRemoved)
        {
            return;
        }

        _checkFlagRemovalTimer ??= new Timer(Mission.CurrentTime, _flagDominationClient.FlagsRemovalTime);
        if (!_checkFlagRemovalTimer.Check(Mission.CurrentTime))
        {
            return;
        }

        var lastFlag = GetLastFlag();

        int[] flagIndexesToRemove = _flags
            .Where(f => f.FlagIndex != lastFlag.FlagIndex)
            .Select(RemoveFlag)
            .ToArray();

        _wereFlagsRemoved = true;

        if (flagIndexesToRemove.Length > 0) // In case there is only one flag on the map.
        {
            NotificationsComponent.FlagXRemaining(lastFlag);

            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(new FlagDominationFlagsRemovedMessage());
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);

            _flagDominationClient.ChangeNumberOfFlags();
            Debug.Print("Flags were removed");
        }
    }

    private Team? GetFlagOwner(FlagCapturePoint flag)
    {
        return _flagOwners[flag.FlagIndex];
    }

    private int GetNumberOfAttackersAroundFlag(FlagCapturePoint flag)
    {
        var flagOwner = GetFlagOwner(flag);
        if (flagOwner == null)
        {
            return 0;
        }

        int count = 0;
        var proximitySearch = AgentProximityMap.BeginSearch(Mission.Current, flag.Position.AsVec2, FlagCaptureRange);
        for (; proximitySearch.LastFoundAgent != null; AgentProximityMap.FindNext(Mission.Current, ref proximitySearch))
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

    /// <summary>Gets the last flag that should not be removed.</summary>
    private FlagCapturePoint GetLastFlag()
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

        var contestedFlags = dominatingTeamFlags.Where(f => GetNumberOfAttackersAroundFlag(f) > 0).ToArray();
        if (contestedFlags.Length > 0)
        {
            Debug.Print("Last flag is a contested one of the dominating team");
            return contestedFlags.GetRandomElement();
        }

        Debug.Print("Last flag is a random one of the dominating team");
        return dominatingTeamFlags.GetRandomElement();
    }

    private int RemoveFlag(FlagCapturePoint flag)
    {
        flag.RemovePointAsServer();
        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new FlagDominationCapturePointMessage(flag.FlagIndex, null));
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
        return flag.FlagIndex;
    }

    private void OnPreRoundEnding()
    {
        foreach (var flag in _flags)
        {
            if (!flag.IsDeactivated)
            {
                flag.SetMoveNone();
            }
        }

        bool timedOut = RoundController.RemainingRoundTime <= 0.0 && !CheckIfOvertime();

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

        CaptureTheFlagCaptureResultEnum roundResult;
        if (winnerSide != BattleSideEnum.None)
        {
            roundResult = winnerSide == BattleSideEnum.Defender
                ? CaptureTheFlagCaptureResultEnum.DefendersWin
                : CaptureTheFlagCaptureResultEnum.AttackersWin;
            RoundController.RoundWinner = winnerSide;
            RoundController.RoundEndReason = timedOut ? RoundEndReason.RoundTimeEnded : RoundEndReason.GameModeSpecificEnded;
        }
        else
        {
            bool defenderTeamAlive = Mission.DefenderTeam.ActiveAgents.Count > 0;
            bool attackerTeamAlive = Mission.AttackerTeam.ActiveAgents.Count > 0;
            if (defenderTeamAlive && attackerTeamAlive)
            {
                if (_morale > 0.0f)
                {
                    roundResult = CaptureTheFlagCaptureResultEnum.AttackersWin;
                    RoundController.RoundWinner = BattleSideEnum.Attacker;
                }
                else if (_morale < 0.0)
                {
                    roundResult = CaptureTheFlagCaptureResultEnum.DefendersWin;
                    RoundController.RoundWinner = BattleSideEnum.Defender;
                }
                else
                {
                    roundResult = CaptureTheFlagCaptureResultEnum.Draw;
                    RoundController.RoundWinner = BattleSideEnum.None;
                }

                RoundController.RoundEndReason = RoundEndReason.RoundTimeEnded;
            }
            else if (defenderTeamAlive)
            {
                roundResult = CaptureTheFlagCaptureResultEnum.DefendersWin;
                RoundController.RoundWinner = BattleSideEnum.Defender;
                RoundController.RoundEndReason = RoundEndReason.SideDepleted;
            }
            else if (attackerTeamAlive)
            {
                roundResult = CaptureTheFlagCaptureResultEnum.AttackersWin;
                RoundController.RoundWinner = BattleSideEnum.Attacker;
                RoundController.RoundEndReason = RoundEndReason.SideDepleted;
            }
            else // Everyone ded
            {
                roundResult = CaptureTheFlagCaptureResultEnum.Draw;
                RoundController.RoundWinner = BattleSideEnum.None;
                RoundController.RoundEndReason = RoundEndReason.SideDepleted;
            }
        }

        Debug.Print($"Team {RoundController.RoundWinner} won on map {Mission.SceneName} with {GameNetwork.NetworkPeers.Count()} players");
        CheerForRoundEnd(roundResult);
    }

    private void CheerForRoundEnd(CaptureTheFlagCaptureResultEnum roundResult)
    {
        AgentVictoryLogic missionBehavior = Mission.GetMissionBehavior<AgentVictoryLogic>();
        if (roundResult == CaptureTheFlagCaptureResultEnum.AttackersWin)
        {
            missionBehavior.SetTimersOfVictoryReactionsOnBattleEnd(BattleSideEnum.Attacker);
        }
        else if (roundResult == CaptureTheFlagCaptureResultEnum.DefendersWin)
        {
            missionBehavior.SetTimersOfVictoryReactionsOnBattleEnd(BattleSideEnum.Defender);
        }
    }
}
