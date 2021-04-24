using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Clans.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Clans.Queries
{
    public record GetClansQuery : IMediatorRequest<IList<ClanWithMemberCountViewModel>>
    {
        internal class Handler : IMediatorRequestHandler<GetClansQuery, IList<ClanWithMemberCountViewModel>>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Result<IList<ClanWithMemberCountViewModel>>> Handle(GetClansQuery req, CancellationToken cancellationToken)
            {
                var clans = await _db.Clans
                    .ProjectTo<ClanWithMemberCountViewModel>(_mapper.ConfigurationProvider)
                    .OrderByDescending(c => c.MemberCount)
                    .ToArrayAsync(cancellationToken);
                return new(clans);
            }
        }
    }
}
