using System.Numerics;
using Crpg.Module.Common;
using Crpg.Module.Helpers;
using TaleWorlds.Library;

namespace Crpg.Module.Balancing;

internal class MatchBalancer
{
    public const float PowerParameter = 1f;
    public const int MaximumNumberOfSwaps = 20; // upperbound to limit number of swaps. Numberofswaps is often<3

    public GameMatch NaiveCaptainBalancing(GameMatch gameMatch)
    {
        List<WeightedCrpgUser> allWeightedCrpgUsers = new();
        allWeightedCrpgUsers.AddRange(gameMatch.TeamA);
        allWeightedCrpgUsers.AddRange(gameMatch.TeamB);
        allWeightedCrpgUsers.AddRange(gameMatch.Waiting);
        GameMatch returnedGameMatch = new();
        bool teamA = true;
        foreach (WeightedCrpgUser user in allWeightedCrpgUsers.OrderByDescending(u => u.Weight))
        {
            if (teamA)
            {
                returnedGameMatch.TeamA.Add(user);
                teamA = !teamA;
            }
            else
            {
                returnedGameMatch.TeamB.Add(user);
                teamA = !teamA;
            }
        }

        return returnedGameMatch;
    }

    public GameMatch BannerBalancingWithEdgeCases(GameMatch gameMatch, bool firstBalance = true, bool balanceOnce = true)
    {
        MatchBalancingHelpers.DumpTeamsStatus(gameMatch);
        Debug.Print(nameof(BannerBalancingWithEdgeCases));
        GameMatch balancedBannerGameMatch;

        if (gameMatch.TotalCount <= 2)
        {
            Debug.Print("Game has 2 or less players");
            balancedBannerGameMatch = RandomlyAssignWaitingPlayersTeam(gameMatch);
            MatchBalancingHelpers.DumpTeamsStatus(balancedBannerGameMatch);
            return balancedBannerGameMatch;
        }

        // This is the path we take when team were randomly assigned.
        // we do not care of completely scrambling the teams since no round was played yet
        if (firstBalance)
        {
            Debug.Print("This is the first Round");
            Debug.Print("--------------------------------------------");
            Debug.Print("Now splitting the clan groups between the two team");
            balancedBannerGameMatch = KkMakeTeamOfSimilarSizesWithoutSplittingClanGroups(gameMatch);
        }

        // in this path , the teams already played at least one round , so we do not want to scramble the teams.
        else
        {
            // when balanceOnce is true we rebalance the team only in case of extreme differences between both
            // the goal is to keep the team made at warmup so people feel they play for the same team the whole match
            // this works well if bo7 matches.
            if (balanceOnce && IsBalanceGoodEnough(gameMatch, maxSizeRatio: 0.7f, maxDifference: 15f, percentageDifference: 0.20f))
            {
                Debug.Print("This is not the first Round");
                Debug.Print("Balance is still good");
                return RandomlyAssignWaitingPlayersTeam(gameMatch);
            }
            else
            {
                Debug.Print("This is not the first Round and Balance was bad");
                Debug.Print("--------------------------------------------");
                Debug.Print("moving clanmates players to their clanmates teams , and moving players isolated from their clan to the opposite team.");
                balancedBannerGameMatch = MatchBalancingHelpers.RejoinClans(gameMatch);
            }
        }

        MatchBalancingHelpers.DumpTeamsStatus(balancedBannerGameMatch);

        if (IsBalanceGoodEnough(balancedBannerGameMatch, maxSizeRatio: 0.85f, maxDifference: 10f, percentageDifference: 0.10f))
        {
            Debug.Print("No need to do banner balancing");
            return RandomlyAssignWaitingPlayersTeam(gameMatch);
        }

        Debug.Print("Banner balancing now");
        balancedBannerGameMatch = BalanceTeamOfSimilarSizes(balancedBannerGameMatch, bannerBalance: true, 0.025f);
        Debug.Print("Banner balancing done");
        MatchBalancingHelpers.DumpTeamsStatus(balancedBannerGameMatch);

        if (IsBalanceGoodEnough(balancedBannerGameMatch, maxSizeRatio: 0.85f, maxDifference: 10f, percentageDifference: 0.10f))
        {
            Debug.Print("Banner Balance was enough.");
        }
        else
        {
            // This are failcases in case bannerbalance was not enough
            Debug.Print("ratio difference is above 10%");
            balancedBannerGameMatch = BalanceTeamOfSimilarSizes(balancedBannerGameMatch, bannerBalance: false, 0.10f);

            if (IsBalanceGoodEnough(balancedBannerGameMatch, maxSizeRatio: 0.75f, maxDifference: 10f, percentageDifference: 0.15f))
            {
                // A few swaps solved the problem. Most of the clangroups are intact
                Debug.Print("Ratio Difference is below 15%, we're not nuking.");
            }
            else
            {
                // A few swaps were not enough. Swaps are a form of gradient descent. Sometimes there are local extremas that are not global extremas
                // Here we completely abandon banner balance by completely reshuffling the card then redoing swaps
                Debug.Print("Swaps were not enough. This should really not happen often (ratio difference above 15%)");
                MatchBalancingHelpers.DumpTeamsStatus(balancedBannerGameMatch);
                MatchBalancingHelpers.DumpTeams(balancedBannerGameMatch);
                Debug.Print("NaiveCaptainBalancing + Balancing Without BannerGrouping");
                balancedBannerGameMatch = NaiveCaptainBalancing(balancedBannerGameMatch);
                balancedBannerGameMatch = BalanceTeamOfSimilarSizes(balancedBannerGameMatch, false, 0.001f);
            }
        }

        MatchBalancingHelpers.DumpTeamsStatus(balancedBannerGameMatch);
        return balancedBannerGameMatch;
    }

