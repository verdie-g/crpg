using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Battles.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Models;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Items.Queries;

public record GetItemUpgrades : IMediatorRequest<IList<ItemViewModel>>
{
    public string BaseId { get; init; } = string.Empty;
    internal class Handler : IMediatorRequestHandler<GetItemUpgrades, IList<ItemViewModel>>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<IList<ItemViewModel>>> Handle(GetItemUpgrades req, CancellationToken cancellationToken)
        {
            return new(await _db.Items
                .AsNoTracking()
                .Where(i => i.BaseId == req.BaseId)
                .OrderBy(i => i.Rank)
                .ProjectTo<ItemViewModel>(_mapper.ConfigurationProvider)
                .ToArrayAsync(cancellationToken));
        }
    }
}
