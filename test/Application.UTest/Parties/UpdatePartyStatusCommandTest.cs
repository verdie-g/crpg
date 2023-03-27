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

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PartyNotFound));
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

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PartyNotFound));
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

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PartyInBattle));
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
        Assert.That(party, Is.Not.Null);
        Assert.That(party.Id, Is.EqualTo(user.Id));
        Assert.That(party.Status, Is.EqualTo(PartyStatus.Idle));
        Assert.That(party.Waypoints.Count, Is.EqualTo(0));
        Assert.That(party.TargetedParty, Is.Null);
        Assert.That(party.TargetedSettlement, Is.Null);
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
        Assert.That(party, Is.Not.Null);
        Assert.That(party.Id, Is.EqualTo(user.Id));
        Assert.That(party.Status, Is.EqualTo(PartyStatus.MovingToPoint));
        Assert.That(party.Waypoints.Count, Is.EqualTo(2));
        Assert.That(party.Waypoints[0], Is.EqualTo(new Point(4, 5)));
        Assert.That(party.Waypoints[1], Is.EqualTo(new Point(6, 7)));
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

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotFound));
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

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PartyNotInSight));
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
        Assert.That(party, Is.Not.Null);
        Assert.That(party.Id, Is.EqualTo(user.Id));
        Assert.That(party.Status, Is.EqualTo(status));
        Assert.That(party.TargetedParty!.Id, Is.EqualTo(targetUser.Id));
        Assert.That(party.TargetedSettlement, Is.Null);
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
        Assert.That(party, Is.Not.Null);
        Assert.That(party.Id, Is.EqualTo(user.Id));
        Assert.That(party.Status, Is.EqualTo(PartyStatus.FollowingParty));
        Assert.That(party.TargetedParty!.Id, Is.EqualTo(targetUser.Id));
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

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.SettlementNotFound));
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
        Assert.That(party, Is.Not.Null);
        Assert.That(party.Id, Is.EqualTo(user.Id));
        Assert.That(party.Status, Is.EqualTo(status));
        Assert.That(party.TargetedParty, Is.Null);
        Assert.That(party.TargetedSettlement!.Id, Is.EqualTo(targetSettlement.Id));
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
        Assert.That(party, Is.Not.Null);
        Assert.That(party.Id, Is.EqualTo(user.Id));
        Assert.That(party.Status, Is.EqualTo(PartyStatus.MovingToAttackSettlement));
        Assert.That(party.TargetedSettlement!.Id, Is.EqualTo(targetSettlement.Id));
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

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PartyNotInASettlement));
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

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data!.Status, Is.EqualTo(PartyStatus.IdleInSettlement));
        Assert.That(res.Data!.TargetedSettlement, Is.Not.Null);
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

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PartyNotInASettlement));
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

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data!.Status, Is.EqualTo(PartyStatus.RecruitingInSettlement));
        Assert.That(res.Data!.TargetedSettlement, Is.Not.Null);
    }
}
