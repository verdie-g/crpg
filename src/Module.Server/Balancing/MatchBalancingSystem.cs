using System.Numerics;
using Crpg.Module.Api.Models.Users;
using Crpg.Module.Helpers;
using TaleWorlds.Library;

namespace Crpg.Module.Balancing;

internal class MatchBalancingSystem
{
    public const float PowerParameter = 1f;
    public const int MaximumNumberOfSwaps = 20; // upperbound to limit number of swaps. Numberofswaps is often<3

    public GameMatch NaiveCaptainBalancing(GameMatch gameMatch)
    {
        List<CrpgUser> allCrpgUsers = new();
        allCrpgUsers.AddRange(gameMatch.TeamA);
        allCrpgUsers.AddRange(gameMatch.TeamB);
        allCrpgUsers.AddRange(gameMatch.Waiting);
        GameMatch returnedGameMatch = new();
        bool teamA = true;
        foreach (CrpgUser user in allCrpgUsers.OrderByDescending(u => u.Character.Rating.Value))
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

    public GameMatch BannerBalancingWithEdgeCases(GameMatch gameMatch, bool firstBalance = true)
    {
        MatchBalancingHelpers.DumpTeamsStatus(gameMatch);
        Debug.Print(nameof(BannerBalancingWithEdgeCases));

        GameMatch balancedBannerGameMatch;
        // This is the path we take when team were randomly assigned.
        // we do not care of completely scrambling the teams since no round was played yet
        if (firstBalance)
        {
            Debug.Print("--------------------------------------------");
            Debug.Print("Now splitting the clan groups between the two team");
            balancedBannerGameMatch = KkMakeTeamOfSimilarSizesWithoutSplittingClanGroups(gameMatch);
        }

        // in this path , the teams already played at least one round , so we do not want to scramble the teams.
        else
        {
            Debug.Print("--------------------------------------------");
            Debug.Print("moving clanmates players to their clanmates teams , and moving players isolated from their clan to the opposite team.");
            balancedBannerGameMatch = MatchBalancingHelpers.RegroupClans(gameMatch);
        }

        MatchBalancingHelpers.DumpTeamsStatus(balancedBannerGameMatch);
        if (UserCount(balancedBannerGameMatch) < 4)
        {
            Debug.Print("Very Low Player Count => naivecaptainbalancing");
            balancedBannerGameMatch = NaiveCaptainBalancing(balancedBannerGameMatch);
            MatchBalancingHelpers.DumpTeams(balancedBannerGameMatch);
            return balancedBannerGameMatch;
        }

        Debug.Print("Banner balancing now");
        balancedBannerGameMatch = BalanceTeamOfSimilarSizes(balancedBannerGameMatch, bannerBalance: true, 0.025f);
        Debug.Print("Banner balancing done");
        MatchBalancingHelpers.DumpTeamsStatus(balancedBannerGameMatch);

        if (IsBalanceGoodEnough(balancedBannerGameMatch, maxSizeRatio: 0.75f, maxDifference: 10f, percentageDifference: 0.10f))
        {
            Debug.Print("Balance is acceptable");
        }
        else
        {
            // This are failcases in case bannerbalance was not enough
            Debug.Print("Balance is unnacceptable");
            balancedBannerGameMatch = BalanceTeamOfSimilarSizes(balancedBannerGameMatch, bannerBalance: false, 0.10f);

            if (IsBalanceGoodEnough(balancedBannerGameMatch, maxSizeRatio: 0.75f, maxDifference: 10f, percentageDifference: 0.15f))
            {
                // A few swaps solved the problem. Most of the clangroups are intact
                Debug.Print("Balance is now Acceptable");
            }
            else
            {
                // A few swaps were not enough. Swaps are a form of gradient descent. Sometimes there are local extremas that are not global extremas
                // Here we completely abandon banner balance by completely reshuffling the card then redoing swaps
                Debug.Print("Swaps were not enough. This should really not happen often");
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
        List<CrpgUser> allUsers = new();
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
            Waiting = new List<CrpgUser>(),
        };
    }

    public GameMatch BalanceTeamOfSimilarSizes(GameMatch gameMatch, bool bannerBalance, float threshold)
    {
        string methodUsed = bannerBalance ? "using bannerBalance" : "without bannerBalance";
        float sizeScaler = (gameMatch.TeamA.Sum(u => Math.Abs(u.Character.Rating.Value)) +
                           gameMatch.TeamB.Sum(u => Math.Abs(u.Character.Rating.Value))) / (gameMatch.TeamA.Count + gameMatch.TeamB.Count);
        for (int i = 0; i < MaximumNumberOfSwaps; i++)
        {
            if (IsBalanceGoodEnough(gameMatch, maxSizeRatio: 0.75f, maxDifference: 10f, percentageDifference: threshold))
            {
                Debug.Print($"Made {i} Swaps {methodUsed}");
                Debug.Print("Teams are of similar sizes and similar ratings");
                break;
            }

            if (bannerBalance)
            {
                if (!FindAndSwapClanGroups(gameMatch, sizeScaler))
                {
                    Debug.Print("Made " + i + " Swaps");
                    Debug.Print("No More Swap With BannerGrouping Available");
                    break;
                }
            }
            else
            {
                if (!FindAndSwapUsers(gameMatch, sizeScaler))
                {
                    Debug.Print("Made " + i + " Swaps");
                    Debug.Print("No more swap without BannerGrouping available");
                    break;
                }
            }
        }

        return gameMatch;
    }

    /// <summary>
    /// Looks for a single clan group in the team that has the least players and swaps it with a list of clan groups
    /// from the other team. The swap is done to minimize the difference in both sizes and ratings between the twom teams.
    /// </summary>
    /// <returns>False if it found no suitable swaps.</returns>
    private bool FindAndSwapClanGroups(GameMatch gameMatch, float sizeScaler)
    {
        ClanGroupsGameMatch clanGroupGameMatch = MatchBalancingHelpers.GroupTeamsByClan(gameMatch);

        List<CrpgUser> weakTeam;
        List<CrpgUser> strongTeam;
        List<ClanGroup> weakClanGroupsTeam;
        List<ClanGroup> strongClanGroupsTeam;
        if (RatingHelpers.ComputeTeamRatingDifference(gameMatch) < 0)
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

        float teamRatingDiff = Math.Abs(RatingHelpers.ComputeTeamRatingDifference(gameMatch));
        weakClanGroupsTeam = weakClanGroupsTeam.OrderBy(c => c.RatingPMean()).ToList();
        strongClanGroupsTeam = strongClanGroupsTeam.OrderBy(c => c.RatingPMean()).ToList();
        var clanGroupsToSwapUsingAngleTuple = FindBestClanGroupsSwap(weakClanGroupsTeam, strongClanGroupsTeam, teamRatingDiff / 2f, userCountDifference / 2, true, swappingFromWeakTeam, sizeScaler);
        var clanGroupsToSwapUsingDistanceTuple = FindBestClanGroupsSwap(weakClanGroupsTeam, strongClanGroupsTeam, teamRatingDiff / 2f, userCountDifference / 2, false, swappingFromWeakTeam, sizeScaler);

        ClanGroup clanGroupToSwap1;
        List<ClanGroup> clanGroupsToSwap2;
        if (clanGroupsToSwapUsingAngleTuple.distanceToTarget < clanGroupsToSwapUsingDistanceTuple.distanceToTarget)
        {
            clanGroupToSwap1 = clanGroupsToSwapUsingAngleTuple.clanGrouptoSwap1;
            clanGroupsToSwap2 = clanGroupsToSwapUsingAngleTuple.clanGroupsToSwap2;
        }
        else
        {
            clanGroupToSwap1 = clanGroupsToSwapUsingDistanceTuple.clanGrouptoSwap1;
            clanGroupsToSwap2 = clanGroupsToSwapUsingDistanceTuple.clanGroupsToSwap2;
        }

        float newTeamRatingDiff = swappingFromWeakTeam
            ? RatingHelpers.ClanGroupsPowerSum(strongClanGroupsTeam) - 2f * RatingHelpers.ClanGroupsPowerSum(clanGroupsToSwap2) + 2f * clanGroupToSwap1.RatingPsum() - RatingHelpers.ClanGroupsPowerSum(weakClanGroupsTeam)
            : RatingHelpers.ClanGroupsPowerSum(strongClanGroupsTeam) + 2f * RatingHelpers.ClanGroupsPowerSum(clanGroupsToSwap2) - 2f * clanGroupToSwap1.RatingPsum() - RatingHelpers.ClanGroupsPowerSum(weakClanGroupsTeam);
        float newTeamSizeDiff = swappingFromWeakTeam
            ? strongTeam.Count - 2 * clanGroupsToSwap2.Sum(c => c.Size) + 2f * clanGroupToSwap1.Size - weakTeam.Count
            : strongTeam.Count + 2 * clanGroupsToSwap2.Sum(c => c.Size) - 2f * clanGroupToSwap1.Size - weakTeam.Count;
        Vector2 oldDifferenceVector = new((strongTeam.Count - weakTeam.Count) * sizeScaler,
            strongTeam.Sum(u => u.Character.Rating.Value) - weakTeam.Sum(u => u.Character.Rating.Value));
        Vector2 newDifferenceVector = new(newTeamSizeDiff * sizeScaler, newTeamRatingDiff);
        if (newDifferenceVector.Length() >= oldDifferenceVector.Length())
        {
            return false;
        }

        (List<CrpgUser> teamToSwapFrom, List<CrpgUser> teamToSwapInto) = swappingFromWeakTeam
            ? (weakTeam, strongTeam)
            : (strongTeam, weakTeam);

        foreach (var clanGroup in clanGroupsToSwap2)
        {
            foreach (CrpgUser user in clanGroup.Members)
            {
                teamToSwapInto.Remove(user);
                teamToSwapFrom.Add(user);
            }
        }

        foreach (CrpgUser user in clanGroupToSwap1.Members)
        {
            teamToSwapFrom.Remove(user);
            teamToSwapInto.Add(user);
        }

        return true;
    }

    private bool FindAndSwapUsers(GameMatch gameMatch, float sizeScaler)
    {
        (List<CrpgUser> weakTeam, List<CrpgUser> strongTeam) = RatingHelpers.ComputeTeamRatingDifference(gameMatch) < 0
            ? (gameMatch.TeamA, gameMatch.TeamB)
            : (gameMatch.TeamB, gameMatch.TeamA);
        int userCountDifference = weakTeam.Count - strongTeam.Count;
        bool swappingFromWeakTeam = userCountDifference <= 0;
        float sizeOffset = Math.Abs(userCountDifference);

        (List<CrpgUser> teamToSwapFrom, List<CrpgUser> teamToSwapTo) = swappingFromWeakTeam
            ? (weakTeam, strongTeam)
            : (strongTeam, weakTeam);

        CrpgUser bestCrpgUserToSwap1 = swappingFromWeakTeam
            ? weakTeam.OrderBy(c => c.Character.Rating.Value).FirstOrDefault()
            : strongTeam.OrderBy(c => c.Character.Rating.Value).LastOrDefault();
        double teamRatingDiff = Math.Abs(RatingHelpers.ComputeTeamRatingDifference(gameMatch));
        float bestCrpgUserToSwap1Rating = bestCrpgUserToSwap1 != null ? bestCrpgUserToSwap1.Character.Rating.Value : 0;
        double targetRating = swappingFromWeakTeam
            ? bestCrpgUserToSwap1Rating + Math.Abs(teamRatingDiff) / 2f
            : bestCrpgUserToSwap1Rating - Math.Abs(teamRatingDiff) / 2f;
        List<CrpgUser> bestCrpgUsersToSwap2 = MatchBalancingHelpers.FindCrpgUsersToSwap((float)targetRating, teamToSwapTo, sizeOffset / 2f, sizeScaler);

        // the pair difference (strong - weak) needs to be close to TargetVector
        Vector2 targetVector = new(sizeOffset * sizeScaler, (float)teamRatingDiff / 2f);
        Vector2 bestPairVector = new(
            (bestCrpgUsersToSwap2.Count - 1) * sizeScaler,
            Math.Abs(bestCrpgUserToSwap1Rating - bestCrpgUsersToSwap2.Sum(u => u.Character.Rating.Value)));

        foreach (var user in teamToSwapFrom)
        {
            targetRating = swappingFromWeakTeam
                ? user.Character.Rating.Value + Math.Abs(teamRatingDiff) / 2f
                : user.Character.Rating.Value - Math.Abs(teamRatingDiff) / 2f;
            List<CrpgUser> potentialCrpgUsersToSwap = MatchBalancingHelpers.FindCrpgUsersToSwap((float)targetRating, teamToSwapTo, sizeOffset / 2f, sizeScaler);
            Vector2 potentialPairVector = new(
                (potentialCrpgUsersToSwap.Count - 1) * sizeScaler,
                Math.Abs(user.Character.Rating.Value - potentialCrpgUsersToSwap.Sum(u => u.Character.Rating.Value)));
            if ((targetVector - potentialPairVector).Length() < (targetVector - bestPairVector).Length())
            {
                bestCrpgUserToSwap1 = user;
                bestCrpgUsersToSwap2 = potentialCrpgUsersToSwap;
                bestPairVector = potentialPairVector;
            }
        }

        float newTeamRatingDiff = swappingFromWeakTeam
            ? strongTeam.Sum(u => u.Character.Rating.Value) + 2f * bestCrpgUserToSwap1Rating - 2f * bestCrpgUsersToSwap2.Sum(u => u.Character.Rating.Value) - weakTeam.Sum(u => u.Character.Rating.Value)
            : strongTeam.Sum(u => u.Character.Rating.Value) - 2f * bestCrpgUserToSwap1Rating + 2f * bestCrpgUsersToSwap2.Sum(u => u.Character.Rating.Value) - weakTeam.Sum(u => u.Character.Rating.Value);
        float newTeamSizeDiff = swappingFromWeakTeam
            ? strongTeam.Count + 2 - 2f * bestCrpgUsersToSwap2.Count - weakTeam.Count
            : strongTeam.Count - 2 + 2f * bestCrpgUsersToSwap2.Count - weakTeam.Count;

        Vector2 oldDifferenceVector = new((strongTeam.Count - weakTeam.Count) * sizeScaler,
            strongTeam.Sum(u => u.Character.Rating.Value) - weakTeam.Sum(u => u.Character.Rating.Value));
        Vector2 newDifferenceVector = new(newTeamSizeDiff * sizeScaler, newTeamRatingDiff);
        if (newDifferenceVector.Length() >= oldDifferenceVector.Length())
        {
            return false;
        }

        if (swappingFromWeakTeam)
        {
            foreach (CrpgUser user in bestCrpgUsersToSwap2)
            {
                weakTeam.Add(user);
                strongTeam.Remove(user);
            }

            // null when the team that has the least player is null
            if (bestCrpgUserToSwap1 != null)
            {
                strongTeam.Add(bestCrpgUserToSwap1);
                weakTeam.Remove(bestCrpgUserToSwap1);
            }
        }
        else
        {
            foreach (CrpgUser user in bestCrpgUsersToSwap2)
            {
                weakTeam.Remove(user);
                strongTeam.Add(user);
            }

            // null when the team that has the least player is null
            if (bestCrpgUserToSwap1 != null)
            {
                strongTeam.Remove(bestCrpgUserToSwap1);
                weakTeam.Add(bestCrpgUserToSwap1);
            }
        }

        return true;
    }

    /// <summary>
    /// Find a clan group from the team with that has the least player and swap it with a list of clan groups from the
    /// opposite team in order to minimize rating and size differences.
    ///
    /// The task at hand is complex because it requires us to balance teams while making sure both ratings and size
    /// are similar. To do this, we convert this to geometrical problem.
    ///
    /// A clan group is now a vector of dimension 2 => (size, rating).
    /// </summary>
    /// <param name="weakClanGroupsTeam">Weak team.</param>
    /// <param name="strongClanGroupsTeam">Strong team.</param>
    /// <param name="halfRatingDifference">Half of the current rating absolute difference between the teams.</param>
    /// <param name="targetSwapSizeDifference">The target size difference between the two members of the swap.</param>
    /// <param name="usingAngle">If the angle method should be used.</param>
    /// <param name="swappingFromWeakTeam">Has the weak team the least players.</param>
    /// <returns>The swap to perform.</returns>
    private (ClanGroup clanGrouptoSwap1, List<ClanGroup> clanGroupsToSwap2, float distanceToTarget) FindBestClanGroupsSwap(
        List<ClanGroup> weakClanGroupsTeam, List<ClanGroup> strongClanGroupsTeam, float halfRatingDifference,
        int targetSwapSizeDifference, bool usingAngle, bool swappingFromWeakTeam, float sizeScaler)
    {
        (List<ClanGroup> teamToSwapFrom, List<ClanGroup> teamToSwapInto) = swappingFromWeakTeam
            ? (weakClanGroupsTeam, strongClanGroupsTeam)
            : (strongClanGroupsTeam, weakClanGroupsTeam);
        // Rescaling X dimension to the Y scale to make it as important.
        Vector2 targetVector = new(targetSwapSizeDifference * sizeScaler, halfRatingDifference);
        // if the team that has the least players is empty , we will do the swap with an empty clangroup. Iterating on the empty clangroup will just do nothing.
        ClanGroup empty = new(null);
        ClanGroup weakClanGroupToSwap = weakClanGroupsTeam.DefaultIfEmpty(empty).First();
        ClanGroup strongClanGroupToSwap = strongClanGroupsTeam.DefaultIfEmpty(empty).Last();

        // Initializing a first pair to compare afterward with other pairs

        float bestClanGroupToSwapTargetRating = swappingFromWeakTeam
        ? weakClanGroupToSwap.RatingPsum() + halfRatingDifference
        : strongClanGroupToSwap.RatingPsum() - halfRatingDifference;

        ClanGroup bestClanGroupToSwapSource = swappingFromWeakTeam ? weakClanGroupToSwap : strongClanGroupToSwap;

        List<ClanGroup> bestClanGroupToSwapDestination = MatchBalancingHelpers.FindAClanGroupToSwapUsing(
            bestClanGroupToSwapTargetRating,
            bestClanGroupToSwapSource.Size + Math.Abs(targetSwapSizeDifference),
            sizeScaler,
            teamToSwapInto,
            usingAngle);

        Vector2 bestSwapVector = new(
            (MatchBalancingHelpers.ClanGroupsSize(bestClanGroupToSwapDestination) - bestClanGroupToSwapSource.Size) * sizeScaler,
            Math.Abs(bestClanGroupToSwapSource.RatingPsum() - MatchBalancingHelpers.ClanGroupsRating(bestClanGroupToSwapDestination)));
        float distanceToTargetVector = (targetVector - bestSwapVector).Length();

        foreach (ClanGroup clanGroup in teamToSwapFrom)
        {
            // c is the potential first member of the pair (potentialClanGrouptoSwap1)
            // we compute below what's the target rating for the second member of the pair
            float potentialClanGroupToSwapTargetRating = swappingFromWeakTeam
                ? clanGroup.RatingPsum() + halfRatingDifference
                : clanGroup.RatingPsum() - halfRatingDifference;
            // potential second member of the pair
            List<ClanGroup> potentialClanGroupToSwap = MatchBalancingHelpers.FindAClanGroupToSwapUsing(
                potentialClanGroupToSwapTargetRating,
                sizeScaler,
                clanGroup.Size + Math.Abs(targetSwapSizeDifference),
                teamToSwapInto,
                usingAngle);
            Vector2 potentialSwapVector = new(
                (MatchBalancingHelpers.ClanGroupsSize(potentialClanGroupToSwap) - clanGroup.Size) * sizeScaler,
                Math.Abs(clanGroup.RatingPsum() - MatchBalancingHelpers.ClanGroupsRating(potentialClanGroupToSwap)));
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

    private bool IsRatingRatioAcceptable(GameMatch gameMatch, float percentageDifference)
    {
        double ratingRatio = Math.Abs(
            (RatingHelpers.ComputeTeamRatingPowerSum(gameMatch.TeamB) - RatingHelpers.ComputeTeamRatingPowerSum(gameMatch.TeamA))
            / RatingHelpers.ComputeTeamRatingPowerSum(gameMatch.TeamA));
        return MathHelper.Within((float)ratingRatio, 0f, percentageDifference);
    }

    private bool IsTeamSizeDifferenceAcceptable(GameMatch gameMatch, float maxSizeRatio, float maxDifference)
    {
        bool tooMuchSizeRatioDifference = !MathHelper.Within(
            gameMatch.TeamA.Count / (float)gameMatch.TeamB.Count,
            maxSizeRatio,
            1f / maxSizeRatio);
        bool sizeDifferenceGreaterThanThreshold = Math.Abs(gameMatch.TeamA.Count - gameMatch.TeamB.Count) > maxDifference;
        return !tooMuchSizeRatioDifference && !sizeDifferenceGreaterThanThreshold;
    }

    private bool IsBalanceGoodEnough(GameMatch gameMatch, float maxSizeRatio, float maxDifference, float percentageDifference)
    {
        return IsTeamSizeDifferenceAcceptable(gameMatch, maxSizeRatio, maxDifference) && IsRatingRatioAcceptable(gameMatch, percentageDifference);
    }

    private int UserCount(GameMatch gameMatch)
    {
        return gameMatch.TeamA.Count + gameMatch.TeamB.Count + gameMatch.Waiting.Count;
    }
}
