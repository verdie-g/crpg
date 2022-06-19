using System;
using System.Collections.Generic;
using System.Linq;

namespace Crpg.BalancingAndRating.Balancing
{
    /// <summary>
    /// i don't know yet.
    /// </summary>
    internal interface IcRPGRatingSystem
    {
        /// <summary>
        /// Compute the new Elo given the elo of both parties and the outcome of the match.
        /// </summary>
        int UpdateElo(int previousElo, int opponentElo, int eloChangeFactor, int result);

        /// <summary>
        /// Compute a TeamElo P Mean.
        /// </summary>
        int ComputeTeamEloPowerMean(List<User> team, int p);

        /// <summary>
        /// Compute a TeamElo P Sum.
        /// </summary>
        int ComputeTeamEloPowerSum(List<User> team, int p);

        /// <summary>
        /// Compute the winchance of party A given the Elo of both parties.
        /// </summary>
        double WinChance(int eLoA, int eloB);

        /// <summary>
        /// TrueSkill V function
        /// </summary>
        double V(double t, double epsilon);
        /// <summary>
        /// TrueSkill W function
        /// </summary>
        double W(double t, double epsilon);
    }

    internal class Rating : IcRPGRatingSystem
    {
        public int UpdateElo(int previousElo, int opponentElo, int eloChangeFactor, int result)
        {
            return (int)(previousElo + eloChangeFactor * (result - WinChance(previousElo, opponentElo)));
        }

        public int ComputeTeamEloPowerMean(List<User> team, int p)
        {
            List<int> elos = team.Select(u => u.Elo).ToList();
            return MathHelper.PowerMean(elos, p);
        }

        public int ComputeTeamEloPowerSum(List<User> team, int p)
        {
            List<int> elos = (List<int>)team.Select(u => u.Elo);
            return MathHelper.PowerSum(elos, p);
        }

        public double WinChance(int eLoA, int eloB)
        {
            return 1 / (1 + Math.Pow(10, (eloB - eLoA) / 400));
        }

        public double V(double t, double epsilon)
        {
            return MathHelper.N(t - epsilon, 0, 1);
        }

        public double W(double t, double epsilon)
        {
            return V(t, epsilon) * (V(t, epsilon) + t - epsilon);
        } 
    }
}
