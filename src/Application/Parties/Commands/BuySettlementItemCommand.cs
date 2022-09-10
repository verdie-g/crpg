using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Parties.Models;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Parties;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Parties.Commands;

public record BuySettlementItemCommand : IMediatorRequest<ItemStack>
{
    public int PartyId { get; set; }
    public string ItemId { get; init; } = string.Empty;
    public int ItemCount { get; init; }
    public int SettlementId { get; init; }

    public class Validator : AbstractValidator<BuySettlementItemCommand>
    {
        public Validator()
        {
            RuleFor(m => m.ItemCount).GreaterThan(0);
        }
    }

    internal class Handler : IMediatorRequestHandler<BuySettlementItemCommand, ItemStack>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<BuySettlementItemCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly IStrategusMap _strategusMap;

        public Handler(ICrpgDbContext db, IMapper mapper, IStrategusMap strategusMap)
        {
            _db = db;
            _mapper = mapper;
            _strategusMap = strategusMap;
        }

        public async Task<Result<ItemStack>> Handle(BuySettlementItemCommand req,
            CancellationToken cancellationToken)
        {
            var party = await _db.Parties
                .FirstOrDefaultAsync(h => h.Id == req.PartyId, cancellationToken);
            if (party == null)
            {
                return new(CommonErrors.PartyNotFound(req.PartyId));
            }

            var settlement = await _db.Settlements
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == req.SettlementId, cancellationToken);
            if (settlement == null)
            {
                return new(CommonErrors.SettlementNotFound(req.PartyId));
            }

            if (!_strategusMap.ArePointsAtInteractionDistance(party.Position, settlement.Position))
            {
                return new(CommonErrors.SettlementTooFar(req.SettlementId));
            }

            var item = await _db.Items
                .FirstOrDefaultAsync(i => i.Id == req.ItemId, cancellationToken);
            if (item == null)
            {
                return new(CommonErrors.ItemNotFound(req.ItemId));
            }

            if (item.Culture != Culture.Neutral && item.Culture != settlement.Culture)
            {
                return new(CommonErrors.ItemNotBuyable(req.ItemId));
            }

            int cost = item.Price * req.ItemCount;
            if (party.Gold < cost)
            {
                return new(CommonErrors.NotEnoughGold(cost, party.Gold));
            }

            var partyItem = await _db.PartyItems
                .Include(pi => pi.Item)
                .FirstOrDefaultAsync(pi => pi.PartyId == party.Id && pi.ItemId == item.Id, cancellationToken);
            if (partyItem == null)
            {
                partyItem = new PartyItem
                {
                    Item = item,
                    Count = req.ItemCount,
                    Party = party,
                };
                _db.PartyItems.Add(partyItem);
            }
            else
            {
                partyItem.Count += req.ItemCount;
            }

            party.Gold -= cost;
            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("Party '{0}' bought {1} items '{2}'", req.PartyId, req.ItemCount, req.ItemId);
            return new(_mapper.Map<ItemStack>(partyItem));
        }
    }
}
