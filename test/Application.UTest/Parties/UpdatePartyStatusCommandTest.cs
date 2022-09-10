using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Parties.Commands;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Users;
using Moq;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Parties;

public class UpdatePartyStatusCommandTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfNotFound()
    {
        UpdatePartyStatusCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IStrategusMap>());
        var res = await handler.Handle(new UpdatePartyStatusCommand
        {
            PartyId = 1,
            Waypoints = MultiPoint.Empty,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.PartyNotFound, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldReturnErrorUserNotRegisteredToStrategus()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        UpdatePartyStatusCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IStrategusMap>());
        var res = await handler.Handle(new UpdatePartyStatusCommand
        {
            PartyId = user.Id,
            Waypoints = MultiPoint.Empty,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.PartyNotFound, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldReturnErrorIfUserIsInBattle()
    {
        User user = new()
        {
            Party = new Party
            {
                Status = PartyStatus.InBattle,
            },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        UpdatePartyStatusCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IStrategusMap>());
        var res = await handler.Handle(new UpdatePartyStatusCommand
        {
            PartyId = user.Id,
            Waypoints = new MultiPoint(new[]
            {
                new Point(4, 5),
                new Point(6, 7),
            }),
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.PartyInBattle, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldClearMovementIfStatusIdle()
    {
        User user = new()
        {
            Party = new Party
            {
                Status = PartyStatus.MovingToSettlement,
                // Doesn't make sense that the 3 properties are set but it's just for the test.
                Waypoints = new MultiPoint(new[] { new Point(5, 3) }),
                TargetedParty = new Party { User = new User() },
                TargetedSettlement = new Settlement(),
            },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        UpdatePartyStatusCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IStrategusMap>());
        var res = await handler.Handle(new UpdatePartyStatusCommand
        {
            PartyId = user.Id,
            Status = PartyStatus.Idle,
        }, CancellationToken.None);

        var party = res.Data!;
        Assert.IsNotNull(party);
        Assert.AreEqual(user.Id, party.Id);
        Assert.AreEqual(PartyStatus.Idle, party.Status);
        Assert.AreEqual(0, party.Waypoints.Count);
        Assert.IsNull(party.TargetedParty);
        Assert.IsNull(party.TargetedSettlement);
    }

    [Test]
    public async Task ShouldUpdatePartyMovementIfStatusMovingToPoint()
    {
        User user = new()
        {
            Party = new Party
            {
                Status = PartyStatus.Idle,
                Waypoints = new MultiPoint(new[] { new Point(3, 4) }),
            },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        UpdatePartyStatusCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IStrategusMap>());
        var res = await handler.Handle(new UpdatePartyStatusCommand
        {
            PartyId = user.Id,
            Status = PartyStatus.MovingToPoint,
            Waypoints = new MultiPoint(new[]
            {
                new Point(4, 5),
                new Point(6, 7),
            }),
        }, CancellationToken.None);

        var party = res.Data!;
        Assert.IsNotNull(party);
        Assert.AreEqual(user.Id, party.Id);
        Assert.AreEqual(PartyStatus.MovingToPoint, party.Status);
        Assert.AreEqual(2, party.Waypoints.Count);
        Assert.AreEqual(new Point(4, 5), party.Waypoints[0]);
        Assert.AreEqual(new Point(6, 7), party.Waypoints[1]);
    }

    [TestCase(PartyStatus.FollowingParty)]
    [TestCase(PartyStatus.MovingToAttackParty)]
    public async Task ShouldReturnErrorIfTargetingNotExistingUser(PartyStatus status)
    {
        User user = new()
        {
            Party = new Party
            {
                Status = PartyStatus.MovingToAttackSettlement,
                TargetedSettlement = new Settlement(),
            },
        };
        ArrangeDb.Users.AddRange(user);
        await ArrangeDb.SaveChangesAsync();

        UpdatePartyStatusCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IStrategusMap>());
        var res = await handler.Handle(new UpdatePartyStatusCommand
        {
            PartyId = user.Id,
            Status = status,
            TargetedPartyId = 10,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.UserNotFound, res.Errors![0].Code);
    }

    [TestCase(PartyStatus.FollowingParty)]
    [TestCase(PartyStatus.MovingToAttackParty)]
    public async Task ShouldReturnErrorIfTargetingNotVisibleParty(PartyStatus status)
    {
        User user = new()
        {
            Party = new Party
            {
                Position = new Point(0, 0),
                Status = PartyStatus.MovingToSettlement,
                TargetedSettlement = new Settlement(),
            },
        };
        User targetUser = new()
        {
            Party = new Party
            {
                Position = new Point(10, 10),
            },
        };
        ArrangeDb.Users.AddRange(user, targetUser);
        await ArrangeDb.SaveChangesAsync();

        Mock<IStrategusMap> strategusMapMock = new();
        strategusMapMock.Setup(m => m.ViewDistance).Returns(0);

        UpdatePartyStatusCommand.Handler handler = new(ActDb, Mapper, strategusMapMock.Object);
        var res = await handler.Handle(new UpdatePartyStatusCommand
        {
            PartyId = user.Id,
            Status = status,
            TargetedPartyId = targetUser.Id,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.PartyNotInSight, res.Errors![0].Code);
    }

    [TestCase(PartyStatus.FollowingParty)]
    [TestCase(PartyStatus.MovingToAttackParty)]
    public async Task ShouldUpdatePartyMovementIfTargetingUser(PartyStatus status)
    {
        User user = new()
        {
            Party = new Party
            {
                Position = new Point(0, 0),
                Status = PartyStatus.MovingToSettlement,
                TargetedSettlement = new Settlement(),
            },
        };
        User targetUser = new()
        {
            Party = new Party
            {
                Position = new Point(10, 10),
            },
        };
        ArrangeDb.Users.AddRange(user, targetUser);
        await ArrangeDb.SaveChangesAsync();

        Mock<IStrategusMap> strategusMapMock = new();
        strategusMapMock.Setup(m => m.ViewDistance).Returns(500);

        UpdatePartyStatusCommand.Handler handler = new(ActDb, Mapper, strategusMapMock.Object);
        var res = await handler.Handle(new UpdatePartyStatusCommand
        {
            PartyId = user.Id,
            Status = status,
            TargetedPartyId = targetUser.Id,
        }, CancellationToken.None);

        var party = res.Data!;
        Assert.IsNotNull(party);
        Assert.AreEqual(user.Id, party.Id);
        Assert.AreEqual(status, party.Status);
        Assert.AreEqual(targetUser.Id, party.TargetedParty!.Id);
        Assert.IsNull(party.TargetedSettlement);
    }

    [Test]
    public async Task ShouldSwitchFromAttackToFollowUser()
    {
        User targetUser = new()
        {
            Party = new Party { Position = new Point(10, 10) },
        };
        User user = new()
        {
            Party = new Party
            {
                Position = new Point(0, 0),
                Status = PartyStatus.MovingToAttackParty,
                TargetedParty = targetUser.Party,
            },
        };
        ArrangeDb.Users.AddRange(user, targetUser);
        await ArrangeDb.SaveChangesAsync();

        Mock<IStrategusMap> strategusMapMock = new();
        strategusMapMock.Setup(m => m.ViewDistance).Returns(500);

        UpdatePartyStatusCommand.Handler handler = new(ActDb, Mapper, strategusMapMock.Object);
        var res = await handler.Handle(new UpdatePartyStatusCommand
        {
            PartyId = user.Id,
            Status = PartyStatus.FollowingParty,
            TargetedPartyId = targetUser.Id,
        }, CancellationToken.None);

        var party = res.Data!;
        Assert.IsNotNull(party);
        Assert.AreEqual(user.Id, party.Id);
        Assert.AreEqual(PartyStatus.FollowingParty, party.Status);
        Assert.AreEqual(targetUser.Id, party.TargetedParty!.Id);
    }

    [TestCase(PartyStatus.MovingToSettlement)]
    [TestCase(PartyStatus.MovingToAttackSettlement)]
    public async Task ShouldReturnErrorIfTargetingNotExistingSettlement(PartyStatus status)
    {
        User user = new()
        {
            Party = new Party
            {
                Status = PartyStatus.FollowingParty,
                TargetedParty = new Party { User = new User() },
            },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        UpdatePartyStatusCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IStrategusMap>());
        var res = await handler.Handle(new UpdatePartyStatusCommand
        {
            PartyId = user.Id,
            Status = status,
            TargetedSettlementId = 10,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.SettlementNotFound, res.Errors![0].Code);
    }

    [TestCase(PartyStatus.MovingToSettlement)]
    [TestCase(PartyStatus.MovingToAttackSettlement)]
    public async Task ShouldUpdatePartyMovementIfTargetingSettlement(PartyStatus status)
    {
        User user = new()
        {
            Party = new Party
            {
                Status = PartyStatus.MovingToAttackParty,
                TargetedParty = new Party { User = new User() },
            },
        };
        Settlement targetSettlement = new();
        ArrangeDb.Users.Add(user);
        ArrangeDb.Settlements.Add(targetSettlement);
        await ArrangeDb.SaveChangesAsync();

        UpdatePartyStatusCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IStrategusMap>());
        var res = await handler.Handle(new UpdatePartyStatusCommand
        {
            PartyId = user.Id,
            Status = status,
            TargetedSettlementId = targetSettlement.Id,
        }, CancellationToken.None);

        var party = res.Data!;
        Assert.IsNotNull(party);
        Assert.AreEqual(user.Id, party.Id);
        Assert.AreEqual(status, party.Status);
        Assert.IsNull(party.TargetedParty);
        Assert.AreEqual(targetSettlement.Id, party.TargetedSettlement!.Id);
    }

    [Test]
    public async Task ShouldSwitchFromMoveToAttackSettlement()
    {
        Settlement targetSettlement = new();
        User user = new()
        {
            Party = new Party
            {
                Position = new Point(0, 0),
                Status = PartyStatus.MovingToSettlement,
                TargetedSettlement = targetSettlement,
            },
        };
        ArrangeDb.Users.AddRange(user);
        await ArrangeDb.SaveChangesAsync();

        UpdatePartyStatusCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IStrategusMap>());
        var res = await handler.Handle(new UpdatePartyStatusCommand
        {
            PartyId = user.Id,
            Status = PartyStatus.MovingToAttackSettlement,
            TargetedSettlementId = targetSettlement.Id,
        }, CancellationToken.None);

        var party = res.Data!;
        Assert.IsNotNull(party);
        Assert.AreEqual(user.Id, party.Id);
        Assert.AreEqual(PartyStatus.MovingToAttackSettlement, party.Status);
        Assert.AreEqual(targetSettlement.Id, party.TargetedSettlement!.Id);
    }

    [Test]
    public async Task ShouldReturnErrorIfTryingToStopRecruitingWhenNotInASettlement()
    {
        User user = new()
        {
            Party = new Party { Status = PartyStatus.Idle },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        UpdatePartyStatusCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IStrategusMap>());
        var res = await handler.Handle(new UpdatePartyStatusCommand
        {
            PartyId = user.Id,
            Status = PartyStatus.IdleInSettlement,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.PartyNotInASettlement, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldSwitchFromRecruitingInSettlementToIdleInSettlement()
    {
        User user = new()
        {
            Party = new Party
            {
                Status = PartyStatus.RecruitingInSettlement,
                TargetedSettlement = new Settlement(),
            },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        UpdatePartyStatusCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IStrategusMap>());
        var res = await handler.Handle(new UpdatePartyStatusCommand
        {
            PartyId = user.Id,
            Status = PartyStatus.IdleInSettlement,
        }, CancellationToken.None);

        Assert.IsNull(res.Errors);
        Assert.AreEqual(PartyStatus.IdleInSettlement, res.Data!.Status);
        Assert.IsNotNull(res.Data!.TargetedSettlement);
    }

    [Test]
    public async Task ShouldReturnErrorIfTryingToRecruitWhenNotInASettlement()
    {
        User user = new()
        {
            Party = new Party { Status = PartyStatus.Idle },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        UpdatePartyStatusCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IStrategusMap>());
        var res = await handler.Handle(new UpdatePartyStatusCommand
        {
            PartyId = user.Id,
            Status = PartyStatus.RecruitingInSettlement,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.PartyNotInASettlement, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldSwitchFromIdleInSettlementToRecruitingInSettlement()
    {
        User user = new()
        {
            Party = new Party
            {
                Status = PartyStatus.IdleInSettlement,
                TargetedSettlement = new Settlement(),
            },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        UpdatePartyStatusCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IStrategusMap>());
        var res = await handler.Handle(new UpdatePartyStatusCommand
        {
            PartyId = user.Id,
            Status = PartyStatus.RecruitingInSettlement,
        }, CancellationToken.None);

        Assert.IsNull(res.Errors);
        Assert.AreEqual(PartyStatus.RecruitingInSettlement, res.Data!.Status);
        Assert.IsNotNull(res.Data!.TargetedSettlement);
    }
}
