using Crpg.Application.Common.Mappings;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities;

namespace Crpg.Application.Bans.Models;

public record BanViewModel : IMapFrom<Ban>
{
    public int Id { get; init; }
    public UserPublicViewModel? BannedUser { get; init; }
    public TimeSpan Duration { get; init; }
    public string Reason { get; init; } = string.Empty;
    public UserPublicViewModel? BannedByUser { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
