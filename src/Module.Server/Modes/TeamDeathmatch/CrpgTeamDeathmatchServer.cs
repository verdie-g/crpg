using Crpg.Module.Common;
using Crpg.Module.Rewards;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.MissionRepresentatives;
using TaleWorlds.ObjectSystem;

namespace Crpg.Module.Modes.TeamDeathmatch;

internal class CrpgTeamDeathmatchServer : MissionMultiplayerGameModeBase
{
    private readonly MissionScoreboardComponent _scoreboardComponent;
    private readonly CrpgRewardServer _rewardServer;

    private MissionTimer? _rewardTickTimer;

    public override bool IsGameModeHidingAllAgentVisuals => true;
    public override bool IsGameModeUsingOpposingTeams => true;

    public CrpgTeamDeathmatchServer(
        MissionScoreboardComponent scoreboardComponent,
        CrpgRewardServer rewardServer)
    {
        _scoreboardComponent = scoreboardComponent;
        _rewardServer = rewardServer;
    }

    public override MissionLobbyComponent.MultiplayerGameType GetMissionType()
    {
        return MissionLobbyComponent.MultiplayerGameType.Battle; // Avoids a crash on mission end.
    }

    public override void AfterStart()
    {
        AddTeams();
    }

    public override void OnClearScene()
    {
        // https://forums.taleworlds.com/index.php?threads/missionbehavior-onmissionrestart-is-never-called.458204
        _scoreboardComponent.ClearScores();
        _scoreboardComponent.ResetBotScores();
        ClearPeerCounts();
    }

    public override void OnMissionTick(float dt)
    {
        if (WarmupComponent.IsInWarmup)
        {
            return;
        }

        RewardUsers();
    }

    public override void OnAgentRemoved(Agent affectedAgent, Agent? affectorAgent, AgentState agentState, KillingBlow blow)
    {
        if (blow.DamageType == DamageTypes.Invalid
            || (agentState != AgentState.Unconscious && agentState != AgentState.Killed)
            || !affectedAgent.IsHuman)
        {
            return;
        }

        if (affectorAgent != null && affectorAgent.IsEnemyOf(affectedAgent))
        {
            _scoreboardComponent.ChangeTeamScore(affectorAgent.Team, GetScoreForKill(affectedAgent));
        }
        else
        {
            _scoreboardComponent.ChangeTeamScore(affectedAgent.Team, -GetScoreForKill(affectedAgent));
        }
    }

    public override bool CheckForMatchEnd()
    {
        int minScoreToWinMatch = MultiplayerOptions.OptionType.MinScoreToWinMatch.GetIntValue();
        return _scoreboardComponent.Sides.Any(side => side.SideScore >= minScoreToWinMatch);
    }

    public override Team? GetWinnerTeam()
    {
        int minScoreToWinMatch = MultiplayerOptions.OptionType.MinScoreToWinMatch.GetIntValue();
        var sides = _scoreboardComponent.Sides;

        Team? winnerTeam = null;
        if (sides[(int)BattleSideEnum.Attacker].SideScore < minScoreToWinMatch
            && sides[(int)BattleSideEnum.Defender].SideScore >= minScoreToWinMatch)
        {
            winnerTeam = Mission.Teams.Defender;
        }

        if (sides[(int)BattleSideEnum.Defender].SideScore < minScoreToWinMatch
            && sides[(int)BattleSideEnum.Attacker].SideScore >= minScoreToWinMatch)
        {
            winnerTeam = Mission.Teams.Attacker;
        }

        return winnerTeam;
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
}
