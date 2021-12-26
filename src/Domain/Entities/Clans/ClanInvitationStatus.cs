namespace Crpg.Domain.Entities.Clans;

/// <summary>
/// Status on a <see cref="ClanInvitation"/>.
/// </summary>
public enum ClanInvitationStatus
{
    /// <summary>
    /// <see cref="ClanInvitation"/> is waiting for a response.
    /// </summary>
    Pending,

    /// <summary>
    /// <see cref="ClanInvitation"/> was declined by a clan member.
    /// </summary>
    Declined,

    /// <summary>
    /// <see cref="ClanInvitation"/> was accepted by a clan member.
    /// </summary>
    Accepted,
}
