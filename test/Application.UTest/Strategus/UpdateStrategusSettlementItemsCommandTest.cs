using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Results;
using Crpg.Application.Strategus.Commands;
using Crpg.Application.Strategus.Models;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Strategus;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.Strategus
{
    public class UpdateStrategusSettlementItemsCommandTest : TestBase
    {
        [Test]
        public async Task ShouldReturnErrorIfHeroNotFound()
        {
            UpdateStrategusSettlementItemsCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new UpdateStrategusSettlementItemsCommand
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
            StrategusSettlement settlement = new();
            ArrangeDb.StrategusSettlements.Add(settlement);
            StrategusHero hero = new()
            {
                Status = StrategusHeroStatus.Idle,
                User = new User(),
            };
            ArrangeDb.StrategusHeroes.Add(hero);
            await ArrangeDb.SaveChangesAsync();

            UpdateStrategusSettlementItemsCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new UpdateStrategusSettlementItemsCommand
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
            StrategusSettlement settlement = new();
            ArrangeDb.StrategusSettlements.Add(settlement);
            StrategusHero hero = new()
            {
                Status = StrategusHeroStatus.IdleInSettlement,
                TargetedSettlement = new StrategusSettlement(),
                User = new User(),
            };
            ArrangeDb.StrategusHeroes.Add(hero);
            await ArrangeDb.SaveChangesAsync();

            UpdateStrategusSettlementItemsCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new UpdateStrategusSettlementItemsCommand
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

            StrategusSettlement settlement = new();
            ArrangeDb.StrategusSettlements.Add(settlement);

            StrategusHero hero = new()
            {
                Status = StrategusHeroStatus.RecruitingInSettlement,
                TargetedSettlement = settlement,
                User = new User(),
                Items = { new StrategusHeroItem { Item = item0, Count = 5 } },
            };
            ArrangeDb.StrategusHeroes.Add(hero);

            await ArrangeDb.SaveChangesAsync();

            UpdateStrategusSettlementItemsCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new UpdateStrategusSettlementItemsCommand
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

            StrategusSettlement settlement = new()
            {
                Items = { new StrategusSettlementItem { Item = item0, Count = 2 } },
            };
            ArrangeDb.StrategusSettlements.Add(settlement);

            StrategusHero hero = new()
            {
                Status = StrategusHeroStatus.RecruitingInSettlement,
                TargetedSettlement = settlement,
                User = new User(),
                Items = new List<StrategusHeroItem>(),
            };
            ArrangeDb.StrategusHeroes.Add(hero);

            await ArrangeDb.SaveChangesAsync();

            UpdateStrategusSettlementItemsCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new UpdateStrategusSettlementItemsCommand
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

            StrategusSettlement settlement = new()
            {
                Items = { new StrategusSettlementItem { Item = item0, Count = 2 } },
            };
            ArrangeDb.StrategusSettlements.Add(settlement);

            StrategusHero hero = new()
            {
                Status = StrategusHeroStatus.RecruitingInSettlement,
                TargetedSettlement = settlement,
                User = new User(),
                Items = new List<StrategusHeroItem>(),
            };
            ArrangeDb.StrategusHeroes.Add(hero);

            await ArrangeDb.SaveChangesAsync();

            UpdateStrategusSettlementItemsCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new UpdateStrategusSettlementItemsCommand
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

            StrategusSettlement settlement = new()
            {
                Items =
                {
                    new StrategusSettlementItem { Item = item0, Count = 2 },
                    new StrategusSettlementItem { Item = item1, Count = 3 },
                },
            };
            ArrangeDb.StrategusSettlements.Add(settlement);

            StrategusHero hero = new()
            {
                Status = StrategusHeroStatus.RecruitingInSettlement,
                TargetedSettlement = settlement,
                User = new User(),
                Items =
                {
                    new StrategusHeroItem { Item = item2, Count = 4 },
                    new StrategusHeroItem { Item = item3, Count = 5 },
                },
                OwnedSettlements = { settlement },
            };
            ArrangeDb.StrategusHeroes.Add(hero);

            await ArrangeDb.SaveChangesAsync();

            UpdateStrategusSettlementItemsCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new UpdateStrategusSettlementItemsCommand
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

            settlement = await AssertDb.StrategusSettlements
                .Include(s => s.Items)
                .FirstAsync(s => s.Id == settlement.Id);
            Assert.AreEqual(3, settlement.Items.Count);
            Assert.AreEqual(item0.Id, settlement.Items[0].ItemId);
            Assert.AreEqual(2, settlement.Items[0].Count);
            Assert.AreEqual(item2.Id, settlement.Items[1].ItemId);
            Assert.AreEqual(4, settlement.Items[1].Count);
            Assert.AreEqual(item3.Id, settlement.Items[2].ItemId);
            Assert.AreEqual(2, settlement.Items[2].Count);

            hero = await AssertDb.StrategusHeroes
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
