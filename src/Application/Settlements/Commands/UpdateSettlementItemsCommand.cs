using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Strategus.Models;
using Crpg.Domain.Entities.Heroes;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Settlements;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Settlements.Commands
{
    public record UpdateSettlementItemsCommand : IMediatorRequest<IList<ItemStack>>
    {
        public int HeroId { get; init; }
        public int SettlementId { get; init; }
        public IList<ItemIdStack> Items { get; init; } = Array.Empty<ItemIdStack>();

        public class Validator : AbstractValidator<UpdateSettlementItemsCommand>
        {
            public Validator()
            {
                RuleForEach(c => c.Items).ChildRules(i => i.RuleFor(s => s.Count).GreaterThanOrEqualTo(0));
            }
        }

        internal class Handler : IMediatorRequestHandler<UpdateSettlementItemsCommand, IList<ItemStack>>
        {
            private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpdateSettlementItemsCommand>();

            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Result<IList<ItemStack>>> Handle(UpdateSettlementItemsCommand req, CancellationToken cancellationToken)
            {
                var hero = await _db.Heroes
                    .AsSplitQuery()
                    .Include(h => h.TargetedSettlement).ThenInclude(s => s!.Items).ThenInclude(i => i.Item)
                    .Include(h => h.Items)
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

                var newSettlementItems = await LoadSettlementItems(req.Items, cancellationToken);
                var itemDiff = ComputeSettlementItemDiff(hero.TargetedSettlement!.Items, newSettlementItems);
                var updatedSettlementItemsRes = UpdateHeroAndSettlementItems(hero, hero.TargetedSettlement, itemDiff);
                if (updatedSettlementItemsRes.Errors != null)
                {
                    return new(updatedSettlementItemsRes.Errors);
                }

                await _db.SaveChangesAsync(cancellationToken);
                Logger.LogInformation("Hero '{0}' updated settlement '{1}' items", hero.Id, hero.TargetedSettlementId);
                return new(_mapper.Map<IList<ItemStack>>(hero.TargetedSettlement.Items));
            }

            private async Task<IEnumerable<(Item item, int count)>> LoadSettlementItems(IList<ItemIdStack> itemStacks,
                CancellationToken cancellationToken)
            {
                int[] itemIds = itemStacks.Select(i => i.ItemId).ToArray();
                Dictionary<int, Item> itemsById = await _db.Items
                    .Where(i => itemIds.Contains(i.Id))
                    .ToDictionaryAsync(i => i.Id, cancellationToken);
                return itemStacks.Select(i => (itemsById[i.ItemId], i.Count));
            }

            private IEnumerable<(Item item, int diff)> ComputeSettlementItemDiff(
                IEnumerable<SettlementItem> oldSettlementItems,
                IEnumerable<(Item item, int count)> newSettlementItems)
            {
                var itemDiff = oldSettlementItems.ToDictionary(hi => hi.ItemId, hi => (hi.Item!, -hi.Count));
                foreach (var (item, count) in newSettlementItems)
                {
                    int diff = (itemDiff.TryGetValue(item.Id, out var itemStack) ? itemStack.Item2 : 0) + count;
                    itemDiff[item.Id] = (item, diff);
                }

                return itemDiff.Values;
            }

            private Result UpdateHeroAndSettlementItems(Hero hero, Settlement settlement,
                IEnumerable<(Item item, int diff)> settlementItemDiff)
            {
                var heroItemsById = hero.Items.ToDictionary(i => i.ItemId);
                var settlementItemsById = settlement.Items.ToDictionary(i => i.ItemId);

                foreach (var itemDiff in settlementItemDiff)
                {
                    if (itemDiff.diff == 0)
                    {
                        continue;
                    }

                    var heroItem = heroItemsById.GetValueOrDefault(itemDiff.item.Id);
                    var settlementItem = settlementItemsById.GetValueOrDefault(itemDiff.item.Id);

                    if (itemDiff.diff > 0) // hero -> settlement
                    {
                        if (heroItem == null || heroItem.Count < itemDiff.diff)
                        {
                            return new(CommonErrors.ItemNotOwned(itemDiff.item.Id));
                        }
                    }
                    else // settlement -> hero
                    {
                        // Only owner can take items from their settlements.
                        if (settlement.OwnerId != hero.Id)
                        {
                            return new(CommonErrors.HeroNotSettlementOwner(hero.Id, settlement.Id));
                        }

                        if (settlementItem == null || settlementItem.Count < Math.Abs(itemDiff.diff))
                        {
                            return new(CommonErrors.ItemNotOwned(itemDiff.item.Id));
                        }
                    }

                    if (heroItem == null) // If hero did not have this item before.
                    {
                        heroItem = new HeroItem
                        {
                            Count = -itemDiff.diff,
                            Item = itemDiff.item,
                        };
                        hero.Items.Add(heroItem);
                    }
                    else // Update existing item stack.
                    {
                        heroItem.Count -= itemDiff.diff;
                        if (heroItem.Count == 0)
                        {
                            _db.HeroItems.Remove(heroItem);
                        }
                    }

                    if (settlementItem == null) // If settlement did not have this item before.
                    {
                        settlementItem = new SettlementItem
                        {
                            Count = itemDiff.diff,
                            Item = itemDiff.item,
                        };
                        settlement.Items.Add(settlementItem);
                    }
                    else // Update existing item stack.
                    {
                        settlementItem.Count += itemDiff.diff;
                        if (settlementItem.Count == 0)
                        {
                            _db.SettlementItems.Remove(settlementItem);
                        }
                    }
                }

                return Result.NoErrors;
            }
        }
    }
}
