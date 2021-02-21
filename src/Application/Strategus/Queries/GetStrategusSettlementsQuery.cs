using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Strategus.Models;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Strategus.Queries
{
    public class GetStrategusSettlementsQuery : IMediatorRequest<IList<StrategusSettlementViewModel>>
    {
        internal class Handler : IMediatorRequestHandler<GetStrategusSettlementsQuery, IList<StrategusSettlementViewModel>>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Result<IList<StrategusSettlementViewModel>>> Handle(GetStrategusSettlementsQuery req, CancellationToken cancellationToken)
            {
                var clan = await _db.StrategusSettlements
                    .ProjectTo<StrategusSettlementViewModel>(_mapper.ConfigurationProvider)
                    .ToArrayAsync(cancellationToken);

                return new Result<IList<StrategusSettlementViewModel>>(clan);
            }
        }
    }
}
