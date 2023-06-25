using Crpg.Application.Common.Mappings;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities.Clans;

namespace Crpg.Application.Clans.Models;

public record ClanMemberViewModel : IMapFrom<ClanMember>
{
    public UserPublicViewModel User { get; init; } = default!;
    public ClanMemberRole Role { get; init; }
    public Clan? Clan { get; set; }
}
