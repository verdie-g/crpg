using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.MissionRepresentatives;
using TaleWorlds.MountAndBlade.Objects;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.Modes.Conquest;

internal class CrpgConquestServer : MissionMultiplayerGameModeBase, IAnalyticsFlagInfo
{
    private const float FlagCaptureRange = 4f;
    private const float FlagCaptureRangeSquared = FlagCaptureRange * FlagCaptureRange;

    private readonly MissionScoreboardComponent _missionScoreboardComponent;

    private Team?[] _flagOwners = Array.Empty<Team>();
    private FlagCapturePoint[][] _flagStages = Array.Empty<FlagCapturePoint[]>();
    private bool _gameStarted;
    private int _currentStage;
    private MissionTimer _flagTickTimer = default!;
    private MissionTimer _currentStageTimer = default!;

    public CrpgConquestServer(
        MissionScoreboardComponent missionScoreboardComponent)
    {
        _missionScoreboardComponent = missionScoreboardComponent;
    }

    public override bool IsGameModeHidingAllAgentVisuals => true;

    public override bool IsGameModeUsingOpposingTeams => true;

    public override MissionLobbyComponent.MultiplayerGameType GetMissionType()
        => MissionLobbyComponent.MultiplayerGameType.FreeForAll; // Helps to avoid a few crashes.

    public override bool UseRoundController() => false;

    public MBReadOnlyList<FlagCapturePoint> AllCapturePoints { get; private set; } = new(new List<FlagCapturePoint>());

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();

