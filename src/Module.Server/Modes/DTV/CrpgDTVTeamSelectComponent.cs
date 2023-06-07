using TaleWorlds.MountAndBlade;
using Crpg.Module.Api.Models.Users;

#if CRPG_SERVER
using System.Text;
using Crpg.Module.Balancing;
using Crpg.Module.Common.Network;
using NetworkMessages.FromClient;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.PlayerServices;
#endif

namespace Crpg.Module.Common;

/// <summary>
/// Disables team selection and randomly assign teams if the native balancer is enabled. Else use the cRPG balancer
/// to balance teams after each round.
/// </summary>
internal class CrpgDTVTeamSelectComponent : MultiplayerTeamSelectComponent
{
#if CRPG_SERVER
    private readonly MultiplayerWarmupComponent _warmupComponent;
    private readonly MultiplayerRoundController? _roundController;
    private readonly PeriodStatsHelper _periodStatsHelper;

    /// <summary>
    /// Players waiting to be assigned to a team when the cRPG balancer is enabled.
    /// </summary>
    private readonly HashSet<PlayerId> _playersWaitingForTeam;

    public CrpgDTVTeamSelectComponent(MultiplayerWarmupComponent warmupComponent, MultiplayerRoundController? roundController)
    {
        _warmupComponent = warmupComponent;
        _roundController = roundController;
        _periodStatsHelper = new PeriodStatsHelper();
        _playersWaitingForTeam = new HashSet<PlayerId>();
    }
#endif

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();

#if CRPG_SERVER
        _warmupComponent.OnWarmupEnded += OnWarmupEnded;
        if (_roundController != null)
        {
            _roundController.OnPostRoundEnded += OnRoundEnded;
        }
    }

    public override void OnRemoveBehavior()
    {
        base.OnRemoveBehavior();
        if (_roundController != null)
        {
            _roundController.OnPostRoundEnded -= OnRoundEnded;
        }

        _warmupComponent.OnWarmupEnded -= OnWarmupEnded;
    }

    public void SetPlayerAgentsTeam()
    {
        Debug.Print("Setting player agents' team");

        DTVGameMatch gameMatch = TeamsToGameMatch();
        DTVGameMatch balancedGameMatch = MovePlayersToDefenderTeam(gameMatch);

        Dictionary<int, Team> usersToMove = ResolveTeamMoves(current: gameMatch, target: balancedGameMatch);
        var crpgNetworkPeers = GetCrpgNetworkPeers();
        SendSwapNotification(usersToMove, crpgNetworkPeers);

        foreach (var userToMove in usersToMove)
        {
            if (!crpgNetworkPeers.TryGetValue(userToMove.Key, out var networkPeer))
            {
                continue;
            }

            ChangeTeamServer(networkPeer, userToMove.Value);
        }
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        registerer.Register<TeamChange>(HandleTeamChange);
    }

    private bool HandleTeamChange(NetworkCommunicator peer, TeamChange message)
    {
        if (message.Team == Mission.SpectatorTeam && !message.AutoAssign)
        {
            ChangeTeamServer(peer, message.Team);
        }
        else if (_warmupComponent.IsInWarmup)
        {
            AutoAssignTeam(peer);
        }
        else
        {
            var missionPeer = peer.GetComponent<MissionPeer>();
            if (missionPeer is { Team: null })
            {
                // If the player just connected to the server, auto-assign their team so they have a chance
                // to play the round.
                AutoAssignTeam(peer);
            }
            else
            {
                _playersWaitingForTeam.Add(peer.VirtualPlayer.Id);
            }
        }

        return true;
    }

    private void OnRoundEnded()
    {
        if (!_roundController!.IsMatchEnding)
        {
            SetPlayerAgentsTeam();
        }
    }

    private void OnWarmupEnded()
    {
        SetPlayerAgentsTeam();
    }

    /// <summary>Create a <see cref="DTVGameMatch"/> object used as input for the balancing from the current teams.</summary>
    private DTVGameMatch TeamsToGameMatch()
    {
        DTVGameMatch gameMatch = new();
        foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
        {
            var missionPeer = networkPeer.GetComponent<MissionPeer>();
            var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
            if (!networkPeer.IsSynchronized || missionPeer == null || crpgPeer?.User == null)
            {
                continue;
            }

            if (_playersWaitingForTeam.Contains(networkPeer.VirtualPlayer.Id))
            {
                gameMatch.Waiting.Add(crpgPeer.User);
                continue;
            }

            var team = missionPeer.Team?.Side switch
            {
                BattleSideEnum.Defender => gameMatch.TeamA,
                BattleSideEnum.Attacker => gameMatch.TeamB,
                _ => null,
            };
            team?.Add(crpgPeer.User);
        }

        _playersWaitingForTeam.Clear();
        return gameMatch;
    }

    private DTVGameMatch MovePlayersToDefenderTeam(DTVGameMatch gameMatch)
    {
        List<CrpgUser> allUsers = new();
        allUsers.AddRange(gameMatch.TeamA);
        allUsers.AddRange(gameMatch.TeamB);
        allUsers.AddRange(gameMatch.Waiting);

        return new DTVGameMatch
        {
            TeamA = allUsers,
            Waiting = new List<CrpgUser>(),
        };
    }

    /// <summary>Find the difference between <see cref="DTVGameMatch"/>es, and generates the moves accordingly.</summary>
    private Dictionary<int, Team> ResolveTeamMoves(DTVGameMatch current, DTVGameMatch target)
    {
        Dictionary<int, BattleSideEnum> userCurrentSides = new();
        foreach (var wuser in current.TeamA)
        {
            userCurrentSides[wuser.Id] = BattleSideEnum.Defender;
        }

        foreach (var wuser in current.TeamB)
        {
            userCurrentSides[wuser.Id] = BattleSideEnum.Attacker;
        }

        foreach (var wuser in current.Waiting)
        {
            userCurrentSides[wuser.Id] = BattleSideEnum.None;
        }

        Dictionary<int, Team> usersToMove = new();
        foreach (var wuser in target.TeamA)
        {
            if (userCurrentSides.TryGetValue(wuser.Id, out BattleSideEnum currentSide) && currentSide != BattleSideEnum.Defender)
            {
                usersToMove[wuser.Id] = Mission.DefenderTeam;
            }
        }

        foreach (var wuser in target.TeamB)
        {
            if (userCurrentSides.TryGetValue(wuser.Id, out BattleSideEnum currentSide) && currentSide != BattleSideEnum.Attacker)
            {
                usersToMove[wuser.Id] = Mission.AttackerTeam;
            }
        }

        foreach (var wuser in target.Waiting)
        {
            if (userCurrentSides.TryGetValue(wuser.Id, out BattleSideEnum currentSide) && currentSide != BattleSideEnum.None)
            {
                usersToMove[wuser.Id] = Mission.SpectatorTeam;
            }
        }

        return usersToMove;
    }

    private Dictionary<int, NetworkCommunicator> GetCrpgNetworkPeers()
    {
        Dictionary<int, NetworkCommunicator> crpgNetworkPeers = new();
        foreach (var networkPeer in GameNetwork.NetworkPeers)
        {
            var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
            if (!networkPeer.IsSynchronized || crpgPeer?.User == null)
            {
                continue;
            }

            crpgNetworkPeers[crpgPeer.User.Id] = networkPeer;
        }

        return crpgNetworkPeers;
    }

    private void SendSwapNotification(Dictionary<int, Team> usersToMove,
        Dictionary<int, NetworkCommunicator> crpgNetworkPeers)
    {
        int defendersMovedToAttackers = 0;
        int attackersMovedToDefenders = 0;
        int attackersJoined = 0;
        int defendersJoined = 0;
        foreach (var userToMove in usersToMove)
        {
            if (!crpgNetworkPeers.TryGetValue(userToMove.Key, out var networkPeer))
            {
                continue;
            }

            var missionPeer = networkPeer.GetComponent<MissionPeer>();
            if (missionPeer == null)
            {
                continue;
            }

            if (missionPeer.Team == Mission.DefenderTeam && userToMove.Value == Mission.AttackerTeam)
            {
                defendersMovedToAttackers += 1;
            }
            else if (missionPeer.Team == Mission.AttackerTeam && userToMove.Value == Mission.DefenderTeam)
            {
                attackersMovedToDefenders += 1;
            }
            else if (missionPeer.Team == null || missionPeer.Team == Mission.SpectatorTeam)
            {
                if (userToMove.Value == Mission.DefenderTeam)
                {
                    defendersJoined += 1;
                }
                else if (userToMove.Value == Mission.AttackerTeam)
                {
                    attackersJoined += 1;
                }
            }
        }

        StringBuilder notifBuilder = new();
        if (defendersMovedToAttackers != 0)
        {
            notifBuilder.Append($"{defendersMovedToAttackers} player{(defendersMovedToAttackers > 1 ? "s were" : " was")} moved to the attackers team{{newline}}");
        }

        if (attackersMovedToDefenders != 0)
        {
            notifBuilder.Append($"{attackersMovedToDefenders} player{(attackersMovedToDefenders > 1 ? "s were" : " was")} moved to the defenders team{{newline}}");
        }

        if (defendersJoined != 0)
        {
            notifBuilder.Append($"{defendersJoined} new player{(defendersJoined > 1 ? "s" : string.Empty)} joined the defenders team{{newline}}");
        }

        if (attackersJoined != 0)
        {
            notifBuilder.Append($"{attackersJoined} new player{(attackersJoined > 1 ? "s" : string.Empty)} joined the attackers team{{newline}}");
        }

        if (notifBuilder.Length == 0)
        {
            return;
        }

        notifBuilder.Length -= "{newline}".Length;

        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new CrpgNotification
        {
            Type = CrpgNotificationType.Notification,
            Message = notifBuilder.ToString(),
        });
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
    }

#else
    }
#endif
}
