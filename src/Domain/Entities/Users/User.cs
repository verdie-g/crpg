using Crpg.Domain.Common;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Restrictions;

namespace Crpg.Domain.Entities.Users;

public class User : AuditableEntity
{
    public int Id { get; set; }

    /// <summary>
    /// The platform (e.g. Steam) used to play Bannerlord.
    /// </summary>
    public Platform Platform { get; set; }
    public string PlatformUserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Gold { get; set; }
    public int HeirloomPoints { get; set; }
    public Role Role { get; set; }

    /// <summary>
    /// True if the user is donating on https://www.patreon.com/crpg.
    /// </summary>
    public bool IsDonor { get; set; }

    /// <summary>
    /// 32x32.
    /// </summary>
    public Uri? AvatarSmall { get; set; }

    /// <summary>
    /// 64x64.
    /// </summary>
    public Uri? AvatarMedium { get; set; }

    /// <summary>
    /// 184x184.
    /// </summary>
    public Uri? AvatarFull { get; set; }

    /// <summary>
    /// Character the user will play in game.
    /// </summary>
    public int? ActiveCharacterId { get; set; }

    /// <summary>
    /// Not null if the user deleted its account.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    public IList<UserItem> Items { get; set; } = new List<UserItem>();
    public IList<Character> Characters { get; set; } = new List<Character>();
    public Character? ActiveCharacter { get; set; }
    public IList<Restriction> Restrictions { get; set; } = new List<Restriction>();
    public ClanMember? ClanMembership { get; set; }
    public Party? Party { get; set; }
}
