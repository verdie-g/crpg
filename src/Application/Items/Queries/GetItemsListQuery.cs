using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Models;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Items.Queries
{
    public class GetItemsListQuery : IMediatorRequest<IList<ItemViewModel>>
    {
        public class Handler : IMediatorRequestHandler<GetItemsListQuery, IList<ItemViewModel>>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Result<IList<ItemViewModel>>> Handle(GetItemsListQuery req, CancellationToken cancellationToken)
            {
                var items = await _db.Items
                    .AsNoTracking()
                    .OrderBy(i => i.Value)
                    .Where(i => i.Rank == 0) // don't return broken or loomed items
                    .ToListAsync(cancellationToken);

                return new Result<IList<ItemViewModel>>(_mapper.Map<IList<ItemViewModel>>(items));
            }
        }
    }
}
