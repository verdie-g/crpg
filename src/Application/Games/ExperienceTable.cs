using System;

namespace Crpg.Application.Games
{
    public static class ExperienceTable
    {
        private const int TableStartLevel = 1;

        // TODO: get real values from old crpg
        private static readonly int[] Table =
        {
            0, // level 1
            600, // level 2
            1_360, // level 3
            2_296, // ...
            3_426,
            4_768,
            6_345,
            8_179,
            10_297,
            13_010,
            16_161,
            19_806,
            24_007,
            28_832,
            34_362,
            40_682,
            47_892,
            56_103,
            65_441,
            77_233,
            90_809,
            106_425,
            124_371,
            144_981,
            168_636,
            195_769,
            226_879,
            262_533,
            303_381,
            350_164,
            412_091,
            484_440,
            568_974,
            667_638,
            782_877,
            917_424,
            1_074_494,
            1_257_843,
            1_471_851,
            1_721_626,
            2_070_551,
            2_489_361,
        };

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

