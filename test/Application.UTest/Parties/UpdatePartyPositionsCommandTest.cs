using Crpg.Application.Common.Services;
using Crpg.Application.Parties.Commands;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Moq;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Parties;

public class UpdatePartyPositionsCommandTest : TestBase
{
    private static readonly IStrategusSpeedModel SpeedModelMock = Mock.Of<IStrategusSpeedModel>();

    [Test]
    public async Task UsersMovingToPointShouldMove()
    {
        Point position = new(1, 2);
        Point destination = new(5, 6);
        Party party = new()
        {
            Status = PartyStatus.MovingToPoint,
            Position = position,
            Waypoints = new MultiPoint(new[] { destination }),
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);
        await ArrangeDb.SaveChangesAsync();
        Point newPosition = new(2, 3);
        Mock<IStrategusMap> strategusMapMock = new();
        strategusMapMock
            .Setup(m => m.MovePointTowards(position, destination, It.IsAny<double>()))
            .Returns(newPosition);
        strategusMapMock
            .Setup(m => m.ArePointsEquivalent(newPosition, destination))
            .Returns(false);
        UpdatePartyPositionsCommand.Handler handler = new(ActDb, strategusMapMock.Object, SpeedModelMock);
        await handler.Handle(new UpdatePartyPositionsCommand
        {
            DeltaTime = TimeSpan.FromMinutes(1),
        }, CancellationToken.None);

        party = await AssertDb.Parties.FirstAsync(u => u.Id == party.Id);
        Assert.AreEqual(PartyStatus.MovingToPoint, party.Status);
        Assert.AreEqual(newPosition, party.Position);
        Assert.AreEqual(1, party.Waypoints.Count);
    }

    [Test]
    public async Task ReachedWaypointShouldBeRemovedForMovingToPointUsers()
    {
        Point position = new(1, 2);
        Point destination = new(5, 6);
        Party party = new()
        {
            Status = PartyStatus.MovingToPoint,
            Position = position,
            Waypoints = new MultiPoint(new[] { destination, new Point(10, 10) }),
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);
        await ArrangeDb.SaveChangesAsync();
        Point newPosition = new(5, 5);
        Mock<IStrategusMap> strategusMapMock = new();
        strategusMapMock
            .Setup(m => m.MovePointTowards(position, destination, It.IsAny<double>()))
            .Returns(newPosition);
        strategusMapMock
            .Setup(m => m.ArePointsEquivalent(newPosition, destination))
            .Returns(true);
        UpdatePartyPositionsCommand.Handler handler = new(ActDb, strategusMapMock.Object, SpeedModelMock);
        await handler.Handle(new UpdatePartyPositionsCommand
        {
            DeltaTime = TimeSpan.FromMinutes(1),
        }, CancellationToken.None);

        party = await AssertDb.Parties.FirstAsync(u => u.Id == party.Id);
        Assert.AreEqual(PartyStatus.MovingToPoint, party.Status);
        Assert.AreEqual(newPosition, party.Position);
        Assert.AreEqual(1, party.Waypoints.Count);
    }

    [Test]
    public async Task MovingUsersShouldChangeToIdleIfLastWaypointReached()
    {
        Point position = new(1, 2);
        Point destination = new(5, 6);
        Party party = new()
        {
            Status = PartyStatus.MovingToPoint,
            Position = position,
            Waypoints = new MultiPoint(new[] { destination }),
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);
        await ArrangeDb.SaveChangesAsync();

        Point newPosition = new(5, 5);
        Mock<IStrategusMap> strategusMapMock = new();
        strategusMapMock
            .Setup(m => m.MovePointTowards(position, destination, It.IsAny<double>()))
            .Returns(newPosition);
        strategusMapMock
            .Setup(m => m.ArePointsEquivalent(newPosition, destination))
            .Returns(true);
        UpdatePartyPositionsCommand.Handler handler = new(ActDb, strategusMapMock.Object, SpeedModelMock);
        await handler.Handle(new UpdatePartyPositionsCommand
        {
            DeltaTime = TimeSpan.FromMinutes(1),
        }, CancellationToken.None);

        party = await AssertDb.Parties.FirstAsync(u => u.Id == party.Id);
        Assert.AreEqual(PartyStatus.Idle, party.Status);
        Assert.AreEqual(newPosition, party.Position);
        Assert.AreEqual(0, party.Waypoints.Count);
    }

