using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Models;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Items.Queries;

public record GetItemsByBaseIdQuery : IMediatorRequest<IList<ItemViewModel>>
{
    public string BaseId { get; init; } = string.Empty;
    internal class Handler : IMediatorRequestHandler<GetItemsByBaseIdQuery, IList<ItemViewModel>>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<IList<ItemViewModel>>> Handle(GetItemsByBaseIdQuery req, CancellationToken cancellationToken)
        {
            var items = await _db.Items
                .AsNoTracking()
                .Where(i => i.BaseId == req.BaseId)
                .OrderBy(i => i.Rank)
                .ToListAsync(cancellationToken);

            return new(_mapper.Map<IList<ItemViewModel>>(items));
        }
    }
}
