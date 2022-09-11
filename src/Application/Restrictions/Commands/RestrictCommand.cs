using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Restrictions.Models;
using Crpg.Domain.Entities.Restrictions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Restrictions.Commands;

public record RestrictCommand : IMediatorRequest<RestrictionViewModel>
{
    public int RestrictedUserId { get; init; }
    public TimeSpan Duration { get; init; }
    public RestrictionType Type { get; init; }
    public string Reason { get; init; } = default!;
    public int RestrictedByUserId { get; init; }

    public class Validator : AbstractValidator<RestrictCommand>
    {
        public Validator()
        {
            RuleFor(r => r.RestrictedUserId).Must((b, userId) => userId != b.RestrictedByUserId);
            RuleFor(r => r.Type).IsInEnum();
            RuleFor(r => r.Reason).NotEmpty();
        }
    }

    internal class Handler : IMediatorRequestHandler<RestrictCommand, RestrictionViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<RestrictCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<RestrictionViewModel>> Handle(RestrictCommand req, CancellationToken cancellationToken)
        {
            var restrictedUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == req.RestrictedUserId, cancellationToken);
            if (restrictedUser == null)
            {
                return new(CommonErrors.UserNotFound(req.RestrictedUserId));
            }

            var restrictingUser = await _db.Users.FirstOrDefaultAsync(u => u.Id == req.RestrictedByUserId, cancellationToken);
            if (restrictingUser == null)
            {
                return new(CommonErrors.UserNotFound(req.RestrictedByUserId));
            }

            Restriction restriction = new()
            {
                RestrictedUser = restrictedUser,
                RestrictedByUser = restrictingUser,
                Duration = req.Duration,
                Type = req.Type,
                Reason = req.Reason,
            };

            restrictedUser.Restrictions.Add(restriction);
            await _db.SaveChangesAsync(cancellationToken);

            Logger.LogInformation("User '{0}' restricted '{1}' for user '{2}'",
                req.RestrictedByUserId, req.Type, req.RestrictedUserId);
            return new(_mapper.Map<RestrictionViewModel>(restriction));
        }
    }
}
