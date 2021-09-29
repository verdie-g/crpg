using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Results;
using Crpg.Application.Settlements.Commands;
using Crpg.Domain.Entities.Heroes;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.Settlements
{
    public class AddSettlementItemCommandTest : TestBase
    {
        [Test]
        public async Task ShouldReturnErrorIfHeroNotFound()
        {
            AddSettlementItemCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new AddSettlementItemCommand
            {
                HeroId = 99,
                SettlementId = 99,
                ItemId = 99,
                Count = 0,
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

            AddSettlementItemCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new AddSettlementItemCommand
            {
                HeroId = hero.Id,
                SettlementId = settlement.Id,
                ItemId = 99,
                Count = 0,
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

            AddSettlementItemCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new AddSettlementItemCommand
            {
                HeroId = hero.Id,
                SettlementId = settlement.Id,
                ItemId = 99,
                Count = 0,
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

            AddSettlementItemCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new AddSettlementItemCommand
            {
                HeroId = hero.Id,
                SettlementId = settlement.Id,
                ItemId = item1.Id,
                Count = 0,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.ItemNotOwned, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfHeroDoesntHaveEnoughItems()
        {
            Item item0 = new();
            ArrangeDb.Items.AddRange(item0);

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

            AddSettlementItemCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new AddSettlementItemCommand
            {
                HeroId = hero.Id,
                SettlementId = settlement.Id,
                ItemId = item0.Id,
                Count = 6,
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

            AddSettlementItemCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new AddSettlementItemCommand
            {
                HeroId = hero.Id,
                SettlementId = settlement.Id,
                ItemId = item0.Id,
                Count = -2,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.HeroNotSettlementOwner, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfHeroTakesItemsTheSettlementDoesNotHave()
        {
            Item item0 = new();
            Item item1 = new();
            ArrangeDb.Items.AddRange(item0, item1);

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
                OwnedSettlements = { settlement },
            };
            ArrangeDb.Heroes.Add(hero);

            await ArrangeDb.SaveChangesAsync();

            AddSettlementItemCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new AddSettlementItemCommand
            {
                HeroId = hero.Id,
                SettlementId = settlement.Id,
                ItemId = item1.Id,
                Count = -1,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.ItemNotOwned, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfHeroTakesItemsWhenTheSettlementDoesntHaveEnough()
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
                OwnedSettlements = { settlement },
            };
            ArrangeDb.Heroes.Add(hero);

            await ArrangeDb.SaveChangesAsync();

            AddSettlementItemCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new AddSettlementItemCommand
            {
                HeroId = hero.Id,
                SettlementId = settlement.Id,
                ItemId = item0.Id,
                Count = -3,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.ItemNotOwned, res.Errors![0].Code);
        }

        [TestCase(3, 7, 11)]
        [TestCase(3, 0, 11)]
        [TestCase(-3, 7, 11)]
        [TestCase(-3, 7, 0)]
        public async Task ShouldGiveTakeItemsToFromSettlement(int diff, int settlementItemCount, int heroItemCount)
        {
            Item item0 = new();
            ArrangeDb.Items.AddRange(item0);

            Settlement settlement = new();
            if (settlementItemCount != 0)
            {
                settlement.Items.Add(new SettlementItem { Item = item0, Count = settlementItemCount });
            }

            ArrangeDb.Settlements.Add(settlement);

            Hero hero = new()
            {
                Status = HeroStatus.IdleInSettlement,
                TargetedSettlement = settlement,
                User = new User(),
                OwnedSettlements = { settlement },
            };
            if (heroItemCount != 0)
            {
                hero.Items.Add(new HeroItem { Item = item0, Count = heroItemCount });
            }

            ArrangeDb.Heroes.Add(hero);

            await ArrangeDb.SaveChangesAsync();

            AddSettlementItemCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new AddSettlementItemCommand
            {
                HeroId = hero.Id,
                SettlementId = settlement.Id,
                ItemId = item0.Id,
                Count = diff,
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            var itemStack = res.Data!;
            Assert.AreEqual(item0.Id, itemStack.Item.Id);
            Assert.AreEqual(settlementItemCount + diff, itemStack.Count);

            settlement = await AssertDb.Settlements
                .Include(s => s.Items)
                .FirstAsync(s => s.Id == settlement.Id);
            Assert.AreEqual(1, settlement.Items.Count);
            Assert.AreEqual(item0.Id, settlement.Items[0].ItemId);
            Assert.AreEqual(settlementItemCount + diff, settlement.Items[0].Count);

            hero = await AssertDb.Heroes
                .Include(h => h.Items)
                .FirstAsync(h => h.Id == hero.Id);
            Assert.AreEqual(1, hero.Items.Count);
            Assert.AreEqual(item0.Id, hero.Items[0].ItemId);
            Assert.AreEqual(heroItemCount - diff, hero.Items[0].Count);
        }
    }
}
