﻿using System.Xml.Serialization;
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

    private int _currentRound;
    private int _currentWave;
    private bool _gameStarted;
    private bool _waveStarted;
    private MissionTimer? _waveStartTimer;
    private MissionTimer? _endGameTimer;

    public CrpgDtvServer(CrpgRewardServer rewardServer)
    {
        _rewardServer = rewardServer;
        _dtvData = ReadDtvData();
        _currentRound = -1;
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
                MissionLobbyComponent.SetStateEndingAsServer();
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
        RefillDefendersHealthPointsAndAmmo();
        SpawningBehavior.RequestSpawnSessionForWaveStart(CurrentWaveData);
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
            // TODO: scoreboard lost
            EndGame();
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

        // TODO: reward players.
        if (_currentRound < RoundsCount - 1)
        {
            StartNextRound();
        }
        else
        {
            // TODO: scoreboard win
            EndGame();
        }
    }

    private void EndGame()
    {
        _endGameTimer = new MissionTimer(10f);
    }

    private void RefillDefendersHealthPointsAndAmmo()
    {
        foreach (Agent agent in Mission.DefenderTeam.ActiveAgents)
        {
            agent.Health = agent.HealthLimit;
            for (EquipmentIndex equipmentIndex = EquipmentIndex.WeaponItemBeginSlot; equipmentIndex < EquipmentIndex.NumAllWeaponSlots; equipmentIndex += 1)
            {
                if (!agent.Equipment[equipmentIndex].IsEmpty
                    && (agent.Equipment[equipmentIndex].CurrentUsageItem.WeaponClass == WeaponClass.Arrow
                        || agent.Equipment[equipmentIndex].CurrentUsageItem.WeaponClass == WeaponClass.Bolt
                        || agent.Equipment[equipmentIndex].CurrentUsageItem.WeaponClass == WeaponClass.Stone
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
