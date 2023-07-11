using Crpg.Module.Modes.Battle.FlagSystems;
using Crpg.Module.Modes.Skirmish;
using Crpg.Module.Rewards;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.Modes.Battle;

internal class CrpgBattleServer : MissionMultiplayerGameModeBase
{
    private readonly bool _isSkirmish;
    private readonly CrpgRewardServer _rewardServer;
    private MultipleFlagSystem _flagSystem = default!;

    public override bool IsGameModeHidingAllAgentVisuals => true;
    public override bool IsGameModeUsingOpposingTeams => true;
    public override bool AllowCustomPlayerBanners() => false;
    public override bool UseRoundController() => true;

    public CrpgBattleServer(bool isSkirmish, CrpgRewardServer rewardServer)
    {
        _isSkirmish = isSkirmish;
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

    public override void OnClearScene()
    {
        _flagSystem.Reset();
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

        _flagSystem.Tick(dt);
    }

    public override bool CheckForRoundEnd()
    {
        if (!CanGameModeSystemsTickThisFrame)
        {
            return false;
        }

        if (_flagSystem.HasRoundEnded())
        {
            return true;
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

    public override bool CheckIfOvertime()
    {
        return _flagSystem.ShouldOvertime();
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        // Apparently AddRemoveMessageHandlers is the first method to be called so we create the flag system here.
        _flagSystem = new MultipleFlagSystem(Mission, NotificationsComponent, _isSkirmish);
        _flagSystem.AddRemoveMessageHandlers(registerer);
    }

    protected override void HandleNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
    {
        _flagSystem.HandleNewClient(networkPeer);
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
        bool timedOut = RoundController.RemainingRoundTime <= 0.0 && !CheckIfOvertime();
        var roundWinner = _flagSystem.GetRoundWinner(timedOut);

        RoundController.RoundWinner = roundWinner.side;
        RoundController.RoundEndReason = roundWinner.reason;

        Debug.Print($"Team {roundWinner.side} won on map {Mission.SceneName} with {GameNetwork.NetworkPeers.Count()} players");

        float roundDuration = MultiplayerOptions.OptionType.RoundTimeLimit.GetIntValue() - RoundController.RemainingRoundTime;
        _ = _rewardServer.UpdateCrpgUsersAsync(
            durationRewarded: roundDuration,
            defenderMultiplierGain: roundWinner.side == BattleSideEnum.Defender ? 1 : -CrpgRewardServer.ExperienceMultiplierMax,
            attackerMultiplierGain: roundWinner.side == BattleSideEnum.Attacker ? 1 : -CrpgRewardServer.ExperienceMultiplierMax,
            valourTeamSide: roundWinner.side.GetOppositeSide());
    }
}
