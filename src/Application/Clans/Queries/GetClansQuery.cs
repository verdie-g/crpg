using System.Collections.Generic;
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
    public record GetClansQuery : IMediatorRequest<IList<ClanViewModel>>
    {
        public int ClanId { get; set; }

        internal class Handler : IMediatorRequestHandler<GetClansQuery, IList<ClanViewModel>>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Result<IList<ClanViewModel>>> Handle(GetClansQuery req, CancellationToken cancellationToken)
            {
                var clans = await _db.Clans
                    .ProjectTo<ClanViewModel>(_mapper.ConfigurationProvider)
                    .ToArrayAsync(cancellationToken);
                return new(clans);
            }
        }
    }
}