    [TestCase(PartyStatus.FollowingParty)]
    [TestCase(PartyStatus.MovingToAttackParty)]
    public async Task ShouldStopIfMovingToAUserNotInSight(PartyStatus status)
    {
        Party party = new()
        {
            Status = status,
            Position = new Point(1, 2),
            TargetedParty = new Party
            {
                Position = new Point(5, 6),
                User = new User(),
            },
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);
        await ArrangeDb.SaveChangesAsync();

        Mock<IStrategusMap> strategusMapMock = new();
        strategusMapMock.Setup(m => m.ViewDistance).Returns(0);
        UpdatePartyPositionsCommand.Handler handler = new(ActDb, strategusMapMock.Object, SpeedModelMock);
        await handler.Handle(new UpdatePartyPositionsCommand
        {
            DeltaTime = TimeSpan.FromMinutes(1),
        }, CancellationToken.None);

        party = await AssertDb.Parties.FirstAsync(u => u.Id == party.Id);
        Assert.AreEqual(PartyStatus.Idle, party.Status);
        Assert.IsNull(party.TargetedPartyId);
    }

    [TestCase(PartyStatus.FollowingParty)]
    [TestCase(PartyStatus.MovingToAttackParty)]
    public async Task MovingToAnotherUserShouldMove(PartyStatus status)
    {
        Point position = new(1, 2);
        Point destination = new(5, 6);
        Party party = new()
        {
            Status = status,
            Position = position,
            TargetedParty = new Party
            {
                Position = destination,
                User = new User(),
            },
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);
        await ArrangeDb.SaveChangesAsync();

        Point newPosition = new(2, 3);
        Mock<IStrategusMap> strategusMapMock = new();
        strategusMapMock.Setup(m => m.ViewDistance).Returns(500);
        strategusMapMock
            .Setup(m => m.MovePointTowards(position, destination, It.IsAny<double>()))
            .Returns(newPosition);
        if (status == PartyStatus.FollowingParty)
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

        UpdatePartyPositionsCommand.Handler handler = new(ActDb, strategusMapMock.Object, SpeedModelMock);
        await handler.Handle(new UpdatePartyPositionsCommand
        {
            DeltaTime = TimeSpan.FromMinutes(1),
        }, CancellationToken.None);

        party = await AssertDb.Parties.FirstAsync(u => u.Id == party.Id);
        Assert.AreEqual(status, party.Status);
        Assert.AreEqual(newPosition, party.Position);
    }

    [TestCase(PartyStatus.IdleInSettlement)]
    [TestCase(PartyStatus.RecruitingInSettlement)]
    [TestCase(PartyStatus.InBattle)]
    public async Task ShouldNotAttackPartyIfInAUnattackableStatus(PartyStatus targetPartyStatus)
    {
        Point position = new(1, 2);
        Point destination = new(5, 6);
        Party targetParty = new()
        {
            Status = targetPartyStatus,
            Position = destination,
            User = new User(),
        };
        Party party = new()
        {
            Status = PartyStatus.MovingToAttackParty,
            Position = position,
            TargetedParty = targetParty,
            User = new User(),
        };
        ArrangeDb.Parties.AddRange(party, targetParty);
        await ArrangeDb.SaveChangesAsync();

        Point newPosition = new(2, 3);
        Mock<IStrategusMap> strategusMapMock = new();
        strategusMapMock.Setup(m => m.ViewDistance).Returns(500);
        strategusMapMock
            .Setup(m => m.MovePointTowards(position, destination, It.IsAny<double>()))
            .Returns(newPosition);
        strategusMapMock
            .Setup(m => m.ArePointsAtInteractionDistance(newPosition, destination))
            .Returns(true);
        UpdatePartyPositionsCommand.Handler handler = new(ActDb, strategusMapMock.Object, SpeedModelMock);
        await handler.Handle(new UpdatePartyPositionsCommand
        {
            DeltaTime = TimeSpan.FromMinutes(1),
        }, CancellationToken.None);

        Assert.AreEqual(0, await AssertDb.Battles.CountAsync());
    }

