using System.ComponentModel;
using TaleWorlds.MountAndBlade;
using TaleWorlds.PlayerServices;

namespace Crpg.Module.Common;

/// <summary>
/// Used to reassign the team when a player reconnects -> Avoid random team.
/// </summary>
internal class ReassignTeamComponent : NoTeamSelectComponent
{
    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;
    private readonly MissionScoreboardComponent _missionScoreboardComponent;
    private readonly Dictionary<PlayerId, Team> _playerTeamList;
    private readonly MultiplayerRoundController _roundController;
    private bool _isAutobalanceCheck;

    public ReassignTeamComponent(MissionScoreboardComponent missionScoreboardComponent, MultiplayerRoundController roundController)
    {
        _missionScoreboardComponent = missionScoreboardComponent;
        _roundController = roundController;
        _isAutobalanceCheck = false;
        _playerTeamList = new Dictionary<PlayerId, Team>();
    }

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
        _missionScoreboardComponent.OnPlayerSideChanged += OnPlayerChangeSide;
        Mission.OnMissionReset += OnMissionReset;
        _roundController.OnRoundStarted += OnRoundStarted;
    }

    protected override void HandleLateNewClientAfterSynchronized(NetworkCommunicator networkPeer)
    {
        AutoAssignTeam(networkPeer);
    }

    private void OnRoundStarted()
    {
        _isAutobalanceCheck = false;
    }

    private void OnMissionReset(object sender, PropertyChangedEventArgs e)
    {
        _isAutobalanceCheck = MultiplayerOptions.OptionType.AutoTeamBalanceThreshold.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions) != 0;
    }

    private void OnPlayerChangeSide(Team previousTeam, Team newTeam, MissionPeer peer)
    {
        // Disconnect
        if (newTeam == null)
        {
            return;
        }

        NetworkCommunicator networkPeer = peer.GetNetworkPeer();
        PlayerId playerId = networkPeer.VirtualPlayer.Id;

        // First connect
        if (previousTeam == null && !_playerTeamList.ContainsKey(playerId))
        {
            _playerTeamList.Add(playerId, newTeam);
            return;
        }

        if (newTeam == previousTeam || _playerTeamList[playerId].Side == newTeam.Side)
        {
            return;
        }

        bool isTeamBalance = false;
        if (_isAutobalanceCheck)
        {
            int attackerPlayerCount = GetPlayerCountForTeam(Mission.Current.AttackerTeam);
            int defenderPlayerCount = GetPlayerCountForTeam(Mission.Current.DefenderTeam);
            int teamBalanceDifference = GetAutoTeamBalanceDifference((AutoTeamBalanceLimits)MultiplayerOptions.OptionType.AutoTeamBalanceThreshold.GetIntValue());
            isTeamBalance = Math.Abs(attackerPlayerCount - defenderPlayerCount) > teamBalanceDifference;
        }

        // If he was moved by autobalance
        if (isTeamBalance)
        {
            _playerTeamList[playerId] = newTeam;
            return;
        }

        Team targetTeam = _playerTeamList[playerId].Side == Mission.Teams.Defender.Side ? Mission.Teams.Defender : Mission.Teams.Attacker;
        ChangeTeamServer(networkPeer, targetTeam);
    }
}
