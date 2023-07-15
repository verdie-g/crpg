using System.Xml.Serialization;
using Crpg.Module.Rewards;
using TaleWorlds.Core;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.Modes.Dtv;

internal class CrpgDtvServer : MissionMultiplayerGameModeBase
{
    private readonly CrpgRewardServer _rewardServer;
    private readonly CrpgDtvData _dtvData;

    private int _currentGame;
    private int _currentRound;
    private int _currentRoundDefendersCount;
    private int _currentWave;
    private bool _gameStarted;
    private bool _waveStarted;
    private MissionTimer? _waveStartTimer;
    private MissionTimer? _endGameTimer;

    public CrpgDtvServer(CrpgRewardServer rewardServer)
    {
        _rewardServer = rewardServer;
        _dtvData = ReadDtvData();
        _currentGame = 0;
    }

    public override bool IsGameModeHidingAllAgentVisuals => true;
    public override bool IsGameModeUsingOpposingTeams => true;
    public override bool AllowCustomPlayerBanners() => false;
    public override bool UseRoundController() => false;

    private CrpgDtvSpawningBehavior SpawningBehavior => (CrpgDtvSpawningBehavior)SpawnComponent.SpawningBehavior;
    private int RoundsCount => _dtvData.Rounds.Count;
    private CrpgDtvRound CurrentRoundData => _dtvData.Rounds[_currentRound];
    private CrpgDtvWave CurrentWaveData => _dtvData.Rounds[_currentRound].Waves[_currentWave];
    private int WavesCountForCurrentRound => CurrentRoundData.Waves.Count;

    public override MissionLobbyComponent.MultiplayerGameType GetMissionType()
    {
        return MissionLobbyComponent.MultiplayerGameType.Battle;
    }

    public override void AfterStart()
    {
        base.AfterStart();
        AddTeams();
    }

    public override void OnClearScene()
    {
        _gameStarted = false;
        _currentRound = -1;
        ClearPeerCounts();
        Mission.GetMissionBehavior<MissionScoreboardComponent>().ResetBotScores();
    }

    public override bool CheckForWarmupEnd()
    {
        return true;
    }

    public override void OnMissionTick(float dt)
    {
        base.OnMissionTick(dt);
        if (MissionLobbyComponent.CurrentMultiplayerState != MissionLobbyComponent.MultiplayerGameState.Playing
            || !CanGameModeSystemsTickThisFrame)
        {
            return;
        }

        if (_endGameTimer != null)
        {
            if (_endGameTimer.Check())
            {
                _currentGame += 1;
                if (_currentGame == 5)
                {
                    MissionLobbyComponent.SetStateEndingAsServer();
                }
                else
                {
                    _endGameTimer = null;
                    Mission.ResetMission();
                }
            }

            return;
        }

        if (!_gameStarted)
        {
            _gameStarted = true;
            StartNextRound();
        }
        else if (_waveStarted)
        {
            CheckForWaveEnd();
        }
        else if (_waveStartTimer != null && _waveStartTimer.Check())
        {
            _waveStartTimer = null;
            StartNextWave();
        }
    }

    protected override void HandleNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
    {
        var missionPeer = networkPeer.GetComponent<MissionPeer>();
        missionPeer.Team = Mission.DefenderTeam;
    }

    private void StartNextRound()
    {
        _currentRound += 1;
        _currentWave = -1;
        SpawningBehavior.RequestSpawnSessionForRoundStart(firstRound: _currentRound == 0);
        SendDataToPeers(new CrpgDtvRoundStartMessage { Round = _currentRound });
        _waveStartTimer = new MissionTimer(10f);
        _waveStarted = false;
    }

    private void StartNextWave()
    {
        _currentWave += 1;
        bool firstWave = _currentWave == 0;
        RefillDefendersHealthPointsAndAmmo(refillAmmo: firstWave);
        _currentRoundDefendersCount = firstWave ? GetDefendersCount() : _currentRoundDefendersCount;
        SpawningBehavior.RequestSpawnSessionForWaveStart(CurrentWaveData, _currentRoundDefendersCount);
        SendDataToPeers(new CrpgDtvWaveStartMessage { Wave = _currentWave });
        _waveStarted = true;
    }