    public GameMatch KkMakeTeamOfSimilarSizesWithoutSplittingClanGroups(GameMatch gameMatch)
    {
        List<WeightedCrpgUser> allUsers = new();
        allUsers.AddRange(gameMatch.TeamA);
        allUsers.AddRange(gameMatch.TeamB);
        allUsers.AddRange(gameMatch.Waiting);

        var clanGroups = MatchBalancingHelpers.SplitUsersIntoClanGroups(allUsers);
        ClanGroup[] clanGroupsArray = clanGroups.ToArray();
        float[] clanGroupSizes = new float[clanGroups.Count];
        for (int i = 0; i < clanGroupsArray.Length; i++)
        {
            clanGroupSizes[i] = clanGroups[i].Size;
        }

        // The value 2 means we're splitting clan groups into two teams
        var partition = MatchBalancingHelpers.Heuristic(clanGroupsArray, clanGroupSizes, 2, preSorted: false);
        return new GameMatch
        {
            TeamA = MatchBalancingHelpers.JoinClanGroupsIntoUsers(partition.Partition[0].ToList()),
            TeamB = MatchBalancingHelpers.JoinClanGroupsIntoUsers(partition.Partition[1].ToList()),
            Waiting = new List<WeightedCrpgUser>(),
        };
    }

    public GameMatch BalanceTeamOfSimilarSizes(GameMatch gameMatch, bool bannerBalance, float threshold)
    {
        // Rescaling X dimension to the Y scale to make it as important.
        float sizeScaler = (gameMatch.TeamA.Sum(u => Math.Abs(u.Weight)) +
                            gameMatch.TeamB.Sum(u => Math.Abs(u.Weight))) / (gameMatch.TeamA.Count + gameMatch.TeamB.Count);
        int i = 0;
        for (; i < MaximumNumberOfSwaps; i++)
        {
            if (IsBalanceGoodEnough(gameMatch, maxSizeRatio: 0.75f, maxDifference: 10f, percentageDifference: threshold))
            {
                Debug.Print("Teams are of similar sizes and similar weights");
                break;
            }

            if (bannerBalance)
            {
                if (!FindAndSwapClanGroups(gameMatch, sizeScaler))
                {
                    Debug.Print("No more swap with banner balance available");
                    break;
                }
            }
            else
            {
                if (!FindAndSwapUsers(gameMatch, sizeScaler))
                {
                    Debug.Print("No more swap without banner balance available");
                    break;
                }
            }
        }

        Debug.Print($"Made {i} swaps '{(bannerBalance ? "using banner balance" : "without banner balance")}'");

        return gameMatch;
    }

