using Crpg.Domain.Common;
using Crpg.Domain.Entities.Users;

namespace Crpg.Domain.Entities;

/// <summary>
/// Represents a user ban. Used by a game server to know if a user is allowed to play.
/// </summary>
public class Ban : AuditableEntity
{
    public int Id { get; set; }
    public int BannedUserId { get; set; }

    /// <summary>
    /// Duration of the ban. <see cref="TimeSpan.Zero"/> stands for unban.
    /// </summary>
    /// <remarks>Add it <see cref="AuditableEntity.CreatedAt"/> to get the end of the ban.</remarks>
    public TimeSpan Duration { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int BannedByUserId { get; set; }

    public User? BannedUser { get; set; }
    public User? BannedByUser { get; set; }
}
