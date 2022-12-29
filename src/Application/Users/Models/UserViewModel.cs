using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Users;

namespace Crpg.Application.Users.Models;

public record UserViewModel : IMapFrom<User>
{
    public int Id { get; init; }
    public Platform Platform { get; init; }
    public string PlatformUserId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int Gold { get; init; }
    public int HeirloomPoints { get; init; }
    public float ExperienceMultiplier { get; init; }
    public Role Role { get; init; }
    public Region? Region { get; init; }
    public bool IsDonor { get; init; }
    public Uri? AvatarSmall { get; init; }
    public Uri? AvatarMedium { get; init; }
    public Uri? AvatarFull { get; init; }
    public int? ActiveCharacterId { get; init; }
}
