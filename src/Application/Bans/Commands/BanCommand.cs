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

        public class Handler : IMediatorRequestHandler<BanCommand, BanViewModel>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Result<BanViewModel>> Handle(BanCommand request, CancellationToken cancellationToken)
            {
                var bannedUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.BannedUserId, cancellationToken);
                if (bannedUser == null)
                {
                    return new Result<BanViewModel>(CommonErrors.UserNotFound(request.BannedUserId));
                }

                var banningUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.BannedByUserId, cancellationToken);
                if (banningUser == null)
                {
                    return new Result<BanViewModel>(CommonErrors.UserNotFound(request.BannedByUserId));
                }

                var ban = new Ban
                {
                    BannedUser = bannedUser,
                    BannedByUser = banningUser,
                    Duration = request.Duration,
                    Reason = request.Reason,
                };

                bannedUser.Bans.Add(ban);
                await _db.SaveChangesAsync(cancellationToken);
                return new Result<BanViewModel>(_mapper.Map<BanViewModel>(ban));
            }
        }
    }
}
