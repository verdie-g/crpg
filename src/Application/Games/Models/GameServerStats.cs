namespace Crpg.Application.Games.Models;

public record GameServerStats
{
    /// <summary>
    /// Number of users currently playing on servers.
    /// </summary>
    public int PlayingCount { get; set; }
}
