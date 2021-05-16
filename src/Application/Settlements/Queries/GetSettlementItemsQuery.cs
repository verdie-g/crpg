using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Strategus.Models;
using Crpg.Domain.Entities.Heroes;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Settlements.Queries
{
    public record GetSettlementItemsQuery : IMediatorRequest<IList<ItemStack>>
    {
        public int HeroId { get; init; }
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
                var hero = await _db.Heroes
                    .AsSplitQuery()
                    .Include(h => h.TargetedSettlement).ThenInclude(s => s!.Items).ThenInclude(i => i.Item)
                    .FirstOrDefaultAsync(h => h.Id == req.HeroId, cancellationToken);
                if (hero == null)
                {
                    return new(CommonErrors.HeroNotFound(req.HeroId));
                }

                if ((hero.Status != HeroStatus.IdleInSettlement
                     && hero.Status != HeroStatus.RecruitingInSettlement)
                    || hero.TargetedSettlementId != req.SettlementId)
                {
                    return new(CommonErrors.HeroNotInASettlement(hero.Id));
                }

                // Only the settlement owner can see the items.
                if (hero.TargetedSettlement!.OwnerId != hero.Id)
                {
                    return new(CommonErrors.HeroNotSettlementOwner(hero.Id, hero.TargetedSettlementId.Value));
                }

                return new(_mapper.Map<IList<ItemStack>>(hero.TargetedSettlement!.Items));
            }
        }
    }
}
