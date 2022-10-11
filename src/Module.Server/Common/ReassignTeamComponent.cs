using System.ComponentModel;
using TaleWorlds.MountAndBlade;
using TaleWorlds.PlayerServices;

namespace Crpg.Module.Common;

/// <summary>
/// Used to reassign the team when a player reconnects -> Avoid random team.
/// </summary>
internal class ReassignTeamComponent : MissionBehavior
{
    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;
    private readonly MissionScoreboardComponent _missionScoreboardComponent;
    private readonly Dictionary<PlayerId, Team> _playerTeamList;
    private readonly MultiplayerTeamSelectComponent _multiplayerTeamSelectComponent;
    private readonly MultiplayerRoundController _roundController;
    private bool _isAutobalanceCheck;

    public ReassignTeamComponent(MissionScoreboardComponent missionScoreboardComponent, MultiplayerTeamSelectComponent multiplayerTeamSelectComponent, MultiplayerRoundController roundController)
    {
        _missionScoreboardComponent = missionScoreboardComponent;
        _multiplayerTeamSelectComponent = multiplayerTeamSelectComponent;
        _roundController = roundController;
        _isAutobalanceCheck = false;
        _playerTeamList = new Dictionary<PlayerId, Team>();
    }

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
        _missionScoreboardComponent.OnPlayerSideChanged += OnPlayerChangeSide;
        Mission.Current.OnMissionReset += OnMissionReset;
        _roundController.OnRoundEnding += OnRoundEnding;
    }

    private void OnRoundEnding()
    {
        _isAutobalanceCheck = false;
        Console.WriteLine("Round ending");
    }

    private void OnMissionReset(object sender, PropertyChangedEventArgs e)
    {
        _isAutobalanceCheck = true && MultiplayerOptions.OptionType.AutoTeamBalanceThreshold.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions) != 0;
        Console.WriteLine("Autobalance check.. _isAutobalanceCheck " + _isAutobalanceCheck);
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

        int attackerPlayerCount = _multiplayerTeamSelectComponent.GetPlayerCountForTeam(Mission.Current.AttackerTeam);
        int defenderPlayerCount = _multiplayerTeamSelectComponent.GetPlayerCountForTeam(Mission.Current.DefenderTeam);
        int teamBalanceDifference = MultiplayerTeamSelectComponent.GetAutoTeamBalanceDifference((AutoTeamBalanceLimits)MultiplayerOptions.OptionType.AutoTeamBalanceThreshold.GetIntValue());
        bool isTeamBalance = Math.Abs(attackerPlayerCount - defenderPlayerCount) > teamBalanceDifference;

        // If he was moved by autobalance
        if (_isAutobalanceCheck && isTeamBalance)
        {
            _playerTeamList[playerId] = newTeam;
            return;
        }

        // If he rejoined his old team or his newTeam is his oldTeam
        if (!isTeamBalance && (newTeam == previousTeam || _playerTeamList[playerId] == newTeam))
        {
            return;
        }

        Team targetTeam = _playerTeamList[playerId].Side == Mission.Teams.Defender.Side ? Mission.Teams.Defender : Mission.Teams.Attacker;
        _multiplayerTeamSelectComponent.ChangeTeamServer(networkPeer, targetTeam);
    }
}
