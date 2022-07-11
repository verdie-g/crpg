using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Parties.Models;
using Crpg.Domain.Entities.Parties;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Settlements.Queries;

public record GetSettlementItemsQuery : IMediatorRequest<IList<ItemStack>>
{
    public int PartyId { get; init; }
    public int SettlementId { get; init; }

    internal class Handler : IMediatorRequestHandler<GetSettlementItemsQuery, IList<ItemStack>>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<IList<ItemStack>>> Handle(GetSettlementItemsQuery req,
            CancellationToken cancellationToken)
        {
            var party = await _db.Parties
                .AsSplitQuery()
                .Include(h => h.TargetedSettlement).ThenInclude(s => s!.Items).ThenInclude(i => i.Item)
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

            // Only the settlement owner can see the items.
            if (party.TargetedSettlement!.OwnerId != party.Id)
            {
                return new(CommonErrors.PartyNotSettlementOwner(party.Id, party.TargetedSettlementId.Value));
            }

            return new(_mapper.Map<IList<ItemStack>>(party.TargetedSettlement!.Items));
        }
    }
}
