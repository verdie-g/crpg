using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities;

namespace Crpg.Application.Items.Queries
{
    public class GetItemQuery : IRequest<ItemViewModel>
    {
        public int ItemId { get; set; }

        public class Handler : IRequestHandler<GetItemQuery, ItemViewModel>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<ItemViewModel> Handle(GetItemQuery request, CancellationToken cancellationToken)
            {
                var item = await _db.Items.FirstOrDefaultAsync(i => i.Id == request.ItemId, cancellationToken);
                if (item == null)
                {
                    throw new NotFoundException(nameof(Item), request.ItemId);
                }

                return _mapper.Map<ItemViewModel>(item);
            }
        }
    }
}
