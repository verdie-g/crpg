using System;

namespace Crpg.Application.Common.Services
{
    public class ExperienceTable
    {
        private readonly Constants _constants;
        private readonly int[] _table;

        public ExperienceTable(Constants constants)
        {
            _constants = constants;
            _table = ComputeExperienceTable();
        }

        /// <summary>
        /// Get the level for the total experience gained.
        /// </summary>
        public int GetLevelForExperience(int experience)
        {
            int level = Array.BinarySearch(_table, experience);
            return level >= 0 ? level + _constants.MinimumLevel : ~level;
        }

        /// <summary>
        /// Get the total experience needed to reach <see cref="level"/>.
        /// </summary>
        public int GetExperienceForLevel(int level)
        {
            return _table[level - _constants.MinimumLevel];
        }

        private int[] ComputeExperienceTable()
        {
            var table = new int[_constants.MaximumLevel - _constants.MinimumLevel + 1];
            table[0] = 0; // lvl 1: 0

            for (int lvl = _constants.MinimumLevel + 1; lvl <= 30; lvl += 1)
            {
                float a = _constants.ExperienceForLevelCoefs[0];
                float b = _constants.ExperienceForLevelCoefs[1];
                float c = _constants.ExperienceForLevelCoefs[2];
                table[lvl - _constants.MinimumLevel] = (int)(a * Math.Pow(1.26, lvl) + b * lvl + c);
            }

            for (int lvl = 31; lvl <= _constants.MaximumLevel; lvl += 1)
            {
                table[lvl - _constants.MinimumLevel] = table[lvl - _constants.MinimumLevel - 1] * 2;
            }

            return table;
        }
    }
}
