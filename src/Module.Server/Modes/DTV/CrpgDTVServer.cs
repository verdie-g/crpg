using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using Crpg.Module.Api.Models;
using Crpg.Module.Common;
using Crpg.Module.Common.Network;
using Crpg.Module.Rewards;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade.MissionRepresentatives;
using TaleWorlds.MountAndBlade.Objects;
using TaleWorlds.ObjectSystem;
using TaleWorlds.PlayerServices;
using MathF = TaleWorlds.Library.MathF;
using Timer = TaleWorlds.Core.Timer;

namespace Crpg.Module.Modes.DTV;

internal class CrpgDTVServer : MissionMultiplayerGameModeBase
{
    private const int totalRounds = 7;
    private const int totalWaves = 3;
    private const int _botRespawnTime = 3;
    private const int _newRoundRespawnTime = 20;

    private readonly CrpgDTVClient _dtvClient;
    private readonly CrpgRewardServer _rewardServer;
    private readonly CrpgDTVTeamSelectComponent _teamSelectComponent;
    private readonly CrpgDTVSpawningBehavior _spawningBehavior;

    private int currentWave = 1;
    private int currentRound = 1;
    private bool _waitingForBotSpawn = false;
    private MissionTimer? _botRespawnTimer;

    public override bool IsGameModeHidingAllAgentVisuals => true;
    public override bool IsGameModeUsingOpposingTeams => true;
    public override bool AllowCustomPlayerBanners() => false;
    public override bool UseRoundController() => true;

    public CrpgDTVServer(CrpgDTVClient dtvClient,
        CrpgRewardServer rewardServer,
        CrpgDTVTeamSelectComponent teamSelectComponent,
        CrpgDTVSpawningBehavior spawningBehavior)
    {
        _dtvClient = dtvClient;
        _rewardServer = rewardServer;
        _teamSelectComponent = teamSelectComponent;
        _spawningBehavior = spawningBehavior;
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
        // TODO: SetTeamColorsWithAllSynched
    }

    public override void OnRemoveBehavior()
    {
        RoundController.OnPreRoundEnding -= OnPreRoundEnding;
        base.OnRemoveBehavior();
    }

    public override void OnClearScene()
    {
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
            || !CanGameModeSystemsTickThisFrame)
        {
            return;
        }

        CheckForAttackers();

