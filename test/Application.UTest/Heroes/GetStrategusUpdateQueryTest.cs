using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Heroes.Queries;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Heroes;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Users;
using Moq;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Heroes
{
    public class GetStrategusUpdateQueryTest : TestBase
    {
        [Test]
        public async Task ShouldReturnErrorIfNotFound()
        {
            GetStrategusUpdateQuery.Handler handler = new(ActDb, Mapper, Mock.Of<IStrategusMap>());
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
            User user = new();
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            GetStrategusUpdateQuery.Handler handler = new(ActDb, Mapper, Mock.Of<IStrategusMap>());
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
            Hero hero = new()
            {
                Position = new Point(10, 10),
                User = new User(),
            };
            Hero closeHero = new()
            {
                Position = new Point(9.9, 9.9),
                User = new User(),
            };
            Hero closeHeroInBattle = new()
            {
                Position = new Point(9.8, 9.8),
                User = new User(),
                Status = HeroStatus.InBattle,
            };
            Hero farHero = new()
            {
                Position = new Point(1000, 1000),
                User = new User(),
            };
            Hero heroInSettlement = new()
            {
                Position = new Point(10.1, 10.1),
                Status = HeroStatus.IdleInSettlement,
                User = new User(),
            };
            ArrangeDb.Heroes.AddRange(hero, closeHero, farHero, heroInSettlement, closeHeroInBattle);

            Settlement closeSettlement = new() { Position = new Point(10.1, 10.1) };
            Settlement farSettlement = new() { Position = new Point(-1000, -1000) };
            ArrangeDb.Settlements.AddRange(closeSettlement, farSettlement);
            await ArrangeDb.SaveChangesAsync();

            Battle closeBattle = new() { Position = new Point(9.0, 9.0) };
            Battle closeEndedBattle = new()
            {
                Position = new Point(8.0, 8.0),
                Phase = BattlePhase.End,
            };
            Battle farBattle = new() { Position = new Point(-999, -999) };
            ArrangeDb.Battles.AddRange(closeBattle, closeEndedBattle, farBattle);
            await ArrangeDb.SaveChangesAsync();

            Mock<IStrategusMap> strategusMapMock = new();
            strategusMapMock.Setup(m => m.ViewDistance).Returns(50);

            GetStrategusUpdateQuery.Handler handler = new(ActDb, Mapper, strategusMapMock.Object);
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
            Assert.AreEqual(1, update.VisibleBattles.Count);
            Assert.AreEqual(closeBattle.Id, update.VisibleBattles[0].Id);
        }
    }
}
