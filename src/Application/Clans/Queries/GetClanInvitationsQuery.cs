using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Clans.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Clans;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Clans.Queries
{
    public record GetClanInvitationsQuery : IMediatorRequest<IList<ClanInvitationViewModel>>
    {
        public int UserId { get; init; }
        public int ClanId { get; init; }
        public IList<ClanInvitationType> Types { get; init; } = Array.Empty<ClanInvitationType>();
        public IList<ClanInvitationStatus> Statuses { get; init; } = Array.Empty<ClanInvitationStatus>();

        internal class Handler : IMediatorRequestHandler<GetClanInvitationsQuery, IList<ClanInvitationViewModel>>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;
            private readonly IClanService _clanService;

            public Handler(ICrpgDbContext db, IMapper mapper, IClanService clanService)
            {
                _db = db;
                _mapper = mapper;
                _clanService = clanService;
            }

            public async Task<Result<IList<ClanInvitationViewModel>>> Handle(GetClanInvitationsQuery req, CancellationToken cancellationToken)
            {
                var user = await _db.Users
                    .Include(u => u.ClanMembership)
                    .FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);
                if (user == null)
                {
                    return new(CommonErrors.UserNotFound(req.UserId));
                }

                var error = _clanService.CheckClanMembership(user, req.ClanId);
                if (error != null)
                {
                    return new(error);
                }

                if (user.ClanMembership!.Role != ClanMemberRole.Admin && user.ClanMembership.Role != ClanMemberRole.Leader)
                {
                    return new(CommonErrors.ClanMemberRoleNotMet(
                        user.Id, ClanMemberRole.Admin, user.ClanMembership.Role));
                }

                var invitations = await _db.ClanInvitations
                    .Include(ci => ci.Invitee)
                    .Include(ci => ci.Inviter)
                    .Where(ci => ci.ClanId == req.ClanId
                                 && (req.Types.Count == 0 || req.Types.Contains(ci.Type))
                                 && (req.Statuses.Count == 0 || req.Statuses.Contains(ci.Status)))
                    .OrderByDescending(ci => ci.UpdatedAt)
                    .ProjectTo<ClanInvitationViewModel>(_mapper.ConfigurationProvider)
                    .ToArrayAsync(cancellationToken);

                return new(invitations);
            }
        }
    }
}