    private void CheckForWaveEnd()
    {
        bool viscountDead = !Mission.DefenderTeam.HasBots;
        bool defendersDepleted = Mission.DefenderTeam.ActiveAgents.Count == (viscountDead ? 0 : 1);
        if (viscountDead || defendersDepleted)
        {
            SendDataToPeers(new CrpgDtvGameEnd { ViscountDead = viscountDead });
            _ = _rewardServer.UpdateCrpgUsersAsync(
                durationRewarded: ComputeRoundReward(CurrentRoundData, wavesWon: _currentWave),
                updateUserStats: false);
            EndGame(Mission.AttackerTeam);
            return;
        }

        bool attackersDepleted = !Mission.AttackerTeam.HasBots;
        if (!attackersDepleted)
        {
            return;
        }

        if (_currentWave < WavesCountForCurrentRound - 1)
        {
            StartNextWave();
            return;
        }

        _ = _rewardServer.UpdateCrpgUsersAsync(
            durationRewarded: ComputeRoundReward(CurrentRoundData, wavesWon: _currentWave + 1),
            updateUserStats: false);

        if (_currentRound < RoundsCount - 1)
        {
            StartNextRound();
        }
        else
        {
            EndGame(Mission.DefenderTeam);
        }
    }

    private void EndGame(Team winnerTeam)
    {
        Mission.GetMissionBehavior<MissionScoreboardComponent>().ChangeTeamScore(winnerTeam, 1);
        _endGameTimer = new MissionTimer(8f);
    }

    private void RefillDefendersHealthPointsAndAmmo(bool refillAmmo)
    {
        foreach (Agent agent in Mission.DefenderTeam.ActiveAgents)
        {
            agent.Health = agent.HealthLimit;
            if (!refillAmmo)
            {
                continue;
            }

            for (EquipmentIndex i = EquipmentIndex.WeaponItemBeginSlot; i < EquipmentIndex.NumAllWeaponSlots; i += 1)
            {
                if (!agent.Equipment[i].IsEmpty
                    && (agent.Equipment[i].CurrentUsageItem.WeaponClass == WeaponClass.Arrow
                        || agent.Equipment[i].CurrentUsageItem.WeaponClass == WeaponClass.Bolt
                        || agent.Equipment[i].CurrentUsageItem.WeaponClass == WeaponClass.Stone
                        || agent.Equipment[i].CurrentUsageItem.WeaponClass == WeaponClass.Javelin
                        || agent.Equipment[i].CurrentUsageItem.WeaponClass == WeaponClass.ThrowingAxe
                        || agent.Equipment[i].CurrentUsageItem.WeaponClass == WeaponClass.ThrowingKnife)
                    && agent.Equipment[i].Amount < agent.Equipment[i].ModifiedMaxAmount)
                {
                    agent.SetWeaponAmountInSlot(i, agent.Equipment[i].ModifiedMaxAmount, true);
                }
            }
        }
    }

    private int GetDefendersCount()
    {
        int defendersCount = 0;
        foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
        {
            var missionPeer = networkPeer.GetComponent<MissionPeer>();
            if (!networkPeer.IsSynchronized
                || missionPeer == null
                || missionPeer.Team == null
                || missionPeer.Team.Side == BattleSideEnum.None)
            {
                continue;
            }

            defendersCount += 1;
        }

        return defendersCount;
    }

    private float ComputeRoundReward(CrpgDtvRound data, int wavesWon)
    {
        float defendersScale = 1 + (_currentRoundDefendersCount - 1) * 0.05f;
        float lostRoundPenalty = (float)wavesWon / data.Waves.Count;
        return data.Reward * defendersScale * lostRoundPenalty;
    }

    private void SendDataToPeers(GameNetworkMessage message)
    {
        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(message);
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
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

    private CrpgDtvData ReadDtvData()
    {
        XmlSerializer ser = new(typeof(CrpgDtvData));
        using StreamReader sr = new(ModuleHelper.GetXmlPath("Crpg", "dtv_data"));
        return (CrpgDtvData)ser.Deserialize(sr);
    }
}
