using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Results;
using Crpg.Application.Settlements.Commands;
using Crpg.Application.Strategus.Models;
using Crpg.Domain.Entities.Heroes;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.Settlements
{
    public class UpdateSettlementItemsCommandTest : TestBase
    {
        [Test]
        public async Task ShouldReturnErrorIfHeroNotFound()
        {
            UpdateSettlementItemsCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new UpdateSettlementItemsCommand
            {
                HeroId = 99,
                SettlementId = 99,
                Items = Array.Empty<ItemIdStack>(),
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.HeroNotFound, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfHeroNotInASettlement()
        {
            Settlement settlement = new();
            ArrangeDb.Settlements.Add(settlement);
            Hero hero = new()
            {
                Status = HeroStatus.Idle,
                User = new User(),
            };
            ArrangeDb.Heroes.Add(hero);
            await ArrangeDb.SaveChangesAsync();

            UpdateSettlementItemsCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new UpdateSettlementItemsCommand
            {
                HeroId = hero.Id,
                SettlementId = settlement.Id,
                Items = Array.Empty<ItemIdStack>(),
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.HeroNotInASettlement, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfHeroNotInTheSettlement()
        {
            Settlement settlement = new();
            ArrangeDb.Settlements.Add(settlement);
            Hero hero = new()
            {
                Status = HeroStatus.IdleInSettlement,
                TargetedSettlement = new Settlement(),
                User = new User(),
            };
            ArrangeDb.Heroes.Add(hero);
            await ArrangeDb.SaveChangesAsync();

            UpdateSettlementItemsCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new UpdateSettlementItemsCommand
            {
                HeroId = hero.Id,
                SettlementId = settlement.Id,
                Items = Array.Empty<ItemIdStack>(),
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.HeroNotInASettlement, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfHeroGiveItemsTheyDontOwn()
        {
            Item item0 = new();
            Item item1 = new();
            ArrangeDb.Items.AddRange(item0, item1);

            Settlement settlement = new();
            ArrangeDb.Settlements.Add(settlement);

            Hero hero = new()
            {
                Status = HeroStatus.RecruitingInSettlement,
                TargetedSettlement = settlement,
                User = new User(),
                Items = { new HeroItem { Item = item0, Count = 5 } },
            };
            ArrangeDb.Heroes.Add(hero);

            await ArrangeDb.SaveChangesAsync();

            UpdateSettlementItemsCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new UpdateSettlementItemsCommand
            {
                HeroId = hero.Id,
                SettlementId = settlement.Id,
                Items = new[] { new ItemIdStack { ItemId = item1.Id, Count = 1 } },
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.ItemNotOwned, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfHeroTakesItemsFromUnownedSettlement()
        {
            Item item0 = new();
            ArrangeDb.Items.AddRange(item0);

            Settlement settlement = new()
            {
                Items = { new SettlementItem { Item = item0, Count = 2 } },
            };
            ArrangeDb.Settlements.Add(settlement);

            Hero hero = new()
            {
                Status = HeroStatus.RecruitingInSettlement,
                TargetedSettlement = settlement,
                User = new User(),
                Items = new List<HeroItem>(),
            };
            ArrangeDb.Heroes.Add(hero);

            await ArrangeDb.SaveChangesAsync();

            UpdateSettlementItemsCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new UpdateSettlementItemsCommand
            {
                HeroId = hero.Id,
                SettlementId = settlement.Id,
                Items = new[] { new ItemIdStack { ItemId = item0.Id, Count = 1 } },
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.HeroNotSettlementOwner, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfHeroTakesItemsTheSettlementDoesNotHave()
        {
            Item item0 = new();
            ArrangeDb.Items.AddRange(item0);

            Settlement settlement = new()
            {
                Items = { new SettlementItem { Item = item0, Count = 2 } },
            };
            ArrangeDb.Settlements.Add(settlement);

            Hero hero = new()
            {
                Status = HeroStatus.RecruitingInSettlement,
                TargetedSettlement = settlement,
                User = new User(),
                Items = new List<HeroItem>(),
            };
            ArrangeDb.Heroes.Add(hero);

            await ArrangeDb.SaveChangesAsync();

            UpdateSettlementItemsCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new UpdateSettlementItemsCommand
            {
                HeroId = hero.Id,
                SettlementId = settlement.Id,
                Items = new[] { new ItemIdStack { ItemId = item0.Id, Count = 3 } },
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.ItemNotOwned, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldGiveToAndTakeFromItemsSettlement()
        {
            Item item0 = new();
            Item item1 = new();
            Item item2 = new();
            Item item3 = new();
            ArrangeDb.Items.AddRange(item0, item1, item2, item3);

            Settlement settlement = new()
            {
                Items =
                {
                    new SettlementItem { Item = item0, Count = 2 },
                    new SettlementItem { Item = item1, Count = 3 },
                },
            };
            ArrangeDb.Settlements.Add(settlement);

            Hero hero = new()
            {
                Status = HeroStatus.RecruitingInSettlement,
                TargetedSettlement = settlement,
                User = new User(),
                Items =
                {
                    new HeroItem { Item = item2, Count = 4 },
                    new HeroItem { Item = item3, Count = 5 },
                },
                OwnedSettlements = { settlement },
            };
            ArrangeDb.Heroes.Add(hero);

            await ArrangeDb.SaveChangesAsync();

            UpdateSettlementItemsCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new UpdateSettlementItemsCommand
            {
                HeroId = hero.Id,
                SettlementId = settlement.Id,
                Items = new[]
                {
                    new ItemIdStack { ItemId = item0.Id, Count = 2 }, // Don't give/take item0.
                    // Take all item1.
                    new ItemIdStack { ItemId = item2.Id, Count = 4 }, // Give all item2.
                    new ItemIdStack { ItemId = item3.Id, Count = 2 }, // Give 2 item3.
                },
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            var settlementItems = res.Data!;
            Assert.AreEqual(3, settlementItems.Count);
            Assert.AreEqual(item0.Id, settlementItems[0].Item.Id);
            Assert.AreEqual(2, settlementItems[0].Count);
            Assert.AreEqual(item2.Id, settlementItems[1].Item.Id);
            Assert.AreEqual(4, settlementItems[1].Count);
            Assert.AreEqual(item3.Id, settlementItems[2].Item.Id);
            Assert.AreEqual(2, settlementItems[2].Count);

            settlement = await AssertDb.Settlements
                .Include(s => s.Items)
                .FirstAsync(s => s.Id == settlement.Id);
            Assert.AreEqual(3, settlement.Items.Count);
            Assert.AreEqual(item0.Id, settlement.Items[0].ItemId);
            Assert.AreEqual(2, settlement.Items[0].Count);
            Assert.AreEqual(item2.Id, settlement.Items[1].ItemId);
            Assert.AreEqual(4, settlement.Items[1].Count);
            Assert.AreEqual(item3.Id, settlement.Items[2].ItemId);
            Assert.AreEqual(2, settlement.Items[2].Count);

            hero = await AssertDb.Heroes
                .Include(h => h.Items)
                .FirstAsync(h => h.Id == hero.Id);
            Assert.AreEqual(2, hero.Items.Count);
            Assert.AreEqual(item3.Id, hero.Items[0].ItemId);
            Assert.AreEqual(3, hero.Items[0].Count);
            Assert.AreEqual(item1.Id, hero.Items[1].ItemId);
            Assert.AreEqual(3, hero.Items[1].Count);
        }
    }
}
