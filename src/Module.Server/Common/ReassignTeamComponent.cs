using System.ComponentModel;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.PlayerServices;

namespace Crpg.Module.Common;

/// <summary>
/// Used to reassign the team when a player reconnects -> Avoid random team.
/// This component is for the server. The client doe not have a round controller and also doesn't need to run any of this code.
/// The <see cref="NoTeamSelectComponent"/> is still used to disable the team selection menu.
/// </summary>
internal class ReassignTeamComponent : NoTeamSelectComponent
{
    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;
    private readonly MissionScoreboardComponent _missionScoreboardComponent;
    private readonly Dictionary<PlayerId, BattleSideEnum> _playerTeamList;
    private readonly MultiplayerRoundController _roundController;
    private bool _isAutobalanceCheck;

    public ReassignTeamComponent(MissionScoreboardComponent missionScoreboardComponent, MultiplayerRoundController roundController)
    {
        _missionScoreboardComponent = missionScoreboardComponent;
        _roundController = roundController;
        _isAutobalanceCheck = false;
        _playerTeamList = new Dictionary<PlayerId, BattleSideEnum>();
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

    /// <summary>
    /// Disables the check if autobalance was running.
    /// </summary>
    private void OnRoundStarted()
    {
        _isAutobalanceCheck = false;
    }

    /// <summary>
    /// Between the calls of <see cref="OnRoundStarted"/> and <see cref="OnMissionReset"/> the autobalance runs.
    /// Therefore we allow teamswitches in the time between both method calls.
    /// </summary>
    private void OnMissionReset(object sender, PropertyChangedEventArgs e)
    {
        _isAutobalanceCheck = MultiplayerOptions.OptionType.AutoTeamBalanceThreshold.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions) != (int)AutoTeamBalanceLimits.Off;
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
            _playerTeamList.Add(playerId, newTeam.Side);
            return;
        }

        if (newTeam == previousTeam || _playerTeamList[playerId] == newTeam.Side)
        {
            return;
        }

        // If he was moved by autobalance
        if (_isAutobalanceCheck && AreTeamsBalanced())
        {
            _playerTeamList[playerId] = newTeam.Side;
            return;
        }

        Team targetTeam = _playerTeamList[playerId] == Mission.Teams.Defender.Side ? Mission.Teams.Defender : Mission.Teams.Attacker;
        ChangeTeamServer(networkPeer, targetTeam);
    }

    private bool AreTeamsBalanced()
    {
        int attackerPlayerCount = GetPlayerCountForTeam(Mission.Current.AttackerTeam);
        int defenderPlayerCount = GetPlayerCountForTeam(Mission.Current.DefenderTeam);
        int teamBalanceDifference = GetAutoTeamBalanceDifference((AutoTeamBalanceLimits)MultiplayerOptions.OptionType.AutoTeamBalanceThreshold.GetIntValue());
        return Math.Abs(attackerPlayerCount - defenderPlayerCount) > teamBalanceDifference;
    }
}
