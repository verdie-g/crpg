using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Models;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Items.Queries;

public record GetItemsQuery : IMediatorRequest<IList<ItemViewModel>>
{
    /// <summary>
    /// True if only the items of rank 0 should be returned.
    /// </summary>
    public bool BaseItems { get; init; }

    internal class Handler : IMediatorRequestHandler<GetItemsQuery, IList<ItemViewModel>>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<IList<ItemViewModel>>> Handle(GetItemsQuery req, CancellationToken cancellationToken)
        {
            var items = await _db.Items
                .AsNoTracking()
                .OrderBy(i => i.Price)
                .Where(i => !req.BaseItems || i.Rank == 0)
                .ToListAsync(cancellationToken);

            return new(_mapper.Map<IList<ItemViewModel>>(items));
        }
    }
}
