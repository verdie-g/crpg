using Crpg.Module.Common;
using Crpg.Module.Rewards;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.MissionRepresentatives;
using TaleWorlds.MountAndBlade.Objects;
using TaleWorlds.ObjectSystem;
using MathF = TaleWorlds.Library.MathF;

namespace Crpg.Module.Modes.Siege;

internal class CrpgSiegeServer : MissionMultiplayerGameModeBase, IAnalyticsFlagInfo
{
    private const float MoraleTickTimeInSeconds = 1f;
    private const int MaxMoraleGainPerFlag = 90;
    private const int MoraleBoostOnFlagRemoval = 90;
    private const int MoraleDecayInTick = 1;
    private const int MoraleGainPerFlag = 1;
    private const float FlagCaptureRange = 4f;
    private const float FlagCaptureRangeSquared = FlagCaptureRange * FlagCaptureRange;

    private readonly CrpgSiegeClient _client;
    private readonly MissionScoreboardComponent _missionScoreboardComponent;
    private readonly CrpgRewardServer _rewardServer;

    private int[] _morales = Array.Empty<int>();
    private Agent? _closestAgentToMasterFlag;
    private FlagCapturePoint _masterFlag = default!;
    private Team?[] _flagOwners = Array.Empty<Team>();
    private int[] _flagRemainingMoraleGains = Array.Empty<int>();
    private float _dtSumCheckMorales;
    private float _dtSumTickFlags;
    private bool _firstTickDone;
    private MissionTimer? _rewardTickTimer;

    public CrpgSiegeServer(
        CrpgSiegeClient client,
        MissionScoreboardComponent missionScoreboardComponent,
        CrpgRewardServer rewardServer)
    {
        _client = client;
        _missionScoreboardComponent = missionScoreboardComponent;
        _rewardServer = rewardServer;
    }

    public override bool IsGameModeHidingAllAgentVisuals => true;

    public override bool IsGameModeUsingOpposingTeams => true;

    public override MultiplayerGameType GetMissionType()
        => MultiplayerGameType.FreeForAll; // Helps to avoid a few crashes.

    public override bool UseRoundController() => false;

    public MBReadOnlyList<FlagCapturePoint> AllCapturePoints { get; private set; } = new(new List<FlagCapturePoint>());

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();

