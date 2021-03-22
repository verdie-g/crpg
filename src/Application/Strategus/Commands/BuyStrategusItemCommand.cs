using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Strategus.Models;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Strategus;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Strategus.Commands
{
    public class BuyStrategusItemCommand : IMediatorRequest<StrategusOwnedItemViewModel>
    {
        public int HeroId { get; set; }
        public int ItemId { get; set; }
        public int ItemCount { get; set; }
        public int SettlementId { get; set; }

        public class Validator : AbstractValidator<BuyStrategusItemCommand>
        {
            public Validator()
            {
                RuleFor(m => m.ItemCount).GreaterThan(0);
            }
        }

        internal class Handler : IMediatorRequestHandler<BuyStrategusItemCommand, StrategusOwnedItemViewModel>
        {
            private static readonly ILogger Logger = LoggerFactory.CreateLogger<BuyStrategusItemCommand>();

            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;
            private readonly IStrategusMap _strategusMap;

            public Handler(ICrpgDbContext db, IMapper mapper, IStrategusMap strategusMap)
            {
                _db = db;
                _mapper = mapper;
                _strategusMap = strategusMap;
            }

            public async Task<Result<StrategusOwnedItemViewModel>> Handle(BuyStrategusItemCommand req,
                CancellationToken cancellationToken)
            {
                var hero = await _db.StrategusHeroes
                    .FirstOrDefaultAsync(h => h.Id == req.HeroId, cancellationToken);
                if (hero == null)
                {
                    return new Result<StrategusOwnedItemViewModel>(CommonErrors.HeroNotFound(req.HeroId));
                }

                var settlement = await _db.StrategusSettlements
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == req.SettlementId, cancellationToken);
                if (settlement == null)
                {
                    return new Result<StrategusOwnedItemViewModel>(CommonErrors.SettlementNotFound(req.HeroId));
                }

                if (!_strategusMap.ArePointsAtInteractionDistance(hero.Position, settlement.Position))
                {
                    return new Result<StrategusOwnedItemViewModel>(CommonErrors.SettlementTooFar(req.SettlementId));
                }

                var item = await _db.Items
                    .FirstOrDefaultAsync(i => i.Id == req.ItemId, cancellationToken);
                if (item == null)
                {
                    return new Result<StrategusOwnedItemViewModel>(CommonErrors.ItemNotFound(req.ItemId));
                }

                if (item.Rank != 0 || (item.Culture != Culture.Neutral && item.Culture != settlement.Culture))
                {
                    return new Result<StrategusOwnedItemViewModel>(CommonErrors.ItemNotBuyable(req.ItemId));
                }

                int cost = item.Value * req.ItemCount;
                if (hero.Gold < cost)
                {
                    return new Result<StrategusOwnedItemViewModel>(CommonErrors.NotEnoughGold(cost, hero.Gold));
                }

                var ownedItem = await _db.StrategusOwnedItems
                    .Include(oi => oi.Item)
                    .FirstOrDefaultAsync(oi => oi.HeroId == hero.Id && oi.ItemId == item.Id, cancellationToken);
                if (ownedItem == null)
                {
                    ownedItem = new StrategusOwnedItem
                    {
                        Item = item,
                        Count = req.ItemCount,
                        Hero = hero,
                    };
                    _db.StrategusOwnedItems.Add(ownedItem);
                }
                else
                {
                    ownedItem.Count += req.ItemCount;
                }

                hero.Gold -= cost;
                await _db.SaveChangesAsync(cancellationToken);
                Logger.LogInformation("Hero '{0}' bought {1} items '{2}'", req.HeroId, req.ItemCount, req.ItemId);
                return new Result<StrategusOwnedItemViewModel>(_mapper.Map<StrategusOwnedItemViewModel>(ownedItem));
            }
        }
    }
}
