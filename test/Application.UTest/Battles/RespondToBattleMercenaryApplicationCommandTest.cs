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

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.PartyNotFound, res.Errors![0].Code);
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

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.PartyNotAFighter, res.Errors![0].Code);
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

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.PartiesNotOnTheSameSide, res.Errors![0].Code);
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

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.BattleInvalidPhase, res.Errors![0].Code);
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

        Assert.IsNull(res.Errors);
        var applicationVm = res.Data!;
        Assert.AreEqual(application.Id, applicationVm.Id);
        Assert.AreEqual(BattleMercenaryApplicationStatus.Declined, applicationVm.Status);

        Assert.AreEqual(0, await AssertDb.BattleMercenaries.CountAsync());
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

        Assert.IsNull(res.Errors);
        var applicationVm = res.Data!;
        Assert.AreEqual(application.Id, applicationVm.Id);
        Assert.AreEqual(BattleMercenaryApplicationStatus.Accepted, applicationVm.Status);

        Assert.AreEqual(1, await AssertDb.BattleMercenaries.CountAsync());
        Assert.AreEqual(4, await AssertDb.BattleMercenaryApplications.CountAsync());
    }
}
