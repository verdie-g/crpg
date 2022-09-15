using Crpg.Application.Common.Mappings;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities.Clans;

namespace Crpg.Application.Clans.Models;

public record GameClanMemberViewModel : IMapFrom<ClanMember>
{
    public int ClanId { get; set; }
    public ClanMemberRole Role { get; set; }
}
