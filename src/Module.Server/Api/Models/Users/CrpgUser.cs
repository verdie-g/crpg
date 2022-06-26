using Crpg.Module.Api.Models.Characters;

namespace Crpg.Module.Api.Models.Users;

// Copy of Crpg.Application.Games.Models.GameUser
internal class CrpgUser
{
    public int Id { get; set; }
    public Platform Platform { get; set; }
    public string PlatformUserId { get; set; } = string.Empty;
    public int Gold { get; set; }
    public CrpgCharacter Character { get; set; } = default!;
    public CrpgBan? Ban { get; set; }
}
