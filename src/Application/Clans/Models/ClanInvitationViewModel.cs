using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Clans;

namespace Crpg.Application.Clans.Models
{
    public class ClanInvitationViewModel : IMapFrom<ClanInvitation>
    {
        public int Id { get; set; }
        public int ClanId { get; set; }
        public int InviteeUserId { get; set; }
        public int InviterUserId { get; set; }
        public ClanInvitationType Type { get; set; }
        public ClanInvitationStatus Status { get; set; }
    }
}
