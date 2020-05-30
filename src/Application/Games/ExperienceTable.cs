using System;
using System.Linq;

namespace Crpg.Application.Games
{
    public static class ExperienceTable
    {
        internal const int MinLevel = 1;
        internal const int MaxLevel = 35;

        private static readonly int[] Table = ComputeExperienceTable();

        private static int[] ComputeExperienceTable()
        {
            var table = new int[MaxLevel - MinLevel + 1];
            table[0] = 0; // lvl 1: 0

            for (int lvl = MinLevel + 1; lvl <= 30; lvl += 1)
            {
                const int a = 3937;
                const int b = 13750;
                const int c = -30750;
                table[lvl - MinLevel] = (int)(a * Math.Pow(1.26, lvl) + b * lvl + c);
            }

            for (int lvl = 31; lvl <= MaxLevel; lvl += 1)
            {
                table[lvl - MinLevel] = table[lvl - MinLevel - 1] * 2;
            }

            return table;
        }

        /// <summary>
        /// Get the level for the total experience gained.
        /// </summary>
        public static int GetLevelForExperience(int experience)
        {
            int level = Array.BinarySearch(Table, experience);
            return level >= 0 ? level + MinLevel : ~level;
        }

        /// <summary>
        /// Get the total experience needed to reach <see cref="level"/>.
        /// </summary>
        public static int GetExperienceForLevel(int level)
        {
            return Table[level - MinLevel];
        }
    }
}
