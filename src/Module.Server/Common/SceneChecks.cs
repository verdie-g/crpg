using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Objects;

namespace Crpg.Module.Common;

internal static class SceneChecks
{
    /// <summary>Checks the flag index are from 0 to N.</summary>
    public static void ThrowOnBadFlagIndexes(IReadOnlyList<FlagCapturePoint> flags)
    {
        int expectedIndex = 0;
        foreach (var flag in flags.OrderBy(f => f.FlagIndex))
        {
            if (flag.FlagIndex != expectedIndex)
            {
                throw new Exception($"Invalid scene '{Mission.Current?.SceneName}': Flag indexes should be numbered from 0 to {flags.Count}");
            }

            expectedIndex += 1;
        }
    }
}
