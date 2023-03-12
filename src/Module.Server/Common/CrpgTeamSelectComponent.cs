﻿using System.Text;
using Crpg.Module.Common.Network;
using TaleWorlds.MountAndBlade;

#if CRPG_SERVER
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Api.Models.Users;
using Crpg.Module.Balancing;
using Crpg.Module.Rating;
using NetworkMessages.FromClient;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;
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
    private readonly MatchBalancer _balancer;

    /// <summary>
    /// Players waiting to be assigned to a team when the cRPG balancer is enabled.
    /// </summary>
    private readonly HashSet<PlayerId> _playersWaitingForTeam;

    public CrpgTeamSelectComponent(MultiplayerWarmupComponent warmupComponent, MultiplayerRoundController roundController)
    {
        _warmupComponent = warmupComponent;
        _roundController = roundController;
        _balancer = new MatchBalancer();
        _playersWaitingForTeam = new HashSet<PlayerId>();
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

    private void OnRoundEnded()
    {
        if (_roundController.IsMatchEnding)
        {
            return;
        }

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
                gameMatch.Waiting.Add(WeightUser(crpgPeer.User));
                continue;
            }

            var team = missionPeer.Team?.Side switch
            {
                BattleSideEnum.Defender => gameMatch.TeamA,
                BattleSideEnum.Attacker => gameMatch.TeamB,
                _ => null,
            };
            team?.Add(WeightUser(crpgPeer.User));
        }

        _playersWaitingForTeam.Clear();
        return gameMatch;
    }

    /// <summary>Find the difference between <see cref="GameMatch"/>es, and generates the moves accordingly.</summary>
    private Dictionary<int, Team> ResolveTeamMoves(GameMatch current, GameMatch target)
    {
        Dictionary<int, BattleSideEnum> userCurrentSides = new();
        foreach (var wuser in current.TeamA)
        {
            userCurrentSides[wuser.User.Id] = BattleSideEnum.Defender;
        }

        foreach (var wuser in current.TeamB)
        {
            userCurrentSides[wuser.User.Id] = BattleSideEnum.Attacker;
        }

        foreach (var wuser in current.Waiting)
        {
            userCurrentSides[wuser.User.Id] = BattleSideEnum.None;
        }

        Dictionary<int, Team> usersToMove = new();
        foreach (var wuser in target.TeamA)
        {
            if (userCurrentSides.TryGetValue(wuser.User.Id, out BattleSideEnum currentSide) && currentSide != BattleSideEnum.Defender)
            {
                usersToMove[wuser.User.Id] = Mission.DefenderTeam;
            }
        }

        foreach (var wuser in target.TeamB)
        {
            if (userCurrentSides.TryGetValue(wuser.User.Id, out BattleSideEnum currentSide) && currentSide != BattleSideEnum.Attacker)
            {
                usersToMove[wuser.User.Id] = Mission.AttackerTeam;
            }
        }

        foreach (var wuser in target.Waiting)
        {
            if (userCurrentSides.TryGetValue(wuser.User.Id, out BattleSideEnum currentSide) && currentSide != BattleSideEnum.None)
            {
                usersToMove[wuser.User.Id] = Mission.SpectatorTeam;
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

    private WeightedCrpgUser WeightUser(CrpgUser user)
    {
        float weight = ComputeWeight(user);
        return new WeightedCrpgUser(user, weight);
    }

    private float ComputeWeight(CrpgUser user)
    {
        float ratingWeight = ComputeRatingWeight(user);

        float itemsPrice = ComputeEquippedItemsPrice(user.Character.EquippedItems);
        float itemsWeight = 1f + itemsPrice / 50_000f;

        // Ideally the rating should be elastic enough to change when the character
        // retires but that's not the case so for now let's use the level to compute
        // the weight.
        float levelWeight = 1f + user.Character.Level / 30f;

        return ratingWeight * itemsWeight * levelWeight;
    }

    private float ComputeRatingWeight(CrpgUser user)
    {
        var rating = user.Character.Rating;
        float regionPenalty = CrpgRatingHelper.ComputeRegionRatingPenalty(user.Region);
        // https://www.desmos.com/calculator/23qchzvrw3
        return 0.0001f * (float)Math.Pow(rating.Value - 2 * rating.Deviation, 2f) * regionPenalty;
    }

    private float ComputeEquippedItemsPrice(IList<CrpgEquippedItem> equippedItems)
    {
        float price = 0f;
        float weaponMaxPrice = 0f;
        foreach (var ei in equippedItems)
        {
            var itemObject = MBObjectManager.Instance.GetObject<ItemObject>(ei.UserItem.BaseItemId);
            if (itemObject == null)
            {
                continue;
            }

            if (itemObject.HasWeaponComponent && itemObject.Type != ItemObject.ItemTypeEnum.Shield)
            {
                weaponMaxPrice = Math.Max(weaponMaxPrice, itemObject.Value);
            }
            else
            {
                price += itemObject.Value;
            }
        }

        return price + weaponMaxPrice;
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
