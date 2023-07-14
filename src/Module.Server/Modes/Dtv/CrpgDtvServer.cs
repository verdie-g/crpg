using System.Xml.Serialization;
using Crpg.Module.Rewards;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.Modes.Dtv;

internal class CrpgDtvServer : MissionMultiplayerGameModeBase
{
    private const int NewWaveBotRespawnTime = 1;
    private const int NewRoundBotRespawnTime = 5;
    private const int GameStartTime = 5;

    private readonly CrpgRewardServer _rewardServer;
    private readonly CrpgDtvData _dtvData;
    private readonly int _totalRounds;

    private int _totalWaves;
    private int _currentRoundCount;
    private CrpgDtvRound _currentRound;
    private int _currentWaveCount;
    private CrpgDtvWave _currentWave;
    private bool _waitingForGameStart;
    private bool _waitingForBotSpawn;
    private MissionTimer? _gameStartTimer;
    private MissionTimer? _botRespawnTimer;

    public override bool IsGameModeHidingAllAgentVisuals => true;
    public override bool IsGameModeUsingOpposingTeams => true;
    public override bool AllowCustomPlayerBanners() => false;
    public override bool UseRoundController() => true;

    public CrpgDtvServer(CrpgRewardServer rewardServer)
    {
        _rewardServer = rewardServer;
        _dtvData = ReadDtvData();
        _totalRounds = _dtvData.Rounds.Count;
        _currentRoundCount = 1;
        _currentRound = GetCurrentRound();
        _currentWaveCount = 1;
        _currentWave = GetCurrentWave();
        _waitingForGameStart = true;
        _waitingForBotSpawn = true;
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

        if (_waitingForGameStart)
        {
            _gameStartTimer ??= new MissionTimer(GameStartTime);
            if (_gameStartTimer.Check())
            {
                SpawnWave(_currentWave);
                _waitingForGameStart = false;
            }
        }
        else
        {
            CheckForWaveEnd();

            if (BotRespawnDelayEnded())
            {
                SpawnWave(_currentWave);
            }
        }
    }

    public override bool CheckForRoundEnd()
    {
        if (!CanGameModeSystemsTickThisFrame)
        {
            return false;
        }

        bool viscountDead = !Mission.DefenderTeam.HasBots;
        if (viscountDead)
        {
            SendDataToPeers(new CrpgDtvViscountDeathMessage());
            return true;
        }

        bool missionComplete = _currentRoundCount > _totalRounds;
        bool defenderTeamDepleted = Mission.DefenderTeam.ActiveAgents.Count == 0;
        return defenderTeamDepleted || missionComplete;
    }

    protected override void HandleNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
    {
        var missionPeer = networkPeer.GetComponent<MissionPeer>();
        missionPeer.Team = Mission.DefenderTeam;
    }

