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
                HeroId = 1,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.HeroNotFound, res.Errors![0].Code);
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
                HeroId = user.Id,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.HeroNotFound, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnStrategusHeroWithWhatsVisible()
        {
            var hero = new StrategusHero
            {
                Position = new Point(10, 10),
                User = new User(),
            };
            var closeHero = new StrategusHero
            {
                Position = new Point(9.9, 9.9),
                User = new User(),
            };
            var farHero = new StrategusHero
            {
                Position = new Point(1000, 1000),
                User = new User(),
            };
            var heroInSettlement = new StrategusHero
            {
                Position = new Point(10.1, 10.1),
                Status = StrategusHeroStatus.IdleInSettlement,
                User = new User(),
            };
            ArrangeDb.StrategusHeroes.AddRange(hero, closeHero, farHero, heroInSettlement);

            var closeSettlement = new StrategusSettlement { Position = new Point(10.1, 10.1) };
            var farSettlement = new StrategusSettlement { Position = new Point(-1000, -1000) };
            ArrangeDb.StrategusSettlements.AddRange(closeSettlement, farSettlement);
            await ArrangeDb.SaveChangesAsync();

            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock.Setup(m => m.ViewDistance).Returns(50);

            var handler = new GetStrategusUpdateQuery.Handler(ActDb, Mapper, strategusMapMock.Object);
            var res = await handler.Handle(new GetStrategusUpdateQuery
            {
                HeroId = hero.UserId,
            }, CancellationToken.None);

            var update = res.Data!;
            Assert.IsNotNull(update);
            Assert.NotNull(update.User);
            Assert.AreEqual(1, update.VisibleHeroes.Count);
            Assert.AreEqual(closeHero.UserId, update.VisibleHeroes[0].Id);
            Assert.AreEqual(1, update.VisibleSettlements.Count);
            Assert.AreEqual(closeSettlement.Id, update.VisibleSettlements[0].Id);
        }
    }
}
