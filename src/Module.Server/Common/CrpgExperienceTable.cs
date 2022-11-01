namespace Crpg.Module.Common;

internal class CrpgExperienceTable
{
    private readonly CrpgConstants _constants;
    private readonly int[] _table;

    public CrpgExperienceTable(CrpgConstants constants)
    {
        _constants = constants;
        _table = ComputeExperienceTable();
    }

    public int GetLevelForExperience(int experience)
    {
        int level = Array.BinarySearch(_table, experience);
        return level >= 0 ? level + _constants.MinimumLevel : ~level;
    }

    public int GetExperienceForLevel(int level)
    {
        return _table[level - _constants.MinimumLevel];
    }

    private int[] ComputeExperienceTable()
    {
        int[] table = new int[_constants.MaximumLevel - _constants.MinimumLevel + 1];
        table[0] = 0; // lvl 1: 0

        for (int lvl = _constants.MinimumLevel + 1; lvl <= 30; lvl += 1)
        {
            float scaler = (float)Math.Pow(29f, 5.65f) + 150000f * 29f;
            table[lvl - _constants.MinimumLevel] = 4420824 * (int)(Math.Pow(lvl - 1, 5.65f) + 150000f * (lvl - 1));
        }

        for (int lvl = 31; lvl <= _constants.MaximumLevel; lvl += 1)
        {
            table[lvl - _constants.MinimumLevel] = table[lvl - _constants.MinimumLevel - 1] * 2;
        }

        return table;
    }
}
