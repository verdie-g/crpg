using Crpg.Domain.Common;
using Crpg.Domain.Entities.Users;

namespace Crpg.Domain.Entities.Restrictions;

/// <summary>
/// Represents a user restriction. Used by a game server to know if a user is allowed to play/chat.
/// </summary>
public class Restriction : AuditableEntity
{
    public int Id { get; set; }
    public int RestrictedUserId { get; set; }

    /// <summary>
    /// Duration of the restriction. <see cref="TimeSpan.Zero"/> to cancel the past restriction.
    /// </summary>
    /// <remarks>Add it <see cref="AuditableEntity.CreatedAt"/> to get the end of the restriction.</remarks>
    public TimeSpan Duration { get; set; }
    public RestrictionType Type { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int RestrictedByUserId { get; set; }

    public User? RestrictedUser { get; set; }
    public User? RestrictedByUser { get; set; }
}
