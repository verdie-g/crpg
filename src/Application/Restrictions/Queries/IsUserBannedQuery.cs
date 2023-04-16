using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Restrictions;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Restrictions.Queries;

public record IsUserBannedQuery : IMediatorRequest<bool>
{
    public int UserId { get; init; }

    internal class Handler : IMediatorRequestHandler<IsUserBannedQuery, bool>
    {
        private readonly ICrpgDbContext _db;
        private readonly IDateTime _dateTime;

        public Handler(ICrpgDbContext db, IDateTime dateTime)
        {
            _db = db;
            _dateTime = dateTime;
        }

        public async Task<Result<bool>> Handle(IsUserBannedQuery request, CancellationToken cancellationToken)
        {
            var lastAllRestriction = await _db.Restrictions
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync(r => r.RestrictedUserId == request.UserId
                                          && r.Type == RestrictionType.All,
                    cancellationToken);
            if (lastAllRestriction == null)
            {
                return new(false);
            }

            bool active = _dateTime.UtcNow < lastAllRestriction.CreatedAt + lastAllRestriction.Duration;
            return new(active);
        }
    }
}
