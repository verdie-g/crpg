using System.Text.RegularExpressions;
using JetBrains.Annotations;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

public static class CrpgServerConfiguration
{
    public static void Init()
    {
        DedicatedServerConsoleCommandManager.AddType(typeof(CrpgServerConfiguration));
    }

    public static float ServerExperienceMultiplier { get; private set; } = 1.0f;
    public static Tuple<TimeSpan, TimeSpan>? ServerHappyHours { get; private set; }
    public static bool IsTournament { get; private set; }

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
    }

    [UsedImplicitly]
    [ConsoleCommandMethod("crpg_happy_hours", "Sets the happy local hours. Format: HH:MM-HH:MM")]
    private static void SetServerHappyHours(string? happHoursStr)
    {
        if (happHoursStr == null)
        {
            Debug.Print("Invalid happy hours: null");
            return;
        }

        Match match = Regex.Match(happHoursStr, "(\\d\\d:\\d\\d)-(\\d\\d:\\d\\d)");
        if (match.Groups.Count != 3
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

        ServerHappyHours = Tuple.Create(startTime, endTime);
    }

    [UsedImplicitly]
    [ConsoleCommandMethod("crpg_tournament", "Sets the server in tournament mode.")]
    private static void SetServerTournamentMode(string? enableStr)
    {
        if (enableStr == null || !bool.TryParse(enableStr, out bool enable))
        {
            Debug.Print($"Invalid tournament mode: {enableStr}");
            return;
        }

        IsTournament = enable;
    }
}
