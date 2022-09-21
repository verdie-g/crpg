using Crpg.Domain.Common;

namespace Crpg.Domain.Entities.Clans;

public class Clan : AuditableEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Short name of the clan.
    /// </summary>
    public string Tag { get; set; } = string.Empty;

    /// <summary>
    /// Primary color (ARGB32) of the clan.
    /// </summary>
    public uint PrimaryColor { get; set; }

    /// <summary>
    /// Secondary color (ARGB32) of the clan.
    /// </summary>
    public uint SecondaryColor { get; set; }

    /// <summary>
    /// Full name of the clan.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Bannerlord's banner key of the clan.
    /// </summary>
    public string BannerKey { get; set; } = string.Empty;

    /// <summary>
    /// Region of the clan.
    /// </summary>
    public Region Region { get; set; }

    public IList<ClanMember> Members { get; set; } = new List<ClanMember>();
    public IList<ClanInvitation> Invitations { get; set; } = new List<ClanInvitation>();
}
