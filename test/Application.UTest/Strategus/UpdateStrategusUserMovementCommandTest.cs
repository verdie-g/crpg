using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Results;
using Crpg.Application.Strategus.Commands;
using Crpg.Domain.Entities.Strategus;
using Crpg.Domain.Entities.Users;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Strategus
{
    public class UpdateStrategusUserMovementCommandTest : TestBase
    {
        [Test]
        public async Task ShouldReturnErrorIfNotFound()
        {
            var handler = new UpdateStrategusUserMovementCommand.Handler(ActDb, Mapper);
            var res = await handler.Handle(new UpdateStrategusUserMovementCommand
            {
                UserId = 1,
                Waypoints = MultiPoint.Empty,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.UserNotFound, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorUserNotRegisteredToStrategus()
        {
            var user = new User();
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateStrategusUserMovementCommand.Handler(ActDb, Mapper);
            var res = await handler.Handle(new UpdateStrategusUserMovementCommand
            {
                UserId = user.Id,
                Waypoints = MultiPoint.Empty,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.UserNotFound, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfUserIsInBattle()
        {
            var user = new User
            {
                StrategusUser = new StrategusUser
                {
                    Status = StrategusUserStatus.InBattle,
                }
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateStrategusUserMovementCommand.Handler(ActDb, Mapper);
            var res = await handler.Handle(new UpdateStrategusUserMovementCommand
            {
                UserId = user.Id,
                Waypoints = new MultiPoint(new[]
                {
                    new Point(4, 5),
                    new Point(6, 7),
                }),
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.UserInBattle, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldClearMovementIfStatusIdle()
        {
            var user = new User
            {
                StrategusUser = new StrategusUser
                {
                    Status = StrategusUserStatus.MovingToSettlement,
                    // Doesn't make sense that the 3 properties are set but it's just for the test.
                    Waypoints = new MultiPoint(new[] { new Point(5, 3) }),
                    TargetedUser = new StrategusUser { User = new User() },
                    TargetedSettlement = new StrategusSettlement(),
                }
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateStrategusUserMovementCommand.Handler(ActDb, Mapper);
            var res = await handler.Handle(new UpdateStrategusUserMovementCommand
            {
                UserId = user.Id,
                Status = StrategusUserStatus.Idle,
            }, CancellationToken.None);

            var strategusUser = res.Data!;
            Assert.IsNotNull(strategusUser);
            Assert.AreEqual(user.Id, strategusUser.Id);
            Assert.AreEqual(StrategusUserStatus.Idle, strategusUser.Status);
            Assert.AreEqual(0, strategusUser.Waypoints.Count);
            Assert.IsNull(strategusUser.TargetedUser);
            Assert.IsNull(strategusUser.TargetedSettlement);
        }

        [Test]
        public async Task ShouldUpdateStrategusUserMovementIfStatusMovingToPoint()
        {
            var user = new User
            {
                StrategusUser = new StrategusUser
                {
                    Status = StrategusUserStatus.Idle,
                    Waypoints = new MultiPoint(new[] { new Point(3, 4) })
                }
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateStrategusUserMovementCommand.Handler(ActDb, Mapper);
            var res = await handler.Handle(new UpdateStrategusUserMovementCommand
            {
                UserId = user.Id,
                Status = StrategusUserStatus.MovingToPoint,
                Waypoints = new MultiPoint(new[]
                {
                    new Point(4, 5),
                    new Point(6, 7),
                }),
            }, CancellationToken.None);

            var strategusUser = res.Data!;
            Assert.IsNotNull(strategusUser);
            Assert.AreEqual(user.Id, strategusUser.Id);
            Assert.AreEqual(StrategusUserStatus.MovingToPoint, strategusUser.Status);
            Assert.AreEqual(2, strategusUser.Waypoints.Count);
            Assert.AreEqual(new Point(4, 5), strategusUser.Waypoints[0]);
            Assert.AreEqual(new Point(6, 7), strategusUser.Waypoints[1]);
        }

        [TestCase(StrategusUserStatus.FollowingUser)]
        [TestCase(StrategusUserStatus.MovingToAttackUser)]
        public async Task ShouldReturnErrorIfTargetingNotExistingUser(StrategusUserStatus status)
        {
            var user = new User
            {
                StrategusUser = new StrategusUser
                {
                    Status = StrategusUserStatus.MovingToAttackSettlement,
                    TargetedSettlement = new StrategusSettlement(),
                }
            };
            ArrangeDb.Users.AddRange(user);
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateStrategusUserMovementCommand.Handler(ActDb, Mapper);
            var res = await handler.Handle(new UpdateStrategusUserMovementCommand
            {
                UserId = user.Id,
                Status = status,
                TargetedUserId = 10,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.UserNotFound, res.Errors![0].Code);
        }

        [TestCase(StrategusUserStatus.FollowingUser)]
        [TestCase(StrategusUserStatus.MovingToAttackUser)]
        public async Task ShouldUpdateStrategusUserMovementIfTargetingUser(StrategusUserStatus status)
        {
            var user = new User
            {
                StrategusUser = new StrategusUser
                {
                    Status = StrategusUserStatus.MovingToSettlement,
                    TargetedSettlement = new StrategusSettlement(),
                }
            };
            var targetUser = new User { StrategusUser = new StrategusUser() };
            ArrangeDb.Users.AddRange(user, targetUser);
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateStrategusUserMovementCommand.Handler(ActDb, Mapper);
            var res = await handler.Handle(new UpdateStrategusUserMovementCommand
            {
                UserId = user.Id,
                Status = status,
                TargetedUserId = targetUser.Id,
            }, CancellationToken.None);

            var strategusUser = res.Data!;
            Assert.IsNotNull(strategusUser);
            Assert.AreEqual(user.Id, strategusUser.Id);
            Assert.AreEqual(status, strategusUser.Status);
            Assert.AreEqual(targetUser.Id, strategusUser.TargetedUser!.Id);
            Assert.IsNull(strategusUser.TargetedSettlement);
        }

        [TestCase(StrategusUserStatus.MovingToSettlement)]
        [TestCase(StrategusUserStatus.MovingToAttackSettlement)]
        public async Task ShouldReturnErrorIfTargetingNotExistingSettlement(StrategusUserStatus status)
        {
            var user = new User
            {
                StrategusUser = new StrategusUser
                {
                    Status = StrategusUserStatus.FollowingUser,
                    TargetedUser = new StrategusUser { User = new User() },
                }
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateStrategusUserMovementCommand.Handler(ActDb, Mapper);
            var res = await handler.Handle(new UpdateStrategusUserMovementCommand
            {
                UserId = user.Id,
                Status = status,
                TargetedSettlementId = 10,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.SettlementNotFound, res.Errors![0].Code);
        }

        [TestCase(StrategusUserStatus.MovingToSettlement)]
        [TestCase(StrategusUserStatus.MovingToAttackSettlement)]
        public async Task ShouldUpdateStrategusUserMovementIfTargetingSettlement(StrategusUserStatus status)
        {
            var user = new User
            {
                StrategusUser = new StrategusUser
                {
                    Status = StrategusUserStatus.MovingToAttackUser,
                    TargetedUser = new StrategusUser { User = new User() },
                }
            };
            var targetSettlement = new StrategusSettlement();
            ArrangeDb.Users.Add(user);
            ArrangeDb.StrategusSettlements.Add(targetSettlement);
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateStrategusUserMovementCommand.Handler(ActDb, Mapper);
            var res = await handler.Handle(new UpdateStrategusUserMovementCommand
            {
                UserId = user.Id,
                Status = status,
                TargetedSettlementId = targetSettlement.Id,
            }, CancellationToken.None);

            var strategusUser = res.Data!;
            Assert.IsNotNull(strategusUser);
            Assert.AreEqual(user.Id, strategusUser.Id);
            Assert.AreEqual(status, strategusUser.Status);
            Assert.IsNull(strategusUser.TargetedUser);
            Assert.AreEqual(targetSettlement.Id, strategusUser.TargetedSettlement!.Id);
        }
    }
}
