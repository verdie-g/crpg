using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Settlements.Queries;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Heroes;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Users;
using Moq;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Settlements
{
    public class GetSettlementShopItemsQueryTest : TestBase
    {
        [Test]
        public async Task ShouldReturnErrorIfUserNotFound()
        {
            var handler = new GetSettlementShopItemsQuery.Handler(ActDb, Mapper, Mock.Of<IStrategusMap>());
            var res = await handler.Handle(new GetSettlementShopItemsQuery
            {
                HeroId = 1,
                SettlementId = 2,
            }, CancellationToken.None);

            Assert.NotNull(res.Errors);
            Assert.AreEqual(ErrorCode.HeroNotFound, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfSettlementNotFound()
        {
            var hero = new Hero { User = new User() };
            ArrangeDb.Heroes.Add(hero);
            await ArrangeDb.SaveChangesAsync();

            var handler = new GetSettlementShopItemsQuery.Handler(ActDb, Mapper, Mock.Of<IStrategusMap>());
            var res = await handler.Handle(new GetSettlementShopItemsQuery
            {
                HeroId = hero.Id,
                SettlementId = 2,
            }, CancellationToken.None);

            Assert.NotNull(res.Errors);
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

            var hero = new Hero { Position = userPosition, User = new User() };
            ArrangeDb.Heroes.Add(hero);
            var settlement = new Settlement { Position = settlementPosition };
            ArrangeDb.Settlements.Add(settlement);
            await ArrangeDb.SaveChangesAsync();

            var handler = new GetSettlementShopItemsQuery.Handler(ActDb, Mapper, strategusMapMock.Object);
            var res = await handler.Handle(new GetSettlementShopItemsQuery
            {
                HeroId = hero.Id,
                SettlementId = settlement.Id,
            }, CancellationToken.None);

            Assert.NotNull(res.Errors);
            Assert.AreEqual(ErrorCode.SettlementTooFar, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnItemsForTheSettlementCulture()
        {
            var userPosition = new Point(1, 2);
            var settlementPosition = new Point(3, 4);

            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock
                .Setup(m => m.ArePointsAtInteractionDistance(userPosition, settlementPosition))
                .Returns(true);

            var hero = new Hero { Position = userPosition, User = new User() };
            ArrangeDb.Heroes.Add(hero);
            var settlement = new Settlement { Position = settlementPosition, Culture = Culture.Battania };
            ArrangeDb.Settlements.Add(settlement);
            var items = new[]
            {
                new Item { Culture = Culture.Aserai, Rank = 0 },
                new Item { Culture = Culture.Aserai, Rank = 1 },
                new Item { Culture = Culture.Battania, Rank = 2 },
                new Item { Culture = Culture.Battania, Rank = 0 },
                new Item { Culture = Culture.Battania, Rank = 0 },
            };
            ArrangeDb.Items.AddRange(items);
            await ArrangeDb.SaveChangesAsync();

            var handler = new GetSettlementShopItemsQuery.Handler(ActDb, Mapper, strategusMapMock.Object);
            var res = await handler.Handle(new GetSettlementShopItemsQuery
            {
                HeroId = hero.Id,
                SettlementId = settlement.Id,
            }, CancellationToken.None);

            Assert.Null(res.Errors);
            Assert.AreEqual(2, res.Data!.Count);
        }
    }
}
