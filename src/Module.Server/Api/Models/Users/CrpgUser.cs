using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Api.Models.Clans;
using Crpg.Module.Api.Models.Restrictions;

namespace Crpg.Module.Api.Models.Users;

// Copy of Crpg.Application.Games.Models.GameUserViewModel
internal class CrpgUser
{
    public int Id { get; set; }
    public Platform Platform { get; set; }
    public string PlatformUserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Gold { get; set; }
    public CrpgUserRole Role { get; set; }
    public CrpgRegion? Region { get; set; }
    public DateTime CreatedAt { get; set; }
    public CrpgCharacter Character { get; set; } = default!;
    public IList<CrpgRestriction> Restrictions { get; set; } = Array.Empty<CrpgRestriction>();
    public CrpgClanMember? ClanMembership { get; set; }
}
