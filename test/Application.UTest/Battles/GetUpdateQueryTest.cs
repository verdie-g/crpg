using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Heroes.Queries;
using Crpg.Domain.Entities.Heroes;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Users;
using Moq;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Battles
{
    public class GetUpdateQueryTest : TestBase
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
        public async Task ShouldReturnHeroWithWhatsVisible()
        {
            var hero = new Hero
            {
                Position = new Point(10, 10),
                User = new User(),
            };
            var closeHero = new Hero
            {
                Position = new Point(9.9, 9.9),
                User = new User(),
            };
            var farHero = new Hero
            {
                Position = new Point(1000, 1000),
                User = new User(),
            };
            var heroInSettlement = new Hero
            {
                Position = new Point(10.1, 10.1),
                Status = HeroStatus.IdleInSettlement,
                User = new User(),
            };
            ArrangeDb.Heroes.AddRange(hero, closeHero, farHero, heroInSettlement);

            var closeSettlement = new Settlement { Position = new Point(10.1, 10.1) };
            var farSettlement = new Settlement { Position = new Point(-1000, -1000) };
            ArrangeDb.Settlements.AddRange(closeSettlement, farSettlement);
            await ArrangeDb.SaveChangesAsync();

            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock.Setup(m => m.ViewDistance).Returns(50);

            var handler = new GetStrategusUpdateQuery.Handler(ActDb, Mapper, strategusMapMock.Object);
            var res = await handler.Handle(new GetStrategusUpdateQuery
            {
                HeroId = hero.Id,
            }, CancellationToken.None);

            var update = res.Data!;
            Assert.IsNotNull(update);
            Assert.NotNull(update.Hero);
            Assert.AreEqual(1, update.VisibleHeroes.Count);
            Assert.AreEqual(closeHero.Id, update.VisibleHeroes[0].Id);
            Assert.AreEqual(1, update.VisibleSettlements.Count);
            Assert.AreEqual(closeSettlement.Id, update.VisibleSettlements[0].Id);
        }
    }
}
