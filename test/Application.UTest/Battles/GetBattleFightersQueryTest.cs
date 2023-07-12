using Crpg.Application.Battles.Queries;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Parties;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Battles;

public class GetBattleFightersQueryTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfBattleNotFound()
    {
        GetBattleFightersQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetBattleFightersQuery
        {
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

        GetBattleFightersQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetBattleFightersQuery
        {
            BattleId = battle.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.BattleInvalidPhase));
    }

    [Test]
    public async Task ShouldReturnFighters()
    {
        Battle battle = new()
        {
            Phase = BattlePhase.Hiring,
            Fighters =
            {
                new BattleFighter
                {
                    Settlement = new Settlement { Name = "a" },
                },
                new BattleFighter
                {
                    Party = new Party { User = new User { Name = "b" } },
                    Commander = true,
                },
                new BattleFighter
                {
                    Party = new Party { User = new User { Name = "c" } },
                },
            },
        };
        ArrangeDb.Battles.Add(battle);
        await ArrangeDb.SaveChangesAsync();

        GetBattleFightersQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetBattleFightersQuery
        {
            BattleId = battle.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        var fighters = res.Data!;
        Assert.That(fighters.Count, Is.EqualTo(3));
        Assert.That(fighters[0].Settlement!.Name, Is.EqualTo("a"));
        Assert.That(fighters[1].Party!.User.Name, Is.EqualTo("b"));
        Assert.That(fighters[1].Commander, Is.EqualTo(true));
        Assert.That(fighters[2].Party!.User.Name, Is.EqualTo("c"));
        Assert.That(fighters[2].Commander, Is.EqualTo(false));
    }
}
