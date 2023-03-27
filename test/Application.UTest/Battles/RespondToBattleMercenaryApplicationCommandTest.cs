using Crpg.Application.Battles.Commands;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Battles;

public class RespondToBattleMercenaryApplicationCommandTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfPartyIsNotFound()
    {
        RespondToBattleMercenaryApplicationCommand.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassResolver>());
        var res = await handler.Handle(new RespondToBattleMercenaryApplicationCommand
        {
            PartyId = 99,
            MercenaryApplicationId = 99,
            Accept = true,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PartyNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorIfApplicationIsNotFound()
    {
        Party party = new() { User = new User() };
        ArrangeDb.Parties.Add(party);
        await ArrangeDb.SaveChangesAsync();

        RespondToBattleMercenaryApplicationCommand.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassResolver>());
        var res = await handler.Handle(new RespondToBattleMercenaryApplicationCommand
        {
            PartyId = party.Id,
            MercenaryApplicationId = 99,
            Accept = true,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.ApplicationNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorIfPartyIsNotAFighter()
    {
        Party party = new() { User = new User() };
        ArrangeDb.Parties.Add(party);

        Battle battle = new()
        {
            Phase = BattlePhase.Hiring,
        };
        ArrangeDb.Battles.Add(battle);

        BattleMercenaryApplication application = new()
        {
            Side = BattleSide.Attacker,
            Status = BattleMercenaryApplicationStatus.Pending,
            Battle = battle,
            Character = new Character { User = new User() },
        };
        ArrangeDb.BattleMercenaryApplications.Add(application);
        await ArrangeDb.SaveChangesAsync();

        RespondToBattleMercenaryApplicationCommand.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassResolver>());
        var res = await handler.Handle(new RespondToBattleMercenaryApplicationCommand
        {
            PartyId = party.Id,
            MercenaryApplicationId = application.Id,
            Accept = true,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PartyNotAFighter));
    }

    [Test]
    public async Task ShouldReturnErrorIfPartyAndCharacterNotOnTheSameSide()
    {
        Party party = new() { User = new User() };
        ArrangeDb.Parties.Add(party);

        Battle battle = new()
        {
            Phase = BattlePhase.Hiring,
            Fighters =
            {
                new BattleFighter
                {
                    Side = BattleSide.Defender,
                    Commander = false,
                    Party = party,
                },
            },
        };
        ArrangeDb.Battles.Add(battle);

        BattleMercenaryApplication application = new()
        {
            Side = BattleSide.Attacker,
            Status = BattleMercenaryApplicationStatus.Pending,
            Battle = battle,
            Character = new Character { User = new User() },
        };
        ArrangeDb.BattleMercenaryApplications.Add(application);
        await ArrangeDb.SaveChangesAsync();

        RespondToBattleMercenaryApplicationCommand.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassResolver>());
        var res = await handler.Handle(new RespondToBattleMercenaryApplicationCommand
        {
            PartyId = party.Id,
            MercenaryApplicationId = application.Id,
            Accept = true,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PartiesNotOnTheSameSide));
    }

    [TestCase(BattlePhase.Preparation)]
    [TestCase(BattlePhase.Scheduled)]
    [TestCase(BattlePhase.Live)]
    [TestCase(BattlePhase.End)]
    public async Task ShouldReturnErrorIfBattleIsNotInHiringPhase(BattlePhase battlePhase)
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
                    Commander = false,
                    Party = party,
                },
            },
        };
        ArrangeDb.Battles.Add(battle);

        BattleMercenaryApplication application = new()
        {
            Side = BattleSide.Attacker,
            Status = BattleMercenaryApplicationStatus.Pending,
            Battle = battle,
            Character = new Character { User = new User() },
        };
        ArrangeDb.BattleMercenaryApplications.Add(application);
        await ArrangeDb.SaveChangesAsync();

        RespondToBattleMercenaryApplicationCommand.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassResolver>());
        var res = await handler.Handle(new RespondToBattleMercenaryApplicationCommand
        {
            PartyId = party.Id,
            MercenaryApplicationId = application.Id,
            Accept = true,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.BattleInvalidPhase));
    }

    [TestCase(BattleMercenaryApplicationStatus.Declined)]
    [TestCase(BattleMercenaryApplicationStatus.Accepted)]
    public async Task ShouldReturnErrorIfApplicationIsClosed(BattleMercenaryApplicationStatus applicationStatus)
    {
        Party party = new() { User = new User() };
        ArrangeDb.Parties.Add(party);

        Battle battle = new()
        {
            Phase = BattlePhase.Hiring,
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

        BattleMercenaryApplication application = new()
        {
            Side = BattleSide.Attacker,
            Status = applicationStatus,
            Battle = battle,
            Character = new Character { User = new User() },
        };
        ArrangeDb.BattleMercenaryApplications.Add(application);
        await ArrangeDb.SaveChangesAsync();

        RespondToBattleMercenaryApplicationCommand.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassResolver>());
        var res = await handler.Handle(new RespondToBattleMercenaryApplicationCommand
        {
            PartyId = party.Id,
            MercenaryApplicationId = application.Id,
            Accept = true,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.ApplicationClosed));
    }

    [Test]
    public async Task ShouldDeclineApplication()
    {
        Party party = new() { User = new User() };
        ArrangeDb.Parties.Add(party);

        Battle battle = new()
        {
            Phase = BattlePhase.Hiring,
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

        BattleMercenaryApplication application = new()
        {
            Side = BattleSide.Attacker,
            Status = BattleMercenaryApplicationStatus.Pending,
            Battle = battle,
            Character = new Character { User = new User() },
        };
        ArrangeDb.BattleMercenaryApplications.Add(application);
        await ArrangeDb.SaveChangesAsync();

        RespondToBattleMercenaryApplicationCommand.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassResolver>());
        var res = await handler.Handle(new RespondToBattleMercenaryApplicationCommand
        {
            PartyId = party.Id,
            MercenaryApplicationId = application.Id,
            Accept = false,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        var applicationVm = res.Data!;
        Assert.That(applicationVm.Id, Is.EqualTo(application.Id));
        Assert.That(applicationVm.Status, Is.EqualTo(BattleMercenaryApplicationStatus.Declined));

        Assert.That(await AssertDb.BattleMercenaries.CountAsync(), Is.EqualTo(0));
    }

    [Test]
    public async Task ShouldAcceptApplication()
    {
        User applyingUser = new()
        {
            Characters = { new Character(), new Character() },
        };
        ArrangeDb.Users.Add(applyingUser);

        Party party = new() { User = new User() };
        ArrangeDb.Parties.Add(party);

        Battle battle = new()
        {
            Phase = BattlePhase.Hiring,
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

        BattleMercenaryApplication application = new()
        {
            Side = BattleSide.Attacker,
            Status = BattleMercenaryApplicationStatus.Pending,
            Battle = battle,
            Character = applyingUser.Characters[0],
        };
        BattleMercenaryApplication[] otherApplications =
        {
            new() // Should get deleted.
            {
                Status = BattleMercenaryApplicationStatus.Pending,
                Battle = battle,
                Character = applyingUser.Characters[0],
            },
            new() // Should stay.
            {
                Status = BattleMercenaryApplicationStatus.Accepted,
                Battle = battle,
                Character = applyingUser.Characters[0],
            },
            new() // Should stay.
            {
                Status = BattleMercenaryApplicationStatus.Pending,
                Battle = new Battle(),
                Character = applyingUser.Characters[1],
            },
            new() // Should get deleted.
            {
                Status = BattleMercenaryApplicationStatus.Pending,
                Battle = battle,
                Character = applyingUser.Characters[1],
            },
            new() // Should stay.
            {
                Status = BattleMercenaryApplicationStatus.Pending,
                Battle = battle,
                Character = new Character { User = new User() },
            },
        };
        ArrangeDb.BattleMercenaryApplications.Add(application);
        ArrangeDb.BattleMercenaryApplications.AddRange(otherApplications);
        await ArrangeDb.SaveChangesAsync();

        RespondToBattleMercenaryApplicationCommand.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassResolver>());
        var res = await handler.Handle(new RespondToBattleMercenaryApplicationCommand
        {
            PartyId = party.Id,
            MercenaryApplicationId = application.Id,
            Accept = true,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        var applicationVm = res.Data!;
        Assert.That(applicationVm.Id, Is.EqualTo(application.Id));
        Assert.That(applicationVm.Status, Is.EqualTo(BattleMercenaryApplicationStatus.Accepted));

        Assert.That(await AssertDb.BattleMercenaries.CountAsync(), Is.EqualTo(1));
        Assert.That(await AssertDb.BattleMercenaryApplications.CountAsync(), Is.EqualTo(4));
    }
}
