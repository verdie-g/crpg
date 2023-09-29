using Crpg.Domain.Common;

namespace Crpg.Domain.Entities.Servers;

public class GameServerConfig : AuditableEntity
{
    // Common settings for all type of server
    public string ServerName { get; set; } = string.Empty;
    public Region Region { get; set; }
    public int ServerInstance { get; set; }
    public GameMode GameMode { get; set; }
    public string? GamePassword { get; set; }
    public string? WelcomeMessage { get; set; }
    public List<string> Maps { get; set; } = new List<string>();
    public bool? AllowPollsToKickPlayers { get; set; }
    public bool? AllowPollsToChangeMaps { get; set; }
    public bool? DisableCultureVoting { get; set; }
    public Culture CultureTeam1 { get; set; }
    public Culture CultureTeam2 { get; set; }
    public int MaxNumberOfPlayers { get; set; }
    public int? AutoTeamBalanceThreshold { get; set; }
    public int? FriendlyFireDamageMeleeFriendPercent { get; set; }
    public int? FriendlyFireDamageMeleeSelfPercent { get; set; }
    public int? FriendlyFireDamageRangedFriendPercent { get; set; }
    public int? FriendlyFireDamageRangedSelfPercent { get; set; }
    public int NumberOfBotsTeam1 { get; set; }
    public int NumberOfBotsTeam2 { get; set; }
    public int? MinNumberOfPlayersForMatchStart { get; set; } // Battle, Siege, TDeathmatch
    public int? RoundTotal { get; set; }
    public int? MapTimeLimit { get; set; }
    public int? RoundTimeLimit { get; set; }
    public int? WarmupTimeLimit { get; set; }
    public int? RoundPreparationTimeLimit { get; set; } // Battle, Conquest, DTV, Siege, Deathmatch
    public bool? Enable_automated_battle_switching { get; set; }

    // Conquest type of server settings
    public int? RespawnPeriodTeam1 { get; set; }
    public int? RespawnPeriodTeam2 { get; set; }
    public bool? Set_automated_battle_count { get; set; }

    // Duel type of server settings
    public int? MinScoreToWinDuel { get; set; }
    public bool? End_game_after_mission_is_over { get; set; }

    // cRPG specific settings
    public float? Crpg_team_balancer_clan_group_size_penalty { get; set; } // Apply a rating increase to members of the same clan that are playing in the same team
    public float? Crpg_experience_multiplier { get; set; } // Sets a reward multiplier for the server.
    public int? Crpg_reward_tick { get; set; } // Sets the reward tick duration in seconds for Conquest/Siege/Team Deatmatch.
    public bool? Crpg_team_balance_once { get; set; } // Sets if the team balancer should balance only after warmup.
    public TimeSpan? Crpg_happy_hours { get; set; } // Sets the happy hours. Format: HH:MM-HH:MM,TZ
    public bool? Crpg_apply_harmony_patches { get; set; } // Apply Harmony patches
}
