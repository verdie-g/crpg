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
using FluentValidation;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Clans.Commands
{
    public record UpdateClanMemberCommand : IMediatorRequest<ClanMemberViewModel>
    {
        public int UserId { get; init; }
        public int ClanId { get; init; }
        public int MemberId { get; init; }
        public ClanMemberRole Role { get; init; }

        public class Validator : AbstractValidator<UpdateClanMemberCommand>
        {
            public Validator()
            {
                RuleFor(cmd => cmd.Role).IsInEnum();
            }
        }

        internal class Handler : IMediatorRequestHandler<UpdateClanMemberCommand, ClanMemberViewModel>
        {
            private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpdateClanMemberCommand>();

            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;
            private readonly IClanService _clanService;

            public Handler(ICrpgDbContext db, IMapper mapper, IClanService clanService)
            {
                _db = db;
                _mapper = mapper;
                _clanService = clanService;
            }

            public async Task<Result<ClanMemberViewModel>> Handle(UpdateClanMemberCommand req, CancellationToken cancellationToken)
            {
                var userRes = await _clanService.GetClanMember(_db, req.UserId, req.ClanId, cancellationToken);
                if (userRes.Errors != null)
                {
                    return new(userRes.Errors);
                }

                User user = userRes.Data!;
                if (user.ClanMembership!.Role != ClanMemberRole.Leader)
                {
                    return new(CommonErrors.ClanMemberRoleNotMet(req.UserId, ClanMemberRole.Leader,
                        user.ClanMembership.Role));
                }

                var toUpdateUserRes = await _clanService.GetClanMember(_db, req.MemberId, req.ClanId, cancellationToken);
                if (toUpdateUserRes.Errors != null)
                {
                    return new(toUpdateUserRes.Errors);
                }

                User toUpdateUser = toUpdateUserRes.Data!;
                if (user.Id == toUpdateUser.Id) // Leader cannot change its own role.
                {
                    return new(CommonErrors.ClanMemberRoleNotMet(req.UserId, ClanMemberRole.Officer,
                        user.ClanMembership.Role));
                }

                toUpdateUser.ClanMembership!.Role = req.Role;

                await _db.SaveChangesAsync(cancellationToken);
                Logger.LogInformation("User '{0}' updated member '{1}' from clan '{2}'", req.UserId,
                    req.MemberId, req.ClanId);
                return new(_mapper.Map<ClanMemberViewModel>(toUpdateUser.ClanMembership));
            }
        }
    }
}
