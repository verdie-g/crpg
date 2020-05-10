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