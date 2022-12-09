﻿using System;
using System.Collections.Generic;
using System.Linq;
using Crpg.Module.Balancing;
using System.Numerics;
using Crpg.Module.Api.Models.Users;
using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Api.Models.Clans;
using TaleWorlds.LinQuick;

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
        Console.WriteLine("BannerBalancingWithEdgeCases");
        Console.WriteLine("--------------------------------------------");
        Console.WriteLine("Now Making Team Of Similar Sizes with Banner");
        GameMatch balancedBannerGameMatch = KKMakeTeamOfSimilarSizesWithBannerBalance(gameMatch);
        Console.WriteLine("Teams are now Of Similar Sizes With Banner");
        Console.WriteLine("Team A Count " + balancedBannerGameMatch.TeamA.Count + " Rating: " + RatingHelpers.ComputeTeamRatingPowerSum(balancedBannerGameMatch.TeamA));
        Console.WriteLine("Team B Count " + balancedBannerGameMatch.TeamB.Count + " Rating: " + RatingHelpers.ComputeTeamRatingPowerSum(balancedBannerGameMatch.TeamB));
        Console.WriteLine("--------------------------------------------");
        Console.WriteLine("Banner Balancing Now");
        balancedBannerGameMatch = BalanceTeamOfSimilarSizes(balancedBannerGameMatch, true, 0.025f);
        Console.WriteLine("Banner Balancing Done");
        Console.WriteLine("Team A Count " + balancedBannerGameMatch.TeamA.Count + " Rating: " + RatingHelpers.ComputeTeamRatingPowerSum(balancedBannerGameMatch.TeamA));
        Console.WriteLine("Team B Count " + balancedBannerGameMatch.TeamB.Count + " Rating: " + RatingHelpers.ComputeTeamRatingPowerSum(balancedBannerGameMatch.TeamB));
        Console.WriteLine("--------------------------------------------");



   
        if (IsRatingRatioTooBad(balancedBannerGameMatch, 0.2f) || IsSizeDifferencetooMuch(balancedBannerGameMatch))
        {
            Console.WriteLine("Unnacceptable");
            balancedBannerGameMatch = BalanceTeamOfSimilarSizes(balancedBannerGameMatch, false, 0.10f);
        }
        else
        {
            Console.WriteLine("Acceptable");
        }
        if (IsRatingRatioTooBad(balancedBannerGameMatch, 0.2f) || IsSizeDifferencetooMuch(balancedBannerGameMatch))
        {
            Console.WriteLine("Swaps were not enough. This should Really not Happen often");
            Console.WriteLine("Horror");
            Console.WriteLine("----------------------------------");
            MatchBalancingHelpers.DumpTeam(balancedBannerGameMatch.TeamA);
            Console.WriteLine("----------------------------------");
            Console.WriteLine("NaiveCaptainBalancing + Balancing Without Banner");
            MatchBalancingHelpers.DumpTeam(balancedBannerGameMatch.TeamB);
            balancedBannerGameMatch = NaiveCaptainBalancing(balancedBannerGameMatch);
            balancedBannerGameMatch = BalanceTeamOfSimilarSizes(balancedBannerGameMatch, false, 0.10f);
        }
        else
        {
            Console.WriteLine("Acceptable");
        }
        Console.WriteLine("--------------------------------------------");
        Console.WriteLine("Team A Count " + balancedBannerGameMatch.TeamA.Count + " Rating: " + RatingHelpers.ComputeTeamRatingPowerSum(balancedBannerGameMatch.TeamA));
        Console.WriteLine("Team B Count " + balancedBannerGameMatch.TeamB.Count + " Rating: " + RatingHelpers.ComputeTeamRatingPowerSum(balancedBannerGameMatch.TeamB));
        return balancedBannerGameMatch;
    }

    public GameMatch KKMakeTeamOfSimilarSizesWithBannerBalance(GameMatch gameMatch)
    {
        List<CrpgUser> allCrpgUsers = new();
        allCrpgUsers.AddRange(gameMatch.TeamA);
        allCrpgUsers.AddRange(gameMatch.TeamB);
        allCrpgUsers.AddRange(gameMatch.Waiting);
        var clangroupList = MatchBalancingHelpers.ConvertCrpgUserListToClanGroups(allCrpgUsers);
        GameMatch returnedGameMatch = new();
        int i = 0;
        ClanGroup[] clangroupsArray = new ClanGroup[clangroupList.Count];
        double[] size = new double[clangroupList.Count];
        foreach (ClanGroup clangroup in clangroupList.OrderByDescending(c => c.Size()))
        {
            clangroupsArray[i] = clangroup;
            size[i] = clangroup.Size();
            i++;
        }

        var partition = MatchBalancingHelpers.Heuristic(clangroupsArray, size, 2);
        returnedGameMatch.TeamA = MatchBalancingHelpers.ConvertClanGroupsToCrpgUserList(partition.Partition[0].ToList());
        returnedGameMatch.TeamB = MatchBalancingHelpers.ConvertClanGroupsToCrpgUserList(partition.Partition[1].ToList());
        return returnedGameMatch;
    }

    public GameMatch BalanceTeamOfSimilarSizes(GameMatch gameMatch, bool bannerBalance, float threshold)
    {
        for (int i = 0; i < 20; i++)
        {
            if (!IsSizeDifferencetooMuch(gameMatch) && !IsRatingRatioTooBad(gameMatch, threshold))
            {
                Console.WriteLine("Made " + i + " Swaps");
                Console.WriteLine("Team are of Similar Sizes and Similar Ratings");
                break;
            }
            else
            {
                if (bannerBalance)
                {
                    if (!SwapDoneWithBanner(gameMatch))
                    {
                        Console.WriteLine("Made " + i + " Swaps");
                        Console.WriteLine("No More Swap With Banner Available");
                        break;
                    }
                }
                else
                {
                    if (!SwapDoneWithoutBanner(gameMatch))
                    {
                        Console.WriteLine("Made " + i + " Swaps");
                        Console.WriteLine("No More Swap Without Banner Available");
                        break;
                    }
                }
            }
        }

        return gameMatch;
    }

    public bool SwapDoneWithBanner(GameMatch gameMatch)
    {
        double diff = RatingHelpers.ComputeTeamRatingDifference(gameMatch);
        ClanGroupsGameMatch clanGroupGameMatch = MatchBalancingHelpers.ConvertGameMatchToClanGroupsGameMatchList(gameMatch);
        List<CrpgUser> weakTeam;
        List<CrpgUser> strongTeam;
        List<ClanGroup> weakClanGroupsTeam;
        List<ClanGroup> strongClanGroupsTeam;
        if (diff < 0)
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

        diff = RatingHelpers.ComputeTeamRatingPowerSum(strongTeam) - RatingHelpers.ComputeTeamRatingPowerSum(weakTeam); // diff >0
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

        float clanGroupToSwapTargetRating = swapingFromWeakTeam ? weakClanGroupToSwap.RatingPsum() + (float)Math.Abs(diff) / 2f : strongClanGroupToSwap.RatingPsum() - (float)Math.Abs(diff) / 2f;
        var clanGroupsToSwapUsingAngleTuple = FindBestPairForSwapDoneWithBanner(weakClanGroupsTeam, strongClanGroupsTeam, diff, playerCountDifference / 2, true, swapingFromWeakTeam);
        var clanGroupsToSwapUsingDistanceTuple = FindBestPairForSwapDoneWithBanner(weakClanGroupsTeam, strongClanGroupsTeam, diff, playerCountDifference / 2, false, swapingFromWeakTeam);

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

        if (Math.Abs(newdiff) < Math.Abs(diff))
        {
            foreach (var clanGroup in clanGroupstoSwap2)
            {
                foreach (CrpgUser user in clanGroup.MemberList())
                {
                    teamtoSwapInto.Remove(user);
                    teamtoSwapFrom.Add(user);
                }
            }

            foreach (CrpgUser user in clanGrouptoSwap1.MemberList())
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

    public bool SwapDoneWithoutBanner(GameMatch gameMatch)
    {
        double diff = RatingHelpers.ComputeTeamRatingDifference(gameMatch);
        List<CrpgUser> weakTeam;
        List<CrpgUser> strongTeam;
        if (diff < 0)
        {
            weakTeam = gameMatch.TeamA;
            strongTeam = gameMatch.TeamB;
        }
        else
        {
            weakTeam = gameMatch.TeamB;
            strongTeam = gameMatch.TeamA;
        }
        diff = Math.Abs(diff);
        int playerCountDifference = weakTeam.Count - strongTeam.Count;
        bool swapingFromWeakTeam = playerCountDifference <= 0;
        List<CrpgUser> teamTeamToSwapFrom = swapingFromWeakTeam ? weakTeam : strongTeam;
        List<CrpgUser> teamTeamToSwapInto = swapingFromWeakTeam ? strongTeam : weakTeam;
        CrpgUser bestCrpgUserToSwap1 = swapingFromWeakTeam? weakTeam.OrderBy(c => c.Character.Rating.Value).First() : strongTeam.OrderBy(c => c.Character.Rating.Value).Last();
        float sizeOffset = Math.Abs(playerCountDifference);
        float targetSizeRescaling = (float)diff / (2f * sizeOffset);
        double targetRating = swapingFromWeakTeam ? bestCrpgUserToSwap1.Character.Rating.Value + Math.Abs(diff) / 2f : bestCrpgUserToSwap1.Character.Rating.Value - Math.Abs(diff) / 2f;
        List<CrpgUser> bestCrpgUsersToSwap2 = MatchBalancingHelpers.FindCrpgUsersToSwap((float)targetRating, teamTeamToSwapInto, sizeOffset /2f);
        // the pair difference (strong - weak) needs to be close to TargetVector
        Vector2 targetVector = new(sizeOffset * targetSizeRescaling, (float)diff / 2f);
        Vector2 bestPairVector = new((bestCrpgUsersToSwap2.Count - 1) * targetSizeRescaling, Math.Abs(bestCrpgUserToSwap1.Character.Rating.Value - bestCrpgUsersToSwap2.Sum(u => u.Character.Rating.Value)));
        foreach (var user in teamTeamToSwapFrom)
        {
            targetRating = swapingFromWeakTeam ? user.Character.Rating.Value + Math.Abs(diff) / 2f : user.Character.Rating.Value - Math.Abs(diff) / 2f;
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
        if (Math.Abs(newdiff) < Math.Abs(diff))
        {
            if(swapingFromWeakTeam)
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

    private void MakeTeamCountCloser(GameMatch gameMatch)
    {
        Console.WriteLine("MakeTeamCountCloser LOGs");
        double diff = RatingHelpers.ComputeTeamRatingDifference(gameMatch);
        List<CrpgUser> weakTeam;
        List<CrpgUser> strongTeam;
        if (diff < 0)
        {
            weakTeam = gameMatch.TeamA;
            strongTeam = gameMatch.TeamB;
        }
        else
        {
            weakTeam = gameMatch.TeamB;
            strongTeam = gameMatch.TeamA;
        }

        if (weakTeam.Count >= strongTeam.Count)
        {
            CrpgUser weakCrpgUser = weakTeam.OrderBy(u => u.Character.Rating.Value).First();
            Console.WriteLine("Strong CrpgUser To Swap" + weakCrpgUser.Character.Name);
            weakTeam.Remove(weakCrpgUser);
            strongTeam.Add(weakCrpgUser);
        }
        else
        {
            CrpgUser strongCrpgUser = strongTeam.OrderBy(u => u.Character.Rating.Value).Last();
            Console.WriteLine("Strong CrpgUser To Swap" + strongCrpgUser.Character.Name);
            weakTeam.Add(strongCrpgUser);
            strongTeam.Remove(strongCrpgUser);
        }
    }

    private bool IsRatingRatioTooBad(GameMatch gameMatch, float threshold)
    {
        double ratingRatio = Math.Abs(
            (RatingHelpers.ComputeTeamRatingPowerSum(gameMatch.TeamB)
             - RatingHelpers.ComputeTeamRatingPowerSum(gameMatch.TeamA))
            / RatingHelpers.ComputeTeamRatingPowerSum(gameMatch.TeamA));
        return !MathHelper.Within((float)ratingRatio, 0f, threshold);
    }

    private bool IsSizeDifferencetooMuch(GameMatch gameMatch)
    {
        bool tooMuchSizeRatioDifference = !MathHelper.Within(Math.Abs(gameMatch.TeamA.Count / (float)gameMatch.TeamB.Count), 0.75f, 1.34f);
        bool sizeDifferenceGreaterThanThreshold = Math.Abs(gameMatch.TeamA.Count - gameMatch.TeamB.Count) > 10;
        return tooMuchSizeRatioDifference || sizeDifferenceGreaterThanThreshold;
    }


}
