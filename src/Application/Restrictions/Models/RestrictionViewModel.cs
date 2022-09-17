using Crpg.Application.Common.Mappings;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities.Restrictions;

namespace Crpg.Application.Restrictions.Models;

public record RestrictionViewModel : IMapFrom<Restriction>
{
    public int Id { get; init; }
    public UserPublicViewModel? RestrictedUser { get; init; }
    public TimeSpan Duration { get; init; }
    public RestrictionType Type { get; init; }
    public string Reason { get; init; } = string.Empty;
    public UserPublicViewModel? RestrictedByUser { get; init; }
    public DateTime CreatedAt { get; init; }
}
