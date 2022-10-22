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

        float inflictedRatio = blow.InflictedDamage / affectedAgent.BaseHealthLimit;
        Debug.Print($"{blow.InflictedDamage} / {affectedAgent.BaseHealthLimit} = {inflictedRatio}");
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
            var crpgRepresentative = agent.MissionPeer.Peer.GetComponent<CrpgRepresentative>();
            if (crpgRepresentative?.User == null)
            {
                return false;
            }

            var characterRating = crpgRepresentative.User.Character.Rating;
            rating = new CrpgRating(characterRating.Value, characterRating.Deviation, characterRating.Volatility);
            _characterRatings[crpgRepresentative.MissionPeer.Peer.Id] = rating;
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
        int ticks = ComputeTicks();
        Debug.Print($"Ticks for round with {GameNetwork.NetworkPeers.Count()}: {ticks}");

        CrpgRatingCalculator.UpdateRatings(_ratingResults);

        Dictionary<int, CrpgRepresentative> crpgRepresentativeByUserId = new();
        var newRoundAllTotalStats = new Dictionary<PlayerId, CrpgCharacterStatistics>();
        List<CrpgUserUpdate> userUpdates = new();
        foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
        {
            var crpgRepresentative = networkPeer.GetComponent<CrpgRepresentative>();
            if (crpgRepresentative?.User == null)
            {
                continue;
            }

            crpgRepresentativeByUserId[crpgRepresentative.User.Id] = crpgRepresentative;

            CrpgUserUpdate userUpdate = new()
            {
                CharacterId = crpgRepresentative.User.Character.Id,
                Reward = new CrpgUserReward { Experience = 0, Gold = 0 },
                Statistics = new CrpgCharacterStatistics { Kills = 0, Deaths = 0, Assists = 0, PlayTime = TimeSpan.Zero },
                Rating = GetNewRating(crpgRepresentative),
                BrokenItems = BreakItems(crpgRepresentative),
            };

            SetReward(userUpdate, crpgRepresentative, ticks);
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
            SendRewardToPeers(res.UpdateResults, crpgRepresentativeByUserId);
        }
        catch (Exception e)
        {
            Debug.Print("Couldn't update users: " + e);
            SendErrorToPeers(crpgRepresentativeByUserId);
        }
    }

    private int ComputeTicks()
    {
        float roundDuration = MultiplayerOptions.OptionType.RoundTimeLimit.GetIntValue() - _roundController.RemainingRoundTime;
        if (roundDuration < 30)
        {
            return 1;
        }

        return 1 + (int)roundDuration / 60;
    }

    private void SetReward(CrpgUserUpdate userUpdate, CrpgRepresentative crpgRepresentative, int ticks)
    {
        if (crpgRepresentative.SpawnTeamThisRound == null)
        {
            return;
        }

        int totalRewardMultiplier = crpgRepresentative.RewardMultiplier * ticks;
        userUpdate.Reward = new CrpgUserReward
        {
            Experience = totalRewardMultiplier * 1000,
            Gold = totalRewardMultiplier * 50,
        };

        crpgRepresentative.RewardMultiplier = _roundController.RoundWinner == crpgRepresentative.SpawnTeamThisRound.Side
            ? Math.Min(5, crpgRepresentative.RewardMultiplier + 1)
            : 1;
    }

    private CrpgCharacterRating GetNewRating(CrpgRepresentative crpgRepresentative)
    {
        if (!_characterRatings.TryGetValue(crpgRepresentative.Peer.Id, out var rating))
        {
            return crpgRepresentative.User!.Character.Rating;
        }

        var a = new CrpgCharacterRating
        {
            Value = (float)rating.Glicko2Rating,
            Deviation = (float)rating.Glicko2RatingDeviation,
            Volatility = (float)rating.Volatility,
        };
        Debug.Print($"{crpgRepresentative.Peer.UserName}: {a.Value - a.Deviation * 2} ({a.Value} {a.Deviation})");
        return a;
    }

    private IList<CrpgUserBrokenItem> BreakItems(CrpgRepresentative crpgRepresentative)
    {
        if (crpgRepresentative.SpawnTeamThisRound == null)
        {
            return Array.Empty<CrpgUserBrokenItem>();
        }

        List<CrpgUserBrokenItem> brokenItems = new();
        foreach (var equippedItem in crpgRepresentative.User!.Character.EquippedItems)
        {
            var mbItem = Game.Current.ObjectManager.GetObject<ItemObject>(equippedItem.UserItem.BaseItemId);
            if (_random.NextDouble() >= _constants.ItemBreakChance)
            {
                continue;
            }

            int repairCost = (int)MathHelper.ApplyPolynomialFunction(mbItem.Value, _constants.ItemRepairCostCoefs);
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
        Dictionary<int, CrpgRepresentative> crpgRepresentativeByUserId)
    {
        foreach (var updateResult in updateResults)
        {
            if (!crpgRepresentativeByUserId.TryGetValue(updateResult.User.Id, out var crpgRepresentative))
            {
                Debug.Print($"Unknown user with id '{updateResult.User.Id}'");
                continue;
            }

            crpgRepresentative.User = updateResult.User;

            GameNetwork.BeginModuleEventAsServer(crpgRepresentative.GetNetworkPeer());
            GameNetwork.WriteMessage(new CrpgRewardUser
            {
                Reward = updateResult.EffectiveReward,
                RepairCost = updateResult.RepairedItems.Sum(r => r.RepairCost),
                SoldItemIds = updateResult.RepairedItems.Where(r => r.Sold).Select(r => r.ItemId).ToList(),
            });
            GameNetwork.EndModuleEventAsServer();
        }
    }

    private void SendErrorToPeers(Dictionary<int, CrpgRepresentative> crpgRepresentativeByUserId)
    {
        foreach (var crpgRepresentative in crpgRepresentativeByUserId.Values)
        {
            GameNetwork.BeginModuleEventAsServer(crpgRepresentative.GetNetworkPeer());
            GameNetwork.WriteMessage(new CrpgRewardError());
            GameNetwork.EndModuleEventAsServer();
        }
    }
}
