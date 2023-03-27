using Crpg.Application.Battles.Queries;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Users;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Battles;

public class GetBattleMercenariesQueryTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfBattleNotFound()
    {
        GetBattleMercenariesQuery.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassResolver>());
        var res = await handler.Handle(new GetBattleMercenariesQuery
        {
            UserId = 99,
            BattleId = 99,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.BattleNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorIfBattleIsInPreparation()
    {
        Battle battle = new() { Phase = BattlePhase.Preparation };
        ArrangeDb.Battles.Add(battle);
        await ArrangeDb.SaveChangesAsync();

        GetBattleMercenariesQuery.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassResolver>());
        var res = await handler.Handle(new GetBattleMercenariesQuery
        {
            UserId = 99,
            BattleId = battle.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.BattleInvalidPhase));
    }

    [Test]
    public async Task ShouldReturnErrorIfBattleIsInHiringPhaseAndUserNotAFighter()
    {
        Battle battle = new() { Phase = BattlePhase.Hiring };
        ArrangeDb.Battles.Add(battle);
        await ArrangeDb.SaveChangesAsync();

        GetBattleMercenariesQuery.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassResolver>());
        var res = await handler.Handle(new GetBattleMercenariesQuery
        {
            UserId = 99,
            BattleId = battle.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.PartyNotAFighter));
    }

    [Test]
    public async Task ShouldOnlyReturnOneSideDuringHiringPhase()
    {
        Battle battle = new()
        {
            Phase = BattlePhase.Hiring,
            Fighters = { new BattleFighter { PartyId = 20, Side = BattleSide.Defender } },
            Mercenaries =
            {
                new BattleMercenary
                {
                    Character = new Character { User = new User() },
                    Side = BattleSide.Attacker,
                },
                new BattleMercenary
                {
                    Character = new Character { User = new User() },
                    Side = BattleSide.Defender,
                },
            },
        };
        ArrangeDb.Battles.Add(battle);
        await ArrangeDb.SaveChangesAsync();

        GetBattleMercenariesQuery.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassResolver>());
        var res = await handler.Handle(new GetBattleMercenariesQuery
        {
            UserId = 20,
            BattleId = battle.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        var mercenaries = res.Data!;
        Assert.That(mercenaries.Count, Is.EqualTo(1));
        Assert.That(mercenaries[0].Side, Is.EqualTo(BattleSide.Defender));
    }

    [TestCase(BattlePhase.Scheduled)]
    [TestCase(BattlePhase.Live)]
    [TestCase(BattlePhase.End)]
    public async Task ShouldOnlyReturnBothSidesDuringOtherPhases(BattlePhase battlePhase)
    {
        Battle battle = new()
        {
            Phase = battlePhase,
            Mercenaries =
            {
                new BattleMercenary
                {
                    Character = new Character { User = new User() },
                    Side = BattleSide.Attacker,
                },
                new BattleMercenary
                {
                    Character = new Character { User = new User() },
                    Side = BattleSide.Defender,
                },
            },
        };
        ArrangeDb.Battles.Add(battle);
        await ArrangeDb.SaveChangesAsync();

        GetBattleMercenariesQuery.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassResolver>());
        var res = await handler.Handle(new GetBattleMercenariesQuery
        {
            UserId = 20,
            BattleId = battle.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        var mercenaries = res.Data!;
        Assert.That(mercenaries.Count, Is.EqualTo(2));
    }
}
