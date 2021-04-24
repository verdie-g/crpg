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
    public record GetOwnedItemsQuery : IMediatorRequest<IList<ItemViewModel>>
    {
        public int UserId { get; init; }

        internal class Handler : IMediatorRequestHandler<GetOwnedItemsQuery, IList<ItemViewModel>>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Result<IList<ItemViewModel>>> Handle(GetOwnedItemsQuery req, CancellationToken cancellationToken)
            {
                var ownedItems = await _db.OwnedItems
                    .Where(oi => oi.UserId == req.UserId)
                    .Include(oi => oi.Item)
                    .Select(oi => oi.Item)
                    .ToListAsync(cancellationToken);

                return new(_mapper.Map<IList<ItemViewModel>>(ownedItems));
            }
        }
    }
}