    /// <summary>
    /// Looks for a single clan group in the team that has the least players and swaps it with a list of clan groups
    /// from the other team. The swap is done to minimize the difference in both sizes and weights between the two teams.
    /// </summary>
    /// <returns>False if it found no suitable swaps.</returns>
    private bool FindAndSwapClanGroups(GameMatch gameMatch, float sizeScaler)
    {
        ClanGroupsGameMatch clanGroupGameMatch = MatchBalancingHelpers.GroupTeamsByClan(gameMatch);

        List<WeightedCrpgUser> weakTeam;
        List<WeightedCrpgUser> strongTeam;
        List<ClanGroup> weakClanGroupsTeam;
        List<ClanGroup> strongClanGroupsTeam;
        if (WeightHelpers.ComputeTeamWeightedDifference(gameMatch) < 0)
        {
            weakTeam = gameMatch.TeamA;
            strongTeam = gameMatch.TeamB;
            weakClanGroupsTeam = clanGroupGameMatch.TeamA;
            strongClanGroupsTeam = clanGroupGameMatch.TeamB;
        }
        else
        {
            weakTeam = gameMatch.TeamB;
            strongTeam = gameMatch.TeamA;
            weakClanGroupsTeam = clanGroupGameMatch.TeamB;
            strongClanGroupsTeam = clanGroupGameMatch.TeamA;
        }

        int userCountDifference = weakClanGroupsTeam.Sum(c => c.Size) - strongClanGroupsTeam.Sum(c => c.Size);
        bool swappingFromWeakTeam = userCountDifference <= 0; // We are swapping from the team with the least player.
        userCountDifference = Math.Abs(userCountDifference);

        float teamWeightDiff = Math.Abs(WeightHelpers.ComputeTeamWeightedDifference(gameMatch));
        weakClanGroupsTeam = weakClanGroupsTeam.OrderBy(c => c.WeightMean()).ToList();
        strongClanGroupsTeam = strongClanGroupsTeam.OrderBy(c => c.WeightMean()).ToList();
        var clanGroupsToSwapUsingAngleTuple = FindBestClanGroupsSwap(weakClanGroupsTeam, strongClanGroupsTeam, teamWeightDiff / 2f, userCountDifference / 2, true, swappingFromWeakTeam, sizeScaler);
        var clanGroupsToSwapUsingDistanceTuple = FindBestClanGroupsSwap(weakClanGroupsTeam, strongClanGroupsTeam, teamWeightDiff / 2f, userCountDifference / 2, false, swappingFromWeakTeam, sizeScaler);

        ClanGroup clanGroupToSwap1;
        List<ClanGroup> clanGroupsToSwap2;
        if (clanGroupsToSwapUsingAngleTuple.distanceToTarget < clanGroupsToSwapUsingDistanceTuple.distanceToTarget)
        {
            Debug.Print($"{nameof(FindAndSwapClanGroups)}: angle method won");
            clanGroupToSwap1 = clanGroupsToSwapUsingAngleTuple.clanGrouptoSwap1;
            clanGroupsToSwap2 = clanGroupsToSwapUsingAngleTuple.clanGroupsToSwap2;
        }
        else
        {
            Debug.Print($"{nameof(FindAndSwapClanGroups)}: distance method won");
            clanGroupToSwap1 = clanGroupsToSwapUsingDistanceTuple.clanGrouptoSwap1;
            clanGroupsToSwap2 = clanGroupsToSwapUsingDistanceTuple.clanGroupsToSwap2;
        }

        if (!IsClanGroupsSwapValid(strongClanGroupsTeam, weakClanGroupsTeam, swappingFromWeakTeam, clanGroupToSwap1.Size,
                clanGroupToSwap1.Weight(), clanGroupsToSwap2.Sum(c => c.Size),
                WeightHelpers.ClanGroupsWeightSum(clanGroupsToSwap2), sizeScaler))
        {
            return false;
        }

        (List<WeightedCrpgUser> teamToSwapFrom, List<WeightedCrpgUser> teamToSwapInto) = swappingFromWeakTeam
            ? (weakTeam, strongTeam)
            : (strongTeam, weakTeam);
        Debug.Print($"Propose Swapping {clanGroupToSwap1.Size} players  from {(swappingFromWeakTeam ? "Weak Team" : "Strong Team")} in exchange of {clanGroupsToSwap2.Sum(c => c.Size)} players");
        bool isComplimentarySwapBetter = clanGroupToSwap1.Size + clanGroupsToSwap2.Sum(c => c.Size) > (teamToSwapFrom.Count + teamToSwapInto.Count) / 2;
        if (!isComplimentarySwapBetter)
        {
            foreach (var clanGroup in clanGroupsToSwap2)
            {
                foreach (WeightedCrpgUser user in clanGroup.Members)
                {
                    teamToSwapInto.Remove(user);
                    teamToSwapFrom.Add(user);
                }
            }

            foreach (WeightedCrpgUser user in clanGroupToSwap1.Members)
            {
                teamToSwapFrom.Remove(user);
                teamToSwapInto.Add(user);
            }

            Debug.Print($"Proposed Swap Done");
        }
        else
        {
            // Complimentary set of users for clanGroupsToSwap2
            List<WeightedCrpgUser> originalUsersToSwap2 = clanGroupsToSwap2.SelectMany(c => c.Members).ToList();
            HashSet<WeightedCrpgUser> crpgUsersToSwap2Set = new(teamToSwapInto);
            crpgUsersToSwap2Set.ExceptWith(originalUsersToSwap2);
            List<WeightedCrpgUser> newUsersToSwap2 = crpgUsersToSwap2Set.ToList();

            // Complimentary set of users for clanGroupToSwap1
            HashSet<WeightedCrpgUser> crpgUsersToSwap1Set = new(teamToSwapFrom);
            crpgUsersToSwap1Set.ExceptWith(clanGroupToSwap1.Members);
            List<WeightedCrpgUser> newUsersToSwap1 = crpgUsersToSwap1Set.ToList();

            // New Swaps
            foreach (WeightedCrpgUser user in newUsersToSwap2)
            {
                teamToSwapInto.Remove(user);
                teamToSwapFrom.Add(user);
            }

            foreach (WeightedCrpgUser user in newUsersToSwap1)
            {
                teamToSwapFrom.Remove(user);
                teamToSwapInto.Add(user);
            }

            Debug.Print($"Complimentary Swap done instead :  {newUsersToSwap1.Count} players  from {(swappingFromWeakTeam ? "Weak Team" : "Strong Team")} in exchange of {newUsersToSwap1.Count} players");
        }

        return true;
    }

