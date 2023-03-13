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

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.CharacterNotFound, res.Errors![0].Code);
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

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.BattleNotFound, res.Errors![0].Code);
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

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.BattleInvalidPhase, res.Errors![0].Code);
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

        Assert.IsNotNull(res.Errors);
        Assert.AreEqual(ErrorCode.PartyFighter, res.Errors![0].Code);
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

        Assert.IsNull(res.Errors);
        var application = res.Data!;
        Assert.AreEqual(existingApplication.Id, application.Id);
        Assert.AreEqual(user.Id, application.User.Id);
        Assert.AreEqual(character.Id, application.Character.Id);
        Assert.AreEqual(existingApplicationStatus, application.Status);
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

        Assert.IsNull(res.Errors);
        var application = res.Data!;
        Assert.NotZero(application.Id);
        Assert.AreEqual(user.Id, application.User.Id);
        Assert.AreEqual(character.Id, application.Character.Id);
        Assert.AreEqual(BattleSide.Defender, application.Side);
        Assert.AreEqual(123, application.Wage);
        Assert.AreEqual("toto", application.Note);
        Assert.AreEqual(BattleMercenaryApplicationStatus.Pending, application.Status);
    }
}
