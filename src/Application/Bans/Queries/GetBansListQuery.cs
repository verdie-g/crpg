using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Bans.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Bans.Queries
{
    public class GetBansListQuery : IMediatorRequest<IList<BanViewModel>>
    {
        internal class Handler : IMediatorRequestHandler<GetBansListQuery, IList<BanViewModel>>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Result<IList<BanViewModel>>> Handle(GetBansListQuery request, CancellationToken cancellationToken)
            {
                // the whole bans table is loaded. Acceptable since only admins can access this resource
                return new Result<IList<BanViewModel>>(await _db.Bans
                    .Include(b => b.BannedUser)
                    .Include(b => b.BannedByUser)
                    .ProjectTo<BanViewModel>(_mapper.ConfigurationProvider)
                    .ToArrayAsync(cancellationToken));
            }
        }
    }
}
