using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Trpg.Application.Common.Interfaces;
using Trpg.Application.Items.Models;

namespace Trpg.Application.Items.Queries
{
    public class GetItemsListQuery : IRequest<IReadOnlyList<ItemViewModel>>
    {
        public class Handler : IRequestHandler<GetItemsListQuery, IReadOnlyList<ItemViewModel>>
        {
            private readonly ITrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ITrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<IReadOnlyList<ItemViewModel>> Handle(GetItemsListQuery request, CancellationToken cancellationToken)
            {
                return await _db.Items
                    .OrderBy(i => i.Value)
                    .ProjectTo<ItemViewModel>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);
            }
        }
    }
}
