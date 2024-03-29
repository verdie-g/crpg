﻿using Crpg.Application.Battles.Commands;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Users;
using Moq;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Battles;

public class ApplyAsFighterToBattleCommandTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfPartyNotFound()
    {
        ApplyAsFighterToBattleCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IStrategusMap>());
        var res = await handler.Handle(new()
        {
            PartyId = 1,
            BattleId = 2,
            Side = BattleSide.Attacker,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PartyNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorIfPartyInBattle()
    {
        Party party = new() { Status = PartyStatus.InBattle, User = new User() };
        ArrangeDb.Parties.Add(party);
        await ArrangeDb.SaveChangesAsync();

        ApplyAsFighterToBattleCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IStrategusMap>());
        var res = await handler.Handle(new()
        {
            PartyId = party.Id,
            BattleId = 2,
            Side = BattleSide.Attacker,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PartyInBattle));
    }

    [Test]
    public async Task ShouldReturnErrorIfBattleNotFound()
    {
        Party party = new() { User = new User() };
        ArrangeDb.Parties.Add(party);
        await ArrangeDb.SaveChangesAsync();

        ApplyAsFighterToBattleCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IStrategusMap>());
        var res = await handler.Handle(new()
        {
            PartyId = party.Id,
            BattleId = 2,
            Side = BattleSide.Attacker,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.BattleNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorIfBattleNotInPreparation()
    {
        Party party = new()
        {
            Status = PartyStatus.Idle,
            Position = new Point(1, 2),
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);
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
            PartyId = party.Id,
            BattleId = battle.Id,
            Side = BattleSide.Attacker,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.BattleInvalidPhase));
    }

    [Test]
    public async Task ShouldReturnErrorIfPartyTooFarFromBattle()
    {
        Party party = new()
        {
            Status = PartyStatus.Idle,
            Position = new Point(1, 2),
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);
        Battle battle = new()
        {
            Phase = BattlePhase.Preparation,
            Position = new Point(3, 4),
        };
        ArrangeDb.Battles.Add(battle);
        await ArrangeDb.SaveChangesAsync();

        Mock<IStrategusMap> strategusMapMock = new(MockBehavior.Strict);
        strategusMapMock
            .Setup(m => m.ArePointsAtInteractionDistance(party.Position, battle.Position))
            .Returns(false);

        ApplyAsFighterToBattleCommand.Handler handler = new(ActDb, Mapper, strategusMapMock.Object);
        var res = await handler.Handle(new()
        {
            PartyId = party.Id,
            BattleId = battle.Id,
            Side = BattleSide.Attacker,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.BattleTooFar));
    }

    [TestCase(BattleFighterApplicationStatus.Pending)]
    [TestCase(BattleFighterApplicationStatus.Accepted)]
    public async Task ShouldReturnExistingApplication(BattleFighterApplicationStatus existingApplicationStatus)
    {
        Party party = new()
        {
            Status = PartyStatus.Idle,
            Position = new Point(1, 2),
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);
        Battle battle = new()
        {
            Phase = BattlePhase.Preparation,
            Position = new Point(3, 4),
        };
        ArrangeDb.Battles.Add(battle);
        BattleFighterApplication existingApplication = new()
        {
            Side = BattleSide.Defender,
            Status = existingApplicationStatus,
            Battle = battle,
            Party = party,
        };
        ArrangeDb.BattleFighterApplications.Add(existingApplication);
        await ArrangeDb.SaveChangesAsync();

        Mock<IStrategusMap> strategusMapMock = new(MockBehavior.Strict);
        strategusMapMock
            .Setup(m => m.ArePointsAtInteractionDistance(party.Position, battle.Position))
            .Returns(true);

        ApplyAsFighterToBattleCommand.Handler handler = new(ActDb, Mapper, strategusMapMock.Object);
        var res = await handler.Handle(new()
        {
            PartyId = party.Id,
            BattleId = battle.Id,
            Side = BattleSide.Defender,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data!.Id, Is.EqualTo(existingApplication.Id));
    }

    [Test]
    public async Task ShouldApply()
    {
        Party party = new()
        {
            Status = PartyStatus.Idle,
            Position = new Point(1, 2),
            User = new User(),
        };
        ArrangeDb.Parties.Add(party);
        Battle battle = new()
        {
            Phase = BattlePhase.Preparation,
            Position = new Point(3, 4),
        };
        ArrangeDb.Battles.Add(battle);
        await ArrangeDb.SaveChangesAsync();

        Mock<IStrategusMap> strategusMapMock = new(MockBehavior.Strict);
        strategusMapMock
            .Setup(m => m.ArePointsAtInteractionDistance(party.Position, battle.Position))
            .Returns(true);

        ApplyAsFighterToBattleCommand.Handler handler = new(ActDb, Mapper, strategusMapMock.Object);
        var res = await handler.Handle(new()
        {
            PartyId = party.Id,
            BattleId = battle.Id,
            Side = BattleSide.Defender,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        var application = res.Data!;
        Assert.That(application.Id, Is.Not.Zero);
        Assert.That(application.Party!.Id, Is.EqualTo(party.Id));
        Assert.That(application.Side, Is.EqualTo(BattleSide.Defender));
        Assert.That(application.Status, Is.EqualTo(BattleFighterApplicationStatus.Pending));
    }
}
