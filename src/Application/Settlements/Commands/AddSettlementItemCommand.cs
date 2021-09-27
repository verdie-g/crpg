using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Heroes.Models;
using Crpg.Domain.Entities.Heroes;
using Crpg.Domain.Entities.Settlements;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Settlements.Commands
{
    public record AddSettlementItemCommand : IMediatorRequest<ItemStack>
    {
        public int HeroId { get; init; }
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
                var hero = await _db.Heroes
                    .Include(h => h.TargetedSettlement)
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

                var item = await _db.Items.FirstOrDefaultAsync(i => i.Id == req.ItemId, cancellationToken);
                if (item == null)
                {
                    return new(CommonErrors.ItemNotFound(req.ItemId));
                }

                var heroItem = await _db.HeroItems
                    .FirstOrDefaultAsync(hi => hi.HeroId == req.HeroId && hi.ItemId == req.ItemId, cancellationToken);
                var settlementItem = await _db.SettlementItems
                    .FirstOrDefaultAsync(si => si.SettlementId == req.SettlementId && si.ItemId == req.ItemId, cancellationToken);
                if (req.Count >= 0) // hero -> settlement
                {
                    if (heroItem == null || heroItem.Count < req.Count)
                    {
                        return new(CommonErrors.ItemNotOwned(req.ItemId));
                    }
                }
                else // settlement -> hero
                {
                    // Only owner can take items from their settlements.
                    if (hero.TargetedSettlement!.OwnerId != hero.Id)
                    {
                        return new(CommonErrors.HeroNotSettlementOwner(hero.Id, hero.TargetedSettlementId.Value));
                    }

                    if (settlementItem == null || settlementItem.Count < -req.Count)
                    {
                        return new(CommonErrors.ItemNotOwned(req.ItemId));
                    }
                }

                if (heroItem == null) // If hero did not have this item before.
                {
                    heroItem = new HeroItem
                    {
                        Count = -req.Count,
                        Item = item,
                    };
                    hero.Items.Add(heroItem);
                }
                else // Update existing item stack.
                {
                    heroItem.Count -= req.Count;
                    if (heroItem.Count == 0)
                    {
                        _db.HeroItems.Remove(heroItem);
                    }
                }

                if (settlementItem == null) // If settlement did not have this item before.
                {
                    settlementItem = new SettlementItem
                    {
                        Count = req.Count,
                        Item = item,
                    };
                    hero.TargetedSettlement!.Items.Add(settlementItem);
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
                        ? "Hero '{0}' gave item '{1}' (x{2}) to settlement '{3}'"
                        : "Hero '{0}' took item '{1}' (x{2}) from settlement '{3}'",
                    req.HeroId, req.ItemId, req.Count, req.SettlementId);
                return new(_mapper.Map<ItemStack>(settlementItem));
            }
        }
    }
}