    private bool FindAndSwapUsers(GameMatch gameMatch, float sizeScaler)
    {
        (List<WeightedCrpgUser> weakTeam, List<WeightedCrpgUser> strongTeam) = WeightHelpers.ComputeTeamWeightedDifference(gameMatch) < 0
            ? (gameMatch.TeamA, gameMatch.TeamB)
            : (gameMatch.TeamB, gameMatch.TeamA);
        int userCountDifference = weakTeam.Count - strongTeam.Count;
        bool swappingFromWeakTeam = userCountDifference < 0;

        (List<WeightedCrpgUser> teamToSwapFrom, List<WeightedCrpgUser> teamToSwapTo) = swappingFromWeakTeam
            ? (weakTeam, strongTeam)
            : (strongTeam, weakTeam);
        double teamWeightDiff = Math.Abs(WeightHelpers.ComputeTeamWeightedDifference(gameMatch));
        // here the WeightedCrpgUserToSwap can be null if the the team that has the least players is empty.
        WeightedCrpgUser? bestWeightedCrpgUserToSwap1 = swappingFromWeakTeam
            ? weakTeam.OrderBy(c => c.Weight).FirstOrDefault()
            : strongTeam.OrderBy(c => c.Weight).LastOrDefault();
        // These calculation are made to account for both the case where we are swapping a user with a list of user , or swapping no one with a list of users.
        float moveWeightHalfDifference = MatchBalancingHelpers.ComputeMoveWeightHalfDifference(teamToSwapFrom, teamToSwapTo, bestWeightedCrpgUserToSwap1);
        double targetWeight = swappingFromWeakTeam
            ? moveWeightHalfDifference + Math.Abs(teamWeightDiff) / 2f
            : moveWeightHalfDifference - Math.Abs(teamWeightDiff) / 2f;
        List<WeightedCrpgUser> bestWeightedCrpgUserToSwap1List = bestWeightedCrpgUserToSwap1 == null ? new List<WeightedCrpgUser>() : new List<WeightedCrpgUser> { bestWeightedCrpgUserToSwap1 };

        float teamSizeDifference = teamToSwapTo.Count - teamToSwapFrom.Count; // positive value
        float targetSize = (teamSizeDifference + bestWeightedCrpgUserToSwap1List.Count) / 2f;

        List<WeightedCrpgUser> bestWeightedCrpgUsersToSwap2 = MatchBalancingHelpers.FindWeightedCrpgUsersToSwap((float)targetWeight, teamToSwapTo, teamToSwapFrom, targetSize, sizeScaler);

        float sizeDifferenceAfterSwap = teamSizeDifference + (bestWeightedCrpgUserToSwap1List.Count - bestWeightedCrpgUsersToSwap2.Count) * 2f;

        // the pair difference (strong - weak) needs to be close to zero
        Vector2 bestPairVector = new(
            sizeDifferenceAfterSwap * sizeScaler,
            (float)Math.Abs(MatchBalancingHelpers.ComputeTeamDiffAfterSwap(teamToSwapFrom, teamToSwapTo, bestWeightedCrpgUserToSwap1List, bestWeightedCrpgUsersToSwap2)));

        foreach (var user in teamToSwapFrom)
        {
            targetSize = (teamSizeDifference + 1) / 2f;
            List<WeightedCrpgUser> userSingleton = new() { user };
            moveWeightHalfDifference = MatchBalancingHelpers.ComputeMoveWeightHalfDifference(teamToSwapFrom, teamToSwapTo, user);
            targetWeight = swappingFromWeakTeam
            ? moveWeightHalfDifference + Math.Abs(teamWeightDiff) / 2f
            : moveWeightHalfDifference - Math.Abs(teamWeightDiff) / 2f;
            List<WeightedCrpgUser> potentialWeightedCrpgUsersToSwap = MatchBalancingHelpers.FindWeightedCrpgUsersToSwap((float)targetWeight, teamToSwapTo, teamToSwapFrom, targetSize, sizeScaler);
            sizeDifferenceAfterSwap = teamSizeDifference + (1 - potentialWeightedCrpgUsersToSwap.Count) * 2;
            Vector2 potentialPairVector = new(
                sizeDifferenceAfterSwap * sizeScaler,
                (float)Math.Abs(MatchBalancingHelpers.ComputeTeamDiffAfterSwap(teamToSwapFrom, teamToSwapTo, userSingleton, potentialWeightedCrpgUsersToSwap)));
            if (potentialPairVector.Length() < bestPairVector.Length())
            {
                bestWeightedCrpgUserToSwap1 = user;
                bestWeightedCrpgUsersToSwap2 = potentialWeightedCrpgUsersToSwap;
                bestPairVector = potentialPairVector;
            }
        }

        bestWeightedCrpgUserToSwap1List = bestWeightedCrpgUserToSwap1 == null ? new List<WeightedCrpgUser>() : new List<WeightedCrpgUser> { bestWeightedCrpgUserToSwap1 };

        // TODO : checking only for weight difference , but what about size?
        if (Math.Abs(MatchBalancingHelpers.ComputeTeamDiffAfterSwap(teamToSwapFrom, teamToSwapTo, bestWeightedCrpgUserToSwap1List, bestWeightedCrpgUsersToSwap2)) - teamWeightDiff > 0)
        {
            return false;
        }

        if (swappingFromWeakTeam)
        {
            foreach (WeightedCrpgUser user in bestWeightedCrpgUsersToSwap2)
            {
                weakTeam.Add(user);
                strongTeam.Remove(user);
            }

            // null if the swap is just moving players from one team to another
            if (bestWeightedCrpgUserToSwap1 != null)
            {
                strongTeam.Add(bestWeightedCrpgUserToSwap1);
                weakTeam.Remove(bestWeightedCrpgUserToSwap1);
            }
        }
        else
        {
            foreach (WeightedCrpgUser user in bestWeightedCrpgUsersToSwap2)
            {
                weakTeam.Remove(user);
                strongTeam.Add(user);
            }

            // null when the team that has the least player is null
            if (bestWeightedCrpgUserToSwap1 != null)
            {
                strongTeam.Remove(bestWeightedCrpgUserToSwap1);
                weakTeam.Add(bestWeightedCrpgUserToSwap1);
            }
        }

        return true;
    }