        if (BotRespawnDelayEnded())
        {
            _teamSelectComponent.SetPlayerAgentsTeam();
            SpawnWave(currentRound, currentWave);
            _waitingForBotSpawn = false;
        }
    }

    public void CheckForAttackers()
    {
        bool attackersDepleted = !Mission.AttackerTeam.HasBots;

        if (attackersDepleted && !_waitingForBotSpawn)
        {
            Debug.Print("Attackers depleted");
            currentWave += 1;
            if (currentWave >= totalWaves + 1)
            {
                OnRoundEnd();
            }
            else
            {
                OnWaveEnd();
            }

            if (currentRound > totalRounds)
            {
                Debug.Print("Match complete");
                // end match
            }

            Debug.Print($"Current Round: {currentRound}");
            Debug.Print($"Current Wave: {currentWave}");
        }
    }

    public void SpawnWave(int round, int wave)
    {
        if (SpawnComponent.SpawningBehavior is CrpgDTVSpawningBehavior s)
        {
            s.BotsSpawned = false;
            s.Round = round;
            s.Wave = wave;
        }
    }

    public void OnWaveEnd()
    {
        SendRoundEndDataToPeers();
        Debug.Print("Advancing to next wave");
        _botRespawnTimer = new MissionTimer(_botRespawnTime); // Spawn bots after timer
        _waitingForBotSpawn = true;

        _teamSelectComponent.SetPlayerAgentsTeam();
    }

    public void OnRoundEnd()
    {
        // TODO: award players
        currentRound += 1; // next round
        currentWave = 1;
        SendRoundEndDataToPeers();
        Debug.Print("Advancing to next round");
        _botRespawnTimer = new MissionTimer(_newRoundRespawnTime); // Spawn bots after timer
        _waitingForBotSpawn = true;

        if (SpawnComponent.SpawningBehavior is CrpgDTVSpawningBehavior s)
        {
            s.RequestNewWaveSpawnSession();        // allow players to respawn
        }

        foreach (Agent agent in Mission.DefenderTeam.ActiveAgents) // fill HP & ammo
        {
            agent.Health = agent.HealthLimit;
            for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
            {
                if (!agent.Equipment[equipmentIndex].IsEmpty && (agent.Equipment[equipmentIndex].CurrentUsageItem.WeaponClass == WeaponClass.Arrow ||
                                                                 agent.Equipment[equipmentIndex].CurrentUsageItem.WeaponClass == WeaponClass.Bolt ||
                                                                 agent.Equipment[equipmentIndex].CurrentUsageItem.WeaponClass == WeaponClass.Javelin ||
                                                                 agent.Equipment[equipmentIndex].CurrentUsageItem.WeaponClass == WeaponClass.ThrowingAxe ||
                                                                 agent.Equipment[equipmentIndex].CurrentUsageItem.WeaponClass == WeaponClass.ThrowingKnife)
                                                                 && agent.Equipment[equipmentIndex].Amount < agent.Equipment[equipmentIndex].ModifiedMaxAmount)
                {
                    agent.SetWeaponAmountInSlot(equipmentIndex, agent.Equipment[equipmentIndex].ModifiedMaxAmount, true);
                }
            }
        }

        _teamSelectComponent.SetPlayerAgentsTeam(); // move players to defender's team
    }

    public bool BotRespawnDelayEnded()
    {
        return _botRespawnTimer != null && _botRespawnTimer!.Check() && _waitingForBotSpawn;
    }

    public override bool CheckForRoundEnd()
    {
        if (!CanGameModeSystemsTickThisFrame)
        {
            return false;
        }

        if (SpawnComponent.SpawningBehavior is CrpgDTVSpawningBehavior s && !s.SpawnDelayEnded())
        {
            return false;
        }

        bool defenderTeamDepleted = Mission.DefenderTeam.ActiveAgents.Count == 0;
        bool virginDead = !Mission.DefenderTeam.HasBots;
        bool missionComplete = totalRounds < currentRound;

        return defenderTeamDepleted || virginDead || missionComplete;
    }

    public override void OnAgentBuild(Agent agent, Banner banner)
    {
        agent.UpdateSyncHealthToAllClients(true); // Why is that needed
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
    }

    protected override void HandleNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
    {
    }

    protected override void HandleEarlyNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
    {
        networkPeer.AddComponent<TeamDeathmatchMissionRepresentative>();
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

    private void OnPreRoundEnding()
    {
        bool timedOut = RoundController.RemainingRoundTime <= 0.0;

        BattleSideEnum winnerSide = BattleSideEnum.None;

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
            bool attackerTeamAlive = Mission.AttackerTeam.ActiveAgents.Count > 0;
            bool virginDead = !Mission.DefenderTeam.HasBots;
            bool missionComplete = totalRounds < currentRound;
            if (virginDead)
            {
                Debug.Print("The Virgin has died");
                roundResult = CaptureTheFlagCaptureResultEnum.AttackersWin;
                RoundController.RoundWinner = BattleSideEnum.Attacker;
                RoundController.RoundEndReason = RoundEndReason.GameModeSpecificEnded;
            }
            else if (missionComplete)
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

        float roundDuration = MultiplayerOptions.OptionType.RoundTimeLimit.GetIntValue() - RoundController.RemainingRoundTime;
        var roundWinner = RoundController.RoundWinner;
        _ = _rewardServer.UpdateCrpgUsersAsync(
            durationRewarded: roundDuration,
            defenderMultiplierGain: roundWinner == BattleSideEnum.Defender ? 1 : -CrpgRewardServer.ExperienceMultiplierMax,
            attackerMultiplierGain: roundWinner == BattleSideEnum.Attacker ? 1 : -CrpgRewardServer.ExperienceMultiplierMax,
            valourTeamSide: roundWinner.GetOppositeSide());
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

    private void SendRoundEndDataToPeers()
    {
        foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
        {
            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new CrpgDTVRoundEndMessage
            {
                RoundData = new CrpgDTVRoundData
                {
                    Round = currentRound,
                    Wave = currentWave,
                },
            });
            GameNetwork.EndModuleEventAsServer();

        }
    }
}
