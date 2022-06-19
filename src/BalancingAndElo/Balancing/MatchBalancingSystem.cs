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
