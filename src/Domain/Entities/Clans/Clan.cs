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
    /// Hex color of the clan (e.g. #FA60EE).
    /// </summary>
    public string Color { get; set; } = string.Empty;

    /// <summary>
    /// Full name of the clan.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    public IList<ClanMember> Members { get; set; } = new List<ClanMember>();
    public IList<ClanInvitation> Invitations { get; set; } = new List<ClanInvitation>();
}
