using Crpg.Module.Api;
using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Common;
using Crpg.Module.Common.Network;
using Crpg.Module.Modes.Warmup;
using Crpg.Module.Rating;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.PlayerServices;
using MathF = TaleWorlds.Library.MathF;

namespace Crpg.Module.Rewards;

/// <summary>
/// Gives xp/gold, update rating and stats.
/// </summary>
internal class CrpgRewardServer : MissionLogic
{
    public const int ExperienceMultiplierMin = 1;
    public const int ExperienceMultiplierMax = 5;

    private readonly ICrpgClient _crpgClient;
    private readonly CrpgConstants _constants;
    private readonly CrpgWarmupComponent? _warmupComponent;
    private readonly Dictionary<PlayerId, CrpgPlayerRating> _characterRatings;
    private readonly CrpgRatingPeriodResults _ratingResults;
    private readonly Random _random;
    private readonly PeriodStatsHelper _periodStatsHelper;
    private readonly Dictionary<int, AgentHitRegistry> _agentsThatGotTeamHitThisRoundByCrpgUserId;
    private readonly bool _isTeamHitCompensationsEnabled;

    private bool _lastRewardDuringHappyHours;

    public CrpgRewardServer(
        ICrpgClient crpgClient,
        CrpgConstants constants,
        CrpgWarmupComponent? warmupComponent,
        bool enableTeamHitCompensations)
    {
        _crpgClient = crpgClient;
        _constants = constants;
        _warmupComponent = warmupComponent;
        _characterRatings = new Dictionary<PlayerId, CrpgPlayerRating>();
        _ratingResults = new CrpgRatingPeriodResults();
        _random = new Random();
        _periodStatsHelper = new PeriodStatsHelper();
        _lastRewardDuringHappyHours = false;
        _agentsThatGotTeamHitThisRoundByCrpgUserId = new();
        _isTeamHitCompensationsEnabled = enableTeamHitCompensations;
    }

    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();

