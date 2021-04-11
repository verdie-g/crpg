using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Clans.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Clans.Commands
{
    /// <summary>
    /// Command to invite or request to join a clan.
    /// </summary>
    public record InviteClanMemberCommand : IMediatorRequest<ClanInvitationViewModel>
    {
        public int UserId { get; init; }
        public int ClanId { get; init; }
        public int InviteeId { get; init; }

        internal class Handler : IMediatorRequestHandler<InviteClanMemberCommand, ClanInvitationViewModel>
        {
            private static readonly ILogger Logger = LoggerFactory.CreateLogger<InviteClanMemberCommand>();

            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;
            private readonly IClanService _clanService;

            public Handler(ICrpgDbContext db, IMapper mapper, IClanService clanService)
            {
                _db = db;
                _mapper = mapper;
                _clanService = clanService;
            }

            public async Task<Result<ClanInvitationViewModel>> Handle(InviteClanMemberCommand req, CancellationToken cancellationToken)
            {
                var inviter = await _db.Users
                    .Include(u => u.ClanMembership)
                    .FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);
                if (inviter == null)
                {
                    return new(CommonErrors.UserNotFound(req.UserId));
                }

                if (req.InviteeId == req.UserId)
                {
                    return await RequestToJoinClan(inviter, req.ClanId, cancellationToken);
                }

                return await InviteToClan(inviter, req.ClanId, req.InviteeId, cancellationToken);
            }

            private async Task<Result<ClanInvitationViewModel>> RequestToJoinClan(User user, int clanId,
                CancellationToken cancellationToken)
            {
                if (user.ClanMembership != null && user.ClanMembership.ClanId == clanId)
                {
                    return new(CommonErrors.UserAlreadyInTheClan(user.Id, clanId));
                }

                // Check if an invitation already exists.
                var invitation = await _db.ClanInvitations.FirstOrDefaultAsync(ci =>
                    ci.ClanId == clanId && ci.InviteeId == user.Id && ci.Type == ClanInvitationType.Request
                    && ci.Status == ClanInvitationStatus.Pending, cancellationToken);
                if (invitation != null)
                {
                    // There is already a pending request to join the clan.
                    return new(_mapper.Map<ClanInvitationViewModel>(invitation));
                }

                invitation = new ClanInvitation
                {
                    ClanId = clanId,
                    InviteeId = user.Id,
                    InviterId = user.Id,
                    Type = ClanInvitationType.Request,
                    Status = ClanInvitationStatus.Pending,
                };
                _db.ClanInvitations.Add(invitation);
                await _db.SaveChangesAsync(cancellationToken);
                Logger.LogInformation("User '{0}' requested to join clan '{1}'", user.Id, clanId);
                return new(_mapper.Map<ClanInvitationViewModel>(invitation));
            }

            private async Task<Result<ClanInvitationViewModel>> InviteToClan(User inviter, int clanId, int inviteeId,
                CancellationToken cancellationToken)
            {
                var invitee = await _db.Users
                    .Include(u => u.ClanMembership)
                    .FirstOrDefaultAsync(u => u.Id == inviteeId, cancellationToken);
                if (invitee == null)
                {
                    return new(CommonErrors.UserNotFound(inviteeId));
                }

                if (invitee.ClanMembership != null && invitee.ClanMembership.ClanId == clanId)
                {
                    return new(CommonErrors.UserAlreadyInTheClan(invitee.Id, clanId));
                }

                var error = _clanService.CheckClanMembership(inviter, clanId);
                if (error != null)
                {
                    return new(error);
                }

                if (inviter.ClanMembership!.Role != ClanMemberRole.Admin && inviter.ClanMembership.Role != ClanMemberRole.Leader)
                {
                    return new(
                        CommonErrors.ClanMemberRoleNotMet(inviter.Id, ClanMemberRole.Admin, ClanMemberRole.Member));
                }

                // Check if an invitation already exists.
                var invitation = await _db.ClanInvitations.FirstOrDefaultAsync(ci =>
                    ci.ClanId == clanId && ci.InviterId == inviter.Id && ci.Type == ClanInvitationType.Offer
                    && ci.Status == ClanInvitationStatus.Pending, cancellationToken);
                if (invitation != null)
                {
                    // There is already a pending offer to join the clan.
                    return new(_mapper.Map<ClanInvitationViewModel>(invitation));
                }

                invitation = new ClanInvitation
                {
                    ClanId = clanId,
                    InviteeId = inviteeId,
                    InviterId = inviter.Id,
                    Type = ClanInvitationType.Offer,
                    Status = ClanInvitationStatus.Pending,
                };
                _db.ClanInvitations.Add(invitation);
                await _db.SaveChangesAsync(cancellationToken);
                Logger.LogInformation("User '{0}' offered user '{1}' to join clan '{2}'", inviter.Id, clanId, inviteeId);
                return new(_mapper.Map<ClanInvitationViewModel>(invitation));
            }
        }
    }
}
