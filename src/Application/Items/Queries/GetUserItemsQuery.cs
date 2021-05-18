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
    public record GetUserItemsQuery : IMediatorRequest<IList<ItemViewModel>>
    {
        public int UserId { get; init; }

        internal class Handler : IMediatorRequestHandler<GetUserItemsQuery, IList<ItemViewModel>>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Result<IList<ItemViewModel>>> Handle(GetUserItemsQuery req, CancellationToken cancellationToken)
            {
                var userItems = await _db.UserItems
                    .Where(oi => oi.UserId == req.UserId)
                    .Include(oi => oi.Item)
                    .Select(oi => oi.Item)
                    .ToListAsync(cancellationToken);

                return new(_mapper.Map<IList<ItemViewModel>>(userItems));
            }
        }
    }
}