        AddTeams();
        ResetDoors(true);
        ResetFlags();
    }

    public override void OnMissionTick(float dt)
    {
        base.OnMissionTick(dt);
        if (MissionLobbyComponent.CurrentMultiplayerState != MissionLobbyComponent.MultiplayerGameState.Playing
            || WarmupComponent.IsInWarmup)
        {
            return;
        }

        if (!_gameStarted)
        {
            StartStage(0);
            _gameStarted = true;
        }

        TickFlags();
    }

    public override bool CheckForMatchEnd()
    {
        return _currentStageTimer.Check() || _currentStage >= _flagStages.Length;
    }

    public override Team GetWinnerTeam()
    {
        Team? winnerTeam;
        if (_currentStageTimer.Check())
        {
            winnerTeam = Mission.Teams.Defender;
        }
        else if (_currentStage >= _flagStages.Length)
        {
            winnerTeam = Mission.Teams.Attacker;
        }
        else
        {
            winnerTeam = Mission.Teams.Defender;
        }

        _missionScoreboardComponent.ChangeTeamScore(winnerTeam, 1);

        Debug.Print($"Team {winnerTeam.Side} won on map {Mission.SceneName} with {GameNetwork.NetworkPeers.Count()} players");

        return winnerTeam;
    }

    public Team? GetFlagOwnerTeam(FlagCapturePoint flag)
    {
        return _flagOwners[flag.FlagIndex];
    }

    public override bool CheckForWarmupEnd()
    {
        return true;
        return false;
    }

    public override void OnClearScene()
    {
        base.OnClearScene();
        ClearPeerCounts();
        ResetDoors(false);
    }

    protected override void HandleEarlyNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
    {
        // Adding this component triggers some needed events in the HUD
        networkPeer.AddComponent<SiegeMissionRepresentative>();
    }

    protected override void HandleNewClientAfterSynchronized(NetworkCommunicator networkPeer)
    {
        if (networkPeer.IsServerPeer)
        {
            return;
        }

        foreach (var flag in AllCapturePoints)
        {
            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new FlagDominationCapturePointMessage(flag.FlagIndex, _flagOwners[flag.FlagIndex]));
            GameNetwork.EndModuleEventAsServer();
        }

        if (_gameStarted)
        {
            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new CrpgConquestStageStartMessage
            {
                StageIndex = _currentStage,
                StageStartTime = (int)_currentStageTimer.GetStartTime().ToSeconds,
                StageDuration = (int)_currentStageTimer.GetTimerDuration(),
            });
            GameNetwork.EndModuleEventAsServer();
        }
    }

    protected override void HandleNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
    {
    }

    private void AddTeams()
    {
        BasicCultureObject attackerCulture = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue());
        Banner attackerBanner = new(attackerCulture.BannerKey, attackerCulture.BackgroundColor1, attackerCulture.ForegroundColor1);
        Mission.Teams.Add(BattleSideEnum.Attacker, attackerCulture.BackgroundColor1, attackerCulture.ForegroundColor1, attackerBanner, false, true);
        BasicCultureObject defenderCulture = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue());
        Banner defenderBanner = new(defenderCulture.BannerKey, defenderCulture.BackgroundColor2, defenderCulture.ForegroundColor2);
        Mission.Teams.Add(BattleSideEnum.Defender, defenderCulture.BackgroundColor2, defenderCulture.ForegroundColor2, defenderBanner, false, true);
    }

    private void ResetDoors(bool enable)
    {
        foreach (CastleGate castleGate in Mission.MissionObjects.FindAllWithType<CastleGate>())
        {
            if (enable)
            {
                castleGate.OpenDoor();
            }

            foreach (StandingPoint standingPoint in castleGate.StandingPoints)
            {
                standingPoint.SetIsDeactivatedSynched(enable);
            }
        }
    }

    private void ResetFlags()
    {
        AllCapturePoints = new MBReadOnlyList<FlagCapturePoint>(Mission.MissionObjects.FindAllWithType<FlagCapturePoint>().ToArray());
        _flagOwners = new Team[AllCapturePoints.Count];

        var stagesFlags = new List<List<FlagCapturePoint>>();
        foreach (FlagCapturePoint flag in AllCapturePoints)
        {
            _flagOwners[flag.FlagIndex] = Mission.Teams.Defender;
            flag.SetTeamColorsSynched(Mission.Teams.Defender.Color, Mission.Teams.Defender.Color2);

            int flagStage = ResolveFlagStage(flag);
            stagesFlags.AddRange(Enumerable.Range(0, Math.Max(flagStage - stagesFlags.Count + 1, 0)).Select(_ => new List<FlagCapturePoint>()));
            stagesFlags[flagStage].Add(flag);

            if (flagStage != 0)
            {
                flag.RemovePointAsServer();
            }
        }

        if (stagesFlags.Count == 0)
        {
            throw new Exception("No stages found");
        }

        for (int i = 0; i < stagesFlags.Count; i += 1)
        {
            if (stagesFlags[i].Count == 0)
            {
                throw new Exception($"No flags found for stage {i}");
            }
        }

        _flagStages = stagesFlags.Select(s => s.ToArray()).ToArray();
        _currentStage = 0;
        _flagTickTimer = new MissionTimer(0.25f);
    }

    private int ResolveFlagStage(FlagCapturePoint flag)
    {
        for (int stage = 0; stage < CrpgConquestClient.MaxStages; stage += 1)
        {
            if (flag.GameEntity.HasTag(CrpgConquestClient.StageTagPrefix + stage.ToString()))
            {
                return stage;
            }
        }

        throw new Exception("No stage tag found on flag " + flag.FlagIndex);
    }

    private void StartStage(int stageIndex)
    {
        _currentStageTimer = new MissionTimer(MultiplayerOptions.OptionType.RoundTimeLimit.GetIntValue());

        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new CrpgConquestStageStartMessage
        {
            StageIndex = stageIndex,
            StageStartTime = (int)_currentStageTimer.GetStartTime().ToSeconds,
            StageDuration = (int)_currentStageTimer.GetTimerDuration(),
        });
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
    }

    private void TickFlags()
    {
        if (!_flagTickTimer.Check(reset: true))
        {
            return;
        }

        if (_currentStage >= _flagStages.Length) // End of the game.
        {
            return;
        }

        foreach (FlagCapturePoint flag in AllCapturePoints)
        {
            if (flag.IsDeactivated)
            {
                continue;
            }

            Team? flagOwner = GetFlagOwnerTeam(flag);
            Agent? closestAgentToFlag = null;
            float closestAgentDistanceToFlagSquared = float.MaxValue;
            var proximitySearch = AgentProximityMap.BeginSearch(Mission.Current, flag.Position.AsVec2, FlagCaptureRange);
            for (; proximitySearch.LastFoundAgent != null; AgentProximityMap.FindNext(Mission.Current, ref proximitySearch))
            {
                Agent agent = proximitySearch.LastFoundAgent;
                if (agent.IsMount || !agent.IsActive())
                {
                    continue;
                }

                float agentDistanceToFlagSquared = agent.Position.DistanceSquared(flag.Position);
                if (agentDistanceToFlagSquared <= FlagCaptureRangeSquared
                    && agentDistanceToFlagSquared < closestAgentDistanceToFlagSquared)
                {
                    closestAgentToFlag = agent;
                    closestAgentDistanceToFlagSquared = agentDistanceToFlagSquared;
                }
            }

            CaptureTheFlagFlagDirection flagDirection = ComputeFlagDirection(flag, flagOwner, closestAgentToFlag);
            if (flagDirection != CaptureTheFlagFlagDirection.None)
            {
                flag.SetMoveFlag(flagDirection);
            }

            flag.OnAfterTick(closestAgentToFlag != null, out bool flagOwnerChanged);
            Team? flagNewOwner = closestAgentToFlag?.Team;
            if (flagOwnerChanged && flagNewOwner != null)
            {
                OnFlagCaptured(flag, flagNewOwner);
            }
        }
    }

    private void OnFlagCaptured(FlagCapturePoint flag, Team flagNewOwner)
    {
        flag.SetTeamColorsSynched(flagNewOwner.Color, flagNewOwner.Color2);
        _flagOwners[flag.FlagIndex] = flagNewOwner;
        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new FlagDominationCapturePointMessage(flag.FlagIndex, flagNewOwner));
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);

        bool stageEnded = _flagStages[_currentStage].All(f =>
            _flagOwners[f.FlagIndex] == Mission.AttackerTeam);

        if (!stageEnded)
        {
            NotificationsComponent.FlagXCapturedByTeamX(flag, flagNewOwner);
        }
        else
        {
            int endingStage = _currentStage;
            _currentStage += 1;

            if (_currentStage >= _flagStages.Length)
            {
                return;
            }

            foreach (var stageFlag in _flagStages[_currentStage])
            {
                stageFlag.ResetPointAsServer(Mission.DefenderTeam.Color, Mission.DefenderTeam.Color2);
            }

            foreach (var stageFlag in _flagStages[endingStage])
            {
                stageFlag.RemovePointAsServer();
                ((SiegeSpawnFrameBehavior)SpawnComponent.SpawnFrameBehavior).OnFlagDeactivated(stageFlag);
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new FlagDominationFlagsRemovedMessage());
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }

            StartStage(_currentStage);
        }
    }

    private CaptureTheFlagFlagDirection ComputeFlagDirection(
        FlagCapturePoint flag,
        Team? flagOwner,
        Agent? closestAgentToFlag)
    {
        bool isContested = flag.IsContested;
        if (flagOwner == null)
        {
            if (!isContested && closestAgentToFlag != null)
            {
                return CaptureTheFlagFlagDirection.Down;
            }

            if (closestAgentToFlag == null & isContested)
            {
                return CaptureTheFlagFlagDirection.Up;
            }
        }
        else if (closestAgentToFlag != null)
        {
            if (closestAgentToFlag.Team != flagOwner && !isContested)
            {
                return CaptureTheFlagFlagDirection.Down;
            }

            if (closestAgentToFlag.Team == flagOwner && isContested)
            {
                return CaptureTheFlagFlagDirection.Up;
            }
        }
        else if (isContested)
        {
            return CaptureTheFlagFlagDirection.Up;
        }

        return CaptureTheFlagFlagDirection.None;
    }
}
