using System;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Services;
using Crpg.Application.Strategus.Commands;
using Crpg.Domain.Entities.Strategus;
using Crpg.Domain.Entities.Strategus.Battles;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Moq;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Strategus
{
    public class UpdateStrategusHeroPositionsCommandTest : TestBase
    {
        [Test]
        public async Task UsersMovingToPointShouldMove()
        {
            var position = new Point(1, 2);
            var destination = new Point(5, 6);
            var strategusHero = new StrategusHero
            {
                Status = StrategusHeroStatus.MovingToPoint,
                Position = position,
                Waypoints = new MultiPoint(new[] { destination }),
                User = new User(),
            };
            ArrangeDb.StrategusHeroes.Add(strategusHero);
            await ArrangeDb.SaveChangesAsync();

            var newPosition = new Point(2, 3);
            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock
                .Setup(m => m.MovePointTowards(position, destination, It.IsAny<double>()))
                .Returns(newPosition);
            strategusMapMock
                .Setup(m => m.ArePointsEquivalent(newPosition, destination))
                .Returns(false);
            var handler = new UpdateStrategusHeroPositionsCommand.Handler(ActDb, strategusMapMock.Object);
            await handler.Handle(new UpdateStrategusHeroPositionsCommand
            {
                DeltaTime = TimeSpan.FromMinutes(1),
            }, CancellationToken.None);

            strategusHero = await AssertDb.StrategusHeroes.FirstAsync(u => u.Id == strategusHero.Id);
            Assert.AreEqual(StrategusHeroStatus.MovingToPoint, strategusHero.Status);
            Assert.AreEqual(newPosition, strategusHero.Position);
            Assert.AreEqual(1, strategusHero.Waypoints.Count);
        }

        [Test]
        public async Task ReachedWaypointShouldBeRemovedForMovingToPointUsers()
        {
            var position = new Point(1, 2);
            var destination = new Point(5, 6);
            var strategusHero = new StrategusHero
            {
                Status = StrategusHeroStatus.MovingToPoint,
                Position = position,
                Waypoints = new MultiPoint(new[] { destination, new Point(10, 10) }),
                User = new User(),
            };
            ArrangeDb.StrategusHeroes.Add(strategusHero);
            await ArrangeDb.SaveChangesAsync();

            var newPosition = new Point(5, 5);
            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock
                .Setup(m => m.MovePointTowards(position, destination, It.IsAny<double>()))
                .Returns(newPosition);
            strategusMapMock
                .Setup(m => m.ArePointsEquivalent(newPosition, destination))
                .Returns(true);
            var handler = new UpdateStrategusHeroPositionsCommand.Handler(ActDb, strategusMapMock.Object);
            await handler.Handle(new UpdateStrategusHeroPositionsCommand
            {
                DeltaTime = TimeSpan.FromMinutes(1),
            }, CancellationToken.None);

            strategusHero = await AssertDb.StrategusHeroes.FirstAsync(u => u.Id == strategusHero.Id);
            Assert.AreEqual(StrategusHeroStatus.MovingToPoint, strategusHero.Status);
            Assert.AreEqual(newPosition, strategusHero.Position);
            Assert.AreEqual(1, strategusHero.Waypoints.Count);
        }

        [Test]
        public async Task MovingUsersShouldChangeToIdleIfLastWaypointReached()
        {
            var position = new Point(1, 2);
            var destination = new Point(5, 6);
            var strategusHero = new StrategusHero
            {
                Status = StrategusHeroStatus.MovingToPoint,
                Position = position,
                Waypoints = new MultiPoint(new[] { destination }),
                User = new User(),
            };
            ArrangeDb.StrategusHeroes.Add(strategusHero);
            await ArrangeDb.SaveChangesAsync();

            var newPosition = new Point(5, 5);
            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock
                .Setup(m => m.MovePointTowards(position, destination, It.IsAny<double>()))
                .Returns(newPosition);
            strategusMapMock
                .Setup(m => m.ArePointsEquivalent(newPosition, destination))
                .Returns(true);
            var handler = new UpdateStrategusHeroPositionsCommand.Handler(ActDb, strategusMapMock.Object);
            await handler.Handle(new UpdateStrategusHeroPositionsCommand
            {
                DeltaTime = TimeSpan.FromMinutes(1),
            }, CancellationToken.None);

            strategusHero = await AssertDb.StrategusHeroes.FirstAsync(u => u.Id == strategusHero.Id);
            Assert.AreEqual(StrategusHeroStatus.Idle, strategusHero.Status);
            Assert.AreEqual(newPosition, strategusHero.Position);
            Assert.AreEqual(0, strategusHero.Waypoints.Count);
        }

        [TestCase(StrategusHeroStatus.FollowingHero)]
        [TestCase(StrategusHeroStatus.MovingToAttackHero)]
        public async Task ShouldStopIfMovingToAUserNotInSight(StrategusHeroStatus status)
        {
            var strategusHero = new StrategusHero
            {
                Status = status,
                Position = new Point(1, 2),
                TargetedHero = new StrategusHero
                {
                    Position = new Point(5, 6),
                    User = new User(),
                },
                User = new User(),
            };
            ArrangeDb.StrategusHeroes.Add(strategusHero);
            await ArrangeDb.SaveChangesAsync();

            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock.Setup(m => m.ViewDistance).Returns(0);

            var handler = new UpdateStrategusHeroPositionsCommand.Handler(ActDb, strategusMapMock.Object);
            await handler.Handle(new UpdateStrategusHeroPositionsCommand
            {
                DeltaTime = TimeSpan.FromMinutes(1),
            }, CancellationToken.None);

            strategusHero = await AssertDb.StrategusHeroes.FirstAsync(u => u.Id == strategusHero.Id);
            Assert.AreEqual(StrategusHeroStatus.Idle, strategusHero.Status);
            Assert.IsNull(strategusHero.TargetedHeroId);
        }

        [TestCase(StrategusHeroStatus.FollowingHero)]
        [TestCase(StrategusHeroStatus.MovingToAttackHero)]
        public async Task MovingToAnotherUserShouldMove(StrategusHeroStatus status)
        {
            var position = new Point(1, 2);
            var destination = new Point(5, 6);
            var strategusHero = new StrategusHero
            {
                Status = status,
                Position = position,
                TargetedHero = new StrategusHero
                {
                    Position = destination,
                    User = new User(),
                },
                User = new User(),
            };
            ArrangeDb.StrategusHeroes.Add(strategusHero);
            await ArrangeDb.SaveChangesAsync();

            var newPosition = new Point(2, 3);
            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock.Setup(m => m.ViewDistance).Returns(500);
            strategusMapMock
                .Setup(m => m.MovePointTowards(position, destination, It.IsAny<double>()))
                .Returns(newPosition);
            if (status == StrategusHeroStatus.FollowingHero)
            {
                strategusMapMock
                    .Setup(m => m.ArePointsEquivalent(newPosition, destination))
                    .Returns(false);
            }
            else
            {
                strategusMapMock
                    .Setup(m => m.ArePointsAtInteractionDistance(newPosition, destination))
                    .Returns(false);
            }

            var handler = new UpdateStrategusHeroPositionsCommand.Handler(ActDb, strategusMapMock.Object);
            await handler.Handle(new UpdateStrategusHeroPositionsCommand
            {
                DeltaTime = TimeSpan.FromMinutes(1),
            }, CancellationToken.None);

            strategusHero = await AssertDb.StrategusHeroes.FirstAsync(u => u.Id == strategusHero.Id);
            Assert.AreEqual(status, strategusHero.Status);
            Assert.AreEqual(newPosition, strategusHero.Position);
        }

        [TestCase(StrategusHeroStatus.IdleInSettlement)]
        [TestCase(StrategusHeroStatus.RecruitingInSettlement)]
        [TestCase(StrategusHeroStatus.InBattle)]
        public async Task ShouldNotAttackHeroIfInAUnattackableStatus(StrategusHeroStatus targetHeroStatus)
        {
            var position = new Point(1, 2);
            var destination = new Point(5, 6);
            var targetHero = new StrategusHero
            {
                Status = targetHeroStatus,
                Position = destination,
                User = new User(),
            };
            var hero = new StrategusHero
            {
                Status = StrategusHeroStatus.MovingToAttackHero,
                Position = position,
                TargetedHero = targetHero,
                User = new User(),
            };
            ArrangeDb.StrategusHeroes.AddRange(hero, targetHero);
            await ArrangeDb.SaveChangesAsync();

            var newPosition = new Point(2, 3);
            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock.Setup(m => m.ViewDistance).Returns(500);
            strategusMapMock
                .Setup(m => m.MovePointTowards(position, destination, It.IsAny<double>()))
                .Returns(newPosition);
            strategusMapMock
                .Setup(m => m.ArePointsAtInteractionDistance(newPosition, destination))
                .Returns(true);

            var handler = new UpdateStrategusHeroPositionsCommand.Handler(ActDb, strategusMapMock.Object);
            await handler.Handle(new UpdateStrategusHeroPositionsCommand
            {
                DeltaTime = TimeSpan.FromMinutes(1),
            }, CancellationToken.None);

            Assert.AreEqual(0, await AssertDb.StrategusBattles.CountAsync());
        }

        [Test]
        public async Task ShouldAttackHeroIfCloseEnough()
        {
            var position = new Point(1, 2);
            var destination = new Point(5, 6);
            var targetHero = new StrategusHero
            {
                Status = StrategusHeroStatus.Idle,
                Position = destination,
                User = new User(),
            };
            var hero = new StrategusHero
            {
                Status = StrategusHeroStatus.MovingToAttackHero,
                Position = position,
                TargetedHero = targetHero,
                User = new User(),
            };
            ArrangeDb.StrategusHeroes.AddRange(hero, targetHero);
            await ArrangeDb.SaveChangesAsync();

            var newPosition = new Point(3, 4);
            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock.Setup(m => m.ViewDistance).Returns(500);
            strategusMapMock
                .Setup(m => m.MovePointTowards(position, destination, It.IsAny<double>()))
                .Returns(newPosition);
            strategusMapMock
                .Setup(m => m.ArePointsAtInteractionDistance(newPosition, destination))
                .Returns(true);

            var handler = new UpdateStrategusHeroPositionsCommand.Handler(ActDb, strategusMapMock.Object);
            await handler.Handle(new UpdateStrategusHeroPositionsCommand
            {
                DeltaTime = TimeSpan.FromMinutes(1),
            }, CancellationToken.None);

            var battle = await AssertDb.StrategusBattles
                .Include(b => b.AttackedSettlement)
                .Include(b => b.Fighters).ThenInclude(f => f.Hero)
                .FirstOrDefaultAsync();
            Assert.IsNotNull(battle);
            Assert.AreEqual(StrategusBattlePhase.Preparation, battle.Phase);
            Assert.AreEqual(new Point(4, 5), battle.Position);
            Assert.IsNull(battle.AttackedSettlementId);
            Assert.AreEqual(2, battle.Fighters.Count);

            Assert.AreEqual(hero.Id, battle.Fighters[0].HeroId);
            Assert.AreEqual(StrategusHeroStatus.InBattle, battle.Fighters[0].Hero!.Status);
            Assert.AreEqual(StrategusBattleSide.Attacker, battle.Fighters[0].Side);
            Assert.IsTrue(battle.Fighters[0].MainFighter);

            Assert.AreEqual(targetHero.Id, battle.Fighters[1].HeroId);
            Assert.AreEqual(StrategusHeroStatus.InBattle, battle.Fighters[1].Hero!.Status);
            Assert.AreEqual(StrategusBattleSide.Defender, battle.Fighters[1].Side);
            Assert.IsTrue(battle.Fighters[1].MainFighter);
        }

        [TestCase(StrategusHeroStatus.MovingToSettlement)]
        [TestCase(StrategusHeroStatus.MovingToAttackSettlement)]
        public async Task MovingToASettlementShouldMove(StrategusHeroStatus status)
        {
            var position = new Point(1, 2);
            var destination = new Point(5, 6);
            var strategusHero = new StrategusHero
            {
                Status = status,
                Position = position,
                TargetedSettlement = new StrategusSettlement { Position = destination },
                User = new User(),
            };
            ArrangeDb.StrategusHeroes.Add(strategusHero);
            await ArrangeDb.SaveChangesAsync();

            var newPosition = new Point(2, 3);
            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock
                .Setup(m => m.MovePointTowards(position, destination, It.IsAny<double>()))
                .Returns(newPosition);
            strategusMapMock
                .Setup(m => m.ArePointsAtInteractionDistance(newPosition, destination))
                .Returns(false);
            var handler = new UpdateStrategusHeroPositionsCommand.Handler(ActDb, strategusMapMock.Object);
            await handler.Handle(new UpdateStrategusHeroPositionsCommand
            {
                DeltaTime = TimeSpan.FromMinutes(1),
            }, CancellationToken.None);

            strategusHero = await AssertDb.StrategusHeroes.FirstAsync(u => u.Id == strategusHero.Id);
            Assert.AreEqual(status, strategusHero.Status);
            Assert.AreEqual(newPosition, strategusHero.Position);
        }

        [Test]
        public async Task ShouldEnterSettlementIfCloseEnough()
        {
            var position = new Point(1, 2);
            var destination = new Point(5, 6);
            var strategusHero = new StrategusHero
            {
                Status = StrategusHeroStatus.MovingToSettlement,
                Position = position,
                TargetedSettlement = new StrategusSettlement { Position = destination },
                User = new User(),
            };
            ArrangeDb.StrategusHeroes.Add(strategusHero);
            await ArrangeDb.SaveChangesAsync();

            var newPosition = new Point(5, 5);
            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock
                .Setup(m => m.MovePointTowards(position, destination, It.IsAny<double>()))
                .Returns(newPosition);
            strategusMapMock
                .Setup(m => m.ArePointsAtInteractionDistance(newPosition, destination))
                .Returns(true);
            var handler = new UpdateStrategusHeroPositionsCommand.Handler(ActDb, strategusMapMock.Object);
            await handler.Handle(new UpdateStrategusHeroPositionsCommand
            {
                DeltaTime = TimeSpan.FromMinutes(1),
            }, CancellationToken.None);

            strategusHero = await AssertDb.StrategusHeroes.FirstAsync(u => u.Id == strategusHero.Id);
            Assert.AreEqual(StrategusHeroStatus.IdleInSettlement, strategusHero.Status);
            Assert.AreEqual(destination, strategusHero.Position);
        }

        [Test]
        public async Task ShouldNotAttackSettlementIfAlreadyInABattle()
        {
            var position = new Point(1, 2);
            var destination = new Point(5, 6);
            var settlement = new StrategusSettlement { Position = destination };
            var hero = new StrategusHero
            {
                Status = StrategusHeroStatus.MovingToAttackSettlement,
                Position = position,
                TargetedSettlement = settlement,
                User = new User(),
            };
            ArrangeDb.StrategusHeroes.Add(hero);
            var battle = new StrategusBattle
            {
                Phase = StrategusBattlePhase.Preparation,
                AttackedSettlement = settlement,
            };
            ArrangeDb.StrategusBattles.Add(battle);
            await ArrangeDb.SaveChangesAsync();

            var newPosition = new Point(5, 5);
            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock
                .Setup(m => m.MovePointTowards(position, destination, It.IsAny<double>()))
                .Returns(newPosition);
            strategusMapMock
                .Setup(m => m.ArePointsAtInteractionDistance(newPosition, destination))
                .Returns(true);
            var handler = new UpdateStrategusHeroPositionsCommand.Handler(ActDb, strategusMapMock.Object);
            await handler.Handle(new UpdateStrategusHeroPositionsCommand
            {
                DeltaTime = TimeSpan.FromMinutes(1),
            }, CancellationToken.None);

            Assert.AreEqual(1, await AssertDb.StrategusBattles.CountAsync());
        }

        [Test]
        public async Task ShouldAttackSettlementIfCloseEnough()
        {
            var position = new Point(1, 2);
            var destination = new Point(5, 6);
            var settlement = new StrategusSettlement { Position = destination };
            var hero = new StrategusHero
            {
                Status = StrategusHeroStatus.MovingToAttackSettlement,
                Position = position,
                TargetedSettlement = settlement,
                User = new User(),
            };
            ArrangeDb.StrategusHeroes.Add(hero);
            await ArrangeDb.SaveChangesAsync();

            var newPosition = new Point(3, 4);
            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock
                .Setup(m => m.MovePointTowards(position, destination, It.IsAny<double>()))
                .Returns(newPosition);
            strategusMapMock
                .Setup(m => m.ArePointsAtInteractionDistance(newPosition, destination))
                .Returns(true);
            var handler = new UpdateStrategusHeroPositionsCommand.Handler(ActDb, strategusMapMock.Object);
            await handler.Handle(new UpdateStrategusHeroPositionsCommand
            {
                DeltaTime = TimeSpan.FromMinutes(1),
            }, CancellationToken.None);

            var battle = await AssertDb.StrategusBattles
                .Include(b => b.AttackedSettlement)
                .Include(b => b.Fighters).ThenInclude(f => f.Hero)
                .FirstOrDefaultAsync();
            Assert.IsNotNull(battle);
            Assert.AreEqual(StrategusBattlePhase.Preparation, battle.Phase);
            Assert.AreEqual(new Point(4, 5), battle.Position);
            Assert.AreEqual(settlement.Id, battle.AttackedSettlementId);
            Assert.AreEqual(1, battle.Fighters.Count);
            Assert.AreEqual(hero.Id, battle.Fighters[0].HeroId);
            Assert.AreEqual(StrategusHeroStatus.InBattle, battle.Fighters[0].Hero!.Status);
            Assert.AreEqual(StrategusBattleSide.Attacker, battle.Fighters[0].Side);
            Assert.IsTrue(battle.Fighters[0].MainFighter);
        }
    }
}
