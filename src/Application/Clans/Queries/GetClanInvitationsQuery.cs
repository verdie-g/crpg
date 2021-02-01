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
    public class GetClanInvitationsQuery : IMediatorRequest<IList<ClanInvitationViewModel>>
    {
        public int UserId { get; set; }
        public int ClanId { get; set; }
        public IList<ClanInvitationStatus> Statuses { get; set; } = Array.Empty<ClanInvitationStatus>();

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
                    return new Result<IList<ClanInvitationViewModel>>(CommonErrors.UserNotFound(req.UserId));
                }

                var error = _clanService.CheckClanMembership(user, req.ClanId);
                if (error != null)
                {
                    return new Result<IList<ClanInvitationViewModel>>(error);
                }

                if (user.ClanMembership!.Role != ClanMemberRole.Admin && user.ClanMembership.Role != ClanMemberRole.Leader)
                {
                    return new Result<IList<ClanInvitationViewModel>>(CommonErrors.ClanMemberRoleNotMet(
                        user.Id, ClanMemberRole.Admin, user.ClanMembership.Role));
                }

                var invitations = await _db.ClanInvitations
                    .Where(ci => ci.ClanId == req.ClanId && (req.Statuses.Count == 0 || req.Statuses.Contains(ci.Status)))
                    .OrderByDescending(ci => ci.UpdatedAt)
                    .ProjectTo<ClanInvitationViewModel>(_mapper.ConfigurationProvider)
                    .ToArrayAsync(cancellationToken);

                return new Result<IList<ClanInvitationViewModel>>(invitations);
            }
        }
    }
}
