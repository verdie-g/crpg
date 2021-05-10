using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Strategus.Commands;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Strategus;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Moq;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Strategus
{
    public class BuyStrategusItemCommandTest : TestBase
    {
        [Test]
        public async Task ShouldReturnErrorIfHeroNotFound()
        {
            var handler = new BuyStrategusItemCommand.Handler(ActDb, Mapper, Mock.Of<IStrategusMap>());
            var res = await handler.Handle(new BuyStrategusItemCommand
            {
                HeroId = 1,
                ItemId = 2,
                ItemCount = 1,
                SettlementId = 3,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.HeroNotFound, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfSettlementNotFound()
        {
            var hero = new StrategusHero { User = new User() };
            ArrangeDb.StrategusHeroes.Add(hero);
            await ArrangeDb.SaveChangesAsync();

            var handler = new BuyStrategusItemCommand.Handler(ActDb, Mapper, Mock.Of<IStrategusMap>());
            var res = await handler.Handle(new BuyStrategusItemCommand
            {
                HeroId = hero.Id,
                ItemId = 2,
                ItemCount = 1,
                SettlementId = 3,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.SettlementNotFound, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfSettlementTooFar()
        {
            var userPosition = new Point(1, 2);
            var settlementPosition = new Point(3, 4);

            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock
                .Setup(m => m.ArePointsAtInteractionDistance(userPosition, settlementPosition))
                .Returns(false);

            var hero = new StrategusHero { Position = userPosition, User = new User() };
            ArrangeDb.StrategusHeroes.Add(hero);
            var settlement = new StrategusSettlement { Position = settlementPosition };
            ArrangeDb.StrategusSettlements.Add(settlement);
            await ArrangeDb.SaveChangesAsync();

            var handler = new BuyStrategusItemCommand.Handler(ActDb, Mapper, strategusMapMock.Object);
            var res = await handler.Handle(new BuyStrategusItemCommand
            {
                HeroId = hero.Id,
                ItemId = 2,
                ItemCount = 1,
                SettlementId = settlement.Id,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.SettlementTooFar, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfItemNotFound()
        {
            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock
                .Setup(m => m.ArePointsAtInteractionDistance(It.IsAny<Point>(), It.IsAny<Point>()))
                .Returns(true);

            var hero = new StrategusHero { User = new User() };
            ArrangeDb.StrategusHeroes.Add(hero);
            var settlement = new StrategusSettlement();
            ArrangeDb.StrategusSettlements.Add(settlement);
            await ArrangeDb.SaveChangesAsync();

            var handler = new BuyStrategusItemCommand.Handler(ActDb, Mapper, strategusMapMock.Object);
            var res = await handler.Handle(new BuyStrategusItemCommand
            {
                HeroId = hero.Id,
                ItemId = 2,
                ItemCount = 1,
                SettlementId = settlement.Id,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.ItemNotFound, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfItemNotRank0()
        {
            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock
                .Setup(m => m.ArePointsAtInteractionDistance(It.IsAny<Point>(), It.IsAny<Point>()))
                .Returns(true);

            var hero = new StrategusHero { User = new User() };
            ArrangeDb.StrategusHeroes.Add(hero);
            var settlement = new StrategusSettlement { Culture = Culture.Aserai };
            ArrangeDb.StrategusSettlements.Add(settlement);
            var item = new Item { Rank = 1, Culture = Culture.Aserai };
            ArrangeDb.Items.Add(item);
            await ArrangeDb.SaveChangesAsync();

            var handler = new BuyStrategusItemCommand.Handler(ActDb, Mapper, strategusMapMock.Object);
            var res = await handler.Handle(new BuyStrategusItemCommand
            {
                HeroId = hero.Id,
                ItemId = item.Id,
                ItemCount = 1,
                SettlementId = settlement.Id,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.ItemNotBuyable, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfItemIsNotFromTheSettlementCulture()
        {
            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock
                .Setup(m => m.ArePointsAtInteractionDistance(It.IsAny<Point>(), It.IsAny<Point>()))
                .Returns(true);

            var hero = new StrategusHero { User = new User() };
            ArrangeDb.StrategusHeroes.Add(hero);
            var settlement = new StrategusSettlement { Culture = Culture.Aserai };
            ArrangeDb.StrategusSettlements.Add(settlement);
            var item = new Item { Rank = 0, Culture = Culture.Empire };
            ArrangeDb.Items.Add(item);
            await ArrangeDb.SaveChangesAsync();

            var handler = new BuyStrategusItemCommand.Handler(ActDb, Mapper, strategusMapMock.Object);
            var res = await handler.Handle(new BuyStrategusItemCommand
            {
                HeroId = hero.Id,
                ItemId = item.Id,
                ItemCount = 1,
                SettlementId = settlement.Id,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.ItemNotBuyable, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfNotEnoughGold()
        {
            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock
                .Setup(m => m.ArePointsAtInteractionDistance(It.IsAny<Point>(), It.IsAny<Point>()))
                .Returns(true);

            var hero = new StrategusHero { Gold = 21, User = new User() };
            ArrangeDb.StrategusHeroes.Add(hero);
            var settlement = new StrategusSettlement { Culture = Culture.Empire };
            ArrangeDb.StrategusSettlements.Add(settlement);
            var item = new Item { Rank = 0, Culture = Culture.Empire, Value = 11 };
            ArrangeDb.Items.Add(item);
            await ArrangeDb.SaveChangesAsync();

            var handler = new BuyStrategusItemCommand.Handler(ActDb, Mapper, strategusMapMock.Object);
            var res = await handler.Handle(new BuyStrategusItemCommand
            {
                HeroId = hero.Id,
                ItemId = item.Id,
                ItemCount = 2,
                SettlementId = settlement.Id,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.NotEnoughGold, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldAddCountToAlreadyExistingHeroItems()
        {
            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock
                .Setup(m => m.ArePointsAtInteractionDistance(It.IsAny<Point>(), It.IsAny<Point>()))
                .Returns(true);

            var hero = new StrategusHero { Gold = 100, User = new User() };
            ArrangeDb.StrategusHeroes.Add(hero);
            var settlement = new StrategusSettlement { Culture = Culture.Sturgia };
            ArrangeDb.StrategusSettlements.Add(settlement);
            var item = new Item { Rank = 0, Culture = Culture.Sturgia, Value = 10 };
            ArrangeDb.Items.Add(item);
            var heroItem = new StrategusHeroItem { Item = item, Count = 3, Hero = hero };
            ArrangeDb.StrategusHeroItems.Add(heroItem);
            await ArrangeDb.SaveChangesAsync();

            var handler = new BuyStrategusItemCommand.Handler(ActDb, Mapper, strategusMapMock.Object);
            var res = await handler.Handle(new BuyStrategusItemCommand
            {
                HeroId = hero.Id,
                ItemId = item.Id,
                ItemCount = 4,
                SettlementId = settlement.Id,
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            Assert.AreEqual(item.Id, res.Data!.Item.Id);
            Assert.AreEqual(7, res.Data!.Count);

            hero = await AssertDb.StrategusHeroes.FirstAsync(h => h.Id == hero.Id);
            Assert.AreEqual(60, hero.Gold);
        }

        [Test]
        public async Task ShouldAddItemToHero()
        {
            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock
                .Setup(m => m.ArePointsAtInteractionDistance(It.IsAny<Point>(), It.IsAny<Point>()))
                .Returns(true);

            var hero = new StrategusHero { Gold = 100, User = new User() };
            ArrangeDb.StrategusHeroes.Add(hero);
            var settlement = new StrategusSettlement { Culture = Culture.Sturgia };
            ArrangeDb.StrategusSettlements.Add(settlement);
            var item = new Item { Rank = 0, Culture = Culture.Neutral, Value = 10 };
            ArrangeDb.Items.Add(item);
            await ArrangeDb.SaveChangesAsync();

            var handler = new BuyStrategusItemCommand.Handler(ActDb, Mapper, strategusMapMock.Object);
            var res = await handler.Handle(new BuyStrategusItemCommand
            {
                HeroId = hero.Id,
                ItemId = item.Id,
                ItemCount = 10,
                SettlementId = settlement.Id,
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            Assert.AreEqual(item.Id, res.Data!.Item.Id);
            Assert.AreEqual(10, res.Data!.Count);

            hero = await AssertDb.StrategusHeroes.FirstAsync(h => h.Id == hero.Id);
            Assert.AreEqual(0, hero.Gold);
        }
    }
}
