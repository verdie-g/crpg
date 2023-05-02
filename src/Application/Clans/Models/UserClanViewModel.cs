using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Clans;

namespace Crpg.Application.Clans.Models;
public record UserClanViewModel : IMapFrom<ClanMember>
{
    public ClanViewModel Clan { get; init; } = default!;
    public ClanMemberRole Role { get; init; }
}
