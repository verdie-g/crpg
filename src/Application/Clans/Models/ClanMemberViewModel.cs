using Crpg.Application.Common.Mappings;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities.Clans;

namespace Crpg.Application.Clans.Models
{
    public class ClanMemberViewModel : IMapFrom<ClanMember>
    {
        public UserPublicViewModel User { get; set; } = default!;
        public ClanMemberRole Role { get; set; }
    }
}