        if (_warmupComponent != null)
        {
            _warmupComponent.OnWarmupEnded += OnWarmupEnded;
        }
    }

    public override void OnRemoveBehavior()
    {
        base.OnRemoveBehavior();

        if (_warmupComponent != null)
        {
            _warmupComponent.OnWarmupEnded -= OnWarmupEnded;
        }
    }

    public override void OnScoreHit(
        Agent affectedAgent,
        Agent affectorAgent,
        WeaponComponentData attackerWeapon,
        bool isBlocked,
        bool isSiegeEngineHit,
        in Blow blow,
        in AttackCollisionData collisionData,
        float damagedHp,
        float hitDistance,
        float shotDifficulty)
    {
        if (_warmupComponent == null || _warmupComponent.IsInWarmup)
        {
            return;
        }

        if (affectedAgent == affectorAgent) // Self hit.
        {
            return;
        }

        if (affectedAgent.Team == affectorAgent.Team) // Team hit.
        {
            RegisterHitForCompensation(affectedAgent, affectorAgent, (int)damagedHp);
            return;
        }

        if (!TryGetRating(affectedAgent, out var affectedRating)
            || !TryGetRating(affectorAgent, out var affectorRating))
        {
            return;
        }

        float inflictedRatio = MathF.Clamp(blow.InflictedDamage / affectedAgent.BaseHealthLimit, 0f, 1f);
        _ratingResults.AddResult(affectorRating!, affectedRating!, inflictedRatio);
    }

    /// <summary>
    /// Update rating and statistics from the last time this method was called and also give rewards.
    /// </summary>
    /// <param name="durationRewarded">Duration for which the users should be rewarded.</param>
    /// <param name="defenderMultiplierGain">Multiplier to add to the defenders. Can be negative.</param>
    /// <param name="attackerMultiplierGain">Multiplier to add to the attackers. Can be negative.</param>
    /// <param name="valourTeamSide">Team to give valour to.</param>
    /// <param name="constantMultiplier">Multiplier that should be given to everyone disregarding any other parameters.</param>
    /// <param name="updateUserStats">True if score and rating should be saved.</param>
    public async Task UpdateCrpgUsersAsync(
        float durationRewarded,
        int defenderMultiplierGain = 0,
        int attackerMultiplierGain = 0,
        BattleSideEnum? valourTeamSide = null,
        int? constantMultiplier = null,
        bool updateUserStats = true)
    {
        var networkPeers = GameNetwork.NetworkPeers.ToArray();
        if (networkPeers.Length == 0)
        {
            return;
        }

        bool verylowPopulationServer = networkPeers.Length < 2;
        bool lowPopulationServer = networkPeers.Length < 12;
        // Force constant multiplier if there is low population.
        constantMultiplier = verylowPopulationServer ? ExperienceMultiplierMin : constantMultiplier;

        CrpgRatingCalculator.UpdateRatings(_ratingResults);
        Dictionary<PlayerId, PeriodStats> periodStats = _periodStatsHelper.ComputePeriodStats();

        var valorousPlayerIds = verylowPopulationServer || !valourTeamSide.HasValue
            ? new HashSet<PlayerId>()
            : GetValorousPlayers(networkPeers, periodStats, valourTeamSide.Value);

        Dictionary<int, CrpgPeer> crpgPeerByCrpgUserId = new();
        List<CrpgUserUpdate> userUpdates = new();
        Dictionary<int, IList<CrpgUserDamagedItem>> brokenItems = GetBrokenItemsByCrpgUserId(networkPeers, durationRewarded, lowPopulationServer);

        var compensationByCrpgUserId = _isTeamHitCompensationsEnabled ? CalculateCompensationByCrpgUserId(brokenItems) : new();
        foreach (NetworkCommunicator networkPeer in networkPeers)
        {
            var playerId = networkPeer.VirtualPlayer.Id;

            var missionPeer = networkPeer.GetComponent<MissionPeer>();
            var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
            if (missionPeer == null || crpgPeer?.User == null)
            {
                continue;
            }

            int crpgUserId = crpgPeer.User.Id;
            crpgPeerByCrpgUserId[crpgUserId] = crpgPeer;

            CrpgUserUpdate userUpdate = new()
            {
                CharacterId = crpgPeer.User.Character.Id,
                Reward = new CrpgUserReward { Experience = 0, Gold = 0 },
                Statistics = new CrpgCharacterStatistics { Kills = 0, Deaths = 0, Assists = 0, PlayTime = TimeSpan.Zero },
                Rating = crpgPeer.User.Character.Rating,
                BrokenItems = Array.Empty<CrpgUserDamagedItem>(),
            };

            if (CrpgFeatureFlags.IsEnabled(CrpgFeatureFlags.FeatureTournament))
            {
                userUpdates.Add(userUpdate);
                continue;
            }

            if (updateUserStats)
            {
                userUpdate.Rating = GetNewRating(crpgPeer);
            }

            if (missionPeer.Team != null && missionPeer.Team.Side != BattleSideEnum.None)
            {
                bool isValorousPlayer = valorousPlayerIds.Contains(playerId);
                int compensationForCrpgUser = compensationByCrpgUserId.TryGetValue(crpgUserId, out int compensation) ? compensation : 0;
                SetRewardForConnectedPlayer(userUpdate, crpgPeer, durationRewarded, compensationForCrpgUser, isValorousPlayer,
                    defenderMultiplierGain, attackerMultiplierGain, constantMultiplier);

                if (updateUserStats)
                {
                    SetStatistics(userUpdate, networkPeer, periodStats);
                }

                userUpdate.BrokenItems = brokenItems[crpgUserId];

                // TODO should probably implement compensation for disconnected users here
            }

            userUpdates.Add(userUpdate);
        }

        _ratingResults.Clear();
        _characterRatings.Clear();
        _agentsThatGotTeamHitThisRoundByCrpgUserId.Clear();

        if (userUpdates.Count == 0)
        {
            return;
        }

        // TODO: add retry mechanism (the endpoint need to be idempotent though).
        try
        {
            var res = (await _crpgClient.UpdateUsersAsync(new CrpgGameUsersUpdateRequest { Updates = userUpdates })).Data!;
            SendRewardToPeers(res.UpdateResults, crpgPeerByCrpgUserId, valorousPlayerIds, compensationByCrpgUserId);
        }
        catch (Exception e)
        {
            Debug.Print("Couldn't update users: " + e);
            SendErrorToPeers(crpgPeerByCrpgUserId);
        }
    }

    private Dictionary<int, IList<CrpgUserDamagedItem>> GetBrokenItemsByCrpgUserId(NetworkCommunicator[] networkPeers, float durationRewarded, bool isLowPopulationServer)
    {
        Dictionary<int, IList<CrpgUserDamagedItem>> brokenItems = new();
        foreach (NetworkCommunicator networkPeer in networkPeers)
        {
            var missionPeer = networkPeer.GetComponent<MissionPeer>();
            var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
            if (missionPeer == null || crpgPeer?.User == null)
            {
                continue;
            }

            if (missionPeer.Team != null && missionPeer.Team.Side != BattleSideEnum.None)
            {
                int crpgUserId = crpgPeer.User.Id;
                var crpgUserDamagedItems = BreakItems(crpgPeer, durationRewarded * (isLowPopulationServer ? 0f : 1f));
                brokenItems[crpgUserId] = crpgUserDamagedItems;
            }
        }

        return brokenItems;
    }

    private void RegisterHitForCompensation(Agent affectedAgent, Agent affectorAgent, int inflictedDamage)
    {
        if (!TryGetCrpgUserId(affectedAgent, out int affectedCrpgUserId) || !TryGetCrpgUserId(affectorAgent, out int affectorCrpgUserId))
        {
            return;
        }

        if (!_agentsThatGotTeamHitThisRoundByCrpgUserId.TryGetValue(affectedCrpgUserId, out var affectedAgentHitRegistry))
        {
            affectedAgentHitRegistry = new AgentHitRegistry(affectedCrpgUserId, (int)affectedAgent.BaseHealthLimit);
            _agentsThatGotTeamHitThisRoundByCrpgUserId.Add(affectedCrpgUserId, affectedAgentHitRegistry);
        }

        if (affectedAgentHitRegistry.DamageByUserId.ContainsKey(affectorCrpgUserId))
        {
            affectedAgentHitRegistry.DamageByUserId[affectorCrpgUserId] += inflictedDamage;
        }
        else
        {
            affectedAgentHitRegistry.DamageByUserId.Add(affectorCrpgUserId, inflictedDamage);
        }
    }

    private bool TryGetCrpgUserId(Agent agent, out int crpgUserId)
    {
        crpgUserId = 0;
        var user = agent.MissionPeer?.GetNetworkPeer()?.GetComponent<CrpgPeer>()?.User;
        if (user == null)
        {
            return false;
        }

        crpgUserId = user.Id;
        return true;
    }

    private Dictionary<int, int> CalculateCompensationByCrpgUserId(Dictionary<int, IList<CrpgUserDamagedItem>> brokenItemsByCrpgUserId)
    {
        Dictionary<int, int> compensationByCrpgUserId = new();
        foreach (var affectedEntry in _agentsThatGotTeamHitThisRoundByCrpgUserId)
        {
            int affectedCrpgUserId = affectedEntry.Key;
            AgentHitRegistry affectedAgentHitRegistry = affectedEntry.Value;
            foreach (var damageByUserIdEntry in affectedAgentHitRegistry.DamageByUserId)
            {
                int affectorCrpgUserId = damageByUserIdEntry.Key;
                int damageDone = damageByUserIdEntry.Value;

                if (affectedCrpgUserId == affectorCrpgUserId)
                {
                    continue;
                }

                float compensationRatio = damageDone / (float)affectedAgentHitRegistry.BaseHealthLimit;
                int repairCostOfAffectedUser = brokenItemsByCrpgUserId.TryGetValue(affectedCrpgUserId, out var brokenItems) ? brokenItems.Sum(r => r.RepairCost) : 0;
                int compensatedRepairCost = (int)Math.Floor(repairCostOfAffectedUser * compensationRatio);

                if (!compensationByCrpgUserId.TryAdd(affectedCrpgUserId, compensatedRepairCost))
                {
                    compensationByCrpgUserId[affectedCrpgUserId] += compensatedRepairCost;
                }

                if (!compensationByCrpgUserId.TryAdd(affectorCrpgUserId, compensatedRepairCost * -1))
                {
                    compensationByCrpgUserId[affectorCrpgUserId] -= compensatedRepairCost;
                }
            }
        }

        return compensationByCrpgUserId;
    }

    private bool TryGetRating(Agent agent, out CrpgPlayerRating? rating)
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

            // If the user has no region yet or they are not playing locally, act like they weren't there. That is, don't
            // change their rating or their opponent rating.
            if (crpgPeer.User.Region != CrpgServerConfiguration.Region)
            {
                return false;
            }

            var characterRating = crpgPeer.User.Character.Rating;
            rating = new CrpgPlayerRating(characterRating.Value, characterRating.Deviation, characterRating.Volatility);
            _characterRatings[agent.MissionPeer.Peer.Id] = rating;
            _ratingResults.AddParticipant(rating);
        }

        return true;
    }

    private void OnWarmupEnded()
    {
        _ = UpdateCrpgUsersAsync(durationRewarded: 0, updateUserStats: false);
    }

    private void SetRewardForConnectedPlayer(
        CrpgUserUpdate userUpdate,
        CrpgPeer crpgPeer,
        float durationRewarded,
        int compensationAmount,
        bool isValorousPlayer,
        int defenderMultiplierGain,
        int attackerMultiplierGain,
        int? constantMultiplier)
    {
        float serverXpMultiplier = CrpgServerConfiguration.ServerExperienceMultiplier;
        serverXpMultiplier *= IsHappyHour() ? 1.5f : 1f;
        userUpdate.Reward = new CrpgUserReward
        {
            Experience = (int)(serverXpMultiplier * durationRewarded * (_constants.BaseExperienceGainPerSecond
                + crpgPeer.RewardMultiplier * _constants.MultipliedExperienceGainPerSecond)),
            Gold = (int)(durationRewarded * (_constants.BaseGoldGainPerSecond
                + crpgPeer.RewardMultiplier * _constants.MultipliedGoldGainPerSecond)
                + compensationAmount),
        };

        if (constantMultiplier != null)
        {
            crpgPeer.RewardMultiplier = constantMultiplier.Value;
        }
        else if (crpgPeer.LastSpawnTeam != null)
        {
            int rewardMultiplier = crpgPeer.RewardMultiplier;
            if (crpgPeer.LastSpawnTeam.Side == BattleSideEnum.Defender)
            {
                rewardMultiplier += isValorousPlayer ? Math.Max(defenderMultiplierGain, 1) : defenderMultiplierGain;
            }
            else if (crpgPeer.LastSpawnTeam.Side == BattleSideEnum.Attacker)
            {
                rewardMultiplier += isValorousPlayer ? Math.Max(attackerMultiplierGain, 1) : attackerMultiplierGain;
            }

            crpgPeer.RewardMultiplier = Math.Max(Math.Min(rewardMultiplier, ExperienceMultiplierMax), ExperienceMultiplierMin);
        }
    }

    private bool IsHappyHour()
    {
        var happyHours = CrpgServerConfiguration.HappyHours;
        if (happyHours == null)
        {
            return false;
        }

        TimeSpan timeOfDay = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, happyHours.Item3).TimeOfDay;
        if (timeOfDay < happyHours.Item1 || happyHours.Item2 < timeOfDay)
        {
            if (_lastRewardDuringHappyHours)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new CrpgNotification
                {
                    Type = CrpgNotificationType.Announcement,
                    Message = "Happy hours ended!",
                });
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }

            _lastRewardDuringHappyHours = false;
            return false;
        }

        if (!_lastRewardDuringHappyHours)
        {
            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(new CrpgNotification
            {
                Type = CrpgNotificationType.Announcement,
                Message = "It's happy hours time! Experience gain is increased by 50%.",
            });
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
        }

        _lastRewardDuringHappyHours = true;
        return true;
    }

    /// <summary>Valorous players are the top X% of the round of the defeated team.</summary>
    private HashSet<PlayerId> GetValorousPlayers(NetworkCommunicator[] networkPeers,
        Dictionary<PlayerId, PeriodStats> allPeriodStats, BattleSideEnum valourTeamSide)
    {
        var defeatedTeamPlayersWithRoundScore = new List<(PlayerId playerId, int score)>();
        foreach (var networkPeer in networkPeers)
        {
            var missionPeer = networkPeer.GetComponent<MissionPeer>();
            var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
            if (missionPeer == null || crpgPeer == null)
            {
                continue;
            }

            if (crpgPeer.LastSpawnTeam?.Side == valourTeamSide)
            {
                var playerId = networkPeer.VirtualPlayer.Id;
                int roundScore = allPeriodStats.TryGetValue(playerId, out var s) ? s.Score : 0;
                defeatedTeamPlayersWithRoundScore.Add((playerId, roundScore));
            }
        }

        int numberOfPlayersToGiveValour = (int)(0.2f * defeatedTeamPlayersWithRoundScore.Count);
        Debug.Print($"Giving valour to {numberOfPlayersToGiveValour} out of the {defeatedTeamPlayersWithRoundScore.Count} players in the defeated team");
        return defeatedTeamPlayersWithRoundScore
            .OrderByDescending(p => p.score)
            .Take(numberOfPlayersToGiveValour)
            .Select(p => p.playerId)
            .ToHashSet();
    }

    private CrpgCharacterRating GetNewRating(CrpgPeer crpgPeer)
    {
        if (!_characterRatings.TryGetValue(crpgPeer.Peer.Id, out var rating))
        {
            return crpgPeer.User!.Character.Rating;
        }

        // Values are clamped in case there is an issue in the rating algorithm.
        return new CrpgCharacterRating
        {
            Value = MathF.Clamp(rating.Rating, -100_000, 100_000),
            Deviation = MathF.Clamp(rating.RatingDeviation, -100_000, 100_000),
            Volatility = MathF.Clamp(rating.Volatility, -100_000, 100_000),
        };
    }

    private IList<CrpgUserDamagedItem> BreakItems(CrpgPeer crpgPeer, float roundDuration)
    {
        List<CrpgUserDamagedItem> brokenItems = new();
        foreach (var equippedItem in crpgPeer.User!.Character.EquippedItems)
        {
            var mbItem = Game.Current.ObjectManager.GetObject<ItemObject>(equippedItem.UserItem.ItemId);
            if (_random.NextDouble() >= _constants.ItemBreakChance)
            {
                continue;
            }

            int repairCost = (int)(mbItem.Value * roundDuration * _constants.ItemRepairCostPerSecond);
            brokenItems.Add(new CrpgUserDamagedItem
            {
                UserItemId = equippedItem.UserItem.Id,
                RepairCost = repairCost,
            });
        }

        return brokenItems;
    }

    private void SetStatistics(CrpgUserUpdate userUpdate, NetworkCommunicator networkPeer,
        Dictionary<PlayerId, PeriodStats> allPeriodStats)
    {
        if (allPeriodStats.TryGetValue(networkPeer.VirtualPlayer.Id, out var peerPeriodStats))
        {
            userUpdate.Statistics = new CrpgCharacterStatistics
            {
                Kills = peerPeriodStats.Kills,
                Deaths = peerPeriodStats.Deaths,
                Assists = peerPeriodStats.Assists,
                PlayTime = peerPeriodStats.PlayTime,
            };
        }
    }

    private void SendRewardToPeers(IList<UpdateCrpgUserResult> updateResults,
        Dictionary<int, CrpgPeer> crpgPeerByCrpgUserId, HashSet<PlayerId> valorousPlayerIds, Dictionary<int, int> compensationByCrpgUserId)
    {
        foreach (var updateResult in updateResults)
        {
            if (!crpgPeerByCrpgUserId.TryGetValue(updateResult.User.Id, out var crpgPeer))
            {
                Debug.Print($"Unknown user with id '{updateResult.User.Id}'");
                continue;
            }

            var networkPeer = crpgPeer.GetNetworkPeer();
            if (!networkPeer.IsConnectionActive)
            {
                return;
            }

            crpgPeer.User = updateResult.User;
            if (crpgPeer.User.Character.ForTournament && !CrpgFeatureFlags.IsEnabled(CrpgFeatureFlags.FeatureTournament))
            {
                KickHelper.Kick(networkPeer, DisconnectType.KickedByHost, "tournament_only");
                continue;
            }

            int compensationForCrpgUser = compensationByCrpgUserId.TryGetValue(crpgPeer.User.Id, out int compensation) ? compensation : 0;
            // subtracting the compensation gold that was previously added onto the reward for the API call
            // it is for clarity reasons for the message that is being sent to the user
            updateResult.EffectiveReward.Gold -= compensationForCrpgUser;
            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new CrpgRewardUser
            {
                Reward = updateResult.EffectiveReward,
                Valour = valorousPlayerIds.Contains(networkPeer.VirtualPlayer.Id),
                RepairCost = updateResult.RepairedItems.Sum(r => r.RepairCost),
                Compensation = compensationForCrpgUser,
                BrokeItemIds = updateResult.RepairedItems.Where(r => r.Broke).Select(r => r.ItemId).ToList(),
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