    /// <summary>
    /// Find a clan group from the team with that has the least player and swap it with a list of clan groups from the
    /// opposite team in order to minimize weight and size differences.
    ///
    /// The task at hand is complex because it requires us to balance teams while making sure both weight and size
    /// are similar. To do this, we convert this to geometrical problem.
    ///
    /// A clan group is now a vector of dimension 2 => (size, weight).
    /// </summary>
    /// <param name="weakClanGroupsTeam">Weak team.</param>
    /// <param name="strongClanGroupsTeam">Strong team.</param>
    /// <param name="halfWeightDifference">Half of the current weight absolute difference between the teams.</param>
    /// <param name="targetSwapSizeDifference">The target size difference between the two members of the swap.</param>
    /// <param name="usingAngle">If the angle method should be used.</param>
    /// <param name="swappingFromWeakTeam">Has the weak team the least players.</param>
    /// <param name="sizeScaler">Size scaler.</param>
    /// <returns>The swap to perform.</returns>
    private (ClanGroup clanGrouptoSwap1, List<ClanGroup> clanGroupsToSwap2, float distanceToTarget) FindBestClanGroupsSwap(
        List<ClanGroup> weakClanGroupsTeam, List<ClanGroup> strongClanGroupsTeam, float halfWeightDifference,
        int targetSwapSizeDifference, bool usingAngle, bool swappingFromWeakTeam, float sizeScaler)
    {
        (List<ClanGroup> teamToSwapFrom, List<ClanGroup> teamToSwapInto) = swappingFromWeakTeam
            ? (weakClanGroupsTeam, strongClanGroupsTeam)
            : (strongClanGroupsTeam, weakClanGroupsTeam);
        Vector2 targetVector = new(targetSwapSizeDifference * sizeScaler, halfWeightDifference);
        // the clangroups are sets , the empty group act as an empty set. Since we're swaping sets of users between teams , it can be interesting to swap
        // people from one team in exchange of no one.
        ClanGroup emptyClanGroup = new(null);
        // note that clangroup are formed at the begining of the algorithm and never changed. This is what allows us to assign the reference to the same empty clangroup to
        // two different variable as emptyClanGroup is meant to remain empty. It is merely a tool that allows the balancer to not swap players as iterating on it won't do anything.
        ClanGroup weakClanGroupToSwap = emptyClanGroup;
        ClanGroup strongClanGroupToSwap = emptyClanGroup;

        // Initializing a first pair to compare afterward with other pairs
        float bestClanGroupToSwapTargetWeight = swappingFromWeakTeam
            ? weakClanGroupToSwap.Weight() + halfWeightDifference
            : strongClanGroupToSwap.Weight() - halfWeightDifference;

        ClanGroup bestClanGroupToSwapSource = swappingFromWeakTeam ? weakClanGroupToSwap : strongClanGroupToSwap;

        List<ClanGroup> bestClanGroupToSwapDestination = MatchBalancingHelpers.FindAClanGroupToSwapUsing(
            bestClanGroupToSwapTargetWeight,
            bestClanGroupToSwapSource.Size + Math.Abs(targetSwapSizeDifference),
            sizeScaler,
            teamToSwapInto,
            usingAngle);

        Vector2 bestSwapVector = new(
            (MatchBalancingHelpers.ClanGroupsSize(bestClanGroupToSwapDestination) - bestClanGroupToSwapSource.Size) * sizeScaler,
            Math.Abs(bestClanGroupToSwapSource.Weight() - MatchBalancingHelpers.ClanGroupsWeight(bestClanGroupToSwapDestination)));
        float distanceToTargetVector = (targetVector - bestSwapVector).Length();

        foreach (ClanGroup clanGroup in teamToSwapFrom)
        {
            // c is the potential first member of the pair (potentialClanGrouptoSwap1)
            // we compute below what's the target weight for the second member of the pair
            float potentialClanGroupToSwapTargetWeight = swappingFromWeakTeam
                ? clanGroup.Weight() + halfWeightDifference
                : clanGroup.Weight() - halfWeightDifference;
            // potential second member of the pair
            List<ClanGroup> potentialClanGroupToSwap = MatchBalancingHelpers.FindAClanGroupToSwapUsing(
                potentialClanGroupToSwapTargetWeight,
                sizeScaler,
                clanGroup.Size + Math.Abs(targetSwapSizeDifference),
                teamToSwapInto,
                usingAngle);
            Vector2 potentialSwapVector = new(
                (MatchBalancingHelpers.ClanGroupsSize(potentialClanGroupToSwap) - clanGroup.Size) * sizeScaler,
                Math.Abs(clanGroup.Weight() - MatchBalancingHelpers.ClanGroupsWeight(potentialClanGroupToSwap)));
            if ((targetVector - potentialSwapVector).Length() < (targetVector - bestSwapVector).Length())
            {
                bestClanGroupToSwapSource = clanGroup;
                bestClanGroupToSwapDestination = potentialClanGroupToSwap;
                bestSwapVector = potentialSwapVector;
                distanceToTargetVector = (targetVector - potentialSwapVector).Length();
            }
        }

        return (bestClanGroupToSwapSource, bestClanGroupToSwapDestination, distanceToTargetVector);
    }