        ResetMorales();
        ResetFlags();
    }

    public override void AfterStart()
    {
        AddTeams();

        foreach (FlagCapturePoint flag in AllCapturePoints)
        {
            _flagOwners[flag.FlagIndex] = Mission.Teams.Defender;
            flag.SetTeamColors(Mission.Teams.Defender.Color, Mission.Teams.Defender.Color2);
            _client?.OnCapturePointOwnerChanged(flag, Mission.Teams.Defender);
        }
    }

    public override void OnMissionTick(float dt)
    {
        base.OnMissionTick(dt);
        if (!_firstTickDone)
        {
            foreach (CastleGate castleGate in Mission.MissionObjects.FindAllWithType<CastleGate>())
            {
                castleGate.OpenDoor();
                foreach (StandingPoint standingPoint in castleGate.StandingPoints)
                {
                    standingPoint.SetIsDeactivatedSynched(true);
                }
            }

            _firstTickDone = true;
        }

        if (MissionLobbyComponent.CurrentMultiplayerState != MissionLobbyComponent.MultiplayerGameState.Playing || WarmupComponent.IsInWarmup)
        {
            return;
        }

        RewardUsers();
        CheckMorales(dt);
        TickFlags(dt);
    }

    public override bool CheckForMatchEnd()
    {
        return _morales.Any(morale => morale == 0);
    }

    public override Team GetWinnerTeam()
    {
        Team? winnerTeam = null;
        if (_morales[(int)BattleSideEnum.Attacker] <= 0 && _morales[(int)BattleSideEnum.Defender] > 0)
        {
            winnerTeam = Mission.Teams.Defender;
        }

        if (_morales[(int)BattleSideEnum.Defender] <= 0 && _morales[(int)BattleSideEnum.Attacker] > 0)
        {
            winnerTeam = Mission.Teams.Attacker;
        }

        winnerTeam ??= Mission.Teams.Defender;
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
        int playersReady = 0;
        foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
        {
            MissionPeer component = networkPeer.GetComponent<MissionPeer>();
            if (networkPeer.IsSynchronized && component?.Team != null && component.Team.Side != BattleSideEnum.None)
            {
                playersReady += 1;
            }
        }

        return playersReady >= MultiplayerOptions.OptionType.MaxNumberOfPlayers.GetIntValue();
    }

    public override void OnClearScene()
    {
        base.OnClearScene();
        ClearPeerCounts();
        foreach (CastleGate gate in Mission.MissionObjects.FindAllWithType<CastleGate>())
        {
            foreach (StandingPoint standingPoint in gate.StandingPoints)
            {
                standingPoint.SetIsDeactivatedSynched(false);
            }
        }
    }

    protected override void HandleEarlyNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
    {
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
            if (flag.IsDeactivated)
            {
                continue;
            }

            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new FlagDominationCapturePointMessage(flag.FlagIndex, _flagOwners[flag.FlagIndex]));
            GameNetwork.EndModuleEventAsServer();
        }
    }

    protected override void HandleNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
    {
        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new SiegeMoraleChangeMessage(
            attackerMorale: _morales[(int)BattleSideEnum.Attacker],
            defenderMorale: _morales[(int)BattleSideEnum.Defender],
            _flagRemainingMoraleGains));
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
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

    private void RewardUsers()
    {
        _rewardTickTimer ??= new MissionTimer(duration: CrpgServerConfiguration.RewardTick);
        if (_rewardTickTimer.Check(reset: true))
        {
            _ = _rewardServer.UpdateCrpgUsersAsync(
                durationRewarded: _rewardTickTimer.GetTimerDuration(),
                constantMultiplier: 2);
        }
    }

    private void CheckMorales(float dt)
    {
        _dtSumCheckMorales += dt;
        if (_dtSumCheckMorales < MoraleTickTimeInSeconds)
        {
            return;
        }

        _dtSumCheckMorales -= MoraleTickTimeInSeconds;
        int defenderMorale = MBMath.ClampInt(
            _morales[(int)BattleSideEnum.Defender] + GetMoraleGain(BattleSideEnum.Defender),
            0,
            CrpgSiegeClient.StartingMorale);
        int attackerMorale = MathF.Max(_morales[(int)BattleSideEnum.Attacker] + GetMoraleGain(BattleSideEnum.Attacker), 0);
        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new SiegeMoraleChangeMessage(attackerMorale, defenderMorale, _flagRemainingMoraleGains));
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
        _client.OnMoraleChanged(attackerMorale, defenderMorale, _flagRemainingMoraleGains);
        _morales[(int)BattleSideEnum.Defender] = defenderMorale;
        _morales[(int)BattleSideEnum.Attacker] = attackerMorale;
    }

    private int GetMoraleGain(BattleSideEnum side)
    {
        int moraleGain = 0;
        bool teamAgentOnMasterFlag = _closestAgentToMasterFlag != null && _closestAgentToMasterFlag.Team.Side == side;
        if (side == BattleSideEnum.Attacker)
        {
            if (!teamAgentOnMasterFlag)
            {
                moraleGain -= MoraleDecayInTick;
            }

            foreach (FlagCapturePoint flag in AllCapturePoints)
            {
                if (flag == _masterFlag
                    || flag.IsDeactivated
                    || !flag.IsFullyRaised
                    || GetFlagOwnerTeam(flag)!.Side != BattleSideEnum.Attacker)
                {
                    continue;
                }

                _flagRemainingMoraleGains[flag.FlagIndex] -= MoraleGainPerFlag;
                moraleGain += MoraleGainPerFlag;
                if (_flagRemainingMoraleGains[flag.FlagIndex] == 0)
                {
                    moraleGain += MoraleBoostOnFlagRemoval;
                    flag.RemovePointAsServer();
                    ((SiegeSpawnFrameBehavior)SpawnComponent.SpawnFrameBehavior).OnFlagDeactivated(flag);
                    _client.OnNumberOfFlagsChanged();
                    GameNetwork.BeginBroadcastModuleEvent();
                    GameNetwork.WriteMessage(new FlagDominationFlagsRemovedMessage());
                    GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
                    NotificationsComponent.FlagsXRemoved(flag);
                }
            }
        }
        else if (_masterFlag.IsFullyRaised)
        {
            if (GetFlagOwnerTeam(_masterFlag)!.Side == BattleSideEnum.Attacker)
            {
                if (!teamAgentOnMasterFlag)
                {
                    foreach (var flag in AllCapturePoints)
                    {
                        if (flag == _masterFlag)
                        {
                            continue;
                        }

                        moraleGain -= flag.IsDeactivated ? 1 : 0;
                    }
                }
            }
            else
            {
                moraleGain += 1;
            }
        }

        return moraleGain;
    }

    private void TickFlags(float dt)
    {
        _dtSumTickFlags += dt;
        if (_dtSumTickFlags < 0.25)
        {
            return;
        }

        _dtSumTickFlags -= 0.25f;

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

            if (flag == _masterFlag)
            {
                _closestAgentToMasterFlag = closestAgentToFlag;
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
                flag.SetTeamColorsSynched(flagNewOwner.Color, flagNewOwner.Color2);
                _flagOwners[flag.FlagIndex] = flagNewOwner;
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new FlagDominationCapturePointMessage(flag.FlagIndex, flagNewOwner));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
                _client?.OnCapturePointOwnerChanged(flag, flagNewOwner);
                NotificationsComponent.FlagXCapturedByTeamX(flag, closestAgentToFlag!.Team);
            }
        }
    }

    private void ResetMorales()
    {
        _morales = new int[(int)BattleSideEnum.NumSides];
        for (int i = 0; i < _morales.Length; i += 1)
        {
            _morales[i] = CrpgSiegeClient.StartingMorale;
        }
    }

    private void ResetFlags()
    {
        AllCapturePoints = new MBReadOnlyList<FlagCapturePoint>(Mission.MissionObjects.FindAllWithType<FlagCapturePoint>().ToList());
        int maxNumberOfFlags = AllCapturePoints.Select(f => f.FlagIndex).Max() + 1;

        _flagOwners = new Team[maxNumberOfFlags];
        _flagRemainingMoraleGains = new int[maxNumberOfFlags];

        foreach (FlagCapturePoint flag in AllCapturePoints)
        {
            flag.SetTeamColorsSynched(TeammateColorsExtensions.NEUTRAL_COLOR, TeammateColorsExtensions.NEUTRAL_COLOR2);
            _flagOwners[flag.FlagIndex] = null;
            _flagRemainingMoraleGains[flag.FlagIndex] = MaxMoraleGainPerFlag;
            if (flag.GameEntity.HasTag(CrpgSiegeClient.MasterFlagTag))
            {
                _masterFlag = flag;
            }
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

            if (closestAgentToFlag.Team == flagOwner & isContested)
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