    [Test]
    public async Task ShouldAttackPartyIfCloseEnough()
    {
        Point position = new(1, 2);
        Point destination = new(5, 6);
        Party targetParty = new()
        {
            Status = PartyStatus.Idle,
            Position = destination,
            User = new User(),
        };
        Party party = new()
        {
            Status = PartyStatus.MovingToAttackParty,
            Position = position,
            TargetedParty = targetParty,
            User = new User(),
        };
        ArrangeDb.Parties.AddRange(party, targetParty);
        await ArrangeDb.SaveChangesAsync();

        Point newPosition = new(3, 4);
        Mock<IStrategusMap> strategusMapMock = new();
        strategusMapMock.Setup(m => m.ViewDistance).Returns(500);
        strategusMapMock
            .Setup(m => m.MovePointTowards(position, destination, It.IsAny<double>()))
            .Returns(newPosition);
        strategusMapMock
            .Setup(m => m.ArePointsAtInteractionDistance(newPosition, destination))
            .Returns(true);
        UpdatePartyPositionsCommand.Handler handler = new(ActDb, strategusMapMock.Object, SpeedModelMock);
        await handler.Handle(new UpdatePartyPositionsCommand
        {
            DeltaTime = TimeSpan.FromMinutes(1),
        }, CancellationToken.None);

        var battle = await AssertDb.Battles
            .Include(b => b.Fighters).ThenInclude(f => f.Party)
            .FirstOrDefaultAsync();
        Assert.IsNotNull(battle);
        Assert.AreEqual(BattlePhase.Preparation, battle!.Phase);
        Assert.AreEqual(new Point(4, 5), battle.Position);
        Assert.AreEqual(2, battle.Fighters.Count);

        Assert.AreEqual(party.Id, battle.Fighters[0].PartyId);
        Assert.AreEqual(PartyStatus.InBattle, battle.Fighters[0].Party!.Status);
        Assert.AreEqual(BattleSide.Attacker, battle.Fighters[0].Side);
        Assert.IsTrue(battle.Fighters[0].Commander);

        Assert.AreEqual(targetParty.Id, battle.Fighters[1].PartyId);
        Assert.AreEqual(PartyStatus.InBattle, battle.Fighters[1].Party!.Status);
        Assert.AreEqual(BattleSide.Defender, battle.Fighters[1].Side);
        Assert.IsTrue(battle.Fighters[1].Commander);
    }

    [TestCase(PartyStatus.MovingToSettlement)]
    [TestCase(PartyStatus.MovingToAttackSettlement)]
    public async Task MovingToASettlementShouldMove(PartyStatus status)
    {
        Point position = new(1, 2);
        Point destination = new(5, 6);
        Party party = new()
        {
            Status = status,
            Position = position,
            TargetedSettlement = new Settlement { Position = destination },
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);
        await ArrangeDb.SaveChangesAsync();

        Point newPosition = new(2, 3);
        Mock<IStrategusMap> strategusMapMock = new();
        strategusMapMock
            .Setup(m => m.MovePointTowards(position, destination, It.IsAny<double>()))
            .Returns(newPosition);
        strategusMapMock
            .Setup(m => m.ArePointsAtInteractionDistance(newPosition, destination))
            .Returns(false);
        UpdatePartyPositionsCommand.Handler handler = new(ActDb, strategusMapMock.Object, SpeedModelMock);
        await handler.Handle(new UpdatePartyPositionsCommand
        {
            DeltaTime = TimeSpan.FromMinutes(1),
        }, CancellationToken.None);

        party = await AssertDb.Parties.FirstAsync(u => u.Id == party.Id);
        Assert.AreEqual(status, party.Status);
        Assert.AreEqual(newPosition, party.Position);
    }

    [Test]
    public async Task ShouldEnterSettlementIfCloseEnough()
    {
        Point position = new(1, 2);
        Point destination = new(5, 6);
        Party party = new()
        {
            Status = PartyStatus.MovingToSettlement,
            Position = position,
            TargetedSettlement = new Settlement { Position = destination },
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);
        await ArrangeDb.SaveChangesAsync();

        Point newPosition = new(5, 5);
        Mock<IStrategusMap> strategusMapMock = new();
        strategusMapMock
            .Setup(m => m.MovePointTowards(position, destination, It.IsAny<double>()))
            .Returns(newPosition);
        strategusMapMock
            .Setup(m => m.ArePointsAtInteractionDistance(newPosition, destination))
            .Returns(true);
        UpdatePartyPositionsCommand.Handler handler = new(ActDb, strategusMapMock.Object, SpeedModelMock);
        await handler.Handle(new UpdatePartyPositionsCommand
        {
            DeltaTime = TimeSpan.FromMinutes(1),
        }, CancellationToken.None);

        party = await AssertDb.Parties.FirstAsync(u => u.Id == party.Id);
        Assert.AreEqual(PartyStatus.IdleInSettlement, party.Status);
        Assert.AreEqual(destination, party.Position);
    }

