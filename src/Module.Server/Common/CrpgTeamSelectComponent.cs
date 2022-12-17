using TaleWorlds.MountAndBlade;

#if CRPG_SERVER
using Crpg.Module.Balancing;
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
internal class CrpgTeamSelectComponent : MultiplayerTeamSelectComponent
{
#if CRPG_SERVER
    private readonly MultiplayerWarmupComponent _warmupComponent;
    private readonly MultiplayerRoundController _roundController;
    private readonly MatchBalancingSystem _balancer;

    /// <summary>
    /// Players waiting to be assigned to a team when the cRPG balancer is enabled.
    /// </summary>
    private readonly HashSet<PlayerId> _playersWaitingForTeam;

    public CrpgTeamSelectComponent(MultiplayerWarmupComponent warmupComponent, MultiplayerRoundController roundController)
    {
        _warmupComponent = warmupComponent;
        _roundController = roundController;
        _balancer = new MatchBalancingSystem();
        _playersWaitingForTeam = new HashSet<PlayerId>();
    }
#else
    public CrpgTeamSelectComponent()
    {
    }
#endif

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();

#if CRPG_SERVER
        _warmupComponent.OnWarmupEnded += OnWarmupEnded;
        _roundController.OnPostRoundEnded += OnRoundEnded;
    }

    public override void OnRemoveBehavior()
    {
        base.OnRemoveBehavior();
        _roundController.OnPostRoundEnded -= OnRoundEnded;
        _warmupComponent.OnWarmupEnded -= OnWarmupEnded;
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        registerer.Register<TeamChange>(HandleTeamChange);
    }

    private bool HandleTeamChange(NetworkCommunicator peer, TeamChange message)
    {
        if (IsNativeBalancerEnabled())
        {
            if (message.AutoAssign)
            {
                AutoAssignTeam(peer);
            }
            else
            {
                ChangeTeamServer(peer, message.Team);
            }
        }
        else
        {
            if (message.Team == Mission.SpectatorTeam)
            {
                ChangeTeamServer(peer, message.Team);
            }
            else if (_warmupComponent.IsInWarmup)
            {
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
        BalanceTeams(firstBalance: false);
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
        Debug.Print($"Moving {usersToMove.Count} players to balance teams");

        var crpgNetworkPeers = GetCrpgNetworkPeers();
        foreach (var userToMove in usersToMove)
        {
            if (crpgNetworkPeers.TryGetValue(userToMove.Key, out var networkPeer))
            {
                ChangeTeamServer(networkPeer, userToMove.Value);
            }
        }
    }

    /// <summary>Create a <see cref="GameMatch"/> object used as input for the balancing from the current teams.</summary>
    private GameMatch TeamsToGameMatch()
    {
        GameMatch gameMatch = new();
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

    /// <summary>Find the difference between <see cref="GameMatch"/>es, and move the players accordingly.</summary>
    private Dictionary<int, Team> ResolveTeamMoves(GameMatch current, GameMatch target)
    {
        Dictionary<int, BattleSideEnum> userCurrentSides = new();
        foreach (var user in current.TeamA)
        {
            userCurrentSides[user.Id] = BattleSideEnum.Defender;
        }

        foreach (var user in current.TeamB)
        {
            userCurrentSides[user.Id] = BattleSideEnum.Attacker;
        }

        foreach (var user in current.Waiting)
        {
            userCurrentSides[user.Id] = BattleSideEnum.None;
        }

        Dictionary<int, Team> usersToMove = new();
        foreach (var user in target.TeamA)
        {
            if (userCurrentSides.TryGetValue(user.Id, out BattleSideEnum currentSide) && currentSide != BattleSideEnum.Defender)
            {
                usersToMove[user.Id] = Mission.DefenderTeam;
            }
        }

        foreach (var user in target.TeamB)
        {
            if (userCurrentSides.TryGetValue(user.Id, out BattleSideEnum currentSide) && currentSide != BattleSideEnum.Attacker)
            {
                usersToMove[user.Id] = Mission.AttackerTeam;
            }
        }

        foreach (var user in target.Waiting)
        {
            if (userCurrentSides.TryGetValue(user.Id, out BattleSideEnum currentSide) && currentSide != BattleSideEnum.None)
            {
                usersToMove[user.Id] = Mission.SpectatorTeam;
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

    private bool IsNativeBalancerEnabled()
    {
        var autoTeamBalanceThreshold = (AutoTeamBalanceLimits)MultiplayerOptions.OptionType.AutoTeamBalanceThreshold.GetIntValue();
        return autoTeamBalanceThreshold != AutoTeamBalanceLimits.Off;
    }
#else
    }
#endif
}
