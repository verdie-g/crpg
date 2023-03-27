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
internal class CrpgTeamSelectComponent : MultiplayerTeamSelectComponent
{
#if CRPG_SERVER
    private readonly MultiplayerWarmupComponent _warmupComponent;
    private readonly MultiplayerRoundController _roundController;
    private readonly MatchBalancer _balancer;
    private readonly PeriodStatsHelper _periodStatsHelper;

    /// <summary>
    /// Players waiting to be assigned to a team when the cRPG balancer is enabled.
    /// </summary>
    private readonly HashSet<PlayerId> _playersWaitingForTeam;

    public CrpgTeamSelectComponent(MultiplayerWarmupComponent warmupComponent, MultiplayerRoundController roundController)
    {
        _warmupComponent = warmupComponent;
        _roundController = roundController;
        _balancer = new MatchBalancer();
        _periodStatsHelper = new PeriodStatsHelper();
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
        LogRoundResult();
        if (!_roundController.IsMatchEnding)
        {
            BalanceTeams(firstBalance: false);
        }
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
        float itemsWeight = ComputeEquippedItemsWeight(user.Character.EquippedItems);
        float levelWeight = ComputeLevelWeight(user.Character.Level);

        return ratingWeight * itemsWeight * levelWeight;
    }

    private float ComputeRatingWeight(CrpgUser user)
    {
        var rating = user.Character.Rating;
        float regionPenalty = CrpgRatingHelper.ComputeRegionRatingPenalty(user.Region);
        // https://www.desmos.com/calculator/snynzhhoay
        return 6E-8f * (float)Math.Pow(rating.Value - 2 * rating.Deviation, 3f) * regionPenalty;
    }

    private float ComputeEquippedItemsWeight(IList<CrpgEquippedItem> equippedItems)
    {
        float itemsPrice = ComputeEquippedItemsPrice(equippedItems);
        return 1f + itemsPrice / 50_000f;
    }

    private int ComputeEquippedItemsPrice(IList<CrpgEquippedItem> equippedItems)
    {
        int price = 0;
        int weaponMaxPrice = 0;
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

    private float ComputeLevelWeight(int level)
    {
        // Ideally the rating should be elastic enough to change when the character
        // retires but that's not the case so for now let's use the level to compute
        // the weight.
        return 1f + level / 30f;
    }

    private bool IsNativeBalancerEnabled()
    {
        var autoTeamBalanceThreshold =
            (AutoTeamBalanceLimits)MultiplayerOptions.OptionType.AutoTeamBalanceThreshold.GetIntValue();
        return autoTeamBalanceThreshold != AutoTeamBalanceLimits.Off;
    }

    private void LogRoundResult()
    {
        RoundResultData roundResult = new()
        {
            WinnerSide = _roundController.RoundWinner,
            MapId = Mission.SceneName,
            Version = GetType().Assembly.GetName().Version!,
            Date = DateTime.UtcNow,
        };

        var allRoundStats = _periodStatsHelper.ComputePeriodStats();

        foreach (var networkPeer in GameNetwork.NetworkPeers)
        {
            var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
            if (!networkPeer.IsSynchronized || crpgPeer?.User == null || crpgPeer.SpawnTeamThisRound == null)
            {
                continue;
            }

            var character = crpgPeer.User.Character;
            RoundPlayerData roundPlayer = new()
            {
                UserId = crpgPeer.User.Id,
                UserName = crpgPeer.User.Name,
                Weight = ComputeWeight(crpgPeer.User),
                Level = character.Level,
                LevelWeight = ComputeLevelWeight(character.Level),
                Class = character.Class,
                Score = 0,
                Kills = 0,
                Deaths = 0,
                Assists = 0,
                Rating = character.Rating.Value,
                RatingWeight = ComputeRatingWeight(crpgPeer.User),
                EquipmentCost = ComputeEquippedItemsPrice(character.EquippedItems),
                EquipmentWeight = ComputeEquippedItemsWeight(character.EquippedItems),
                ClanTag = crpgPeer.Clan?.Tag,
            };

            if (allRoundStats.TryGetValue(networkPeer.VirtualPlayer.Id, out var roundStats))
            {
                roundPlayer.Score = roundStats.Score;
                roundPlayer.Kills = roundStats.Kills;
                roundPlayer.Deaths = roundStats.Deaths;
                roundPlayer.Assists = roundStats.Assists;
            }

            if (crpgPeer.SpawnTeamThisRound.Side == BattleSideEnum.Defender)
            {
                roundResult.Defenders.Add(roundPlayer);
            }
            else if (crpgPeer.SpawnTeamThisRound.Side == BattleSideEnum.Attacker)
            {
                roundResult.Attackers.Add(roundPlayer);
            }
        }

        string roundResultJson = JsonConvert.SerializeObject(roundResult, new StringEnumConverter());
        byte[] roundResultJsonBytes = Encoding.UTF8.GetBytes(roundResultJson);
        byte[] roundResultJsonCompressedBytes = GzipCompress(roundResultJsonBytes);
        string base64RoundResultJsonCompressedBytes = Convert.ToBase64String(roundResultJsonCompressedBytes);
        Debug.Print("Round result data: " + base64RoundResultJsonCompressedBytes);
    }

    private byte[] GzipCompress(byte[] bytes)
    {
        using var memoryStream = new MemoryStream();
        using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress, true))
        {
            gzipStream.Write(bytes, 0, bytes.Length);
        }

        return memoryStream.ToArray();
    }

    private class RoundResultData
    {
        public BattleSideEnum WinnerSide { get; set; }
        public string MapId { get; set; } = string.Empty;
        public Version Version { get; set; } = default!;
        public DateTime Date { get; set; }
        public List<RoundPlayerData> Defenders { get; set; } = new();
        public List<RoundPlayerData> Attackers { get; set; } = new();
    }

    private class RoundPlayerData
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public float Weight { get; set; }
        public int Level { get; set; }
        public float LevelWeight { get; set; }
        public CrpgCharacterClass Class { get; set; }
        public int Score { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Assists { get; set; }
        public float Rating { get; set; }
        public float RatingWeight { get; set; }
        public int EquipmentCost { get; set; }
        public float EquipmentWeight { get; set; }
        public string? ClanTag { get; set; }
    }
#else
    }
#endif
}
