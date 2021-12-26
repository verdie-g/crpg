namespace Crpg.Domain.Entities.Clans;

/// <summary>
/// Type of a <see cref="ClanInvitation"/>.
/// </summary>
public enum ClanInvitationType
{
    /// <summary>
    /// User requested to join a clan.
    /// </summary>
    Request,

    /// <summary>
    /// User was of offered to join a clan.
    /// </summary>
    Offer,
}