    [Test]
    public async Task ShouldNotAttackSettlementIfAlreadyInABattle()
    {
        Point position = new(1, 2);
        Point destination = new(5, 6);
        Settlement settlement = new() { Position = destination };
        Party party = new()
        {
            Status = PartyStatus.MovingToAttackSettlement,
            Position = position,
            TargetedSettlement = settlement,
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);
        Battle battle = new()
        {
            Phase = BattlePhase.Preparation,
            Fighters =
            {
                new BattleFighter
                {
                    Party = null,
                    Settlement = settlement,
                    Side = BattleSide.Defender,
                    Commander = true,
                },
            },
        };
        ArrangeDb.Battles.Add(battle);
        await ArrangeDb.SaveChangesAsync();

        Point newPosition = new(5, 5);
        Mock<IStrategusMap> strategusMapMock = new();
        strategusMapMock
            .Setup(m => m.MovePointTowards(position, destination, It.IsAny<double>()))
            .Returns(newPosition);
        strategusMapMock
            .Setup(m => m.ArePointsAtInteractionDistance(newPosition, destination))
            .Returns(true);
        UpdatePartyPositionsCommand.Handler handler = new(ActDb, strategusMapMock.Object, SpeedModelMock);
        await handler.Handle(new UpdatePartyPositionsCommand
        {
            DeltaTime = TimeSpan.FromMinutes(1),
        }, CancellationToken.None);

        Assert.AreEqual(1, await AssertDb.Battles.CountAsync());
    }

    [Test]
    public async Task ShouldAttackSettlementIfCloseEnough()
    {
        Point position = new(1, 2);
        Point destination = new(5, 6);
        Settlement settlement = new()
        {
            Region = Region.NorthAmerica,
            Position = destination,
        };
        Party party = new()
        {
            Region = Region.Europe,
            Status = PartyStatus.MovingToAttackSettlement,
            Position = position,
            TargetedSettlement = settlement,
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);
        await ArrangeDb.SaveChangesAsync();

        Point newPosition = new(3, 4);
        Mock<IStrategusMap> strategusMapMock = new();
        strategusMapMock
            .Setup(m => m.MovePointTowards(position, destination, It.IsAny<double>()))
            .Returns(newPosition);
        strategusMapMock
            .Setup(m => m.ArePointsAtInteractionDistance(newPosition, destination))
            .Returns(true);
        UpdatePartyPositionsCommand.Handler handler = new(ActDb, strategusMapMock.Object, SpeedModelMock);
        await handler.Handle(new UpdatePartyPositionsCommand
        {
            DeltaTime = TimeSpan.FromMinutes(1),
        }, CancellationToken.None);

        var battle = await AssertDb.Battles
            .Include(b => b.Fighters).ThenInclude(f => f.Party)
            .FirstOrDefaultAsync();
        Assert.IsNotNull(battle);
        Assert.AreEqual(Region.NorthAmerica, battle!.Region);
        Assert.AreEqual(BattlePhase.Preparation, battle.Phase);
        Assert.AreEqual(new Point(4, 5), battle.Position);

        Assert.AreEqual(2, battle.Fighters.Count);

        Assert.AreEqual(party.Id, battle.Fighters[0].PartyId);
        Assert.AreEqual(PartyStatus.InBattle, battle.Fighters[0].Party!.Status);
        Assert.IsNull(battle.Fighters[0].SettlementId);
        Assert.AreEqual(BattleSide.Attacker, battle.Fighters[0].Side);
        Assert.IsTrue(battle.Fighters[0].Commander);

        Assert.IsNull(battle.Fighters[1].PartyId);
        Assert.AreEqual(settlement.Id, battle.Fighters[1].SettlementId);
        Assert.AreEqual(BattleSide.Defender, battle.Fighters[1].Side);
        Assert.IsTrue(battle.Fighters[1].Commander);
    }
}
