using Crpg.Module.Api;
using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Common.Network;
using Crpg.Module.Common.Warmup;
using Crpg.Module.Rating;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.PlayerServices;

namespace Crpg.Module.Common;

/// <summary>
/// Gives xp/gold, update rating and stats.
/// </summary>
internal class CrpgRewardServer : MissionBehavior
{
    /// <summary>When there is no round controller (e.g. siege), rewards are sent every minute.</summary>
    private const int TickDuration = 60;

    private readonly ICrpgClient _crpgClient;
    private readonly CrpgConstants _constants;
    private readonly CrpgWarmupComponent _warmupComponent;
    private readonly MultiplayerRoundController? _roundController;
    private readonly Dictionary<PlayerId, CrpgRating> _characterRatings;
    private readonly CrpgRatingPeriodResults _ratingResults;
    private readonly Random _random = new();

    private Dictionary<PlayerId, CrpgCharacterStatistics> _lastAllTotalStats = new();
    private MissionTimer? _tickTimer;
    private bool _lastRewardDuringPrimeTime;

    public CrpgRewardServer(
        ICrpgClient crpgClient,
        CrpgConstants constants,
        CrpgWarmupComponent warmupComponent,
        MultiplayerRoundController? roundController)
    {
        _crpgClient = crpgClient;
        _constants = constants;
        _warmupComponent = warmupComponent;
        _roundController = roundController;
        _characterRatings = new Dictionary<PlayerId, CrpgRating>();
        _ratingResults = new CrpgRatingPeriodResults();
        _lastRewardDuringPrimeTime = false;
    }

    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

    public override void AfterStart()
    {
        base.AfterStart();
        if (_roundController != null)
        {
            _roundController.OnPreRoundEnding += OnPreRoundEnding;
        }
        else
        {
            _warmupComponent.OnWarmupEnded += OnWarmupEnded;
        }
    }

    public override void OnRemoveBehavior()
    {
        if (_roundController != null)
        {
            _roundController.OnPreRoundEnding -= OnPreRoundEnding;
        }
        else
        {
            _warmupComponent.OnWarmupEnded -= OnWarmupEnded;
        }

        base.OnRemoveBehavior();
    }

    public override void OnMissionTick(float dt)
    {
        if (_tickTimer != null && _tickTimer.Check(reset: true))
        {
            _ = UpdateCrpgUsersAsync(_tickTimer.GetTimerDuration());
        }
    }

    public override void OnAgentHit(
        Agent affectedAgent,
        Agent affectorAgent,
        in MissionWeapon affectorWeapon,
        in Blow blow,
        in AttackCollisionData attackCollisionData)
    {
        if (_warmupComponent.IsInWarmup)
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
        float duration = MultiplayerOptions.OptionType.RoundTimeLimit.GetIntValue() - _roundController!.RemainingRoundTime;
        _ = UpdateCrpgUsersAsync(duration);
    }

    private void OnWarmupEnded()
    {
        _tickTimer = new MissionTimer(TickDuration);
    }

    private async Task UpdateCrpgUsersAsync(float durationRewarded)
    {
        CrpgRatingCalculator.UpdateRatings(_ratingResults);

        Dictionary<int, CrpgPeer> crpgPeerByUserId = new();
        var newAllTotalStats = new Dictionary<PlayerId, CrpgCharacterStatistics>();
        List<CrpgUserUpdate> userUpdates = new();
        var networkPeers = GameNetwork.NetworkPeers.ToArray();
        bool rewardMultiplierEnabled = networkPeers.Length > 4;
        foreach (NetworkCommunicator networkPeer in networkPeers)
        {
            var missionPeer = networkPeer.GetComponent<MissionPeer>();
            var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
            if (missionPeer == null || crpgPeer?.User == null)
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
                BrokenItems = Array.Empty<CrpgUserBrokenItem>(),
            };

            if ((_roundController != null && crpgPeer.SpawnTeamThisRound != null)
                || (_roundController == null && missionPeer.Team != null && missionPeer.Team.Side != BattleSideEnum.None))
            {
                SetReward(userUpdate, crpgPeer, durationRewarded, rewardMultiplierEnabled);
                SetStatistics(userUpdate, networkPeer, newAllTotalStats);
                userUpdate.BrokenItems = BreakItems(crpgPeer, durationRewarded);
            }

            userUpdates.Add(userUpdate);
        }

