using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Battles.Commands;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Heroes;
using Crpg.Domain.Entities.Users;
using Moq;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Battles
{
    public class ApplyAsFighterToBattleCommandTest : TestBase
    {
        [Test]
        public async Task ShouldReturnErrorIfHeroNotFound()
        {
            ApplyAsFighterToBattleCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IStrategusMap>());
            var res = await handler.Handle(new()
            {
                HeroId = 1,
                BattleId = 2,
                Side = BattleSide.Attacker,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.HeroNotFound, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfHeroInBattle()
        {
            Hero hero = new() { Status = HeroStatus.InBattle, User = new User() };
            ArrangeDb.Heroes.Add(hero);
            await ArrangeDb.SaveChangesAsync();

            ApplyAsFighterToBattleCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IStrategusMap>());
            var res = await handler.Handle(new()
            {
                HeroId = hero.Id,
                BattleId = 2,
                Side = BattleSide.Attacker,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.HeroInBattle, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfBattleNotFound()
        {
            Hero hero = new() { User = new User() };
            ArrangeDb.Heroes.Add(hero);
            await ArrangeDb.SaveChangesAsync();

            ApplyAsFighterToBattleCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IStrategusMap>());
            var res = await handler.Handle(new()
            {
                HeroId = hero.Id,
                BattleId = 2,
                Side = BattleSide.Attacker,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.BattleNotFound, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfBattleNotInPreparation()
        {
            Hero hero = new()
            {
                Status = HeroStatus.Idle,
                Position = new Point(1, 2),
                User = new User(),
            };
            ArrangeDb.Heroes.Add(hero);
            Battle battle = new()
            {
                Phase = BattlePhase.Hiring,
                Position = new Point(3, 4),
            };
            ArrangeDb.Battles.Add(battle);
            await ArrangeDb.SaveChangesAsync();

            ApplyAsFighterToBattleCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IStrategusMap>());
            var res = await handler.Handle(new()
            {
                HeroId = hero.Id,
                BattleId = battle.Id,
                Side = BattleSide.Attacker,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.BattleInvalidPhase, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfHeroTooFarFromBattle()
        {
            Hero hero = new()
            {
                Status = HeroStatus.Idle,
                Position = new Point(1, 2),
                User = new User(),
            };
            ArrangeDb.Heroes.Add(hero);
            Battle battle = new()
            {
                Phase = BattlePhase.Preparation,
                Position = new Point(3, 4),
            };
            ArrangeDb.Battles.Add(battle);
            await ArrangeDb.SaveChangesAsync();

            var strategusMapMock = new Mock<IStrategusMap>(MockBehavior.Strict);
            strategusMapMock
                .Setup(m => m.ArePointsAtInteractionDistance(hero.Position, battle.Position))
                .Returns(false);

            ApplyAsFighterToBattleCommand.Handler handler = new(ActDb, Mapper, strategusMapMock.Object);
            var res = await handler.Handle(new()
            {
                HeroId = hero.Id,
                BattleId = battle.Id,
                Side = BattleSide.Attacker,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.BattleTooFar, res.Errors![0].Code);
        }

        [TestCase(BattleFighterApplicationStatus.Pending)]
        [TestCase(BattleFighterApplicationStatus.Accepted)]
        public async Task ShouldReturnExistingApplication(BattleFighterApplicationStatus existingApplicationStatus)
        {
            Hero hero = new()
            {
                Status = HeroStatus.Idle,
                Position = new Point(1, 2),
                User = new User(),
            };
            ArrangeDb.Heroes.Add(hero);
            Battle battle = new()
            {
                Phase = BattlePhase.Preparation,
                Position = new Point(3, 4),
            };
            ArrangeDb.Battles.Add(battle);
            FighterApplication existingApplication = new()
            {
                Side = BattleSide.Defender,
                Status = existingApplicationStatus,
                Battle = battle,
                Hero = hero,
            };
            ArrangeDb.BattleFighterApplications.Add(existingApplication);
            await ArrangeDb.SaveChangesAsync();

            var strategusMapMock = new Mock<IStrategusMap>(MockBehavior.Strict);
            strategusMapMock
                .Setup(m => m.ArePointsAtInteractionDistance(hero.Position, battle.Position))
                .Returns(true);

            ApplyAsFighterToBattleCommand.Handler handler = new(ActDb, Mapper, strategusMapMock.Object);
            var res = await handler.Handle(new()
            {
                HeroId = hero.Id,
                BattleId = battle.Id,
                Side = BattleSide.Defender,
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            Assert.AreEqual(existingApplication.Id, res.Data!.Id);
        }

        [Test]
        public async Task ShouldApply()
        {
            Hero hero = new()
            {
                Status = HeroStatus.Idle,
                Position = new Point(1, 2),
                User = new User(),
            };
            ArrangeDb.Heroes.Add(hero);
            Battle battle = new()
            {
                Phase = BattlePhase.Preparation,
                Position = new Point(3, 4),
            };
            ArrangeDb.Battles.Add(battle);
            await ArrangeDb.SaveChangesAsync();

            var strategusMapMock = new Mock<IStrategusMap>(MockBehavior.Strict);
            strategusMapMock
                .Setup(m => m.ArePointsAtInteractionDistance(hero.Position, battle.Position))
                .Returns(true);

            ApplyAsFighterToBattleCommand.Handler handler = new(ActDb, Mapper, strategusMapMock.Object);
            var res = await handler.Handle(new()
            {
                HeroId = hero.Id,
                BattleId = battle.Id,
                Side = BattleSide.Defender,
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            var application = res.Data!;
            Assert.NotZero(application.Id);
            Assert.AreEqual(hero.Id, application.Hero!.Id);
            Assert.AreEqual(BattleSide.Defender, application.Side);
            Assert.AreEqual(BattleFighterApplicationStatus.Pending, application.Status);
        }
    }
}
