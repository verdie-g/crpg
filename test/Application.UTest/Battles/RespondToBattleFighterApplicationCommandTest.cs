using Crpg.Application.Battles.Commands;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.Battles;

public class RespondToBattleFighterApplicationCommandTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfPartyIsNotFound()
    {
        RespondToBattleFighterApplicationCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new RespondToBattleFighterApplicationCommand
        {
            PartyId = 99,
            FighterApplicationId = 99,
            Accept = true,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.PartyNotFound, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldReturnErrorIfApplicationIsNotFound()
    {
        Party party = new() { User = new User() };
        ArrangeDb.Parties.Add(party);
        await ArrangeDb.SaveChangesAsync();

        RespondToBattleFighterApplicationCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new RespondToBattleFighterApplicationCommand
        {
            PartyId = party.Id,
            FighterApplicationId = 99,
            Accept = true,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.ApplicationNotFound, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldReturnErrorIfPartyIsNotAFighter()
    {
        Party party = new() { User = new User() };
        ArrangeDb.Parties.Add(party);

        Battle battle = new()
        {
            Phase = BattlePhase.Preparation,
        };
        ArrangeDb.Battles.Add(battle);

        BattleFighterApplication application = new()
        {
            Side = BattleSide.Attacker,
            Status = BattleFighterApplicationStatus.Pending,
            Battle = battle,
            Party = new Party { User = new User() },
        };
        ArrangeDb.BattleFighterApplications.Add(application);
        await ArrangeDb.SaveChangesAsync();

        RespondToBattleFighterApplicationCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new RespondToBattleFighterApplicationCommand
        {
            PartyId = party.Id,
            FighterApplicationId = application.Id,
            Accept = true,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.PartyNotAFighter, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldReturnErrorIfPartyIsNotACommander()
    {
        Party party = new() { User = new User() };
        ArrangeDb.Parties.Add(party);

        Battle battle = new()
        {
            Phase = BattlePhase.Preparation,
            Fighters =
            {
                new BattleFighter
                {
                    Side = BattleSide.Attacker,
                    Commander = false,
                    Party = party,
                },
            },
        };
        ArrangeDb.Battles.Add(battle);

        BattleFighterApplication application = new()
        {
            Side = BattleSide.Attacker,
            Status = BattleFighterApplicationStatus.Pending,
            Battle = battle,
            Party = new Party { User = new User() },
        };
        ArrangeDb.BattleFighterApplications.Add(application);
        await ArrangeDb.SaveChangesAsync();

        RespondToBattleFighterApplicationCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new RespondToBattleFighterApplicationCommand
        {
            PartyId = party.Id,
            FighterApplicationId = application.Id,
            Accept = true,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.FighterNotACommander, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldReturnErrorIfFightersNotOnTheSameSide()
    {
        Party party = new() { User = new User() };
        ArrangeDb.Parties.Add(party);

        Battle battle = new()
        {
            Phase = BattlePhase.Preparation,
            Fighters =
            {
                new BattleFighter
                {
                    Side = BattleSide.Defender,
                    Commander = true,
                    Party = party,
                },
            },
        };
        ArrangeDb.Battles.Add(battle);

        BattleFighterApplication application = new()
        {
            Side = BattleSide.Attacker,
            Status = BattleFighterApplicationStatus.Pending,
            Battle = battle,
            Party = new Party { User = new User() },
        };
        ArrangeDb.BattleFighterApplications.Add(application);
        await ArrangeDb.SaveChangesAsync();

        RespondToBattleFighterApplicationCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new RespondToBattleFighterApplicationCommand
        {
            PartyId = party.Id,
            FighterApplicationId = application.Id,
            Accept = true,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.PartiesNotOnTheSameSide, res.Errors![0].Code);
    }

    [TestCase(BattlePhase.Hiring)]
    [TestCase(BattlePhase.Scheduled)]
    [TestCase(BattlePhase.Live)]
    [TestCase(BattlePhase.End)]
    public async Task ShouldReturnErrorIfBattleIsNotInPreparation(BattlePhase battlePhase)
    {
        Party party = new() { User = new User() };
        ArrangeDb.Parties.Add(party);

        Battle battle = new()
        {
            Phase = battlePhase,
            Fighters =
            {
                new BattleFighter
                {
                    Side = BattleSide.Attacker,
                    Commander = true,
                    Party = party,
                },
            },
        };
        ArrangeDb.Battles.Add(battle);

        BattleFighterApplication application = new()
        {
            Side = BattleSide.Attacker,
            Status = BattleFighterApplicationStatus.Pending,
            Battle = battle,
            Party = new Party { User = new User() },
        };
        ArrangeDb.BattleFighterApplications.Add(application);
        await ArrangeDb.SaveChangesAsync();

        RespondToBattleFighterApplicationCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new RespondToBattleFighterApplicationCommand
        {
            PartyId = party.Id,
            FighterApplicationId = application.Id,
            Accept = true,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.BattleInvalidPhase, res.Errors![0].Code);
    }

    [TestCase(BattleFighterApplicationStatus.Declined)]
    [TestCase(BattleFighterApplicationStatus.Accepted)]
    public async Task ShouldReturnErrorIfApplicationIsClosed(BattleFighterApplicationStatus applicationStatus)
    {
        Party party = new() { User = new User() };
        ArrangeDb.Parties.Add(party);

        Battle battle = new()
        {
            Phase = BattlePhase.Preparation,
            Fighters =
            {
                new BattleFighter
                {
                    Side = BattleSide.Attacker,
                    Commander = true,
                    Party = party,
                },
            },
        };
        ArrangeDb.Battles.Add(battle);

        BattleFighterApplication application = new()
        {
            Side = BattleSide.Attacker,
            Status = applicationStatus,
            Battle = battle,
            Party = new Party { User = new User() },
        };
        ArrangeDb.BattleFighterApplications.Add(application);
        await ArrangeDb.SaveChangesAsync();

        RespondToBattleFighterApplicationCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new RespondToBattleFighterApplicationCommand
        {
            PartyId = party.Id,
            FighterApplicationId = application.Id,
            Accept = true,
        }, CancellationToken.None);

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.ApplicationClosed, res.Errors![0].Code);
    }

    [Test]
    public async Task ShouldDeclineApplication()
    {
        Party party = new() { User = new User() };
        ArrangeDb.Parties.Add(party);

        Battle battle = new()
        {
            Phase = BattlePhase.Preparation,
            Fighters =
            {
                new BattleFighter
                {
                    Side = BattleSide.Attacker,
                    Commander = true,
                    Party = party,
                },
            },
        };
        ArrangeDb.Battles.Add(battle);

        BattleFighterApplication application = new()
        {
            Side = BattleSide.Attacker,
            Status = BattleFighterApplicationStatus.Pending,
            Battle = battle,
            Party = new Party { User = new User() },
        };
        ArrangeDb.BattleFighterApplications.Add(application);
        await ArrangeDb.SaveChangesAsync();

        RespondToBattleFighterApplicationCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new RespondToBattleFighterApplicationCommand
        {
            PartyId = party.Id,
            FighterApplicationId = application.Id,
            Accept = false,
        }, CancellationToken.None);

        Assert.IsNull(res.Errors);
        var applicationVm = res.Data!;
        Assert.AreEqual(application.Id, applicationVm.Id);
        Assert.AreEqual(BattleFighterApplicationStatus.Declined, applicationVm.Status);

        Assert.AreEqual(1, await AssertDb.BattleFighters.CountAsync());
    }

    [Test]
    public async Task ShouldAcceptApplication()
    {
        Party party = new() { User = new User() };
        Party applyingParty = new() { User = new User() };
        ArrangeDb.Parties.AddRange(applyingParty);

        Battle battle = new()
        {
            Phase = BattlePhase.Preparation,
            Fighters =
            {
                new BattleFighter
                {
                    Side = BattleSide.Attacker,
                    Commander = true,
                    Party = party,
                },
            },
        };
        ArrangeDb.Battles.Add(battle);

        BattleFighterApplication application = new()
        {
            Side = BattleSide.Attacker,
            Status = BattleFighterApplicationStatus.Pending,
            Battle = battle,
            Party = applyingParty,
        };
        BattleFighterApplication[] otherApplications =
        {
            new() // Should get deleted.
            {
                Status = BattleFighterApplicationStatus.Pending,
                Battle = battle,
                Party = applyingParty,
            },
            new() // Should stay.
            {
                Status = BattleFighterApplicationStatus.Accepted,
                Battle = battle,
                Party = applyingParty,
            },
            new() // Should stay.
            {
                Status = BattleFighterApplicationStatus.Pending,
                Battle = new Battle(),
                Party = applyingParty,
            },
            new() // Should stay.
            {
                Status = BattleFighterApplicationStatus.Pending,
                Battle = battle,
                Party = new Party { User = new User() },
            },
        };
        ArrangeDb.BattleFighterApplications.Add(application);
        ArrangeDb.BattleFighterApplications.AddRange(otherApplications);
        await ArrangeDb.SaveChangesAsync();

        RespondToBattleFighterApplicationCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new RespondToBattleFighterApplicationCommand
        {
            PartyId = party.Id,
            FighterApplicationId = application.Id,
            Accept = true,
        }, CancellationToken.None);

        Assert.IsNull(res.Errors);
        var applicationVm = res.Data!;
        Assert.AreEqual(application.Id, applicationVm.Id);
        Assert.AreEqual(BattleFighterApplicationStatus.Accepted, applicationVm.Status);

        Assert.AreEqual(2, await AssertDb.BattleFighters.CountAsync());
        Assert.AreEqual(4, await AssertDb.BattleFighterApplications.CountAsync());
    }
}
