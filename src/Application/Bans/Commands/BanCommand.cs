using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Bans.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Crpg.Application.Bans.Commands
{
    public class BanCommand : IMediatorRequest<BanViewModel>
    {
        public int BannedUserId { get; set; }
        public TimeSpan Duration { get; set; }
        public string Reason { get; set; } = default!;
        public int BannedByUserId { get; set; }

        public class Validator : AbstractValidator<BanCommand>
        {
            public Validator()
            {
                RuleFor(b => b.BannedUserId).Must((b, userId) => userId != b.BannedByUserId);
                RuleFor(b => b.Reason).NotEmpty();
            }
        }

        internal class Handler : IMediatorRequestHandler<BanCommand, BanViewModel>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;
            private readonly ILogger<BanCommand> _logger;

            public Handler(ICrpgDbContext db, IMapper mapper, ILogger<BanCommand> logger)
            {
                _db = db;
                _mapper = mapper;
                _logger = logger;
            }

            public async Task<Result<BanViewModel>> Handle(BanCommand req, CancellationToken cancellationToken)
            {
                var bannedUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == req.BannedUserId, cancellationToken);
                if (bannedUser == null)
                {
                    return new Result<BanViewModel>(CommonErrors.UserNotFound(req.BannedUserId));
                }

                var banningUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == req.BannedByUserId, cancellationToken);
                if (banningUser == null)
                {
                    return new Result<BanViewModel>(CommonErrors.UserNotFound(req.BannedByUserId));
                }

                var ban = new Ban
                {
                    BannedUser = bannedUser,
                    BannedByUser = banningUser,
                    Duration = req.Duration,
                    Reason = req.Reason,
                };

                bannedUser.Bans.Add(ban);
                await _db.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("User '{0}' banned user '{1}'", req.BannedByUserId, req.BannedUserId);
                return new Result<BanViewModel>(_mapper.Map<BanViewModel>(ban));
            }
        }
    }
}
