using System.Text.RegularExpressions;
using Crpg.Module.Api.Models;
using JetBrains.Annotations;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

internal static class CrpgServerConfiguration
{
    static CrpgServerConfiguration()
    {
        string? regionStr = Environment.GetEnvironmentVariable("CRPG_REGION");
        Region = Enum.TryParse(regionStr, ignoreCase: true, out CrpgRegion region) ? region : CrpgRegion.Eu;
        Service = Environment.GetEnvironmentVariable("CRPG_SERVICE") ?? "unknown-service";
        Instance = Environment.GetEnvironmentVariable("CRPG_INSTANCE") ?? "unknown-instance";
    }

    public static void Init()
    {
        DedicatedServerConsoleCommandManager.AddType(typeof(CrpgServerConfiguration));
    }

    public static CrpgRegion Region { get; }
    public static string Service { get; }
    public static string Instance { get; }
    public static float ServerExperienceMultiplier { get; private set; } = 1.0f;
    public static int ServerRewardTick { get; private set; } = 60;
    public static Tuple<TimeSpan, TimeSpan, TimeZoneInfo>? ServerHappyHours { get; private set; }

    [UsedImplicitly]
    [ConsoleCommandMethod("crpg_experience_multiplier", "Sets a reward multiplier for the server.")]
    private static void SetServerExperienceMultiplier(string? multiplierStr)
    {
        if (multiplierStr == null
            || !float.TryParse(multiplierStr, out float multiplier)
            || multiplier > 5f)
        {
            Debug.Print($"Invalid server multiplier: {multiplierStr}");
            return;
        }

        ServerExperienceMultiplier = multiplier;
        Debug.Print($"Set server multiplier to {multiplier}");
    }

    [UsedImplicitly]
    [ConsoleCommandMethod("crpg_reward_tick", "Sets the reward tick duration in seconds for Conquest/Siege/Team Deatmatch.")]
    private static void SetServerRewardTick(string? rewardTickStr)
    {
        if (rewardTickStr == null
            || !int.TryParse(rewardTickStr, out int rewardTick)
            || rewardTick < 10
            || rewardTick > 1000)
        {
            Debug.Print($"Invalid reward tick: {rewardTickStr}");
            return;
        }

        ServerRewardTick = rewardTick;
        Debug.Print($"Set reward tick to {rewardTick}");
    }

    [UsedImplicitly]
    [ConsoleCommandMethod("crpg_happy_hours", "Sets the happy hours. Format: HH:MM-HH:MM,TZ")]
    private static void SetServerHappyHours(string? happHoursStr)
    {
        if (happHoursStr == null)
        {
            Debug.Print("Invalid happy hours: null");
            return;
        }

        Match match = Regex.Match(happHoursStr, "^(\\d\\d:\\d\\d)-(\\d\\d:\\d\\d),([\\w/ ]+)$");
        if (match.Groups.Count != 4
            || !TimeSpan.TryParse(match.Groups[1].Value, out var startTime)
            || startTime < TimeSpan.Zero
            || startTime > TimeSpan.FromHours(24)
            || !TimeSpan.TryParse(match.Groups[2].Value, out var endTime)
            || endTime < TimeSpan.Zero
            || endTime > TimeSpan.FromHours(24))
        {
            Debug.Print($"Invalid happy hours: {happHoursStr}");
            return;
        }

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(match.Groups[3].Value);
        ServerHappyHours = Tuple.Create(startTime, endTime, timeZone);
        Debug.Print($"Set happy hours from {startTime} to {endTime} in time zone {timeZone.Id}");
    }
}
