using System.Text.RegularExpressions;
using Crpg.Module.Api.Models;
using Crpg.Module.HarmonyPatches;
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
    public static float TeamBalancerClanGroupSizePenalty { get; private set; } = 0f;
    public static float ServerExperienceMultiplier { get; private set; } = 1.0f;
    public static int RewardTick { get; private set; } = 60;
    public static bool TeamBalanceOnce { get; private set; }
    public static bool FrozenBots { get; private set; } = false;
    public static Tuple<TimeSpan, TimeSpan, TimeZoneInfo>? HappyHours { get; private set; }

    [UsedImplicitly]
    [ConsoleCommandMethod("crpg_team_balancer_clan_group_size_penalty", "Apply a rating increase to members of the same clan that are playing in the same team")]
    private static void SetClanGroupSizePenalty(string? sizePenaltyStr)
    {
        if (sizePenaltyStr == null
            || !float.TryParse(sizePenaltyStr, out float sizePenalty)
            || sizePenalty > 1.5f)
        {
            Debug.Print($"Invalid team balancer clangroup size penalty: {sizePenaltyStr}");
            return;
        }

        TeamBalancerClanGroupSizePenalty = sizePenalty;
        Debug.Print($"Set ClanGroup Size Penalty to {sizePenalty}");
    }

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
    private static void SetRewardTick(string? rewardTickStr)
    {
        if (rewardTickStr == null
            || !int.TryParse(rewardTickStr, out int rewardTick)
            || rewardTick < 10
            || rewardTick > 1000)
        {
            Debug.Print($"Invalid reward tick: {rewardTickStr}");
            return;
        }

        RewardTick = rewardTick;
        Debug.Print($"Set reward tick to {rewardTick}");
    }

    [UsedImplicitly]
    [ConsoleCommandMethod("crpg_team_balance_once", "Sets if the team balancer should balance only after warmup.")]
    private static void SetTeamBalanceOnce(string? teamBalanceOnceStr)
    {
        if (teamBalanceOnceStr == null
            || !bool.TryParse(teamBalanceOnceStr, out bool teamBalanceOnce))
        {
            Debug.Print($"Invalid team balance once: {teamBalanceOnceStr}");
            return;
        }

        TeamBalanceOnce = teamBalanceOnce;
        Debug.Print($"Set team balance once to {teamBalanceOnce}");
    }
    [UsedImplicitly]
    [ConsoleCommandMethod("crpg_frozen_bots", "Sets the Alarmed status of bots to off.")]
    private static void SetFrozenBots(string? frozenBotsStr)
    {
        if (frozenBotsStr == null
            || !bool.TryParse(frozenBotsStr, out bool frozenBots))
        {
            Debug.Print($"Invalid Frozen Bots: {frozenBotsStr}");
            return;
        }

        FrozenBots = frozenBots;
        Debug.Print($"Set team balance once to {frozenBots}");
    }
    [UsedImplicitly]
    [ConsoleCommandMethod("crpg_happy_hours", "Sets the happy hours. Format: HH:MM-HH:MM,TZ")]
    private static void SetHappyHours(string? happHoursStr)
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
        HappyHours = Tuple.Create(startTime, endTime, timeZone);
        Debug.Print($"Set happy hours from {startTime} to {endTime} in time zone {timeZone.Id}");
    }

    [UsedImplicitly]
    [ConsoleCommandMethod("crpg_apply_harmony_patches", "Apply Harmony patches")]
    private static void ApplyHarmonyPatches()
    {
        BannerlordPatches.Apply();
    }
}
