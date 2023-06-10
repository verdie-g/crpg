using Crpg.Module.Api.Models;
using Crpg.Module.Common;
using Crpg.Module.Rewards;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.MissionRepresentatives;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.Modes.Dtv;

internal class CrpgDtvServer : MissionMultiplayerGameModeBase
{
    private const int TotalRounds = 8;
    private const int TotalWaves = 3;
    private const int BotRespawnTime = 3;
    private const int NewRoundRespawnTime = 20;

    private readonly CrpgDtvClient _dtvClient;
    private readonly CrpgRewardServer _rewardServer;
    private readonly CrpgDtvVirginDeathMessage _virginDeathMessage = new() { RoundData = new CrpgDtvRoundData { IsVirginDead = true } };
    private readonly CrpgDtvRoundEndMessage _roundEndMessage = new() { RoundData = new CrpgDtvRoundData { Round = 1 } };
    private readonly CrpgDtvWaveEndMessage _waveEndMessage = new() { RoundData = new CrpgDtvRoundData { Wave = 1 } };

    private int currentWave = 1;
    private int currentRound = 1;
    private bool _waitingForBotSpawn = false;
    private MissionTimer? _botRespawnTimer;

    public override bool IsGameModeHidingAllAgentVisuals => true;
    public override bool IsGameModeUsingOpposingTeams => true;
    public override bool AllowCustomPlayerBanners() => false;
    public override bool UseRoundController() => true;

    public CrpgDtvServer(CrpgDtvClient dtvClient,
        CrpgRewardServer rewardServer)
    {
        _dtvClient = dtvClient;
        _rewardServer = rewardServer;
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

    public override void OnRemoveBehavior()
    {
        RoundController.OnPreRoundEnding -= OnPreRoundEnding;
        base.OnRemoveBehavior();
    }

    public override bool CheckForWarmupEnd()
    {
        return false;
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

        CheckForWaveEnd();

        if (BotRespawnDelayEnded())
        {
            SpawnWave(currentRound, currentWave);
            _waitingForBotSpawn = false;
        }
    }

    public override bool CheckForRoundEnd()
    {
        if (!CanGameModeSystemsTickThisFrame)
        {
            return false;
        }

        if (SpawnComponent.SpawningBehavior is CrpgDtvSpawningBehavior s && !s.SpawnDelayEnded())
        {
            return false;
        }

        bool defenderTeamDepleted = Mission.DefenderTeam.ActiveAgents.Count == 0;
        bool virginDead = !Mission.DefenderTeam.HasBots;
        bool missionComplete = currentRound > TotalRounds;

        if (virginDead)
        {
            SendDataToPeers(_virginDeathMessage);

            return true;
        }

        return defenderTeamDepleted || missionComplete;
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
            bool missionComplete = currentRound > TotalRounds;
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


    private void CheckForWaveEnd()
    {
        bool attackersDepleted = !Mission.AttackerTeam.HasBots;

        if (attackersDepleted && !_waitingForBotSpawn)
        {
            Debug.Print("Attackers depleted");
            currentWave += 1;
            UpdateMessages();
            if (currentWave >= TotalWaves + 1)
            {
                OnRoundEnd();
            }
            else
            {
                OnWaveEnd();
            }

            if (currentRound > TotalRounds)
            {
                Debug.Print("Match complete");
                // end match
            }

            Debug.Print($"Current Round: {currentRound}");
            Debug.Print($"Current Wave: {currentWave}");
        }
    }

    private void SpawnWave(int round, int wave)
    {
        if (SpawnComponent.SpawningBehavior is CrpgDtvSpawningBehavior s)
        {
            s.BotsSpawned = false;
            s.Round = round;
            s.Wave = wave;
        }
    }

    private void OnWaveEnd()
    {
        SendDataToPeers(_waveEndMessage);
        Debug.Print("Advancing to next wave");
        _botRespawnTimer = new MissionTimer(BotRespawnTime); // Spawn bots after timer
        _waitingForBotSpawn = true;
    }

    private void OnRoundEnd()
    {
        // TODO: award players
        currentRound += 1; // next round
        currentWave = 1;
        SendDataToPeers(_roundEndMessage);
        Debug.Print("Advancing to next round");
        _botRespawnTimer = new MissionTimer(NewRoundRespawnTime); // Spawn bots after timer
        _waitingForBotSpawn = true;

        if (SpawnComponent.SpawningBehavior is CrpgDtvSpawningBehavior s)
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
    }

    private bool BotRespawnDelayEnded()
    {
        return _botRespawnTimer != null && _botRespawnTimer!.Check() && _waitingForBotSpawn;
    }

    private void SendDataToPeers(GameNetworkMessage message)
    {
        foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
        {
            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(message);
            GameNetwork.EndModuleEventAsServer();
        }
    }

    private void UpdateMessages()
    {
        _roundEndMessage.RoundData.Round = currentRound;
        _waveEndMessage.RoundData.Wave = currentWave;
    }
}
