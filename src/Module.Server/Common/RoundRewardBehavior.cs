using Crpg.Module.Api;
using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Common.Network;
using Crpg.Module.Helpers;
using Crpg.Module.Rating;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.PlayerServices;

namespace Crpg.Module.Common;

/// <summary>
/// On round ends, give xp/gold, update rating and stats.
/// </summary>
internal class RoundRewardBehavior : MissionBehavior
{
    private readonly MultiplayerRoundController _roundController;
    private readonly ICrpgClient _crpgClient;
    private readonly CrpgConstants _constants;
    private readonly Dictionary<PlayerId, CrpgRating> _characterRatings;
    private readonly CrpgRatingPeriodResults _ratingResults;
    private readonly Random _random = new();

    private Dictionary<PlayerId, CrpgCharacterStatistics> _lastRoundAllTotalStats = new();

    public RoundRewardBehavior(MultiplayerRoundController roundController, ICrpgClient crpgClient, CrpgConstants constants)
    {
        _roundController = roundController;
        _crpgClient = crpgClient;
        _constants = constants;
        _characterRatings = new Dictionary<PlayerId, CrpgRating>();
        _ratingResults = new CrpgRatingPeriodResults();
    }

    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

    public override void AfterStart()
    {
        base.AfterStart();
        _roundController.OnPreRoundEnding += OnPreRoundEnding;
    }

    public override void OnRemoveBehavior()
    {
        _roundController.OnPreRoundEnding -= OnPreRoundEnding;
        base.OnRemoveBehavior();
    }

    public override void OnAgentHit(
        Agent affectedAgent,
        Agent affectorAgent,
        in MissionWeapon affectorWeapon,
        in Blow blow,
        in AttackCollisionData attackCollisionData)
    {
        if (!_roundController.IsRoundInProgress)
        {
            return;
        }

        if (affectedAgent == affectorAgent) // Self hit.
        {
            return;
        }

        if (affectedAgent.Team == affectorAgent.Team) // Team hit.
        {
            return; // TODO: should penalize affector.
        }

        if (!TryGetRating(affectedAgent, out var affectedRating)
            || !TryGetRating(affectorAgent, out var affectorRating))
        {
            return;
        }

        float inflictedRatio = MathF.Clamp(blow.InflictedDamage / affectedAgent.BaseHealthLimit, 0f, 1f);
        _ratingResults.AddResult(affectorRating, affectedRating, inflictedRatio);
    }

    private bool TryGetRating(Agent agent, out CrpgRating rating)
    {
        rating = null!;
        if (agent.MissionPeer == null)
        {
            return false;
        }

        if (!_characterRatings.TryGetValue(agent.MissionPeer.Peer.Id, out rating))
        {
            var crpgPeer = agent.MissionPeer.Peer.GetComponent<CrpgPeer>();
            if (crpgPeer?.User == null)
            {
                return false;
            }

            var characterRating = crpgPeer.User.Character.Rating;
            rating = new CrpgRating(characterRating.Value, characterRating.Deviation, characterRating.Volatility);
            _characterRatings[agent.MissionPeer.Peer.Id] = rating;
            _ratingResults.AddParticipant(rating);
        }

        return true;
    }

    private void OnPreRoundEnding()
    {
        _ = UpdateCrpgUsersAsync();
    }

    private async Task UpdateCrpgUsersAsync()
    {
        float roundDuration = MultiplayerOptions.OptionType.RoundTimeLimit.GetIntValue() - _roundController.RemainingRoundTime;

        CrpgRatingCalculator.UpdateRatings(_ratingResults);

        Dictionary<int, CrpgPeer> crpgPeerByUserId = new();
        var newRoundAllTotalStats = new Dictionary<PlayerId, CrpgCharacterStatistics>();
        List<CrpgUserUpdate> userUpdates = new();
        var networkPeers = GameNetwork.NetworkPeers.ToArray();
        bool rewardMultiplierEnabled = networkPeers.Length > 4;
        foreach (NetworkCommunicator networkPeer in networkPeers)
        {
            var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
            if (crpgPeer?.User == null)
            {
                continue;
            }

            crpgPeerByUserId[crpgPeer.User.Id] = crpgPeer;

            CrpgUserUpdate userUpdate = new()
            {
                CharacterId = crpgPeer.User.Character.Id,
                Reward = new CrpgUserReward { Experience = 0, Gold = 0 },
                Statistics = new CrpgCharacterStatistics { Kills = 0, Deaths = 0, Assists = 0, PlayTime = TimeSpan.Zero },
                Rating = GetNewRating(crpgPeer),
                BrokenItems = BreakItems(crpgPeer, roundDuration),
            };

            SetReward(userUpdate, crpgPeer, roundDuration, rewardMultiplierEnabled);
            SetStatistics(userUpdate, networkPeer, newRoundAllTotalStats);

            userUpdates.Add(userUpdate);
        }

        _ratingResults.Clear();
        _characterRatings.Clear();

        if (userUpdates.Count == 0)
        {
            return;
        }

        // Save last round stats to be able to make the difference next round.
        _lastRoundAllTotalStats = newRoundAllTotalStats;

        // TODO: add retry mechanism (the endpoint need to be idempotent though).
        try
        {
            var res = (await _crpgClient.UpdateUsersAsync(new CrpgGameUsersUpdateRequest { Updates = userUpdates })).Data!;
            SendRewardToPeers(res.UpdateResults, crpgPeerByUserId);
        }
        catch (Exception e)
        {
            Debug.Print("Couldn't update users: " + e);
            SendErrorToPeers(crpgPeerByUserId);
        }
    }

