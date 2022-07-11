using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Parties.Models;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Settlements;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Settlements.Commands;

public record AddSettlementItemCommand : IMediatorRequest<ItemStack>
{
    public int PartyId { get; init; }
    public int SettlementId { get; init; }
    public int ItemId { get; init; }
    public int Count { get; init; }

    internal class Handler : IMediatorRequestHandler<AddSettlementItemCommand, ItemStack>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<AddSettlementItemCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<ItemStack>> Handle(AddSettlementItemCommand req, CancellationToken cancellationToken)
        {
            var party = await _db.Parties
                .Include(h => h.TargetedSettlement)
                .FirstOrDefaultAsync(h => h.Id == req.PartyId, cancellationToken);
            if (party == null)
            {
                return new(CommonErrors.PartyNotFound(req.PartyId));
            }

            if ((party.Status != PartyStatus.IdleInSettlement
                 && party.Status != PartyStatus.RecruitingInSettlement)
                || party.TargetedSettlementId != req.SettlementId)
            {
                return new(CommonErrors.PartyNotInASettlement(party.Id));
            }

            var item = await _db.Items.FirstOrDefaultAsync(i => i.Id == req.ItemId, cancellationToken);
            if (item == null)
            {
                return new(CommonErrors.ItemNotFound(req.ItemId));
            }

            var partyItem = await _db.PartyItems
                .FirstOrDefaultAsync(hi => hi.PartyId == req.PartyId && hi.ItemId == req.ItemId, cancellationToken);
            var settlementItem = await _db.SettlementItems
                .FirstOrDefaultAsync(si => si.SettlementId == req.SettlementId && si.ItemId == req.ItemId, cancellationToken);
            if (req.Count >= 0) // party -> settlement
            {
                if (partyItem == null || partyItem.Count < req.Count)
                {
                    return new(CommonErrors.ItemNotOwned(req.ItemId));
                }
            }
            else // settlement -> party
            {
                // Only owner can take items from their settlements.
                if (party.TargetedSettlement!.OwnerId != party.Id)
                {
                    return new(CommonErrors.PartyNotSettlementOwner(party.Id, party.TargetedSettlementId.Value));
                }

                if (settlementItem == null || settlementItem.Count < -req.Count)
                {
                    return new(CommonErrors.ItemNotOwned(req.ItemId));
                }
            }

            if (partyItem == null) // If party did not have this item before.
            {
                partyItem = new PartyItem
                {
                    Count = -req.Count,
                    Item = item,
                };
                party.Items.Add(partyItem);
            }
            else // Update existing item stack.
            {
                partyItem.Count -= req.Count;
                if (partyItem.Count == 0)
                {
                    _db.PartyItems.Remove(partyItem);
                }
            }

            if (settlementItem == null) // If settlement did not have this item before.
            {
                settlementItem = new SettlementItem
                {
                    Count = req.Count,
                    Item = item,
                };
                party.TargetedSettlement!.Items.Add(settlementItem);
            }
            else // Update existing item stack.
            {
                settlementItem.Count += req.Count;
                if (settlementItem.Count == 0)
                {
                    _db.SettlementItems.Remove(settlementItem);
                }
            }

            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation(req.Count >= 0
                    ? "Party '{0}' gave item '{1}' (x{2}) to settlement '{3}'"
                    : "Party '{0}' took item '{1}' (x{2}) from settlement '{3}'",
                req.PartyId, req.ItemId, req.Count, req.SettlementId);
            return new(_mapper.Map<ItemStack>(settlementItem));
        }
    }
}
