using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Restrictions.Models;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Restrictions.Queries;

public record GetUserRestrictionsQuery : IMediatorRequest<IList<RestrictionViewModel>>
{
    public int UserId { get; init; }

    internal class Handler : IMediatorRequestHandler<GetUserRestrictionsQuery, IList<RestrictionViewModel>>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<IList<RestrictionViewModel>>> Handle(GetUserRestrictionsQuery request, CancellationToken cancellationToken)
        {
            var user = await _db.Users
                .AsNoTracking()
                .Include(u => u.Restrictions).ThenInclude(r => r.RestrictedByUser)
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            return user == null
                ? new(CommonErrors.UserNotFound(request.UserId))
                : new(_mapper.Map<RestrictionViewModel[]>(user.Restrictions));
        }
    }
}
