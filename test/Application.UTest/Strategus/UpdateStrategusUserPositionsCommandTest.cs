using System;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Services;
using Crpg.Application.Strategus.Commands;
using Crpg.Domain.Entities.Strategus;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Moq;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Strategus
{
    public class UpdateStrategusUserPositionsCommandTest : TestBase
    {
        [Test]
        public async Task UsersMovingToPointShouldMove()
        {
            var position = new Point(1, 2);
            var destination = new Point(5, 6);
            var strategusUser = new StrategusUser
            {
                Status = StrategusUserStatus.MovingToPoint,
                Position = position,
                Waypoints = new MultiPoint(new[] { destination }),
                User = new User(),
            };
            ArrangeDb.StrategusUsers.Add(strategusUser);
            await ArrangeDb.SaveChangesAsync();

            var newPosition = new Point(2, 3);
            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock
                .Setup(m => m.MovePointTowards(position, destination, It.IsAny<double>()))
                .Returns(newPosition);
            strategusMapMock
                .Setup(m => m.ArePointsEquivalent(newPosition, destination))
                .Returns(false);
            var handler = new UpdateStrategusUserPositionsCommand.Handler(ActDb, strategusMapMock.Object);
            await handler.Handle(new UpdateStrategusUserPositionsCommand
            {
                DeltaTime = TimeSpan.FromMinutes(1)
            }, CancellationToken.None);

            strategusUser = await AssertDb.StrategusUsers.FirstAsync(u => u.UserId == strategusUser.UserId);
            Assert.AreEqual(StrategusUserStatus.MovingToPoint, strategusUser.Status);
            Assert.AreEqual(newPosition, strategusUser.Position);
            Assert.AreEqual(1, strategusUser.Waypoints.Count);
        }

        [Test]
        public async Task ReachedWaypointShouldBeRemovedForMovingToPointUsers()
        {
            var position = new Point(1, 2);
            var destination = new Point(5, 6);
            var strategusUser = new StrategusUser
            {
                Status = StrategusUserStatus.MovingToPoint,
                Position = position,
                Waypoints = new MultiPoint(new[] { destination, new Point(10, 10) }),
                User = new User(),
            };
            ArrangeDb.StrategusUsers.Add(strategusUser);
            await ArrangeDb.SaveChangesAsync();

            var newPosition = new Point(5, 5);
            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock
                .Setup(m => m.MovePointTowards(position, destination, It.IsAny<double>()))
                .Returns(newPosition);
            strategusMapMock
                .Setup(m => m.ArePointsEquivalent(newPosition, destination))
                .Returns(true);
            var handler = new UpdateStrategusUserPositionsCommand.Handler(ActDb, strategusMapMock.Object);
            await handler.Handle(new UpdateStrategusUserPositionsCommand
            {
                DeltaTime = TimeSpan.FromMinutes(1)
            }, CancellationToken.None);

            strategusUser = await AssertDb.StrategusUsers.FirstAsync(u => u.UserId == strategusUser.UserId);
            Assert.AreEqual(StrategusUserStatus.MovingToPoint, strategusUser.Status);
            Assert.AreEqual(newPosition, strategusUser.Position);
            Assert.AreEqual(1, strategusUser.Waypoints.Count);
        }

        [Test]
        public async Task MovingUsersShouldChangeToIdleIfLastWaypointReached()
        {
            var position = new Point(1, 2);
            var destination = new Point(5, 6);
            var strategusUser = new StrategusUser
            {
                Status = StrategusUserStatus.MovingToPoint,
                Position = position,
                Waypoints = new MultiPoint(new[] { destination }),
                User = new User(),
            };
            ArrangeDb.StrategusUsers.Add(strategusUser);
            await ArrangeDb.SaveChangesAsync();

            var newPosition = new Point(5, 5);
            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock
                .Setup(m => m.MovePointTowards(position, destination, It.IsAny<double>()))
                .Returns(newPosition);
            strategusMapMock
                .Setup(m => m.ArePointsEquivalent(newPosition, destination))
                .Returns(true);
            var handler = new UpdateStrategusUserPositionsCommand.Handler(ActDb, strategusMapMock.Object);
            await handler.Handle(new UpdateStrategusUserPositionsCommand
            {
                DeltaTime = TimeSpan.FromMinutes(1)
            }, CancellationToken.None);

            strategusUser = await AssertDb.StrategusUsers.FirstAsync(u => u.UserId == strategusUser.UserId);
            Assert.AreEqual(StrategusUserStatus.Idle, strategusUser.Status);
            Assert.AreEqual(newPosition, strategusUser.Position);
            Assert.AreEqual(0, strategusUser.Waypoints.Count);
        }

        [TestCase(StrategusUserStatus.FollowingUser)]
        [TestCase(StrategusUserStatus.MovingToAttackUser)]
        public async Task ShouldStopIfMovingToAUserNotInSight(StrategusUserStatus status)
        {
            var strategusUser = new StrategusUser
            {
                Status = status,
                Position = new Point(1, 2),
                TargetedUser = new StrategusUser
                {
                    Position = new Point(5, 6),
                    User = new User(),
                },
                User = new User(),
            };
            ArrangeDb.StrategusUsers.Add(strategusUser);
            await ArrangeDb.SaveChangesAsync();

            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock.Setup(m => m.ViewDistance).Returns(0);

            var handler = new UpdateStrategusUserPositionsCommand.Handler(ActDb, strategusMapMock.Object);
            await handler.Handle(new UpdateStrategusUserPositionsCommand
            {
                DeltaTime = TimeSpan.FromMinutes(1)
            }, CancellationToken.None);

            strategusUser = await AssertDb.StrategusUsers.FirstAsync(u => u.UserId == strategusUser.UserId);
            Assert.AreEqual(StrategusUserStatus.Idle, strategusUser.Status);
            Assert.IsNull(strategusUser.TargetedUserId);
        }

        [TestCase(StrategusUserStatus.FollowingUser)]
        [TestCase(StrategusUserStatus.MovingToAttackUser)]
        public async Task MovingToAnotherUserShouldMove(StrategusUserStatus status)
        {
            var position = new Point(1, 2);
            var destination = new Point(5, 6);
            var strategusUser = new StrategusUser
            {
                Status = status,
                Position = position,
                TargetedUser = new StrategusUser
                {
                    Position = destination,
                    User = new User(),
                },
                User = new User(),
            };
            ArrangeDb.StrategusUsers.Add(strategusUser);
            await ArrangeDb.SaveChangesAsync();

            var newPosition = new Point(2, 3);
            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock.Setup(m => m.ViewDistance).Returns(500);
            strategusMapMock
                .Setup(m => m.MovePointTowards(position, destination, It.IsAny<double>()))
                .Returns(newPosition);
            if (status == StrategusUserStatus.FollowingUser)
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

            var handler = new UpdateStrategusUserPositionsCommand.Handler(ActDb, strategusMapMock.Object);
            await handler.Handle(new UpdateStrategusUserPositionsCommand
            {
                DeltaTime = TimeSpan.FromMinutes(1)
            }, CancellationToken.None);

            strategusUser = await AssertDb.StrategusUsers.FirstAsync(u => u.UserId == strategusUser.UserId);
            Assert.AreEqual(status, strategusUser.Status);
            Assert.AreEqual(newPosition, strategusUser.Position);
        }

        [TestCase(StrategusUserStatus.MovingToSettlement)]
        [TestCase(StrategusUserStatus.MovingToAttackSettlement)]
        public async Task MovingToASettlementShouldMove(StrategusUserStatus status)
        {
            var position = new Point(1, 2);
            var destination = new Point(5, 6);
            var strategusUser = new StrategusUser
            {
                Status = status,
                Position = position,
                TargetedSettlement = new StrategusSettlement { Position = destination },
                User = new User(),
            };
            ArrangeDb.StrategusUsers.Add(strategusUser);
            await ArrangeDb.SaveChangesAsync();

            var newPosition = new Point(2, 3);
            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock
                .Setup(m => m.MovePointTowards(position, destination, It.IsAny<double>()))
                .Returns(newPosition);
            strategusMapMock
                .Setup(m => m.ArePointsAtInteractionDistance(newPosition, destination))
                .Returns(false);
            var handler = new UpdateStrategusUserPositionsCommand.Handler(ActDb, strategusMapMock.Object);
            await handler.Handle(new UpdateStrategusUserPositionsCommand
            {
                DeltaTime = TimeSpan.FromMinutes(1)
            }, CancellationToken.None);

            strategusUser = await AssertDb.StrategusUsers.FirstAsync(u => u.UserId == strategusUser.UserId);
            Assert.AreEqual(status, strategusUser.Status);
            Assert.AreEqual(newPosition, strategusUser.Position);
        }

        [Test]
        public async Task ShouldEnterSettlementIfCloseEnough()
        {
            var position = new Point(1, 2);
            var destination = new Point(5, 6);
            var strategusUser = new StrategusUser
            {
                Status = StrategusUserStatus.MovingToSettlement,
                Position = position,
                TargetedSettlement = new StrategusSettlement { Position = destination },
                User = new User(),
            };
            ArrangeDb.StrategusUsers.Add(strategusUser);
            await ArrangeDb.SaveChangesAsync();

            var newPosition = new Point(5, 5);
            var strategusMapMock = new Mock<IStrategusMap>();
            strategusMapMock
                .Setup(m => m.MovePointTowards(position, destination, It.IsAny<double>()))
                .Returns(newPosition);
            strategusMapMock
                .Setup(m => m.ArePointsAtInteractionDistance(newPosition, destination))
                .Returns(true);
            var handler = new UpdateStrategusUserPositionsCommand.Handler(ActDb, strategusMapMock.Object);
            await handler.Handle(new UpdateStrategusUserPositionsCommand
            {
                DeltaTime = TimeSpan.FromMinutes(1)
            }, CancellationToken.None);

            strategusUser = await AssertDb.StrategusUsers.FirstAsync(u => u.UserId == strategusUser.UserId);
            Assert.AreEqual(StrategusUserStatus.IdleInSettlement, strategusUser.Status);
            Assert.AreEqual(destination, strategusUser.Position);
        }
    }
}
