using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Strategus.Commands;
using Crpg.Domain.Entities.Strategus;
using Crpg.Domain.Entities.Users;
using Moq;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Strategus
{
    public class UpdateStrategusHeroStatusCommandTest : TestBase
    {
        [Test]
        public async Task ShouldReturnErrorIfNotFound()
        {
            var handler = new UpdateStrategusHeroStatusCommand.Handler(ActDb, Mapper, Mock.Of<IStrategusMap>());
            var res = await handler.Handle(new UpdateStrategusHeroStatusCommand
            {
                HeroId = 1,
                Waypoints = MultiPoint.Empty,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.HeroNotFound, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorUserNotRegisteredToStrategus()
        {
            var user = new User();
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateStrategusHeroStatusCommand.Handler(ActDb, Mapper, Mock.Of<IStrategusMap>());
            var res = await handler.Handle(new UpdateStrategusHeroStatusCommand
            {
                HeroId = user.Id,
                Waypoints = MultiPoint.Empty,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.HeroNotFound, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfUserIsInBattle()
        {
            var user = new User
            {
                StrategusHero = new StrategusHero
                {
                    Status = StrategusHeroStatus.InBattle,
                },
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateStrategusHeroStatusCommand.Handler(ActDb, Mapper, Mock.Of<IStrategusMap>());
            var res = await handler.Handle(new UpdateStrategusHeroStatusCommand
            {
                HeroId = user.Id,
                Waypoints = new MultiPoint(new[]
                {
                    new Point(4, 5),
                    new Point(6, 7),
                }),
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.HeroInBattle, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldClearMovementIfStatusIdle()
        {
            var user = new User
            {
                StrategusHero = new StrategusHero
                {
                    Status = StrategusHeroStatus.MovingToSettlement,
                    // Doesn't make sense that the 3 properties are set but it's just for the test.
                    Waypoints = new MultiPoint(new[] { new Point(5, 3) }),
                    TargetedHero = new StrategusHero { User = new User() },
                    TargetedSettlement = new StrategusSettlement(),
                },
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateStrategusHeroStatusCommand.Handler(ActDb, Mapper, Mock.Of<IStrategusMap>());
            var res = await handler.Handle(new UpdateStrategusHeroStatusCommand
            {
                HeroId = user.Id,
                Status = StrategusHeroStatus.Idle,
            }, CancellationToken.None);

            var strategusHero = res.Data!;
            Assert.IsNotNull(strategusHero);
            Assert.AreEqual(user.Id, strategusHero.Id);
            Assert.AreEqual(StrategusHeroStatus.Idle, strategusHero.Status);
            Assert.AreEqual(0, strategusHero.Waypoints.Count);
            Assert.IsNull(strategusHero.TargetedHero);
            Assert.IsNull(strategusHero.TargetedSettlement);
        }

        [Test]
        public async Task ShouldUpdateStrategusHeroMovementIfStatusMovingToPoint()
        {
            var user = new User
            {
                StrategusHero = new StrategusHero
                {
                    Status = StrategusHeroStatus.Idle,
                    Waypoints = new MultiPoint(new[] { new Point(3, 4) }),
                },
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateStrategusHeroStatusCommand.Handler(ActDb, Mapper, Mock.Of<IStrategusMap>());
            var res = await handler.Handle(new UpdateStrategusHeroStatusCommand
            {
                HeroId = user.Id,
                Status = StrategusHeroStatus.MovingToPoint,
                Waypoints = new MultiPoint(new[]
                {
                    new Point(4, 5),
                    new Point(6, 7),
                }),
            }, CancellationToken.None);

            var strategusHero = res.Data!;
            Assert.IsNotNull(strategusHero);
            Assert.AreEqual(user.Id, strategusHero.Id);
            Assert.AreEqual(StrategusHeroStatus.MovingToPoint, strategusHero.Status);
            Assert.AreEqual(2, strategusHero.Waypoints.Count);
            Assert.AreEqual(new Point(4, 5), strategusHero.Waypoints[0]);
            Assert.AreEqual(new Point(6, 7), strategusHero.Waypoints[1]);
        }

        [TestCase(StrategusHeroStatus.FollowingHero)]
        [TestCase(StrategusHeroStatus.MovingToAttackHero)]
        public async Task ShouldReturnErrorIfTargetingNotExistingUser(StrategusHeroStatus status)
        {
            var user = new User
            {
                StrategusHero = new StrategusHero
                {
                    Status = StrategusHeroStatus.MovingToAttackSettlement,
                    TargetedSettlement = new StrategusSettlement(),
                },
            };
            ArrangeDb.Users.AddRange(user);
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateStrategusHeroStatusCommand.Handler(ActDb, Mapper, Mock.Of<IStrategusMap>());
            var res = await handler.Handle(new UpdateStrategusHeroStatusCommand
            {
                HeroId = user.Id,
                Status = status,
                TargetedHeroId = 10,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.UserNotFound, res.Errors![0].Code);
        }

        [TestCase(StrategusHeroStatus.FollowingHero)]
        [TestCase(StrategusHeroStatus.MovingToAttackHero)]
        public async Task ShouldReturnErrorIfTargetingNotVisibleHero(StrategusHeroStatus status)
        {
            var user = new User
            {
                StrategusHero = new StrategusHero
                {
                    Position = new Point(0, 0),
                    Status = StrategusHeroStatus.MovingToSettlement,
                    TargetedSettlement = new StrategusSettlement(),
                },
            };
            var targetUser = new User
            {
                StrategusHero = new StrategusHero
                {
                    Position = new Point(10, 10),
                },
            };
            ArrangeDb.Users.AddRange(user, targetUser);
            await ArrangeDb.SaveChangesAsync();

            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock.Setup(m => m.ViewDistance).Returns(0);

            var handler = new UpdateStrategusHeroStatusCommand.Handler(ActDb, Mapper, strategusMapMock.Object);
            var res = await handler.Handle(new UpdateStrategusHeroStatusCommand
            {
                HeroId = user.Id,
                Status = status,
                TargetedHeroId = targetUser.Id,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.HeroNotInSight, res.Errors![0].Code);
        }

        [TestCase(StrategusHeroStatus.FollowingHero)]
        [TestCase(StrategusHeroStatus.MovingToAttackHero)]
        public async Task ShouldUpdateStrategusHeroMovementIfTargetingUser(StrategusHeroStatus status)
        {
            var user = new User
            {
                StrategusHero = new StrategusHero
                {
                    Position = new Point(0, 0),
                    Status = StrategusHeroStatus.MovingToSettlement,
                    TargetedSettlement = new StrategusSettlement(),
                },
            };
            var targetUser = new User
            {
                StrategusHero = new StrategusHero
                {
                    Position = new Point(10, 10),
                },
            };
            ArrangeDb.Users.AddRange(user, targetUser);
            await ArrangeDb.SaveChangesAsync();

            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock.Setup(m => m.ViewDistance).Returns(500);

            var handler = new UpdateStrategusHeroStatusCommand.Handler(ActDb, Mapper, strategusMapMock.Object);
            var res = await handler.Handle(new UpdateStrategusHeroStatusCommand
            {
                HeroId = user.Id,
                Status = status,
                TargetedHeroId = targetUser.Id,
            }, CancellationToken.None);

            var strategusHero = res.Data!;
            Assert.IsNotNull(strategusHero);
            Assert.AreEqual(user.Id, strategusHero.Id);
            Assert.AreEqual(status, strategusHero.Status);
            Assert.AreEqual(targetUser.Id, strategusHero.TargetedHero!.Id);
            Assert.IsNull(strategusHero.TargetedSettlement);
        }

        [Test]
        public async Task ShouldSwitchFromAttackToFollowUser()
        {
            User targetUser = new()
            {
                StrategusHero = new StrategusHero { Position = new Point(10, 10) },
            };
            var user = new User
            {
                StrategusHero = new StrategusHero
                {
                    Position = new Point(0, 0),
                    Status = StrategusHeroStatus.MovingToAttackHero,
                    TargetedHero = targetUser.StrategusHero,
                },
            };
            ArrangeDb.Users.AddRange(user, targetUser);
            await ArrangeDb.SaveChangesAsync();

            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock.Setup(m => m.ViewDistance).Returns(500);

            var handler = new UpdateStrategusHeroStatusCommand.Handler(ActDb, Mapper, strategusMapMock.Object);
            var res = await handler.Handle(new UpdateStrategusHeroStatusCommand
            {
                HeroId = user.Id,
                Status = StrategusHeroStatus.FollowingHero,
                TargetedHeroId = targetUser.Id,
            }, CancellationToken.None);

            var strategusHero = res.Data!;
            Assert.IsNotNull(strategusHero);
            Assert.AreEqual(user.Id, strategusHero.Id);
            Assert.AreEqual(StrategusHeroStatus.FollowingHero, strategusHero.Status);
            Assert.AreEqual(targetUser.Id, strategusHero.TargetedHero!.Id);
        }

        [TestCase(StrategusHeroStatus.MovingToSettlement)]
        [TestCase(StrategusHeroStatus.MovingToAttackSettlement)]
        public async Task ShouldReturnErrorIfTargetingNotExistingSettlement(StrategusHeroStatus status)
        {
            var user = new User
            {
                StrategusHero = new StrategusHero
                {
                    Status = StrategusHeroStatus.FollowingHero,
                    TargetedHero = new StrategusHero { User = new User() },
                },
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateStrategusHeroStatusCommand.Handler(ActDb, Mapper, Mock.Of<IStrategusMap>());
            var res = await handler.Handle(new UpdateStrategusHeroStatusCommand
            {
                HeroId = user.Id,
                Status = status,
                TargetedSettlementId = 10,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.SettlementNotFound, res.Errors![0].Code);
        }

        [TestCase(StrategusHeroStatus.MovingToSettlement)]
        [TestCase(StrategusHeroStatus.MovingToAttackSettlement)]
        public async Task ShouldUpdateStrategusHeroMovementIfTargetingSettlement(StrategusHeroStatus status)
        {
            var user = new User
            {
                StrategusHero = new StrategusHero
                {
                    Status = StrategusHeroStatus.MovingToAttackHero,
                    TargetedHero = new StrategusHero { User = new User() },
                },
            };
            var targetSettlement = new StrategusSettlement();
            ArrangeDb.Users.Add(user);
            ArrangeDb.StrategusSettlements.Add(targetSettlement);
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateStrategusHeroStatusCommand.Handler(ActDb, Mapper, Mock.Of<IStrategusMap>());
            var res = await handler.Handle(new UpdateStrategusHeroStatusCommand
            {
                HeroId = user.Id,
                Status = status,
                TargetedSettlementId = targetSettlement.Id,
            }, CancellationToken.None);

            var strategusHero = res.Data!;
            Assert.IsNotNull(strategusHero);
            Assert.AreEqual(user.Id, strategusHero.Id);
            Assert.AreEqual(status, strategusHero.Status);
            Assert.IsNull(strategusHero.TargetedHero);
            Assert.AreEqual(targetSettlement.Id, strategusHero.TargetedSettlement!.Id);
        }

        [Test]
        public async Task ShouldSwitchFromMoveToAttackSettlement()
        {
            var targetSettlement = new StrategusSettlement();
            var user = new User
            {
                StrategusHero = new StrategusHero
                {
                    Position = new Point(0, 0),
                    Status = StrategusHeroStatus.MovingToSettlement,
                    TargetedSettlement = targetSettlement,
                },
            };
            ArrangeDb.Users.AddRange(user);
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateStrategusHeroStatusCommand.Handler(ActDb, Mapper, Mock.Of<IStrategusMap>());
            var res = await handler.Handle(new UpdateStrategusHeroStatusCommand
            {
                HeroId = user.Id,
                Status = StrategusHeroStatus.MovingToAttackSettlement,
                TargetedSettlementId = targetSettlement.Id,
            }, CancellationToken.None);

            var strategusHero = res.Data!;
            Assert.IsNotNull(strategusHero);
            Assert.AreEqual(user.Id, strategusHero.Id);
            Assert.AreEqual(StrategusHeroStatus.MovingToAttackSettlement, strategusHero.Status);
            Assert.AreEqual(targetSettlement.Id, strategusHero.TargetedSettlement!.Id);
        }

        [Test]
        public async Task ShouldReturnErrorIfTryingToStopRecruitingWhenNotInASettlement()
        {
            var user = new User
            {
                StrategusHero = new StrategusHero { Status = StrategusHeroStatus.Idle },
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateStrategusHeroStatusCommand.Handler(ActDb, Mapper, Mock.Of<IStrategusMap>());
            var res = await handler.Handle(new UpdateStrategusHeroStatusCommand
            {
                HeroId = user.Id,
                Status = StrategusHeroStatus.IdleInSettlement,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.HeroNotInASettlement, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldSwitchFromRecruitingInSettlementToIdleInSettlement()
        {
            var user = new User
            {
                StrategusHero = new StrategusHero
                {
                    Status = StrategusHeroStatus.RecruitingInSettlement,
                    TargetedSettlement = new StrategusSettlement(),
                },
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateStrategusHeroStatusCommand.Handler(ActDb, Mapper, Mock.Of<IStrategusMap>());
            var res = await handler.Handle(new UpdateStrategusHeroStatusCommand
            {
                HeroId = user.Id,
                Status = StrategusHeroStatus.IdleInSettlement,
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            Assert.AreEqual(StrategusHeroStatus.IdleInSettlement, res.Data!.Status);
            Assert.IsNotNull(res.Data!.TargetedSettlement);
        }

        [Test]
        public async Task ShouldReturnErrorIfTryingToRecruitWhenNotInASettlement()
        {
            var user = new User
            {
                StrategusHero = new StrategusHero { Status = StrategusHeroStatus.Idle },
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateStrategusHeroStatusCommand.Handler(ActDb, Mapper, Mock.Of<IStrategusMap>());
            var res = await handler.Handle(new UpdateStrategusHeroStatusCommand
            {
                HeroId = user.Id,
                Status = StrategusHeroStatus.RecruitingInSettlement,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.HeroNotInASettlement, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldSwitchFromIdleInSettlementToRecruitingInSettlement()
        {
            var user = new User
            {
                StrategusHero = new StrategusHero
                {
                    Status = StrategusHeroStatus.IdleInSettlement,
                    TargetedSettlement = new StrategusSettlement(),
                },
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateStrategusHeroStatusCommand.Handler(ActDb, Mapper, Mock.Of<IStrategusMap>());
            var res = await handler.Handle(new UpdateStrategusHeroStatusCommand
            {
                HeroId = user.Id,
                Status = StrategusHeroStatus.RecruitingInSettlement,
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            Assert.AreEqual(StrategusHeroStatus.RecruitingInSettlement, res.Data!.Status);
            Assert.IsNotNull(res.Data!.TargetedSettlement);
        }
    }
}
