using System;
using System.Collections.Generic;
using System.Linq;

namespace Crpg.Module.Balancing
{
    /// <summary>
    /// i don't know yet.
    /// </summary>
    internal interface IcRPGEloSystem
    {
        /// <summary>
        /// Compute the new Elo given the elo of both parties and the outcome of the match.
        /// </summary>
        int UpdatedElo(int previousElo, int opponentElo, int eloChangeFactor, int result);

        /// <summary>
        /// Compute the winchance of party A given the Elo of both parties.
        /// </summary>
        double WinChance(int eLoA, int eloB);


    }

    internal class CRPGEloSystem : IcRPGEloSystem
    {
        public int UpdatedElo(int previousElo, int opponentElo, int eloChangeFactor, int result)
        {
            return (int)(previousElo + eloChangeFactor * (result - WinChance(previousElo, opponentElo)));
        }

        public double WinChance(int eLoA, int eloB)
        {
            return 1 / (1 + Math.Pow(10, (eloB - eLoA) / 400));
        }

    }
}
