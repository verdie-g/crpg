using System.Text;
using JetBrains.Annotations;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

internal static class CrpgFeatureFlags
{
    public const string FeatureTournament = "tournament";
    public const string FeatureItemsTeamBalance = "items_team_balance";

    private static readonly Dictionary<string, bool> Features = new()
    {
        [FeatureTournament] = false,
        [FeatureItemsTeamBalance] = true,
    };

    public static void Init()
    {
        DedicatedServerConsoleCommandManager.AddType(typeof(CrpgFeatureFlags));
    }

    public static bool IsEnabled(string feature)
    {
        return Features.TryGetValue(feature, out bool enabled) && enabled;
    }

    [UsedImplicitly]
    [ConsoleCommandMethod("crpg_enable_feature", "Enables a cRPG feature")]
    private static void EnableFeature(string? feature)
    {
        EnableFeature(feature, true);
    }

    [UsedImplicitly]
    [ConsoleCommandMethod("crpg_disable_feature", "Disables a cRPG feature")]
    private static void DisableFeature(string? feature)
    {
        EnableFeature(feature, false);
    }

    [UsedImplicitly]
    [ConsoleCommandMethod("crpg_list_features", "Lists cRPG feature")]
    private static void ListFeatures()
    {
        StringBuilder sb = new();
        sb.AppendLine("cRPG features:");
        int featureNameMaxLength = Features.Select(f => f.Key.Length).Max();
        foreach (var feature in Features)
        {
            sb.AppendLine($"{feature.Key.PadRight(featureNameMaxLength)} {(feature.Value ? "ENABLED" : "DISABLED")}");
        }

        Debug.Print(sb.ToString());
    }

    private static void EnableFeature(string? feature, bool enable)
    {
        if (feature == null || !Features.ContainsKey(feature))
        {
            Debug.Print($"Unknown feature '{feature}'");
            return;
        }

        Features[feature] = enable;
        Debug.Print($"Feature '{feature}' {(enable ? "enabled" : "disabled")}");
    }
}