    private void SetReward(CrpgUserUpdate userUpdate, CrpgPeer crpgPeer, float roundDuration, bool rewardMultiplierEnabled)
    {
        if (crpgPeer.SpawnTeamThisRound == null)
        {
            return;
        }

        float totalRewardMultiplier = crpgPeer.RewardMultiplier * roundDuration;
        userUpdate.Reward = new CrpgUserReward
        {
            Experience = (int)(totalRewardMultiplier * _constants.ExperienceGainPerSecond),
            Gold = (int)(totalRewardMultiplier * _constants.GoldGainPerSecond),
        };

        crpgPeer.RewardMultiplier =
            _roundController.RoundWinner == crpgPeer.SpawnTeamThisRound.Side && rewardMultiplierEnabled
                ? Math.Min(5, crpgPeer.RewardMultiplier + 1)
                : 1;
    }

    private CrpgCharacterRating GetNewRating(CrpgPeer crpgPeer)
    {
        if (!_characterRatings.TryGetValue(crpgPeer.Peer.Id, out var rating))
        {
            return crpgPeer.User!.Character.Rating;
        }

        return new CrpgCharacterRating
        {
            Value = (float)rating.Glicko2Rating,
            Deviation = (float)rating.Glicko2RatingDeviation,
            Volatility = (float)rating.Volatility,
        };
    }

    private IList<CrpgUserBrokenItem> BreakItems(CrpgPeer crpgPeer, float roundDuration)
    {
        if (crpgPeer.SpawnTeamThisRound == null)
        {
            return Array.Empty<CrpgUserBrokenItem>();
        }

        List<CrpgUserBrokenItem> brokenItems = new();
        foreach (var equippedItem in crpgPeer.User!.Character.EquippedItems)
        {
            var mbItem = Game.Current.ObjectManager.GetObject<ItemObject>(equippedItem.UserItem.BaseItemId);
            if (_random.NextDouble() >= _constants.ItemBreakChance)
            {
                continue;
            }

            int repairCost = (int)(mbItem.Value * roundDuration * _constants.ItemRepairCostPerSecond);
            brokenItems.Add(new CrpgUserBrokenItem
            {
                UserItemId = equippedItem.UserItem.Id,
                RepairCost = repairCost,
            });
        }

        return brokenItems;
    }

    private void SetStatistics(CrpgUserUpdate userUpdate, NetworkCommunicator networkPeer,
        Dictionary<PlayerId, CrpgCharacterStatistics> newRoundAllTotalStats)
    {
        var missionPeer = networkPeer.GetComponent<MissionPeer>();
        var newRoundTotalStats = new CrpgCharacterStatistics
        {
            Kills = missionPeer.KillCount,
            Deaths = missionPeer.DeathCount,
            Assists = missionPeer.AssistCount,
            PlayTime = DateTime.Now - missionPeer.JoinTime,
        };

        if (_lastRoundAllTotalStats.TryGetValue(networkPeer.VirtualPlayer.Id, out var lastRoundTotalStats))
        {
            userUpdate.Statistics.Kills = newRoundTotalStats.Kills - lastRoundTotalStats.Kills;
            userUpdate.Statistics.Deaths = newRoundTotalStats.Deaths - lastRoundTotalStats.Deaths;
            userUpdate.Statistics.Assists = newRoundTotalStats.Assists - lastRoundTotalStats.Assists;
            userUpdate.Statistics.PlayTime = newRoundTotalStats.PlayTime - lastRoundTotalStats.PlayTime;
        }
        else
        {
            userUpdate.Statistics.Kills = newRoundTotalStats.Kills;
            userUpdate.Statistics.Deaths = newRoundTotalStats.Deaths;
            userUpdate.Statistics.Assists = newRoundTotalStats.Assists;
            userUpdate.Statistics.PlayTime = newRoundTotalStats.PlayTime;
        }

        newRoundAllTotalStats[networkPeer.VirtualPlayer.Id] = newRoundTotalStats;
    }

    private void SendRewardToPeers(IList<UpdateCrpgUserResult> updateResults,
        Dictionary<int, CrpgPeer> crpgPeerByUserId)
    {
        foreach (var updateResult in updateResults)
        {
            if (!crpgPeerByUserId.TryGetValue(updateResult.User.Id, out var crpgPeer))
            {
                Debug.Print($"Unknown user with id '{updateResult.User.Id}'");
                continue;
            }

            crpgPeer.User = updateResult.User;

            GameNetwork.BeginModuleEventAsServer(crpgPeer.GetNetworkPeer());
            GameNetwork.WriteMessage(new CrpgRewardUser
            {
                Reward = updateResult.EffectiveReward,
                RepairCost = updateResult.RepairedItems.Sum(r => r.RepairCost),
                SoldItemIds = updateResult.RepairedItems.Where(r => r.Sold).Select(r => r.ItemId).ToList(),
            });
            GameNetwork.EndModuleEventAsServer();
        }
    }

    private void SendErrorToPeers(Dictionary<int, CrpgPeer> crpgPeerByUserId)
    {
        foreach (var crpgPeer in crpgPeerByUserId.Values)
        {
            GameNetwork.BeginModuleEventAsServer(crpgPeer.GetNetworkPeer());
            GameNetwork.WriteMessage(new CrpgRewardError());
            GameNetwork.EndModuleEventAsServer();
        }
    }
}
