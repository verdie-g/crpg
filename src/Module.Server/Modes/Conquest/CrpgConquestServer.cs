using Crpg.Module.Common;
using Crpg.Module.Notifications;
using Crpg.Module.Rewards;
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
    private const float FlagCaptureRange = 8f;
    private const float FlagCaptureRangeSquared = FlagCaptureRange * FlagCaptureRange;
    private const int StageDuration = 2 * 60;
    private const int FirstStageDuration = 9 * 60;

    private readonly MissionScoreboardComponent _missionScoreboardComponent;
    private readonly CrpgRewardServer _rewardServer;

    private Team?[] _flagOwners = Array.Empty<Team>();
    private FlagCapturePoint[][] _flagStages = Array.Empty<FlagCapturePoint[]>();
    private bool _gameStarted;
    private int _currentStage;
    private MissionTimer _flagTickTimer = default!;
    private MissionTimer _currentStageTimer = default!;
    private MissionTimer? _rewardTickTimer;
    private bool _wasCurrentStageNotifiedAboutOvertime;
    private bool _isOddRewardTick;

    public CrpgConquestServer(
        MissionScoreboardComponent missionScoreboardComponent,
        CrpgRewardServer rewardServer)
    {
        _missionScoreboardComponent = missionScoreboardComponent;
        _rewardServer = rewardServer;
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
            _isOddRewardTick = true;
            StartStage(0);
            _gameStarted = true;
        }

        TickFlags();
        RewardUsers();
    }

    public override void OnObjectUsed(Agent userAgent, UsableMissionObject usedObject)
    {
        if (userAgent.Team.IsDefender && usedObject.InteractionEntity.Name.Equals("open_inside"))
        {
            var networkPeer = userAgent.MissionPeer.GetNetworkPeer();
            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(new CrpgConquestOpenGateMessage
            {
                Peer = networkPeer,
            });
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.ExcludeOtherTeamPlayers, networkPeer);
        }
    }

    public override bool CheckForMatchEnd()
    {
        if (_currentStage >= _flagStages.Length)
        {
            // Attackers captured all the flags.
            return true;
        }

        if (!_currentStageTimer.Check())
        {
            return false;
        }

        FlagCapturePoint? uncappedFlag = null;
        foreach (var flag in _flagStages[_currentStage])
        {
            if (GetFlagOwnerTeam(flag) != Mission.DefenderTeam)
            {
                continue;
            }

            if (uncappedFlag != null) // More than one flag are uncapped -> no overtime.
            {
                return true;
            }

            uncappedFlag = flag;
        }

        foreach (var agent in EnumerateAgentsAroundFlag(uncappedFlag!))
        {
            if (agent.IsMount || !agent.IsActive() || agent.Team?.Side != BattleSideEnum.Attacker)
            {
                continue;
            }

            if (!_wasCurrentStageNotifiedAboutOvertime)
            {
                _wasCurrentStageNotifiedAboutOvertime = true;
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new CrpgNotificationId
                {
                    Type = CrpgNotificationType.Notification,
                    TextId = "str_notification",
                    TextVariation = "conquest_overtime",
                });
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }

            // Attacker still on the last flag of the current stage.
            return false;
        }

        return true;
    }

    public override Team GetWinnerTeam()
    {
        var winnerTeam = _currentStage >= _flagStages.Length ? Mission.Teams.Attacker : Mission.Teams.Defender;

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
        var flagOwners = new List<Team?>();
        var stagesFlags = new List<List<FlagCapturePoint>>();
        foreach (FlagCapturePoint flag in AllCapturePoints)
        {
            flag.SetTeamColorsSynched(Mission.Teams.Defender.Color, Mission.Teams.Defender.Color2);

            flagOwners.AddRange(Enumerable.Range(0, Math.Max(flag.FlagIndex - flagOwners.Count + 1, 0)).Select(_ => (Team?)null));
            flagOwners[flag.FlagIndex] = Mission.Teams.Defender;

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

        _flagOwners = flagOwners.ToArray();
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
        _currentStageTimer = new MissionTimer(stageIndex == 0
            ? FirstStageDuration
            : StageDuration + _currentStageTimer.GetRemainingTimeInSeconds());

        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new CrpgConquestStageStartMessage
        {
            StageIndex = stageIndex,
            StageStartTime = (int)_currentStageTimer.GetStartTime().ToSeconds,
            StageDuration = (int)_currentStageTimer.GetTimerDuration(),
        });
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);

        _rewardTickTimer = new MissionTimer(duration: CrpgServerConfiguration.RewardTick);
        _wasCurrentStageNotifiedAboutOvertime = false;
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

        // Disable flags 30 seconds to avoid back-capping.
        if (MissionTime.Now - _currentStageTimer.GetStartTime() < MissionTime.Seconds(30))
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
            foreach (var agent in EnumerateAgentsAroundFlag(flag))
            {
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
                flag.SetMoveFlag(flagDirection, speedMultiplier: 0.4f);
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

        if (stageEnded)
        {
            OnStageEnd();
        }
        else
        {
            NotificationsComponent.FlagXCapturedByTeamX(flag, flagNewOwner);
        }
    }

    private void OnStageEnd()
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

        _ = _rewardServer.UpdateCrpgUsersAsync(
            durationRewarded: _rewardTickTimer!.GetRemainingTimeInSeconds(),
            defenderMultiplierGain: -CrpgRewardServer.ExperienceMultiplierMax,
            attackerMultiplierGain: CrpgRewardServer.ExperienceMultiplierMax);
        _isOddRewardTick = true;

        StartStage(_currentStage);
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

    private void RewardUsers()
    {
        if (_rewardTickTimer!.Check(reset: true))
        {
            int defenderMultiplierGain;
            int attackerMultiplierGain;
            if (_isOddRewardTick)
            {
                defenderMultiplierGain = 0;
                attackerMultiplierGain = 0;
                _isOddRewardTick = false;
            }
            else
            {
                defenderMultiplierGain = 1;
                attackerMultiplierGain = -1;
                _isOddRewardTick = true;
            }

            _ = _rewardServer.UpdateCrpgUsersAsync(
                durationRewarded: _rewardTickTimer.GetTimerDuration(),
                defenderMultiplierGain: defenderMultiplierGain,
                attackerMultiplierGain: attackerMultiplierGain);
        }
    }

    private IEnumerable<Agent> EnumerateAgentsAroundFlag(FlagCapturePoint flag)
    {
        var proximitySearch = AgentProximityMap.BeginSearch(Mission.Current, flag.Position.AsVec2, FlagCaptureRange);
        for (; proximitySearch.LastFoundAgent != null; AgentProximityMap.FindNext(Mission.Current, ref proximitySearch))
        {
            yield return proximitySearch.LastFoundAgent;
        }
    }
}
