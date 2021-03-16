using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Strategus.Queries;
using Crpg.Domain.Entities.Strategus;
using Crpg.Domain.Entities.Users;
using Moq;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Strategus
{
    public class GetStrategusUpdateQueryTest : TestBase
    {
        [Test]
        public async Task ShouldReturnErrorIfNotFound()
        {
            var handler = new GetStrategusUpdateQuery.Handler(ActDb, Mapper, Mock.Of<IStrategusMap>());
            var res = await handler.Handle(new GetStrategusUpdateQuery
            {
                UserId = 1,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.UserNotFound, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfNotRegisteredToStrategus()
        {
            var user = new User();
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var handler = new GetStrategusUpdateQuery.Handler(ActDb, Mapper, Mock.Of<IStrategusMap>());
            var res = await handler.Handle(new GetStrategusUpdateQuery
            {
                UserId = user.Id,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.UserNotRegisteredToStrategus, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnStrategusUserWithSurroundings()
        {
            var user = new StrategusUser
            {
                Position = new Point(10, 10),
                User = new User(),
            };
            var closeUser = new StrategusUser
            {
                Position = new Point(9.9, 9.9),
                User = new User(),
            };
            var farUser = new StrategusUser
            {
                Position = new Point(1000, 1000),
                User = new User(),
            };
            ArrangeDb.StrategusUsers.AddRange(user, closeUser, farUser);

            var closeSettlement = new StrategusSettlement { Position = new Point(10.1, 10.1) };
            var farSettlement = new StrategusSettlement { Position = new Point(-1000, -1000) };
            ArrangeDb.StrategusSettlements.AddRange(closeSettlement, farSettlement);
            await ArrangeDb.SaveChangesAsync();

            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock.Setup(m => m.ViewDistance).Returns(50);

            var handler = new GetStrategusUpdateQuery.Handler(ActDb, Mapper, strategusMapMock.Object);
            var res = await handler.Handle(new GetStrategusUpdateQuery
            {
                UserId = user.UserId,
            }, CancellationToken.None);

            var update = res.Data!;
            Assert.IsNotNull(update);
            Assert.NotNull(update.User);
            Assert.AreEqual(1, update.VisibleUsers.Count);
            Assert.AreEqual(closeUser.UserId, update.VisibleUsers[0].Id);
            Assert.AreEqual(1, update.VisibleSettlements.Count);
            Assert.AreEqual(closeSettlement.Id, update.VisibleSettlements[0].Id);
        }
    }
}