    private bool IsClanGroupsSwapValid(List<ClanGroup> strongTeam, List<ClanGroup> weakTeam, bool swappingFromWeakTeam,
    int sourceGroupSize, float sourceGroupWeight, int destinationGroupSize, float destinationGroupWeight,
    float sizeScaler)
    {
        float newTeamWeightDiff = swappingFromWeakTeam
            ? strongTeam.Sum(c => c.Weight()) + 2f * sourceGroupWeight - 2f * destinationGroupWeight - weakTeam.Sum(c => c.Weight())
            : strongTeam.Sum(c => c.Weight()) - 2f * sourceGroupWeight + 2f * destinationGroupWeight - weakTeam.Sum(c => c.Weight());
        float newTeamSizeDiff = swappingFromWeakTeam
            ? strongTeam.Count + 2 * sourceGroupSize - 2f * destinationGroupSize - weakTeam.Count
            : strongTeam.Count - 2 * sourceGroupSize + 2f * destinationGroupSize - weakTeam.Count;

        Vector2 oldDifferenceVector = new((strongTeam.Count - weakTeam.Count) * sizeScaler,
            strongTeam.Sum(c => c.Weight()) - weakTeam.Sum(c => c.Weight()));
        Vector2 newDifferenceVector = new(newTeamSizeDiff * sizeScaler, newTeamWeightDiff);
        return newDifferenceVector.Length() < oldDifferenceVector.Length();
    }

