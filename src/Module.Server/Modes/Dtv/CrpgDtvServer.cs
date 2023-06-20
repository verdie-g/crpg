using System.Xml.Serialization;
using Crpg.Module.Rewards;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.MissionRepresentatives;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.Modes.Dtv;

internal class CrpgDtvServer : MissionMultiplayerGameModeBase
{

    private const int BotRespawnTime = 1;
    private const int NewRoundRespawnTime = 5;
    private const int GameStartTime = 5;

    private readonly CrpgDtvData? _dtvData;
    private readonly CrpgDtvClient _dtvClient;
    private readonly CrpgRewardServer _rewardServer;
    private readonly int _totalRounds;
    private int _totalWaves;

    private CrpgDtvRoundEndMessage? _roundEndMessage;
    private CrpgDtvWaveEndMessage? _waveEndMessage;

    private int _currentWaveCount = 1;
    private CrpgDtvWave _currentWave;
    private int _currentRoundCount = 1;
    private CrpgDtvRound _currentRound;
    private bool _waitingForGameStart = true;
    private bool _waitingForBotSpawn = true;
    private MissionTimer? _gameStartTimer;
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
        _dtvData = DeserializeToObject<CrpgDtvData>(ModuleHelper.GetXmlPath("Crpg", "dtv_data"));
        _totalRounds = _dtvData.Rounds.Count();
        _currentRound = GetCurrentRound();
        _currentWave = GetCurrentWave();
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
            _gameStartTimer = _gameStartTimer ?? new(GameStartTime);
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

        bool defenderTeamDepleted = Mission.DefenderTeam.ActiveAgents.Count == 0;
        bool viscountDead = !Mission.DefenderTeam.HasBots;
        bool missionComplete = _currentRoundCount > _totalRounds;

        if (viscountDead)
        {
            CrpgDtvViscountDeathMessage viscountDeathMessage = new();
            SendDataToPeers(viscountDeathMessage);

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
        var missionPeer = networkPeer.GetComponent<MissionPeer>();
        missionPeer.Team = Mission.DefenderTeam;
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
            bool viscountDead = !Mission.DefenderTeam.HasBots;
            bool missionComplete = _currentRoundCount > _totalRounds;
            if (viscountDead)
            {
                Debug.Print("The Viscount has died");
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
    }

    private void SpawnWave(CrpgDtvWave wave)
    {
        if (SpawnComponent.SpawningBehavior is CrpgDtvSpawningBehavior s)
        {
            Debug.Print($"Spawning wave for round: {_currentRound.Id} wave: {_currentWave.Id}!");
            s.SpawnAttackingBots(wave);
            _waitingForBotSpawn = false;
        }
        else
        {
            Debug.Print("Failed to spawn wave!");
        }
    }

    private void OnWaveEnd()
    {
        _waveEndMessage = _waveEndMessage ?? new() { RoundData = new CrpgDtvRoundData() };
        _waveEndMessage.RoundData.Wave = _currentWaveCount;
        SendDataToPeers(_waveEndMessage);
        Debug.Print("Advancing to next wave");
        _currentWave = GetCurrentWave();
        _botRespawnTimer = new MissionTimer(BotRespawnTime); // Spawn bots after timer
        _waitingForBotSpawn = true;
    }

    private void OnRoundEnd()
    {
        // TODO: award players
        _roundEndMessage = _roundEndMessage ?? new() { RoundData = new CrpgDtvRoundData() };
        _roundEndMessage.RoundData.Round = _currentRoundCount;
        SendDataToPeers(_roundEndMessage);
        _currentRoundCount += 1; // next round
        _currentWaveCount = 1;
        if (_currentRoundCount <= _totalRounds) // Continue to next round
        {
            Debug.Print("Advancing to next round");
            _currentRound = GetCurrentRound();
            _currentWave = GetCurrentWave();
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

    private CrpgDtvRound GetCurrentRound()
    {
        if (_dtvData != null)
        {
            if (_dtvData.Rounds != null)
            {
                Debug.Print("Now on round: " + _currentRoundCount);
                CrpgDtvRound currentRound = _dtvData.Rounds[_currentRoundCount - 1];
                if (currentRound.Waves != null)
                {
                    _totalWaves = currentRound.Waves.Count;
                }

                return currentRound;
            }
        }

        return new();
    }

    private CrpgDtvWave GetCurrentWave()
    {
        if (_currentRound != null)
        {
            if (_currentRound.Waves != null)
            {
                Debug.Print("Now on wave: " + _currentWaveCount);
                return _currentRound.Waves[_currentWaveCount - 1];
            }
        }

        return new();
    }

    private T DeserializeToObject<T>(string filepath) where T : class
    {
        XmlSerializer ser = new(typeof(T));

        using (StreamReader sr = new(filepath))
        {
            return (T)ser.Deserialize(sr);
        }
    }
}
