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
    public static Tuple<TimeSpan, TimeSpan>? ServerPrimeTime { get; private set; }

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
    [ConsoleCommandMethod("crpg_prime_time", "Sets the prime time local hours. Format: HH:MM-HH:MM")]
    private static void SetServerPrimeTime(string? primeTimeStr)
    {
        if (primeTimeStr == null)
        {
            Debug.Print("Invalid server multiplier: null");
            return;
        }

        Match match = Regex.Match(primeTimeStr, "(\\d\\d:\\d\\d)-(\\d\\d:\\d\\d)");
        if (match.Groups.Count != 3
            || !TimeSpan.TryParse(match.Groups[1].Value, out var startTime)
            || startTime < TimeSpan.Zero
            || startTime > TimeSpan.FromHours(24)
            || !TimeSpan.TryParse(match.Groups[2].Value, out var endTime)
            || endTime < TimeSpan.Zero
            || endTime > TimeSpan.FromHours(24))
        {
            Debug.Print($"Invalid server multiplier: {primeTimeStr}");
            return;
        }

        ServerPrimeTime = Tuple.Create(startTime, endTime);
    }
}
