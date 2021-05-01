using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Strategus.Commands;
using Crpg.Domain.Entities.Strategus;
using Crpg.Domain.Entities.Strategus.Battles;
using Crpg.Domain.Entities.Users;
using Moq;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Strategus
{
    public class ApplyAsFighterToStrategusBattleCommandTest : TestBase
    {
        [Test]
        public async Task ShouldReturnErrorIfHeroNotFound()
        {
            ApplyAsFighterToStrategusBattleCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IStrategusMap>());
            var res = await handler.Handle(new()
            {
                HeroId = 1,
                BattleId = 2,
                Side = StrategusBattleSide.Attacker,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.HeroNotFound, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfHeroInBattle()
        {
            StrategusHero hero = new() { Status = StrategusHeroStatus.InBattle, User = new User() };
            ArrangeDb.StrategusHeroes.Add(hero);
            await ArrangeDb.SaveChangesAsync();

            ApplyAsFighterToStrategusBattleCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IStrategusMap>());
            var res = await handler.Handle(new()
            {
                HeroId = hero.Id,
                BattleId = 2,
                Side = StrategusBattleSide.Attacker,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.HeroInBattle, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfBattleNotFound()
        {
            StrategusHero hero = new() { User = new User() };
            ArrangeDb.StrategusHeroes.Add(hero);
            await ArrangeDb.SaveChangesAsync();

            ApplyAsFighterToStrategusBattleCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IStrategusMap>());
            var res = await handler.Handle(new()
            {
                HeroId = hero.Id,
                BattleId = 2,
                Side = StrategusBattleSide.Attacker,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.BattleNotFound, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfBattleNotInPreparation()
        {
            StrategusHero hero = new()
            {
                Status = StrategusHeroStatus.Idle,
                Position = new Point(1, 2),
                User = new User(),
            };
            ArrangeDb.StrategusHeroes.Add(hero);
            StrategusBattle battle = new()
            {
                Phase = StrategusBattlePhase.Hiring,
                Position = new Point(3, 4),
            };
            ArrangeDb.StrategusBattles.Add(battle);
            await ArrangeDb.SaveChangesAsync();

            ApplyAsFighterToStrategusBattleCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IStrategusMap>());
            var res = await handler.Handle(new()
            {
                HeroId = hero.Id,
                BattleId = battle.Id,
                Side = StrategusBattleSide.Attacker,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.BattleInvalidPhase, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfHeroTooFarFromBattle()
        {
            StrategusHero hero = new()
            {
                Status = StrategusHeroStatus.Idle,
                Position = new Point(1, 2),
                User = new User(),
            };
            ArrangeDb.StrategusHeroes.Add(hero);
            StrategusBattle battle = new()
            {
                Phase = StrategusBattlePhase.Preparation,
                Position = new Point(3, 4),
            };
            ArrangeDb.StrategusBattles.Add(battle);
            await ArrangeDb.SaveChangesAsync();

            var strategusMapMock = new Mock<IStrategusMap>(MockBehavior.Strict);
            strategusMapMock
                .Setup(m => m.ArePointsAtInteractionDistance(hero.Position, battle.Position))
                .Returns(false);

            ApplyAsFighterToStrategusBattleCommand.Handler handler = new(ActDb, Mapper, strategusMapMock.Object);
            var res = await handler.Handle(new()
            {
                HeroId = hero.Id,
                BattleId = battle.Id,
                Side = StrategusBattleSide.Attacker,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.BattleTooFar, res.Errors![0].Code);
        }

        [TestCase(StrategusBattleFighterApplicationStatus.Pending)]
        [TestCase(StrategusBattleFighterApplicationStatus.Accepted)]
        public async Task ShouldReturnExistingApplication(StrategusBattleFighterApplicationStatus existingApplicationStatus)
        {
            StrategusHero hero = new()
            {
                Status = StrategusHeroStatus.Idle,
                Position = new Point(1, 2),
                User = new User(),
            };
            ArrangeDb.StrategusHeroes.Add(hero);
            StrategusBattle battle = new()
            {
                Phase = StrategusBattlePhase.Preparation,
                Position = new Point(3, 4),
            };
            ArrangeDb.StrategusBattles.Add(battle);
            StrategusBattleFighterApplication existingApplication = new()
            {
                Side = StrategusBattleSide.Defender,
                Status = existingApplicationStatus,
                Battle = battle,
                Hero = hero,
            };
            ArrangeDb.StrategusBattleFighterApplications.Add(existingApplication);
            await ArrangeDb.SaveChangesAsync();

            var strategusMapMock = new Mock<IStrategusMap>(MockBehavior.Strict);
            strategusMapMock
                .Setup(m => m.ArePointsAtInteractionDistance(hero.Position, battle.Position))
                .Returns(true);

            ApplyAsFighterToStrategusBattleCommand.Handler handler = new(ActDb, Mapper, strategusMapMock.Object);
            var res = await handler.Handle(new()
            {
                HeroId = hero.Id,
                BattleId = battle.Id,
                Side = StrategusBattleSide.Defender,
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            Assert.AreEqual(existingApplication.Id, res.Data!.Id);
        }

        [Test]
        public async Task ShouldApply()
        {
            StrategusHero hero = new()
            {
                Status = StrategusHeroStatus.Idle,
                Position = new Point(1, 2),
                User = new User(),
            };
            ArrangeDb.StrategusHeroes.Add(hero);
            StrategusBattle battle = new()
            {
                Phase = StrategusBattlePhase.Preparation,
                Position = new Point(3, 4),
            };
            ArrangeDb.StrategusBattles.Add(battle);
            await ArrangeDb.SaveChangesAsync();

            var strategusMapMock = new Mock<IStrategusMap>(MockBehavior.Strict);
            strategusMapMock
                .Setup(m => m.ArePointsAtInteractionDistance(hero.Position, battle.Position))
                .Returns(true);

            ApplyAsFighterToStrategusBattleCommand.Handler handler = new(ActDb, Mapper, strategusMapMock.Object);
            var res = await handler.Handle(new()
            {
                HeroId = hero.Id,
                BattleId = battle.Id,
                Side = StrategusBattleSide.Defender,
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            var application = res.Data!;
            Assert.NotZero(application.Id);
            Assert.AreEqual(hero.Id, application.Hero!.Id);
            Assert.AreEqual(StrategusBattleSide.Defender, application.Side);
            Assert.AreEqual(StrategusBattleFighterApplicationStatus.Pending, application.Status);
        }
    }
}
