using System;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Services;
using Crpg.Application.Heroes.Commands;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Heroes;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Moq;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Heroes
{
    public class UpdateHeroPositionsCommandTest : TestBase
    {
        private static readonly IStrategusSpeedModel SpeedModelMock = Mock.Of<IStrategusSpeedModel>();

        [Test]
        public async Task UsersMovingToPointShouldMove()
        {
            var position = new Point(1, 2);
            var destination = new Point(5, 6);
            var hero = new Hero
            {
                Status = HeroStatus.MovingToPoint,
                Position = position,
                Waypoints = new MultiPoint(new[] { destination }),
                User = new User(),
            };
            ArrangeDb.Heroes.Add(hero);
            await ArrangeDb.SaveChangesAsync();
            var newPosition = new Point(2, 3);
            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock
                .Setup(m => m.MovePointTowards(position, destination, It.IsAny<double>()))
                .Returns(newPosition);
            strategusMapMock
                .Setup(m => m.ArePointsEquivalent(newPosition, destination))
                .Returns(false);
            var handler = new UpdateHeroPositionsCommand.Handler(ActDb, strategusMapMock.Object, SpeedModelMock);
            await handler.Handle(new UpdateHeroPositionsCommand
            {
                DeltaTime = TimeSpan.FromMinutes(1),
            }, CancellationToken.None);

            hero = await AssertDb.Heroes.FirstAsync(u => u.Id == hero.Id);
            Assert.AreEqual(HeroStatus.MovingToPoint, hero.Status);
            Assert.AreEqual(newPosition, hero.Position);
            Assert.AreEqual(1, hero.Waypoints.Count);
        }

        [Test]
        public async Task ReachedWaypointShouldBeRemovedForMovingToPointUsers()
        {
            var position = new Point(1, 2);
            var destination = new Point(5, 6);
            var hero = new Hero
            {
                Status = HeroStatus.MovingToPoint,
                Position = position,
                Waypoints = new MultiPoint(new[] { destination, new Point(10, 10) }),
                User = new User(),
            };
            ArrangeDb.Heroes.Add(hero);
            await ArrangeDb.SaveChangesAsync();
            var newPosition = new Point(5, 5);
            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock
                .Setup(m => m.MovePointTowards(position, destination, It.IsAny<double>()))
                .Returns(newPosition);
            strategusMapMock
                .Setup(m => m.ArePointsEquivalent(newPosition, destination))
                .Returns(true);
            var handler = new UpdateHeroPositionsCommand.Handler(ActDb, strategusMapMock.Object, SpeedModelMock);
            await handler.Handle(new UpdateHeroPositionsCommand
            {
                DeltaTime = TimeSpan.FromMinutes(1),
            }, CancellationToken.None);

            hero = await AssertDb.Heroes.FirstAsync(u => u.Id == hero.Id);
            Assert.AreEqual(HeroStatus.MovingToPoint, hero.Status);
            Assert.AreEqual(newPosition, hero.Position);
            Assert.AreEqual(1, hero.Waypoints.Count);
        }

        [Test]
        public async Task MovingUsersShouldChangeToIdleIfLastWaypointReached()
        {
            var position = new Point(1, 2);
            var destination = new Point(5, 6);
            var hero = new Hero
            {
                Status = HeroStatus.MovingToPoint,
                Position = position,
                Waypoints = new MultiPoint(new[] { destination }),
                User = new User(),
            };
            ArrangeDb.Heroes.Add(hero);
            await ArrangeDb.SaveChangesAsync();

            var newPosition = new Point(5, 5);
            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock
                .Setup(m => m.MovePointTowards(position, destination, It.IsAny<double>()))
                .Returns(newPosition);
            strategusMapMock
                .Setup(m => m.ArePointsEquivalent(newPosition, destination))
                .Returns(true);
            var handler = new UpdateHeroPositionsCommand.Handler(ActDb, strategusMapMock.Object, SpeedModelMock);
            await handler.Handle(new UpdateHeroPositionsCommand
            {
                DeltaTime = TimeSpan.FromMinutes(1),
            }, CancellationToken.None);

            hero = await AssertDb.Heroes.FirstAsync(u => u.Id == hero.Id);
            Assert.AreEqual(HeroStatus.Idle, hero.Status);
            Assert.AreEqual(newPosition, hero.Position);
            Assert.AreEqual(0, hero.Waypoints.Count);
        }

        [TestCase(HeroStatus.FollowingHero)]
        [TestCase(HeroStatus.MovingToAttackHero)]
        public async Task ShouldStopIfMovingToAUserNotInSight(HeroStatus status)
        {
            var hero = new Hero
            {
                Status = status,
                Position = new Point(1, 2),
                TargetedHero = new Hero
                {
                    Position = new Point(5, 6),
                    User = new User(),
                },
                User = new User(),
            };
            ArrangeDb.Heroes.Add(hero);
            await ArrangeDb.SaveChangesAsync();

            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock.Setup(m => m.ViewDistance).Returns(0);
            var handler = new UpdateHeroPositionsCommand.Handler(ActDb, strategusMapMock.Object, SpeedModelMock);
            await handler.Handle(new UpdateHeroPositionsCommand
            {
                DeltaTime = TimeSpan.FromMinutes(1),
            }, CancellationToken.None);

            hero = await AssertDb.Heroes.FirstAsync(u => u.Id == hero.Id);
            Assert.AreEqual(HeroStatus.Idle, hero.Status);
            Assert.IsNull(hero.TargetedHeroId);
        }

        [TestCase(HeroStatus.FollowingHero)]
        [TestCase(HeroStatus.MovingToAttackHero)]
        public async Task MovingToAnotherUserShouldMove(HeroStatus status)
        {
            var position = new Point(1, 2);
            var destination = new Point(5, 6);
            var hero = new Hero
            {
                Status = status,
                Position = position,
                TargetedHero = new Hero
                {
                    Position = destination,
                    User = new User(),
                },
                User = new User(),
            };
            ArrangeDb.Heroes.Add(hero);
            await ArrangeDb.SaveChangesAsync();

            var newPosition = new Point(2, 3);
            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock.Setup(m => m.ViewDistance).Returns(500);
            strategusMapMock
                .Setup(m => m.MovePointTowards(position, destination, It.IsAny<double>()))
                .Returns(newPosition);
            if (status == HeroStatus.FollowingHero)
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

            var handler = new UpdateHeroPositionsCommand.Handler(ActDb, strategusMapMock.Object, SpeedModelMock);
            await handler.Handle(new UpdateHeroPositionsCommand
            {
                DeltaTime = TimeSpan.FromMinutes(1),
            }, CancellationToken.None);

            hero = await AssertDb.Heroes.FirstAsync(u => u.Id == hero.Id);
            Assert.AreEqual(status, hero.Status);
            Assert.AreEqual(newPosition, hero.Position);
        }

        [TestCase(HeroStatus.IdleInSettlement)]
        [TestCase(HeroStatus.RecruitingInSettlement)]
        [TestCase(HeroStatus.InBattle)]
        public async Task ShouldNotAttackHeroIfInAUnattackableStatus(HeroStatus targetHeroStatus)
        {
            var position = new Point(1, 2);
            var destination = new Point(5, 6);
            var targetHero = new Hero
            {
                Status = targetHeroStatus,
                Position = destination,
                User = new User(),
            };
            var hero = new Hero
            {
                Status = HeroStatus.MovingToAttackHero,
                Position = position,
                TargetedHero = targetHero,
                User = new User(),
            };
            ArrangeDb.Heroes.AddRange(hero, targetHero);
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
            var handler = new UpdateHeroPositionsCommand.Handler(ActDb, strategusMapMock.Object, SpeedModelMock);
            await handler.Handle(new UpdateHeroPositionsCommand
            {
                DeltaTime = TimeSpan.FromMinutes(1),
            }, CancellationToken.None);

            Assert.AreEqual(0, await AssertDb.Battles.CountAsync());
        }

        [Test]
        public async Task ShouldAttackHeroIfCloseEnough()
        {
            var position = new Point(1, 2);
            var destination = new Point(5, 6);
            var targetHero = new Hero
            {
                Status = HeroStatus.Idle,
                Position = destination,
                User = new User(),
            };
            var hero = new Hero
            {
                Status = HeroStatus.MovingToAttackHero,
                Position = position,
                TargetedHero = targetHero,
                User = new User(),
            };
            ArrangeDb.Heroes.AddRange(hero, targetHero);
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
            var handler = new UpdateHeroPositionsCommand.Handler(ActDb, strategusMapMock.Object, SpeedModelMock);
            await handler.Handle(new UpdateHeroPositionsCommand
            {
                DeltaTime = TimeSpan.FromMinutes(1),
            }, CancellationToken.None);

            var battle = await AssertDb.Battles
                .Include(b => b.Fighters).ThenInclude(f => f.Hero)
                .FirstOrDefaultAsync();
            Assert.IsNotNull(battle);
            Assert.AreEqual(BattlePhase.Preparation, battle.Phase);
            Assert.AreEqual(new Point(4, 5), battle.Position);
            Assert.AreEqual(2, battle.Fighters.Count);

            Assert.AreEqual(hero.Id, battle.Fighters[0].HeroId);
            Assert.AreEqual(HeroStatus.InBattle, battle.Fighters[0].Hero!.Status);
            Assert.AreEqual(BattleSide.Attacker, battle.Fighters[0].Side);
            Assert.IsTrue(battle.Fighters[0].Commander);

            Assert.AreEqual(targetHero.Id, battle.Fighters[1].HeroId);
            Assert.AreEqual(HeroStatus.InBattle, battle.Fighters[1].Hero!.Status);
            Assert.AreEqual(BattleSide.Defender, battle.Fighters[1].Side);
            Assert.IsTrue(battle.Fighters[1].Commander);
        }

        [TestCase(HeroStatus.MovingToSettlement)]
        [TestCase(HeroStatus.MovingToAttackSettlement)]
        public async Task MovingToASettlementShouldMove(HeroStatus status)
        {
            var position = new Point(1, 2);
            var destination = new Point(5, 6);
            var hero = new Hero
            {
                Status = status,
                Position = position,
                TargetedSettlement = new Settlement { Position = destination },
                User = new User(),
            };
            ArrangeDb.Heroes.Add(hero);
            await ArrangeDb.SaveChangesAsync();

            var newPosition = new Point(2, 3);
            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock
                .Setup(m => m.MovePointTowards(position, destination, It.IsAny<double>()))
                .Returns(newPosition);
            strategusMapMock
                .Setup(m => m.ArePointsAtInteractionDistance(newPosition, destination))
                .Returns(false);
            var handler = new UpdateHeroPositionsCommand.Handler(ActDb, strategusMapMock.Object, SpeedModelMock);
            await handler.Handle(new UpdateHeroPositionsCommand
            {
                DeltaTime = TimeSpan.FromMinutes(1),
            }, CancellationToken.None);

            hero = await AssertDb.Heroes.FirstAsync(u => u.Id == hero.Id);
            Assert.AreEqual(status, hero.Status);
            Assert.AreEqual(newPosition, hero.Position);
        }

        [Test]
        public async Task ShouldEnterSettlementIfCloseEnough()
        {
            var position = new Point(1, 2);
            var destination = new Point(5, 6);
            var hero = new Hero
            {
                Status = HeroStatus.MovingToSettlement,
                Position = position,
                TargetedSettlement = new Settlement { Position = destination },
                User = new User(),
            };
            ArrangeDb.Heroes.Add(hero);
            await ArrangeDb.SaveChangesAsync();

            var newPosition = new Point(5, 5);
            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock
                .Setup(m => m.MovePointTowards(position, destination, It.IsAny<double>()))
                .Returns(newPosition);
            strategusMapMock
                .Setup(m => m.ArePointsAtInteractionDistance(newPosition, destination))
                .Returns(true);
            var handler = new UpdateHeroPositionsCommand.Handler(ActDb, strategusMapMock.Object, SpeedModelMock);
            await handler.Handle(new UpdateHeroPositionsCommand
            {
                DeltaTime = TimeSpan.FromMinutes(1),
            }, CancellationToken.None);

            hero = await AssertDb.Heroes.FirstAsync(u => u.Id == hero.Id);
            Assert.AreEqual(HeroStatus.IdleInSettlement, hero.Status);
            Assert.AreEqual(destination, hero.Position);
        }

        [Test]
        public async Task ShouldNotAttackSettlementIfAlreadyInABattle()
        {
            var position = new Point(1, 2);
            var destination = new Point(5, 6);
            var settlement = new Settlement { Position = destination };
            var hero = new Hero
            {
                Status = HeroStatus.MovingToAttackSettlement,
                Position = position,
                TargetedSettlement = settlement,
                User = new User(),
            };
            ArrangeDb.Heroes.Add(hero);
            var battle = new Battle
            {
                Phase = BattlePhase.Preparation,
                Fighters =
                {
                    new BattleFighter
                    {
                        Hero = null,
                        Settlement = settlement,
                        Side = BattleSide.Defender,
                        Commander = true,
                    },
                },
            };
            ArrangeDb.Battles.Add(battle);
            await ArrangeDb.SaveChangesAsync();

            var newPosition = new Point(5, 5);
            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock
                .Setup(m => m.MovePointTowards(position, destination, It.IsAny<double>()))
                .Returns(newPosition);
            strategusMapMock
                .Setup(m => m.ArePointsAtInteractionDistance(newPosition, destination))
                .Returns(true);
            var handler = new UpdateHeroPositionsCommand.Handler(ActDb, strategusMapMock.Object, SpeedModelMock);
            await handler.Handle(new UpdateHeroPositionsCommand
            {
                DeltaTime = TimeSpan.FromMinutes(1),
            }, CancellationToken.None);

            Assert.AreEqual(1, await AssertDb.Battles.CountAsync());
        }

        [Test]
        public async Task ShouldAttackSettlementIfCloseEnough()
        {
            var position = new Point(1, 2);
            var destination = new Point(5, 6);
            var settlement = new Settlement
            {
                Region = Region.NorthAmerica,
                Position = destination,
            };
            var hero = new Hero
            {
                Region = Region.Europe,
                Status = HeroStatus.MovingToAttackSettlement,
                Position = position,
                TargetedSettlement = settlement,
                User = new User(),
            };
            ArrangeDb.Heroes.Add(hero);
            await ArrangeDb.SaveChangesAsync();

            var newPosition = new Point(3, 4);
            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock
                .Setup(m => m.MovePointTowards(position, destination, It.IsAny<double>()))
                .Returns(newPosition);
            strategusMapMock
                .Setup(m => m.ArePointsAtInteractionDistance(newPosition, destination))
                .Returns(true);
            var handler = new UpdateHeroPositionsCommand.Handler(ActDb, strategusMapMock.Object, SpeedModelMock);
            await handler.Handle(new UpdateHeroPositionsCommand
            {
                DeltaTime = TimeSpan.FromMinutes(1),
            }, CancellationToken.None);

            var battle = await AssertDb.Battles
                .Include(b => b.Fighters).ThenInclude(f => f.Hero)
                .FirstOrDefaultAsync();
            Assert.IsNotNull(battle);
            Assert.AreEqual(Region.NorthAmerica, battle.Region);
            Assert.AreEqual(BattlePhase.Preparation, battle.Phase);
            Assert.AreEqual(new Point(4, 5), battle.Position);

            Assert.AreEqual(2, battle.Fighters.Count);

            Assert.AreEqual(hero.Id, battle.Fighters[0].HeroId);
            Assert.AreEqual(HeroStatus.InBattle, battle.Fighters[0].Hero!.Status);
            Assert.IsNull(battle.Fighters[0].SettlementId);
            Assert.AreEqual(BattleSide.Attacker, battle.Fighters[0].Side);
            Assert.IsTrue(battle.Fighters[0].Commander);

            Assert.IsNull(battle.Fighters[1].HeroId);
            Assert.AreEqual(settlement.Id, battle.Fighters[1].SettlementId);
            Assert.AreEqual(BattleSide.Defender, battle.Fighters[1].Side);
            Assert.IsTrue(battle.Fighters[1].Commander);
        }
    }
}
