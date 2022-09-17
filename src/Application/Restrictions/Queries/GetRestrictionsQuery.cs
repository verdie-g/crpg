using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Restrictions.Models;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Restrictions.Queries;

public record GetRestrictionsQuery : IMediatorRequest<IList<RestrictionViewModel>>
{
    internal class Handler : IMediatorRequestHandler<GetRestrictionsQuery, IList<RestrictionViewModel>>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<IList<RestrictionViewModel>>> Handle(GetRestrictionsQuery request, CancellationToken cancellationToken)
        {
            // the whole restrictions table is loaded. Acceptable since only admins can access this resource
            return new(await _db.Restrictions
                .Include(r => r.RestrictedUser)
                .Include(r => r.RestrictedByUser)
                .ProjectTo<RestrictionViewModel>(_mapper.ConfigurationProvider)
                .ToArrayAsync(cancellationToken));
        }
    }
}
