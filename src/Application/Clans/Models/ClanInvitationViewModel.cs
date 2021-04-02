using Crpg.Application.Common.Mappings;
using Crpg.Application.Users.Models;
using Crpg.Domain.Entities.Clans;

namespace Crpg.Application.Clans.Models
{
    public record ClanInvitationViewModel : IMapFrom<ClanInvitation>
    {
        public int Id { get; init; }
        public int ClanId { get; init; }
        public UserPublicViewModel InviteeUser { get; init; } = default!;
        public UserPublicViewModel InviterUser { get; init; } = default!;
        public ClanInvitationType Type { get; init; }
        public ClanInvitationStatus Status { get; init; }
    }
}
