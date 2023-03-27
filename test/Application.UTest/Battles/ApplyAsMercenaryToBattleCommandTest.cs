using Crpg.Application.Battles.Commands;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Battles;

public class ApplyAsMercenaryToBattleCommandTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfCharacterNotFound()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        ApplyAsMercenaryToBattleCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new()
        {
            UserId = user.Id,
            CharacterId = 2,
            BattleId = 3,
            Side = BattleSide.Attacker,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.CharacterNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorIfBattleNotFound()
    {
        Character character = new();
        User user = new() { Characters = { character } };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        ApplyAsMercenaryToBattleCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new()
        {
            UserId = user.Id,
            CharacterId = character.Id,
            BattleId = 2,
            Side = BattleSide.Attacker,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.BattleNotFound));
    }

    [TestCase(BattlePhase.Preparation)]
    [TestCase(BattlePhase.Scheduled)]
    [TestCase(BattlePhase.Live)]
    [TestCase(BattlePhase.End)]
    public async Task ShouldReturnErrorIfBattleNotInHiringPhase(BattlePhase battlePhase)
    {
        Character character = new();
        User user = new() { Characters = { character } };
        ArrangeDb.Users.Add(user);

        Battle battle = new() { Phase = battlePhase };
        ArrangeDb.Battles.Add(battle);
        await ArrangeDb.SaveChangesAsync();

        ApplyAsMercenaryToBattleCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new()
        {
            UserId = user.Id,
            CharacterId = character.Id,
            BattleId = battle.Id,
            Side = BattleSide.Attacker,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.BattleInvalidPhase));
    }

    [Test]
    public async Task ShouldReturnErrorIfUserIsAFighter()
    {
        Character character = new();
        User user = new() { Characters = { character } };
        ArrangeDb.Users.Add(user);

        Battle battle = new()
        {
            Phase = BattlePhase.Hiring,
            Fighters = { new BattleFighter { Party = new Party { User = user } } },
        };
        ArrangeDb.Battles.Add(battle);

        await ArrangeDb.SaveChangesAsync();

        ApplyAsMercenaryToBattleCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new()
        {
            UserId = user.Id,
            CharacterId = character.Id,
            BattleId = battle.Id,
            Side = BattleSide.Attacker,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PartyFighter));
    }

    [TestCase(BattleMercenaryApplicationStatus.Pending)]
    [TestCase(BattleMercenaryApplicationStatus.Accepted)]
    public async Task ShouldReturnExistingApplication(BattleMercenaryApplicationStatus existingApplicationStatus)
    {
        Character character = new();
        User user = new() { Characters = { character } };
        ArrangeDb.Users.Add(user);

        Battle battle = new() { Phase = BattlePhase.Hiring };
        ArrangeDb.Battles.Add(battle);

        BattleMercenaryApplication existingApplication = new()
        {
            Side = BattleSide.Attacker,
            Status = existingApplicationStatus,
            Battle = battle,
            Character = character,
        };
        ArrangeDb.BattleMercenaryApplications.Add(existingApplication);
        await ArrangeDb.SaveChangesAsync();

        ApplyAsMercenaryToBattleCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new()
        {
            UserId = user.Id,
            CharacterId = character.Id,
            BattleId = battle.Id,
            Side = BattleSide.Attacker,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        var application = res.Data!;
        Assert.That(application.Id, Is.EqualTo(existingApplication.Id));
        Assert.That(application.User.Id, Is.EqualTo(user.Id));
        Assert.That(application.Character.Id, Is.EqualTo(character.Id));
        Assert.That(application.Status, Is.EqualTo(existingApplicationStatus));
    }

    [Test]
    public async Task ShouldApply()
    {
        Character character = new();
        User user = new() { Characters = { character } };
        ArrangeDb.Users.Add(user);

        Battle battle = new() { Phase = BattlePhase.Hiring };
        ArrangeDb.Battles.Add(battle);
        await ArrangeDb.SaveChangesAsync();

        ApplyAsMercenaryToBattleCommand.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new()
        {
            UserId = user.Id,
            CharacterId = character.Id,
            BattleId = battle.Id,
            Side = BattleSide.Defender,
            Wage = 123,
            Note = "toto",
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        var application = res.Data!;
        Assert.That(application.Id, Is.Not.Zero);
        Assert.That(application.User.Id, Is.EqualTo(user.Id));
        Assert.That(application.Character.Id, Is.EqualTo(character.Id));
        Assert.That(application.Side, Is.EqualTo(BattleSide.Defender));
        Assert.That(application.Wage, Is.EqualTo(123));
        Assert.That(application.Note, Is.EqualTo("toto"));
        Assert.That(application.Status, Is.EqualTo(BattleMercenaryApplicationStatus.Pending));
    }
}
