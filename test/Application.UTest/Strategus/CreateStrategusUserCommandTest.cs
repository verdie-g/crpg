using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Strategus.Commands;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Strategus;
using Crpg.Domain.Entities.Users;
using Moq;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Strategus
{
    public class CreateStrategusHeroCommandTest : TestBase
    {
        private static readonly Constants Constants = new()
        {
            StrategusMinHeroTroops = 1,
        };

        [Test]
        public async Task ShouldReturnErrorIfNotFound()
        {
            var handler = new CreateStrategusHeroCommand.Handler(ActDb, Mapper, Mock.Of<IStrategusMap>(), Constants);
            var res = await handler.Handle(new CreateStrategusHeroCommand
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
                StrategusHero = new StrategusHero()
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var handler = new CreateStrategusHeroCommand.Handler(ActDb, Mapper, Mock.Of<IStrategusMap>(), Constants);
            var res = await handler.Handle(new CreateStrategusHeroCommand
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
            var handler = new CreateStrategusHeroCommand.Handler(ActDb, Mapper, strategusMapMock.Object, Constants);
            var res = await handler.Handle(new CreateStrategusHeroCommand
            {
                UserId = user.Id,
                Region = Region.NorthAmerica,
            }, CancellationToken.None);

            var strategusHero = res.Data!;
            Assert.IsNotNull(strategusHero);
            Assert.AreEqual(user.Id, strategusHero.Id);
            Assert.AreEqual(Region.NorthAmerica, strategusHero.Region);
            Assert.AreEqual(0, strategusHero.Gold);
            Assert.AreEqual(1, strategusHero.Troops);
            Assert.AreEqual(new Point(150.0, 50.0), strategusHero.Position);
            Assert.AreEqual(StrategusHeroStatus.Idle, strategusHero.Status);
            Assert.AreEqual(0, strategusHero.Waypoints.Count);
            Assert.IsNull(strategusHero.TargetedHero);
            Assert.IsNull(strategusHero.TargetedSettlement);
        }
    }
}
