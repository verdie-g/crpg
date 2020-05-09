using System;
using System.Linq;

namespace Crpg.Application.Games
{
    public static class ExperienceTable
    {
        private const int TableStartLevel = 1;

        internal static readonly int[] Table =
            Enumerable.Range(TableStartLevel, 35)
                .Select(lvl =>
                {
                    // TODO: make curve steeper for lvl > 30
                    if (lvl == 1)
                    {
                        return 0;
                    }

                    const int a = 3936;
                    const int b = 13750;
                    const int c = -30750;
                    return (int)(a * Math.Pow(1.26, lvl) + b * lvl + c);
                }).ToArray();

        /// <summary>
        /// Get the level for the total experience gained.
        /// </summary>
        public static int GetLevelForExperience(int experience)
        {
            int level = Array.BinarySearch(Table, experience);
            return level >= 0 ? level + TableStartLevel : ~level;
        }

        /// <summary>
        /// Get the total experience needed to reach <see cref="level"/>.
        /// </summary>
        public static int GetExperienceForLevel(int level)
        {
            return Table[level - TableStartLevel];
        }
    }
}