        _ratingResults.Clear();
        _characterRatings.Clear();

        if (userUpdates.Count == 0)
        {
            return;
        }

        // Save last stats to be able to make the difference next time.
        _lastAllTotalStats = newAllTotalStats;

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

    private void SetReward(CrpgUserUpdate userUpdate, CrpgPeer crpgPeer, float durationRewarded, bool rewardMultiplierEnabled)
    {
        float serverXpMultiplier = CrpgServerConfiguration.ServerExperienceMultiplier;
        serverXpMultiplier *= IsPrimeTime() ? 2 : 1;
        userUpdate.Reward = new CrpgUserReward
        {
            Experience = (int)(serverXpMultiplier * durationRewarded * (_constants.BaseExperienceGainPerSecond
                + crpgPeer.RewardMultiplier * _constants.MultipliedExperienceGainPerSecond)),
            Gold = (int)(durationRewarded * (_constants.BaseGoldGainPerSecond
                + crpgPeer.RewardMultiplier * _constants.MultipliedGoldGainPerSecond)),
        };

        if (!rewardMultiplierEnabled)
        {
            crpgPeer.RewardMultiplier = 1;
        }
        else if (_roundController == null)
        {
            crpgPeer.RewardMultiplier = 2;
        }
        else
        {
            crpgPeer.RewardMultiplier = _roundController.RoundWinner == crpgPeer.SpawnTeamThisRound!.Side
                ? Math.Min(5, crpgPeer.RewardMultiplier + 1)
                : 1;
        }
    }

    private bool IsPrimeTime()
    {
        var primeTime = CrpgServerConfiguration.ServerPrimeTime;
        if (primeTime == null)
        {
            return false;
        }

        TimeSpan timeOfDay = DateTime.Now.TimeOfDay;
        if (timeOfDay < primeTime.Item1 || primeTime.Item2 < timeOfDay)
        {
            if (_lastRewardDuringPrimeTime)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new CrpgNotification
                {
                    Type = CrpgNotification.NotificationType.Announcement,
                    Message = "Prime time ended!",
                });
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }

            _lastRewardDuringPrimeTime = false;
            return false;
        }

        if (!_lastRewardDuringPrimeTime)
        {
            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(new CrpgNotification
            {
                Type = CrpgNotification.NotificationType.Announcement,
                Message = "It's prime time! Experience is multiplied by two during that time.",
            });
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
        }

        _lastRewardDuringPrimeTime = true;
        return true;
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
        Dictionary<PlayerId, CrpgCharacterStatistics> newAllTotalStats)
    {
        var missionPeer = networkPeer.GetComponent<MissionPeer>();
        var newTotalStats = new CrpgCharacterStatistics
        {
            Kills = missionPeer.KillCount,
            Deaths = missionPeer.DeathCount,
            Assists = missionPeer.AssistCount,
            PlayTime = DateTime.Now - missionPeer.JoinTime,
        };

        if (_lastAllTotalStats.TryGetValue(networkPeer.VirtualPlayer.Id, out var lastTotalStats))
        {
            userUpdate.Statistics.Kills = newTotalStats.Kills - lastTotalStats.Kills;
            userUpdate.Statistics.Deaths = newTotalStats.Deaths - lastTotalStats.Deaths;
            userUpdate.Statistics.Assists = newTotalStats.Assists - lastTotalStats.Assists;
            userUpdate.Statistics.PlayTime = newTotalStats.PlayTime - lastTotalStats.PlayTime;
        }
        else
        {
            userUpdate.Statistics.Kills = newTotalStats.Kills;
            userUpdate.Statistics.Deaths = newTotalStats.Deaths;
            userUpdate.Statistics.Assists = newTotalStats.Assists;
            userUpdate.Statistics.PlayTime = newTotalStats.PlayTime;
        }

        newAllTotalStats[networkPeer.VirtualPlayer.Id] = newTotalStats;
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
