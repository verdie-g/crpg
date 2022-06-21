using System;
using System.Collections.Generic;
using System.Linq;
using Crpg.BalancingAndRating.Balancing;
namespace Crpg.BalancingAndRating.Balancing
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
        public GameMatch NaiveCaptainBalancing(GameMatch gameMatch)
        {
            List<User> allUsers = new();
            allUsers.AddRange(gameMatch.TeamA);
            allUsers.AddRange(gameMatch.TeamB);
            allUsers.AddRange(gameMatch.Waiting);
            GameMatch returnedGameMatch = new();
            bool teamA = true;
            foreach (User player in allUsers.OrderByDescending(u => u.Elo))
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
            foreach (User player in allUsers.OrderByDescending(u => u.Elo))
            {
                players[i] = player;
                elos[i] = player.Elo;
                i++;
            }
            var partition = MatchBalancingHelpers.Heuristic(players, elos, 2);
            returnedGameMatch.TeamA = partition.Partition[0];
            returnedGameMatch.TeamB = partition.Partition[1];
            return returnedGameMatch;
        }
        public GameMatch KKMakeTeamOfSimilarSizes(GameMatch gameMatch)
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
        public GameMatch BalanceTeamOfSimilarSizes(GameMatch gameMatch, double threshold = 0.10)
        {
            double diff = RatingHelpers.ComputeTeamRatingDifference(gameMatch);
            GameMatch returnedGameMatch = new();
            if (Math.Abs(diff / RatingHelpers.ComputeTeamRatingPowerSum(gameMatch.TeamA, 1)) < threshold)
            {
            }
            else
            {
                if (diff < 0)
                {
                    returnedGameMatch = DoASwap(returnedGameMatch);
                }

            }
            return returnedGameMatch;
        }

        public GameMatch DoASwap(GameMatch gameMatch)
        {
        return gameMatch;
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
