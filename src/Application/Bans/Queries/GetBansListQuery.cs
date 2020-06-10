using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Bans.Models;
using Crpg.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Bans.Queries
{
    public class GetBansListQuery : IRequest<IList<BanViewModel>>
    {
        public class Handler : IRequestHandler<GetBansListQuery, IList<BanViewModel>>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<IList<BanViewModel>> Handle(GetBansListQuery request, CancellationToken cancellationToken)
            {
                // the whole bans table is loaded. Acceptable since only admins can access this resource
                return await _db.Bans
                    .Include(b => b.BannedUser)
                    .Include(b => b.BannedByUser)
                    .ProjectTo<BanViewModel>(_mapper.ConfigurationProvider)
                    .ToArrayAsync(cancellationToken);
            }
        }
    }
}