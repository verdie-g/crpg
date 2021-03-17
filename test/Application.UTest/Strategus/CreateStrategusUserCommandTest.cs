using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Strategus.Commands;
using Crpg.Application.Strategus.Queries;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Strategus;
using Crpg.Domain.Entities.Users;
using Moq;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Strategus
{
    public class CreateStrategusUserCommandTest : TestBase
    {
        [Test]
        public async Task ShouldReturnErrorIfNotFound()
        {
            var handler = new CreateStrategusUserCommand.Handler(ActDb, Mapper, Mock.Of<IStrategusMap>());
            var res = await handler.Handle(new CreateStrategusUserCommand
            {
                UserId = 1,
                Region = Region.NorthAmerica,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.UserNotFound, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfAlreadyRegistered()
        {
            var user = new User
            {
                StrategusUser = new StrategusUser()
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var handler = new CreateStrategusUserCommand.Handler(ActDb, Mapper, Mock.Of<IStrategusMap>());
            var res = await handler.Handle(new CreateStrategusUserCommand
            {
                UserId = user.Id,
                Region = Region.NorthAmerica,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.UserAlreadyRegisteredToStrategus, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldRegisterToStrategus()
        {
            var user = new User();
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock.Setup(sm => sm.GetSpawnPosition(Region.NorthAmerica)).Returns(new Point(150, 50));
            var handler = new CreateStrategusUserCommand.Handler(ActDb, Mapper, strategusMapMock.Object);
            var res = await handler.Handle(new CreateStrategusUserCommand
            {
                UserId = user.Id,
                Region = Region.NorthAmerica,
            }, CancellationToken.None);

            var strategusUser = res.Data!;
            Assert.IsNotNull(strategusUser);
            Assert.AreEqual(user.Id, strategusUser.Id);
            Assert.AreEqual(Region.NorthAmerica, strategusUser.Region);
            Assert.AreEqual(0, strategusUser.Silver);
            Assert.AreEqual(1, strategusUser.Troops);
            Assert.AreEqual(new Point(150.0, 50.0), strategusUser.Position);
            Assert.AreEqual(StrategusUserStatus.Idle, strategusUser.Status);
            Assert.AreEqual(0, strategusUser.Waypoints.Count);
            Assert.IsNull(strategusUser.TargetedUser);
            Assert.IsNull(strategusUser.TargetedSettlement);
        }
    }
}
