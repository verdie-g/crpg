using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Heroes.Commands;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Heroes;
using Crpg.Domain.Entities.Users;
using Moq;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Heroes
{
    public class CreateHeroCommandTest : TestBase
    {
        private static readonly Constants Constants = new()
        {
            StrategusMinHeroTroops = 1,
        };

        [Test]
        public async Task ShouldReturnErrorIfNotFound()
        {
            var handler = new CreateHeroCommand.Handler(ActDb, Mapper, Mock.Of<IStrategusMap>(), Constants);
            var res = await handler.Handle(new CreateHeroCommand
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
                Hero = new Hero(),
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var handler = new CreateHeroCommand.Handler(ActDb, Mapper, Mock.Of<IStrategusMap>(), Constants);
            var res = await handler.Handle(new CreateHeroCommand
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
            var handler = new CreateHeroCommand.Handler(ActDb, Mapper, strategusMapMock.Object, Constants);
            var res = await handler.Handle(new CreateHeroCommand
            {
                UserId = user.Id,
                Region = Region.NorthAmerica,
            }, CancellationToken.None);

            var hero = res.Data!;
            Assert.IsNotNull(hero);
            Assert.AreEqual(user.Id, hero.Id);
            Assert.AreEqual(Region.NorthAmerica, hero.Region);
            Assert.AreEqual(0, hero.Gold);
            Assert.AreEqual(1, hero.Troops);
            Assert.AreEqual(new Point(150.0, 50.0), hero.Position);
            Assert.AreEqual(HeroStatus.Idle, hero.Status);
            Assert.AreEqual(0, hero.Waypoints.Count);
            Assert.IsNull(hero.TargetedHero);
            Assert.IsNull(hero.TargetedSettlement);
        }
    }
}
