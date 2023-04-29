using Crpg.Application.Common.Mappings;

namespace Crpg.Application.Clans.Models;
using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Clans;


public record UserClanViewModel : IMapFrom<ClanMember>
{
    public ClanViewModel Clan { get; init; } = default!;
    public ClanMemberRole Role { get; init; }
}
