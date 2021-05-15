using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Settlements.Queries
{
    public record GetSettlementShopItemsQuery : IMediatorRequest<IList<ItemViewModel>>
    {
        public int HeroId { get; init; }
        public int SettlementId { get; init; }

        internal class Handler : IMediatorRequestHandler<GetSettlementShopItemsQuery, IList<ItemViewModel>>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;
            private readonly IStrategusMap _strategusMap;

            public Handler(ICrpgDbContext db, IMapper mapper, IStrategusMap strategusMap)
            {
                _db = db;
                _mapper = mapper;
                _strategusMap = strategusMap;
            }

            public async Task<Result<IList<ItemViewModel>>> Handle(GetSettlementShopItemsQuery req,
                CancellationToken cancellationToken)
            {
                var hero = await _db.Heroes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(h => h.Id == req.HeroId, cancellationToken);
                if (hero == null)
                {
                    return new(CommonErrors.HeroNotFound(req.HeroId));
                }

                var settlement = await _db.Settlements
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == req.SettlementId, cancellationToken);
                if (settlement == null)
                {
                    return new(CommonErrors.SettlementNotFound(req.HeroId));
                }

                if (!_strategusMap.ArePointsAtInteractionDistance(hero.Position, settlement.Position))
                {
                    return new(CommonErrors.SettlementTooFar(req.SettlementId));
                }

                // Return items with the same culture as the settlement.
                var items = await _db.Items
                    .AsNoTracking()
                    .Where(i => i.Rank == 0 && (i.Culture == Culture.Neutral || i.Culture == settlement.Culture))
                    .ToArrayAsync(cancellationToken);
                return new(_mapper.Map<ItemViewModel[]>(items));
            }
        }
    }
}
