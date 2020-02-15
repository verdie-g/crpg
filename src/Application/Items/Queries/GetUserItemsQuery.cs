using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Trpg.Application.Common.Interfaces;

namespace Trpg.Application.Items.Queries
{
    public class GetUserItemsQuery : IRequest<IReadOnlyList<ItemViewModel>>
    {
        public int UserId { get; set; }

        public class Handler : IRequestHandler<GetUserItemsQuery, IReadOnlyList<ItemViewModel>>
        {
            private readonly ITrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ITrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<IReadOnlyList<ItemViewModel>> Handle(GetUserItemsQuery request, CancellationToken cancellationToken)
            {
                return await _db.UserItems
                    .Where(ui => ui.UserId == request.UserId)
                    .Include(ui => ui.Item)
                    .Select(ui => ui.Item)
                    .ProjectTo<ItemViewModel>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);
            }
        }
    }
}