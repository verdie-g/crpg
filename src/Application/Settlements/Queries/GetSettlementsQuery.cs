using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Settlements.Models;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Settlements.Queries
{
    public record GetSettlementsQuery : IMediatorRequest<IList<SettlementPublicViewModel>>
    {
        internal class Handler : IMediatorRequestHandler<GetSettlementsQuery, IList<SettlementPublicViewModel>>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Result<IList<SettlementPublicViewModel>>> Handle(GetSettlementsQuery req, CancellationToken cancellationToken)
            {
                var settlements = await _db.Settlements
                    .ProjectTo<SettlementPublicViewModel>(_mapper.ConfigurationProvider)
                    .ToArrayAsync(cancellationToken);

                return new(settlements);
            }
        }
    }
}
