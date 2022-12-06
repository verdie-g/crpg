using System;
using System.Collections.Generic;
using System.Linq;
using Crpg.Module.Balancing;
namespace Crpg.Module.Balancing
{
    /// <summary>
    /// i don't know yet.
    /// </summary>
    internal interface IMatchBalancingSystem
    {
        /// <summary>
        /// Balances the Team by putting the best player into the first team
        /// then second best player into the second team etc.
        /// </summary>
        GameMatch NaiveCaptainBalancing(GameMatch gameMatch);
    }

    internal class MatchBalancingSystem : IMatchBalancingSystem
    {
        public const float PowerParameter = 1f;
        public GameMatch NaiveCaptainBalancing(GameMatch gameMatch)
        {
            List<User> allUsers = new();
            allUsers.AddRange(gameMatch.TeamA);
            allUsers.AddRange(gameMatch.TeamB);
            allUsers.AddRange(gameMatch.Waiting);
            GameMatch returnedGameMatch = new();
            bool teamA = true;
            foreach (User player in allUsers.OrderByDescending(u => u.Rating))
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
        public GameMatch KKBalancing(GameMatch gameMatch)
        {
            List<User> allUsers = new();
            allUsers.AddRange(gameMatch.TeamA);
            allUsers.AddRange(gameMatch.TeamB);
            allUsers.AddRange(gameMatch.Waiting);
            GameMatch returnedGameMatch = new();
            int i = 0;
            User[] players = new User[allUsers.Count];
            double[] elos = new double[allUsers.Count];
            foreach (User player in allUsers.OrderByDescending(u => u.Rating))
            {
                players[i] = player;
                elos[i] = player.Rating;
                i++;
            }

            var partition = MatchBalancingHelpers.Heuristic(players, elos, 2);
            returnedGameMatch.TeamA = partition.Partition[0];
            returnedGameMatch.TeamB = partition.Partition[1];
            return returnedGameMatch;
        }

        public GameMatch PureBannerBalancing(GameMatch gameMatch)
        {
            GameMatch unbalancedBannerGameMatch = KKMakeTeamOfSimilarSizesWithBannerBalance(gameMatch);
            unbalancedBannerGameMatch = BalanceTeamOfSimilarSizes(unbalancedBannerGameMatch, true, 0.025f);
            return unbalancedBannerGameMatch;
        }

        public GameMatch BannerBalancingWithEdgeCases(GameMatch gameMatch)
        {
            Console.WriteLine("BannerBalancingWithEdgeCases");
            Console.WriteLine("--------------------------------------------");
            Console.WriteLine("Now Making Team Of Similar Sizes with Banner");
            GameMatch balancedBannerGameMatch = KKMakeTeamOfSimilarSizesWithBannerBalance(gameMatch);
            Console.WriteLine("Teams are now Of Similar Sizes With Banner");
            Console.WriteLine("Team A Count " + balancedBannerGameMatch.TeamA.Count);
            Console.WriteLine("Team B Count " + balancedBannerGameMatch.TeamB.Count);
            Console.WriteLine("Banner Balancing Now");
            balancedBannerGameMatch = BalanceTeamOfSimilarSizes(balancedBannerGameMatch, true, 0.025f);
            Console.WriteLine("Banner Balancing Done");
            Console.WriteLine("Team A Count " + balancedBannerGameMatch.TeamA.Count + " Rating: " + RatingHelpers.ComputeTeamRatingPowerSum(balancedBannerGameMatch.TeamA));
            Console.WriteLine("Team B Count " + balancedBannerGameMatch.TeamB.Count + " Rating: " + RatingHelpers.ComputeTeamRatingPowerSum(balancedBannerGameMatch.TeamB));
            for (int i = 0; i < (balancedBannerGameMatch.TeamA.Count + balancedBannerGameMatch.TeamB.Count) / 2; i++)
            {
                if (IsSizeDifferencetooMuch(balancedBannerGameMatch))
                {
                    Console.WriteLine("Size Difference between the Two Teams is too big");
                    MakeTeamCountCloser(balancedBannerGameMatch);
                }
                else
                {
                    break;
                }
            }

            if (IsRatingRatioTooBad(balancedBannerGameMatch, 0.2f))
            {
                Console.WriteLine("RatingRatio is Too Bad => Swapping withoutBanner");
                balancedBannerGameMatch = BalanceTeamOfSimilarSizes(balancedBannerGameMatch, false, 0.10f);
            }

            return balancedBannerGameMatch;
        }

        public GameMatch KKMakeTeamOfSimilarSizesWithBannerBalance(GameMatch gameMatch)
        {
            List<User> allUsers = new();
            allUsers.AddRange(gameMatch.TeamA);
            allUsers.AddRange(gameMatch.TeamB);
            allUsers.AddRange(gameMatch.Waiting);
            var clangroupList = MatchBalancingHelpers.ConvertUserListToClanGroups(allUsers);
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
            returnedGameMatch.TeamA = MatchBalancingHelpers.ConvertClanGroupsToUserList(partition.Partition[0].ToList());
            returnedGameMatch.TeamB = MatchBalancingHelpers.ConvertClanGroupsToUserList(partition.Partition[1].ToList());
            return returnedGameMatch;
        }

        public GameMatch BalanceTeamOfSimilarSizes(GameMatch gameMatch, bool bannerBalance, float threshold)
        {
            for (int i = 0; i < 20; i++)
            {
                Console.WriteLine("i = " + i);
                if (!IsSizeDifferencetooMuch(gameMatch) && !IsRatingRatioTooBad(gameMatch, threshold))
                {
                    Console.WriteLine("Team are of Similar Sizes and Similar Ratings");
                    break;
                }
                else
                {
                    if (bannerBalance)
                    {
                        if (!SwapDoneWithBanner(gameMatch))
                        {
                            Console.WriteLine("No More Swap With Banner Available");
                            break;
                        }
                    }
                    else
                    {
                        if (!SwapDoneWithoutBanner(gameMatch))
                        {
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
            List<User> weakTeam;
            List<User> strongTeam;
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

            weakClanGroupsTeam = weakClanGroupsTeam.OrderBy(c => c.RatingPMean()).ToList();
            strongClanGroupsTeam = strongClanGroupsTeam.OrderBy(c => c.RatingPMean()).ToList();

            int playerCountDifference = weakClanGroupsTeam.Sum(c => c.Size()) - strongClanGroupsTeam.Sum(c => c.Size());
            bool swapingFromWeakTeam = playerCountDifference >= 0;
            ClanGroup weakClanGroupToSwap = weakClanGroupsTeam.First();
            ClanGroup strongClanGroupToSwap = strongClanGroupsTeam.Last();

            ClanGroup clanGrouptoSwap1;
            List<ClanGroup> clanGroupstoSwap2;

            clanGrouptoSwap1 = swapingFromWeakTeam ? weakClanGroupToSwap : strongClanGroupToSwap;
            List<ClanGroup> teamClanGroupsToSwapInto = swapingFromWeakTeam ? strongClanGroupsTeam : weakClanGroupsTeam;

            float clanGroupToSwapTargetRating = swapingFromWeakTeam ? weakClanGroupToSwap.RatingPsum() + (float)Math.Abs(diff) / 2f : strongClanGroupToSwap.RatingPsum() - (float)Math.Abs(diff) / 2f;
            var clanGroupsToSwapUsingAngleTuple = FindBestPairForSwapDoneWithBanner(weakClanGroupsTeam,strongClanGroupsTeam,diff, playerCountDifference / 2,true,swapingFromWeakTeam);
            var clanGroupsToSwapUsingDistanceTuple = FindBestPairForSwapDoneWithBanner(weakClanGroupsTeam, strongClanGroupsTeam, diff, playerCountDifference / 2, false, swapingFromWeakTeam);
            if (Math.Abs(RatingHelpers.ClanGroupsPowerSum(clanGroupsToSwapUsingAngleTuple.clanGroupsToSwap2) - clanGroupsToSwapUsingAngleTuple.clanGroupsToSwap2TargetRating) < Math.Abs(RatingHelpers.ClanGroupsPowerSum(clanGroupsToSwapUsingDistanceTuple.clanGroupsToSwap2) - clanGroupsToSwapUsingDistanceTuple.clanGroupsToSwap2TargetRating))
            {
                clanGroupstoSwap2 = clanGroupsToSwapUsingAngleTuple.clanGroupsToSwap2;
            }
            else
            {
                clanGroupstoSwap2 = clanGroupsToSwapUsingDistanceTuple.clanGroupsToSwap2;

            }

            float a = clanGrouptoSwap1.RatingPsum();
            float b = RatingHelpers.ClanGroupsPowerSum(clanGroupstoSwap2);
            bool c = swapingFromWeakTeam;
            float newdiff;
            if (swapingFromWeakTeam)
            {
                newdiff = RatingHelpers.ClanGroupsPowerSum(strongClanGroupsTeam) + 2f * clanGrouptoSwap1.RatingPsum() - 2f * RatingHelpers.ClanGroupsPowerSum(clanGroupstoSwap2) - RatingHelpers.ClanGroupsPowerSum(weakClanGroupsTeam);
            }
            else
            {
                newdiff = RatingHelpers.ClanGroupsPowerSum(strongClanGroupsTeam) + 2f * RatingHelpers.ClanGroupsPowerSum(clanGroupstoSwap2) - 2f * clanGrouptoSwap1.RatingPsum() - RatingHelpers.ClanGroupsPowerSum(weakClanGroupsTeam);
            }
            float newdiff1 = newdiff;
            var teamtoSwapFrom = swapingFromWeakTeam ? weakTeam : strongTeam;
            var teamtoSwapInto = swapingFromWeakTeam ? strongTeam : weakTeam;
            Console.WriteLine("SwapDoneWithBanner Log");
            Console.WriteLine("swappingFromWeakTeam" + swapingFromWeakTeam);
            Console.WriteLine("strongClanGroupsTeam");
            DumpClanGroups(strongClanGroupsTeam);
            Console.WriteLine("weakClanGroupsTeam");
            DumpClanGroups(weakClanGroupsTeam);
            Console.WriteLine("clanGrouptoSwap1");
            DumpClanGroup(clanGrouptoSwap1);
            Console.WriteLine("clanGroupstoSwap2");
            DumpClanGroups(clanGroupstoSwap2);

            if (Math.Abs(newdiff) < Math.Abs(diff))
            {
                foreach (var clanGroup in clanGroupstoSwap2)
                {
                    foreach (User user in clanGroup.MemberList())
                    {
                        teamtoSwapInto.Remove(user);
                        teamtoSwapFrom.Add(user);

                    }

                }

                foreach (User user in clanGrouptoSwap1.MemberList())
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
        private (ClanGroup clanGrouptoSwap1, List<ClanGroup> clanGroupsToSwap2,float clanGroupsToSwap2TargetRating) FindBestPairForSwapDoneWithBanner(List<ClanGroup> weakClanGroupsTeam, List<ClanGroup> strongClanGroupsTeam,double ratingDifference,int sizeOffset, bool usingAngle, bool swapingFromWeakTeam)
        {
            var teamToSwapFrom = swapingFromWeakTeam ? weakClanGroupsTeam : strongClanGroupsTeam;
            var teamToSwapInto = swapingFromWeakTeam ? strongClanGroupsTeam : weakClanGroupsTeam;


            ClanGroup weakClanGroupToSwap = weakClanGroupsTeam.First();
            ClanGroup strongClanGroupToSwap = strongClanGroupsTeam.Last();

            float potentialClanGroupToSwapTargetRating;
            float bestClanGroupToSwapTargetRating = swapingFromWeakTeam ? weakClanGroupToSwap.RatingPsum() + (float)Math.Abs(ratingDifference) / 2f : strongClanGroupToSwap.RatingPsum() - (float)Math.Abs(ratingDifference) / 2f;

            ClanGroup bestClanGrouptoSwap1 = swapingFromWeakTeam ? weakClanGroupToSwap : strongClanGroupToSwap;
            List<ClanGroup> bestClanGroupToSwap2 = MatchBalancingHelpers.FindAClanGroupToSwapUsing(bestClanGroupToSwapTargetRating, bestClanGrouptoSwap1.Size(), teamToSwapInto, Math.Abs(sizeOffset), usingAngle);

            foreach (ClanGroup c in teamToSwapFrom)
            {
                potentialClanGroupToSwapTargetRating = swapingFromWeakTeam ? c.RatingPsum() + (float)Math.Abs(ratingDifference) / 2f : c.RatingPsum() - (float)Math.Abs(ratingDifference) / 2f;
                List<ClanGroup> potentialClanGroupToSwap2 = MatchBalancingHelpers.FindAClanGroupToSwapUsing(potentialClanGroupToSwapTargetRating, c.Size(), teamToSwapInto, Math.Abs(sizeOffset), usingAngle);
                if (Math.Abs(RatingHelpers.ClanGroupsPowerSum(potentialClanGroupToSwap2) - potentialClanGroupToSwapTargetRating)
                    < Math.Abs(RatingHelpers.ClanGroupsPowerSum(bestClanGroupToSwap2) - bestClanGroupToSwapTargetRating))
                {
                    bestClanGrouptoSwap1 = c;
                    bestClanGroupToSwap2 = potentialClanGroupToSwap2;
                    bestClanGroupToSwapTargetRating = potentialClanGroupToSwapTargetRating;
                }
            }

            return (bestClanGrouptoSwap1,bestClanGroupToSwap2, bestClanGroupToSwapTargetRating);
        }

        public bool SwapDoneWithoutBanner(GameMatch gameMatch)
        {
            Console.WriteLine("SwapDoneWithoutBanner");
            double diff = RatingHelpers.ComputeTeamRatingDifference(gameMatch);
            List<User> weakTeam;
            List<User> strongTeam;
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

            User weakUserToSwap = weakTeam.First();
            double targetRating = weakUserToSwap.Rating + Math.Abs(diff) / 2f;
            User strongUserToSwap = MatchBalancingHelpers.FindAUserToSwap((float)targetRating, strongTeam);
            foreach (var user in weakTeam)
            {
                targetRating = user.Rating + Math.Abs(diff) / 2f;
                User potentialUserToSwap = MatchBalancingHelpers.FindAUserToSwap((float)targetRating, strongTeam);
                if (Math.Abs(potentialUserToSwap.Rating - targetRating) < Math.Abs(strongUserToSwap.Rating - weakUserToSwap.Rating - Math.Abs(diff) / 2f))
                {
                    weakUserToSwap = user;
                    strongUserToSwap = potentialUserToSwap;
                }
            }

            double newdiff = RatingHelpers.ComputeTeamRatingPowerMean(strongTeam) + 2f * weakUserToSwap.Rating - 2f * strongUserToSwap.Rating - RatingHelpers.ComputeTeamRatingPowerMean(weakTeam);
            Console.WriteLine("Strong User To Swap" + strongUserToSwap.Name + " : " + strongUserToSwap.Rating);
            Console.WriteLine("weak User To Swap" + weakUserToSwap.Name + " : " + weakUserToSwap.Rating);
            Console.WriteLine("New Diff " + newdiff);
            Console.WriteLine("Old Diff" + diff);

            if (Math.Abs(newdiff) < Math.Abs(diff))
            {
                strongTeam.Remove(strongUserToSwap);
                strongTeam.Add(weakUserToSwap);
                weakTeam.Add(strongUserToSwap);
                weakTeam.Remove(weakUserToSwap);
                return true;
            }
            else
            {
                return false;
            }

        }
        private void MakeTeamCountCloser(GameMatch gameMatch)
        {
            Console.WriteLine("MakeTeamCountCloser LOGs");
            double diff = RatingHelpers.ComputeTeamRatingDifference(gameMatch);
            List<User> weakTeam;
            List<User> strongTeam;
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
                User weakUser = weakTeam.OrderBy(u => u.Rating).First();
                Console.WriteLine("Strong User To Swap" + weakUser.Name);
                weakTeam.Remove(weakUser);
                strongTeam.Add(weakUser);
            }
            else
            {
                User strongUser = strongTeam.OrderBy(u => u.Rating).Last();
                Console.WriteLine("Strong User To Swap" + strongUser.Name);
                weakTeam.Add(strongUser);
                strongTeam.Remove(strongUser);
            }
        }

        private bool IsRatingRatioTooBad(GameMatch gameMatch,float threshold)
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
            return (tooMuchSizeRatioDifference || sizeDifferenceGreaterThanThreshold);
        }
        private void DumpClanGroups(List<ClanGroup> clanGroups)
            {
            int i = 1;
                foreach (ClanGroup clanGroup in clanGroups)
                {
                string clanGroupName = clanGroup.Clan() == null ? "Solo" : clanGroup.Clan()!.Name;
                    Console.WriteLine(clanGroupName);
                    foreach (User u in clanGroup.MemberList())
                    {
                        Console.WriteLine(u.Name + " : " + u.Rating);
                    }
                }
            }

        private void DumpClanGroup(ClanGroup clanGroup)
        {
                foreach (User u in clanGroup.MemberList())
                {
                    Console.WriteLine(u.Name + " : " + u.Rating);
                }
        }

        private void DumpTeam(List<User> team)
        {
            foreach (User u in team)
            {
                Console.WriteLine(u.Name + " : " + u.Rating);
            }
        }

        /*
        public GameMatch BalancingWithClans(GameMatch gameMatch)
        {
            List<User> allUsers = new List<User> { };
            allUsers.AddRange(gameMatch.TeamA);
            allUsers.AddRange(gameMatch.TeamB);
            allUsers.AddRange(gameMatch.Waiting);
            GameMatch returnedGameMatch = new GameMatch { };
            var clanGroups = MatchBalancingHelpers.ConvertUserListToClanGroups(allUsers);

        }
        */
    }
}
