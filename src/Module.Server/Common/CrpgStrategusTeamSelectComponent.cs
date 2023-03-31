using System.IO.Compression;
using Newtonsoft.Json.Converters;
using TaleWorlds.MountAndBlade;

#if CRPG_SERVER
using System.Text;
using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Api.Models.Users;
using Crpg.Module.Balancing;
using Crpg.Module.Common.Network;
using Crpg.Module.Rating;
using NetworkMessages.FromClient;
using Newtonsoft.Json;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;
using TaleWorlds.PlayerServices;
#endif

namespace Crpg.Module.Common;

/// <summary>
/// Disables team selection and randomly assign teams if the native balancer is enabled. Else use the cRPG balancer
/// to balance teams after each round.
/// </summary>
internal class CrpgStrategusTeamSelectComponent : MultiplayerTeamSelectComponent
{
#if CRPG_SERVER
    private readonly MultiplayerWarmupComponent _warmupComponent;
    private readonly MatchBalancer _balancer;

    /// <summary>
    /// Players waiting to be assigned to a team when the cRPG balancer is enabled.
    /// </summary>
    private readonly HashSet<PlayerId> _playersWaitingForTeam;

    public CrpgStrategusTeamSelectComponent(MultiplayerWarmupComponent warmupComponent)
    {
        _warmupComponent = warmupComponent;
        _playersWaitingForTeam = new HashSet<PlayerId>();

    }
#endif

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();

#if CRPG_SERVER
        _warmupComponent.OnWarmupEnded += OnWarmupEnded;
    }

    public override void OnRemoveBehavior()
    {
        base.OnRemoveBehavior();
        _warmupComponent.OnWarmupEnded -= OnWarmupEnded;
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        registerer.Register<TeamChange>(HandleTeamChange);
    }

    private bool HandleTeamChange(NetworkCommunicator peer, TeamChange message)
    {
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
        }

        return true;
    }

    private void OnWarmupEnded()
    {
        BalanceTeams(firstBalance: true);
    }

    private void BalanceTeams(bool firstBalance)
    {
        if (IsNativeBalancerEnabled())
        {
            return;
        }

        GameMatch gameMath = TeamsToGameMatch();
        GameMatch balancedGameMatch = _balancer.BannerBalancingWithEdgeCases(gameMath, firstBalance);

        Dictionary<int, Team> usersToMove = ResolveTeamMoves(current: gameMath, target: balancedGameMatch);
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

    
    private bool IsNativeBalancerEnabled()
    {
        var autoTeamBalanceThreshold =
            (AutoTeamBalanceLimits)MultiplayerOptions.OptionType.AutoTeamBalanceThreshold.GetIntValue();
        return autoTeamBalanceThreshold != AutoTeamBalanceLimits.Off;
    }

#else
    }
#endif
}