    private bool IsWeightRatioAcceptable(GameMatch gameMatch, float percentageDifference)
    {
        double weightRatio = Math.Abs(
            (WeightHelpers.ComputeTeamWeight(gameMatch.TeamB) - WeightHelpers.ComputeTeamWeight(gameMatch.TeamA))
            / WeightHelpers.ComputeTeamAbsWeight(gameMatch.TeamA));
        return MathHelper.Within((float)weightRatio, 0f, percentageDifference);
    }

    private bool IsTeamSizeDifferenceAcceptable(GameMatch gameMatch, float maxSizeRatio, float maxDifference)
    {
        bool tooMuchSizeRatioDifference = !MathHelper.Within(
            gameMatch.TeamA.Count / (float)gameMatch.TeamB.Count,
            maxSizeRatio,
            1f / maxSizeRatio);
        bool sizeDifferenceGreaterThanThreshold = Math.Abs(gameMatch.TeamA.Count - gameMatch.TeamB.Count) > maxDifference;
        bool differenceOfOnlyOne = MathHelper.Within(gameMatch.TeamA.Count - gameMatch.TeamB.Count, -1, 1);
        return (!tooMuchSizeRatioDifference && !sizeDifferenceGreaterThanThreshold) || differenceOfOnlyOne;
    }

    private bool IsBalanceGoodEnough(GameMatch gameMatch, float maxSizeRatio, float maxDifference, float percentageDifference)
    {
        return IsTeamSizeDifferenceAcceptable(gameMatch, maxSizeRatio, maxDifference) && IsWeightRatioAcceptable(gameMatch, percentageDifference);
    }

    private GameMatch RandomlyAssignWaitingPlayersTeam(GameMatch gameMatch)
    {
        GameMatch newGameMatch = new();
        newGameMatch.TeamA.AddRange(gameMatch.TeamA);
        newGameMatch.TeamB.AddRange(gameMatch.TeamB);

        int i = 0;
        foreach (var user in gameMatch.Waiting)
        {
            if (i % 2 == 0)
            {
                newGameMatch.TeamA.Add(user);
            }
            else
            {
                newGameMatch.TeamB.Add(user);
            }

            i += 1;
        }

        return newGameMatch;
    }
}
