using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Trpg.Application.Common.Exceptions;
using Trpg.Application.Common.Interfaces;
using Trpg.Domain.Entities;

namespace Trpg.Application.Items.Queries
{
    public class GetItemQuery : IRequest<ItemViewModel>
    {
        public int ItemId { get; set; }

        public class Handler : IRequestHandler<GetItemQuery, ItemViewModel>
        {
            private readonly ITrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ITrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<ItemViewModel> Handle(GetItemQuery request, CancellationToken cancellationToken)
            {
                var item = await _db.Items
                    .ProjectTo<ItemViewModel>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(i => i.Id == request.ItemId, cancellationToken);

                if (item == null)
                {
                    throw new NotFoundException(nameof(Item), request.ItemId);
                }

                return item;
            }
        }
    }
}