    private void AddTeams()
    {
        BasicCultureObject attackerTeamCulture = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue());
        Banner bannerTeam1 = new(attackerTeamCulture.BannerKey, attackerTeamCulture.BackgroundColor1, attackerTeamCulture.ForegroundColor1);
        Mission.Teams.Add(BattleSideEnum.Attacker, attackerTeamCulture.BackgroundColor1, attackerTeamCulture.ForegroundColor1, bannerTeam1, false, true);
        BasicCultureObject defenderTeamCulture = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue());
        Banner bannerTeam2 = new(defenderTeamCulture.BannerKey, defenderTeamCulture.BackgroundColor2, defenderTeamCulture.ForegroundColor2);
        Mission.Teams.Add(BattleSideEnum.Defender, defenderTeamCulture.BackgroundColor2, defenderTeamCulture.ForegroundColor2, bannerTeam2, false, true);
    }

    private void OnPreRoundEnding()
    {
        bool timedOut = RoundController.RemainingRoundTime <= 0.0;
        bool attackerTeamAlive = Mission.AttackerTeam.ActiveAgents.Count > 0;
        bool viscountDead = !Mission.DefenderTeam.HasBots;
        bool missionComplete = _currentRoundCount > _totalRounds;
        if (timedOut)
        {
            RoundController.RoundWinner = BattleSideEnum.Defender;
            RoundController.RoundEndReason = RoundEndReason.RoundTimeEnded;
        }
        else if (viscountDead)
        {
            Debug.Print("The Viscount has died");
            RoundController.RoundWinner = BattleSideEnum.Attacker;
            RoundController.RoundEndReason = RoundEndReason.GameModeSpecificEnded;
        }
        else if (missionComplete)
        {
            RoundController.RoundWinner = BattleSideEnum.Defender;
            RoundController.RoundEndReason = RoundEndReason.SideDepleted;
        }
        else if (attackerTeamAlive)
        {
            RoundController.RoundWinner = BattleSideEnum.Attacker;
            RoundController.RoundEndReason = RoundEndReason.SideDepleted;
        }
        else
        {
            RoundController.RoundWinner = BattleSideEnum.None;
            RoundController.RoundEndReason = RoundEndReason.SideDepleted;
        }

        Debug.Print($"Team {RoundController.RoundWinner} won on map {Mission.SceneName} with {GameNetwork.NetworkPeers.Count()} players");
        CheerForRoundEnd(RoundController.RoundWinner);
    }

    private void CheerForRoundEnd(BattleSideEnum roundWinner)
    {
        AgentVictoryLogic missionBehavior = Mission.GetMissionBehavior<AgentVictoryLogic>();
        if (roundWinner != BattleSideEnum.None)
        {
            missionBehavior.SetTimersOfVictoryReactionsOnBattleEnd(roundWinner);
        }
    }

    private void CheckForWaveEnd()
    {
        bool attackersDepleted = !Mission.AttackerTeam.HasBots;
        if (!attackersDepleted || _waitingForBotSpawn)
        {
            return;
        }

        Debug.Print("Attackers depleted");
        _currentWaveCount += 1;
        if (_currentWaveCount >= _totalWaves + 1)
        {
            OnRoundEnd();
        }
        else
        {
            OnWaveEnd();
        }
    }

    private void SpawnWave(CrpgDtvWave wave)
    {
        Debug.Print($"Spawning wave for round: {_currentRound.Id} wave: {_currentWave.Id}!");
        ((CrpgDtvSpawningBehavior)SpawnComponent.SpawningBehavior).SpawnAttackingBots(wave);
        _waitingForBotSpawn = false;
    }

    private void OnWaveEnd()
    {
        SendDataToPeers(new CrpgDtvWaveEndMessage { Wave = _currentWaveCount });
        Debug.Print("Advancing to next wave");
        _currentWave = GetCurrentWave();
        _botRespawnTimer = new MissionTimer(NewWaveBotRespawnTime); // Spawn bots after timer
        _waitingForBotSpawn = true;
    }

    private void OnRoundEnd()
    {
        // TODO: award players
        SendDataToPeers(new CrpgDtvRoundEndMessage { Round = _currentRoundCount });
        _currentRoundCount += 1; // next round
        _currentWaveCount = 1;
        if (_currentRoundCount > _totalRounds)
        {
            return;
        }

        Debug.Print("Advancing to next round");
        _currentRound = GetCurrentRound();
        _currentWave = GetCurrentWave();
        _botRespawnTimer = new MissionTimer(NewRoundBotRespawnTime); // Spawn bots after timer
        _waitingForBotSpawn = true;

        ((CrpgDtvSpawningBehavior)SpawnComponent.SpawningBehavior).RequestNewWaveSpawnSession(); // allow players to respawn

        foreach (Agent agent in Mission.DefenderTeam.ActiveAgents) // fill HP & ammo
        {
            agent.Health = agent.HealthLimit;
            for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex++)
            {
                if (!agent.Equipment[equipmentIndex].IsEmpty
                    && (agent.Equipment[equipmentIndex].CurrentUsageItem.WeaponClass == WeaponClass.Arrow
                        || agent.Equipment[equipmentIndex].CurrentUsageItem.WeaponClass == WeaponClass.Bolt
                        || agent.Equipment[equipmentIndex].CurrentUsageItem.WeaponClass == WeaponClass.Javelin
                        || agent.Equipment[equipmentIndex].CurrentUsageItem.WeaponClass == WeaponClass.ThrowingAxe
                        || agent.Equipment[equipmentIndex].CurrentUsageItem.WeaponClass == WeaponClass.ThrowingKnife)
                    && agent.Equipment[equipmentIndex].Amount < agent.Equipment[equipmentIndex].ModifiedMaxAmount)
                {
                    agent.SetWeaponAmountInSlot(equipmentIndex, agent.Equipment[equipmentIndex].ModifiedMaxAmount, true);
                }
            }
        }
    }

    private bool BotRespawnDelayEnded()
    {
        return _botRespawnTimer != null && _botRespawnTimer.Check() && _waitingForBotSpawn;
    }

    private void SendDataToPeers(GameNetworkMessage message)
    {
        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(message);
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
    }

    private CrpgDtvRound GetCurrentRound()
    {
        Debug.Print("Now on round: " + _currentRoundCount);
        CrpgDtvRound currentRound = _dtvData.Rounds[_currentRoundCount - 1];
        _totalWaves = currentRound.Waves.Count;

        return currentRound;
    }

    private CrpgDtvWave GetCurrentWave()
    {
        Debug.Print("Now on wave: " + _currentWaveCount);
        return _currentRound.Waves[_currentWaveCount - 1];
    }

    private CrpgDtvData ReadDtvData()
    {
        XmlSerializer ser = new(typeof(CrpgDtvData));
        using StreamReader sr = new(ModuleHelper.GetXmlPath("Crpg", "dtv_data"));
        return (CrpgDtvData)ser.Deserialize(sr);
    }
}
