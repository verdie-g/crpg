using Crpg.Application.Common.Mappings;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities.Clans;

namespace Crpg.Application.Clans.Models
{
    public class ClanInvitationViewModel : IMapFrom<ClanInvitation>
    {
        public int Id { get; set; }
        public int ClanId { get; set; }
        public UserPublicViewModel InviteeUser { get; set; } = default!;
        public UserPublicViewModel InviterUser { get; set; } = default!;
        public ClanInvitationType Type { get; set; }
        public ClanInvitationStatus Status { get; set; }
    }
}
