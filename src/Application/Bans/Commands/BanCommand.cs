using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Bans.Models;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Common.Interfaces;
using Crpg.Common;
using Crpg.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Bans.Commands
{
    public class BanCommand : IRequest<BanViewModel>
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

        public class Handler : IRequestHandler<BanCommand, BanViewModel>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;
            private readonly IDateTime _dateTime;

            public Handler(ICrpgDbContext db, IMapper mapper, IDateTime dateTime)
            {
                _db = db;
                _mapper = mapper;
                _dateTime = dateTime;
            }

            public async Task<BanViewModel> Handle(BanCommand request, CancellationToken cancellationToken)
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == request.BannedUserId, cancellationToken);
                if (user == null)
                {
                    throw new NotFoundException(nameof(User), request.BannedUserId);
                }

                var ban = new Ban
                {
                    BannedUserId = request.BannedUserId,
                    BannedByUserId = request.BannedByUserId,
                    Until = _dateTime.Now + request.Duration,
                    Reason = request.Reason,
                };

                user.Bans.Add(ban);
                await _db.SaveChangesAsync(cancellationToken);
                return _mapper.Map<BanViewModel>(ban);
            }
        }
    }
}