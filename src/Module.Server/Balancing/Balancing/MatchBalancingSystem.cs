using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Crpg.Module.Api.Models.Users;
using Crpg.Module.Helpers;

namespace Crpg.Module.Balancing;

/// <summary>
/// i don't know yet.
/// </summary>
internal interface IMatchBalancingSystem
{
    /// <summary>
    /// Balances the Team by putting the best player into the first team
    /// then second best player into the second team etc.
    /// </summary>
    GameMatch BannerBalancingWithEdgeCases(GameMatch gameMatch);
}

internal class MatchBalancingSystem : IMatchBalancingSystem
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
        foreach (CrpgUser player in allCrpgUsers.OrderByDescending(u => u.Character.Rating.Value))
        {
            if (teamA)
            {
                returnedGameMatch.TeamA.Add(player);
                teamA = !teamA;
            }
            else
            {
                returnedGameMatch.TeamB.Add(player);
                teamA = !teamA;
            }
        }

        return returnedGameMatch;
    }

    public GameMatch BannerBalancingWithEdgeCases(GameMatch gameMatch)
    {
        MatchBalancingHelpers.DumpTeamsStatus(gameMatch);
        Console.WriteLine(nameof(BannerBalancingWithEdgeCases));
        Console.WriteLine("--------------------------------------------");
        Console.WriteLine("Now splitting the clangroups between the two team");
        GameMatch balancedBannerGameMatch = KKMakeTeamOfSimilarSizesWithoutSplittingClanGroups(gameMatch);
        MatchBalancingHelpers.DumpTeamsStatus(balancedBannerGameMatch);

        Console.WriteLine("Banner balancing now");

        balancedBannerGameMatch = BalanceTeamOfSimilarSizes(balancedBannerGameMatch, bannerBalance: true, 0.025f);

        Console.WriteLine("Banner balancing done");
        MatchBalancingHelpers.DumpTeamsStatus(balancedBannerGameMatch);

        if (IsBalanceGoodEnough(balancedBannerGameMatch, maxSizeRatio: 0.75f, maxDifference: 10f, percentageDifference: 0.10f))
        {
            Console.WriteLine("Balance is acceptable");
        }
        else
        {
            // This are failcases in case bannerbalance was not enough
            Console.WriteLine("Balance is unnacceptable");
            balancedBannerGameMatch = BalanceTeamOfSimilarSizes(balancedBannerGameMatch, bannerBalance: false, 0.10f);

            if (IsBalanceGoodEnough(balancedBannerGameMatch, maxSizeRatio: 0.75f, maxDifference: 10f, percentageDifference: 0.15f))
            {
                // A few swaps solved the problem. Most of the clangroups are intact
                Console.WriteLine("Balance is now Acceptable");
            }
            else
            {
                // A few swaps were not enough. Swaps are a form of gradient descent. Sometimes there are local extremas that are not  global extremas
                // Here we completely abandon bannerbalance by completely reshuffling the card then redoing swaps
                Console.WriteLine("Swaps were not enough. This should really not happen often");
                MatchBalancingHelpers.DumpTeams(balancedBannerGameMatch);
                Console.WriteLine("NaiveCaptainBalancing + Balancing Without BannerGrouping");
                balancedBannerGameMatch = NaiveCaptainBalancing(balancedBannerGameMatch);
                balancedBannerGameMatch = BalanceTeamOfSimilarSizes(balancedBannerGameMatch, false, 0.001f);
            }
        }

        MatchBalancingHelpers.DumpTeamsStatus(balancedBannerGameMatch);
        return balancedBannerGameMatch;
    }

    public GameMatch KKMakeTeamOfSimilarSizesWithoutSplittingClanGroups(GameMatch gameMatch)
    {
        List<CrpgUser> allCrpgUsers = new();
        allCrpgUsers.AddRange(gameMatch.TeamA);
        allCrpgUsers.AddRange(gameMatch.TeamB);
        allCrpgUsers.AddRange(gameMatch.Waiting);
        var clangroups = MatchBalancingHelpers.SplitUsersIntoClanGroups(allCrpgUsers);
        GameMatch returnedGameMatch = new();
        ClanGroup[] clangroupsArray = clangroups.ToArray();
        double[] clangroupSize = new double[clangroups.Count];
        for (int i = 0; i < clangroupsArray.Length; i++)
        {
            clangroupSize[i] = clangroups[i].Size();
        }

        var partition = MatchBalancingHelpers.Heuristic(clangroupsArray, clangroupSize, 2, preSorted: false);
        // the 2 value means we're splitting clangroups into two teams
        returnedGameMatch.TeamA = MatchBalancingHelpers.JoinClanGroupsIntoUsers(partition.Partition[0].ToList());
        returnedGameMatch.TeamB = MatchBalancingHelpers.JoinClanGroupsIntoUsers(partition.Partition[1].ToList());
        return returnedGameMatch;
    }
    public GameMatch BalanceTeamOfSimilarSizes(GameMatch gameMatch, bool bannerBalance, float threshold)
    {
        string methodUsed = bannerBalance ? "using bannerBalance" : "without bannerBalance";
        for (int i = 0; i < MaximumNumberOfSwaps; i++)
        {
            if (IsBalanceGoodEnough(gameMatch, maxSizeRatio: 0.75f, maxDifference: 10f, percentageDifference: threshold))
            {
                Console.WriteLine($"Made {i} Swaps {methodUsed}");
                Console.WriteLine("Teams are of similar sizes and similar ratings");
                break;
            }

            if (bannerBalance)
            {
                if (!FindAndSwapClanGroups(gameMatch))
                {
                    Console.WriteLine("Made " + i + " Swaps");
                    Console.WriteLine("No More Swap With BannerGrouping Available");
                    break;
                }
            }
            else
            {
                if (!FindAndSwapPlayers(gameMatch))
                {
                    Console.WriteLine("Made " + i + " Swaps");
                    Console.WriteLine("No more swap without BannerGrouping available");
                    break;
                }
            }
        }

        return gameMatch;
    }

    public bool FindAndSwapClanGroups(GameMatch gameMatch)
    {
        ClanGroupsGameMatch clanGroupGameMatch = MatchBalancingHelpers.ConvertGameMatchToClanGroupsGameMatchList(gameMatch);
        (List<CrpgUser> weakTeam, List<CrpgUser> strongTeam, List<ClanGroup> weakClanGroupsTeam, List<ClanGroup> strongClanGroupsTeam) = RatingHelpers.ComputeTeamRatingDifference(gameMatch) < 0
        ? (gameMatch.TeamA, gameMatch.TeamB, clanGroupGameMatch.TeamA, clanGroupGameMatch.TeamB)
        : (gameMatch.TeamB, gameMatch.TeamA, clanGroupGameMatch.TeamB, clanGroupGameMatch.TeamA);
        double teamRatingDiff = Math.Abs(RatingHelpers.ComputeTeamRatingDifference(gameMatch)); // diff >0
        weakClanGroupsTeam = weakClanGroupsTeam.OrderBy(c => c.RatingPMean()).ToList();
        strongClanGroupsTeam = strongClanGroupsTeam.OrderBy(c => c.RatingPMean()).ToList();

        int playerCountDifference = weakClanGroupsTeam.Sum(c => c.Size()) - strongClanGroupsTeam.Sum(c => c.Size());
        bool swapingFromWeakTeam = playerCountDifference <= 0;

        // we know wich team has the less players. it is is weakteam if swapingFromWeakTeam==true
        playerCountDifference = Math.Abs(playerCountDifference);
        ClanGroup weakClanGroupToSwap = weakClanGroupsTeam.First();
        ClanGroup strongClanGroupToSwap = strongClanGroupsTeam.Last();

        ClanGroup clanGrouptoSwap1;
        List<ClanGroup> clanGroupstoSwap2;

        List<ClanGroup> teamClanGroupsToSwapInto = swapingFromWeakTeam ? strongClanGroupsTeam : weakClanGroupsTeam;

        float clanGroupToSwapTargetRating = swapingFromWeakTeam ? weakClanGroupToSwap.RatingPsum() + (float)Math.Abs(teamRatingDiff) / 2f : strongClanGroupToSwap.RatingPsum() - (float)Math.Abs(teamRatingDiff) / 2f;
        var clanGroupsToSwapUsingAngleTuple = FindBestPairForSwapDoneWithBanner(weakClanGroupsTeam, strongClanGroupsTeam, teamRatingDiff, playerCountDifference / 2, true, swapingFromWeakTeam);
        var clanGroupsToSwapUsingDistanceTuple = FindBestPairForSwapDoneWithBanner(weakClanGroupsTeam, strongClanGroupsTeam, teamRatingDiff, playerCountDifference / 2, false, swapingFromWeakTeam);

        if (clanGroupsToSwapUsingAngleTuple.distanceToTarget < clanGroupsToSwapUsingDistanceTuple.distanceToTarget)
        {
            clanGrouptoSwap1 = clanGroupsToSwapUsingAngleTuple.clanGrouptoSwap1;
            clanGroupstoSwap2 = clanGroupsToSwapUsingAngleTuple.clanGroupsToSwap2;
        }
        else
        {
            clanGrouptoSwap1 = clanGroupsToSwapUsingDistanceTuple.clanGrouptoSwap1;
            clanGroupstoSwap2 = clanGroupsToSwapUsingDistanceTuple.clanGroupsToSwap2;
        }

        float a = clanGrouptoSwap1.RatingPsum();
        float b = RatingHelpers.ClanGroupsPowerSum(clanGroupstoSwap2);
        bool c = swapingFromWeakTeam;
        float newdiff = swapingFromWeakTeam
            ? RatingHelpers.ClanGroupsPowerSum(strongClanGroupsTeam) + 2f * clanGrouptoSwap1.RatingPsum() - 2f * RatingHelpers.ClanGroupsPowerSum(clanGroupstoSwap2) - RatingHelpers.ClanGroupsPowerSum(weakClanGroupsTeam)
            : RatingHelpers.ClanGroupsPowerSum(strongClanGroupsTeam) + 2f * RatingHelpers.ClanGroupsPowerSum(clanGroupstoSwap2) - 2f * clanGrouptoSwap1.RatingPsum() - RatingHelpers.ClanGroupsPowerSum(weakClanGroupsTeam);
        float newdiff1 = newdiff;
        var teamtoSwapFrom = swapingFromWeakTeam ? weakTeam : strongTeam;
        var teamtoSwapInto = swapingFromWeakTeam ? strongTeam : weakTeam;

        if (Math.Abs(newdiff) < Math.Abs(teamRatingDiff))
        {
            foreach (var clanGroup in clanGroupstoSwap2)
            {
                foreach (CrpgUser user in clanGroup.MemberList)
                {
                    teamtoSwapInto.Remove(user);
                    teamtoSwapFrom.Add(user);
                }
            }

            foreach (CrpgUser user in clanGrouptoSwap1.MemberList)
            {
                teamtoSwapInto.Add(user);
                teamtoSwapFrom.Remove(user);
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    public bool FindAndSwapPlayers(GameMatch gameMatch)
    {
        (List<CrpgUser> weakTeam, List<CrpgUser> strongTeam) = RatingHelpers.ComputeTeamRatingDifference(gameMatch) < 0
            ? (gameMatch.TeamA, gameMatch.TeamB)
            : (gameMatch.TeamB, gameMatch.TeamA);

        double teamRatingDiff = Math.Abs(RatingHelpers.ComputeTeamRatingDifference(gameMatch));
        int playerCountDifference = weakTeam.Count - strongTeam.Count;
        bool swapingFromWeakTeam = playerCountDifference <= 0;
        List<CrpgUser> teamTeamToSwapFrom = swapingFromWeakTeam ? weakTeam : strongTeam;
        List<CrpgUser> teamTeamToSwapInto = swapingFromWeakTeam ? strongTeam : weakTeam;
        CrpgUser bestCrpgUserToSwap1 = swapingFromWeakTeam ? weakTeam.OrderBy(c => c.Character.Rating.Value).First() : strongTeam.OrderBy(c => c.Character.Rating.Value).Last();
        float sizeOffset = Math.Abs(playerCountDifference);
        float targetSizeRescaling = (float)teamRatingDiff / (2f * sizeOffset);
        double targetRating = swapingFromWeakTeam ? bestCrpgUserToSwap1.Character.Rating.Value + Math.Abs(teamRatingDiff) / 2f : bestCrpgUserToSwap1.Character.Rating.Value - Math.Abs(teamRatingDiff) / 2f;
        List<CrpgUser> bestCrpgUsersToSwap2 = MatchBalancingHelpers.FindCrpgUsersToSwap((float)targetRating, teamTeamToSwapInto, sizeOffset / 2f);
        // the pair difference (strong - weak) needs to be close to TargetVector
        Vector2 targetVector = new(sizeOffset * targetSizeRescaling, (float)teamRatingDiff / 2f);
        Vector2 bestPairVector = new((bestCrpgUsersToSwap2.Count - 1) * targetSizeRescaling, Math.Abs(bestCrpgUserToSwap1.Character.Rating.Value - bestCrpgUsersToSwap2.Sum(u => u.Character.Rating.Value)));
        foreach (var user in teamTeamToSwapFrom)
        {
            targetRating = swapingFromWeakTeam ? user.Character.Rating.Value + Math.Abs(teamRatingDiff) / 2f : user.Character.Rating.Value - Math.Abs(teamRatingDiff) / 2f;
            List<CrpgUser> potentialCrpgUsersToSwap = MatchBalancingHelpers.FindCrpgUsersToSwap((float)targetRating, teamTeamToSwapInto, sizeOffset / 2f);
            Vector2 potentialPairVector = new((potentialCrpgUsersToSwap.Count - 1) * targetSizeRescaling, Math.Abs(user.Character.Rating.Value - potentialCrpgUsersToSwap.Sum(u => u.Character.Rating.Value)));
            if ((targetVector - potentialPairVector).Length() < (targetVector - bestPairVector).Length())
            {
                bestCrpgUserToSwap1 = user;
                bestCrpgUsersToSwap2 = potentialCrpgUsersToSwap;
                bestPairVector = potentialPairVector;
            }
        }

        float newdiff = swapingFromWeakTeam
            ? strongTeam.Sum(u => u.Character.Rating.Value) + 2f * bestCrpgUserToSwap1.Character.Rating.Value - 2f * bestCrpgUsersToSwap2.Sum(u => u.Character.Rating.Value) - weakTeam.Sum(u => u.Character.Rating.Value)
            : strongTeam.Sum(u => u.Character.Rating.Value) + 2f * bestCrpgUsersToSwap2.Sum(u => u.Character.Rating.Value) - 2f * bestCrpgUserToSwap1.Character.Rating.Value - weakTeam.Sum(u => u.Character.Rating.Value);
        if (Math.Abs(newdiff) < Math.Abs(teamRatingDiff))
        {
            if (swapingFromWeakTeam)
            {
                foreach (CrpgUser user in bestCrpgUsersToSwap2)
                {
                    weakTeam.Add(user);
                    strongTeam.Remove(user);
                }

                strongTeam.Add(bestCrpgUserToSwap1);
                weakTeam.Remove(bestCrpgUserToSwap1);
            }
            else
            {
                foreach (CrpgUser user in bestCrpgUsersToSwap2)
                {
                    weakTeam.Remove(user);
                    strongTeam.Add(user);
                }

                strongTeam.Remove(bestCrpgUserToSwap1);
                weakTeam.Add(bestCrpgUserToSwap1);
            }

            return true;
        }
        else
        {
            return false;
        }
    }

    // Rating Difference is positive. It has to be Strong Team - WeakTeam.
    private (ClanGroup clanGrouptoSwap1, List<ClanGroup> clanGroupsToSwap2, float distanceToTarget) FindBestPairForSwapDoneWithBanner(List<ClanGroup> weakClanGroupsTeam, List<ClanGroup> strongClanGroupsTeam, double ratingDifference, int targetSizeOffset, bool usingAngle, bool swapingFromWeakTeam)
    {
        float potentialClanGroupToSwapTargetRating;
        float targetSizeRescaling = (float)ratingDifference / (2f * targetSizeOffset);

        var teamToSwapFrom = swapingFromWeakTeam ? weakClanGroupsTeam : strongClanGroupsTeam;
        var teamToSwapInto = swapingFromWeakTeam ? strongClanGroupsTeam : weakClanGroupsTeam;

        // the pair difference (strong - weak) needs to be close to TargetVector
        Vector2 targetVector = new(targetSizeOffset * targetSizeRescaling, (float)ratingDifference / 2f);

        ClanGroup weakClanGroupToSwap = weakClanGroupsTeam.First();
        ClanGroup strongClanGroupToSwap = strongClanGroupsTeam.Last();

        // Initializing a first pair to compare afterward with other pairs
        float bestClanGroupToSwapTargetRating = swapingFromWeakTeam ? weakClanGroupToSwap.RatingPsum() + (float)Math.Abs(ratingDifference) / 2f : strongClanGroupToSwap.RatingPsum() - (float)Math.Abs(ratingDifference) / 2f;
        ClanGroup bestClanGrouptoSwap1 = swapingFromWeakTeam ? weakClanGroupToSwap : strongClanGroupToSwap;
        List<ClanGroup> bestClanGroupToSwap2 = MatchBalancingHelpers.FindAClanGroupToSwapUsing(bestClanGroupToSwapTargetRating, bestClanGrouptoSwap1.Size(), teamToSwapInto, Math.Abs(targetSizeOffset), usingAngle);

        Vector2 bestPairVector = new((MatchBalancingHelpers.ClanGroupsSize(bestClanGroupToSwap2) - bestClanGrouptoSwap1.Size()) * targetSizeRescaling, Math.Abs(bestClanGrouptoSwap1.RatingPsum() - MatchBalancingHelpers.ClanGroupsRating(bestClanGroupToSwap2)));

        foreach (ClanGroup c in teamToSwapFrom)
        {
            // c is the potential first member of the pair (potentialClanGrouptoSwap1)
            float potentialClanGrouptoSwap1Size = c.Size();
            // we compute below what's the target rating for the second member of the pair
            potentialClanGroupToSwapTargetRating = swapingFromWeakTeam ? c.RatingPsum() + (float)ratingDifference / 2f : c.RatingPsum() - (float)ratingDifference / 2f;
            // potential second member of the pair
            List<ClanGroup> potentialClanGroupToSwap2 = MatchBalancingHelpers.FindAClanGroupToSwapUsing(potentialClanGroupToSwapTargetRating, c.Size(), teamToSwapInto, Math.Abs(targetSizeOffset), usingAngle);
            int potentialClanGroupToSwap2Size = MatchBalancingHelpers.ClanGroupsSize(potentialClanGroupToSwap2);
            // pair vector
            Vector2 potentialPairVector = new((MatchBalancingHelpers.ClanGroupsSize(potentialClanGroupToSwap2) - c.Size()) * targetSizeRescaling, Math.Abs(c.RatingPsum() - MatchBalancingHelpers.ClanGroupsRating(potentialClanGroupToSwap2)));
            if ((targetVector - potentialPairVector).Length()
                < (targetVector - bestPairVector).Length())
            {
                bestClanGrouptoSwap1 = c;
                bestClanGroupToSwap2 = potentialClanGroupToSwap2;
                bestPairVector = potentialPairVector;
                bestClanGroupToSwapTargetRating = (targetVector - potentialPairVector).Length();
            }
        }

        return (bestClanGrouptoSwap1, bestClanGroupToSwap2, bestClanGroupToSwapTargetRating);
    }

    private bool IsRatingRatioAcceptable(GameMatch gameMatch, float percentageDifference)
    {
        double ratingRatio = Math.Abs(
            (RatingHelpers.ComputeTeamRatingPowerSum(gameMatch.TeamB)
             - RatingHelpers.ComputeTeamRatingPowerSum(gameMatch.TeamA))
            / RatingHelpers.ComputeTeamRatingPowerSum(gameMatch.TeamA));
        return MathHelper.Within((float)ratingRatio, 0f, percentageDifference);
    }

    private bool IsTeamSizeDifferenceAcceptable(GameMatch gameMatch, float maxSizeRatio, float maxDifference)
    {
        bool tooMuchSizeRatioDifference = !MathHelper.Within(gameMatch.TeamA.Count / (float)gameMatch.TeamB.Count, maxSizeRatio, 1f / maxSizeRatio);
        bool sizeDifferenceGreaterThanThreshold = Math.Abs(gameMatch.TeamA.Count - gameMatch.TeamB.Count) > maxDifference;
        return !tooMuchSizeRatioDifference && !sizeDifferenceGreaterThanThreshold;
    }

    private bool IsBalanceGoodEnough(GameMatch gameMatch, float maxSizeRatio, float maxDifference, float percentageDifference)
    {
        return IsTeamSizeDifferenceAcceptable(gameMatch, maxSizeRatio, maxDifference) && IsRatingRatioAcceptable(gameMatch, percentageDifference);
    }

}
