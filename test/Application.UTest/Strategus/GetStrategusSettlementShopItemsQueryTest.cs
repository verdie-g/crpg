using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Strategus.Queries;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Strategus;
using Crpg.Domain.Entities.Users;
using Moq;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Strategus
{
    public class GetStrategusSettlementShopItemsQueryTest : TestBase
    {
        [Test]
        public async Task ShouldReturnErrorIfUserNotFound()
        {
            var handler = new GetStrategusSettlementShopItemsQuery.Handler(ActDb, Mapper, Mock.Of<IStrategusMap>());
            var res = await handler.Handle(new GetStrategusSettlementShopItemsQuery
            {
                UserId = 1,
                SettlementId = 2,
            }, CancellationToken.None);

            Assert.NotNull(res.Errors);
            Assert.AreEqual(ErrorCode.UserNotFound, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfSettlementNotFound()
        {
            var user = new StrategusUser { User = new User() };
            ArrangeDb.StrategusUsers.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var handler = new GetStrategusSettlementShopItemsQuery.Handler(ActDb, Mapper, Mock.Of<IStrategusMap>());
            var res = await handler.Handle(new GetStrategusSettlementShopItemsQuery
            {
                UserId = user.UserId,
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

            var user = new StrategusUser { Position = userPosition, User = new User() };
            ArrangeDb.StrategusUsers.Add(user);
            var settlement = new StrategusSettlement { Position = settlementPosition };
            ArrangeDb.StrategusSettlements.Add(settlement);
            await ArrangeDb.SaveChangesAsync();

            var handler = new GetStrategusSettlementShopItemsQuery.Handler(ActDb, Mapper, strategusMapMock.Object);
            var res = await handler.Handle(new GetStrategusSettlementShopItemsQuery
            {
                UserId = user.UserId,
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

            var user = new StrategusUser { Position = userPosition, User = new User() };
            ArrangeDb.StrategusUsers.Add(user);
            var settlement = new StrategusSettlement { Position = settlementPosition, Culture = Culture.Battania };
            ArrangeDb.StrategusSettlements.Add(settlement);
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

            var handler = new GetStrategusSettlementShopItemsQuery.Handler(ActDb, Mapper, strategusMapMock.Object);
            var res = await handler.Handle(new GetStrategusSettlementShopItemsQuery
            {
                UserId = user.UserId,
                SettlementId = settlement.Id,
            }, CancellationToken.None);

            Assert.Null(res.Errors);
            Assert.AreEqual(2, res.Data!.Count);
        }
    }
}
