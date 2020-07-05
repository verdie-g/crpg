using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Items.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Items.Queries
{
    public class GetUserItemsQuery : IRequest<IList<ItemViewModel>>
    {
        public int UserId { get; set; }

        public class Handler : IRequestHandler<GetUserItemsQuery, IList<ItemViewModel>>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<IList<ItemViewModel>> Handle(GetUserItemsQuery request, CancellationToken cancellationToken)
            {
                var ownedItems = await _db.UserItems
                    .Where(oi => oi.UserId == request.UserId)
                    .Include(oi => oi.Item)
                    .Select(oi => oi.Item)
                    .ToListAsync(cancellationToken);

                // can't use ProjectTo https://github.com/dotnet/efcore/issues/20729
                return _mapper.Map<IList<ItemViewModel>>(ownedItems);
            }
        }
    }
}
