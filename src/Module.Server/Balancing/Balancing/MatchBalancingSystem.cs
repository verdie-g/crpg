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
        public GameMatch BannerBalancing(GameMatch gameMatch)
        {
            GameMatch unbalancedBannerGameMatch = KKMakeTeamOfSimilarSizesWithBannerBalance(gameMatch);
            unbalancedBannerGameMatch = BalanceTeamOfSimilarSizesWithBannerBalance(unbalancedBannerGameMatch);
            return unbalancedBannerGameMatch;
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

        public GameMatch BalanceTeamOfSimilarSizesWithBannerBalance(GameMatch gameMatch, double threshold = 0.025)
        {
            for (int i = 0; i < 20; i++)
            {
                float diff = RatingHelpers.ComputeTeamRatingDifference(gameMatch);
                Console.WriteLine("i = " + i);
                if (Math.Abs(diff / RatingHelpers.ComputeTeamRatingPowerSum(gameMatch.TeamA)) < threshold)
                {
                    break;
                }
                else
                {
                    if (!SwapDone(gameMatch))
                    {
                        break;
                    }
                }
            }

            return gameMatch;
        }

        public bool SwapDone(GameMatch gameMatch)
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
            ClanGroup weakClanGroupToSwap = weakClanGroupsTeam.First();
            ClanGroup strongClanGroupToSwap = strongClanGroupsTeam.Last();
            ClanGroup clanGroupstoSwap1;
            bool swapingFromWeakTeam = playerCountDifference >= 0;
            clanGroupstoSwap1 = swapingFromWeakTeam ? weakClanGroupToSwap : strongClanGroupToSwap;
            List<ClanGroup> teamToSwapInto = swapingFromWeakTeam ? strongClanGroupsTeam : weakClanGroupsTeam;
            float clanGroupToSwapTargetRating = weakClanGroupToSwap.RatingPsum() + (float)Math.Abs(diff) / 2f;
            List<ClanGroup> clanGroupstoSwap2;
            var clanGroupsToSwapUsingAngle = MatchBalancingHelpers.FindASwapUsing(clanGroupToSwapTargetRating, weakClanGroupToSwap.Size(), teamToSwapInto, Math.Abs(playerCountDifference / 2), true);
            var clanGroupsToSwapUsingDistance = MatchBalancingHelpers.FindASwapUsing(clanGroupToSwapTargetRating, weakClanGroupToSwap.Size(), teamToSwapInto, Math.Abs(playerCountDifference /2), false);
            if (Math.Abs(RatingHelpers.ClanGroupsPowerSum(clanGroupsToSwapUsingAngle) - clanGroupToSwapTargetRating) < Math.Abs(RatingHelpers.ClanGroupsPowerSum(clanGroupsToSwapUsingDistance) - clanGroupToSwapTargetRating))
            {
                clanGroupstoSwap2 = clanGroupsToSwapUsingAngle;
            }
            else
            {
                clanGroupstoSwap2 = clanGroupsToSwapUsingDistance;

            }
            float a = RatingHelpers.ClanGroupsPowerSum(strongClanGroupsTeam);
            float b = 2f * weakClanGroupToSwap.RatingPsum();
            float c = -2f * RatingHelpers.ClanGroupsPowerSum(clanGroupstoSwap2);
            float newdiff = RatingHelpers.ClanGroupsPowerSum(strongClanGroupsTeam) + 2f * weakClanGroupToSwap.RatingPsum() - 2f * RatingHelpers.ClanGroupsPowerSum(clanGroupstoSwap2) - RatingHelpers.ClanGroupsPowerSum(weakClanGroupsTeam);

            if (Math.Abs(newdiff) < Math.Abs(diff))
            {
                foreach (var clanGroup in clanGroupstoSwap2)
                {
                    foreach (User user in clanGroup.MemberList())
                    {
                        strongTeam.Remove(user);
                        weakTeam.Add(user);
                    }

                }

                foreach (User user in weakClanGroupToSwap.MemberList())
                {
                    weakTeam.Remove(user);
                    strongTeam.Add(user);

                }

                return true;
            }
            else
            {
                return false;
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
