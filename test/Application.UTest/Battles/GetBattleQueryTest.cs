using Crpg.Application.Battles.Queries;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Battles;
using NUnit.Framework;

namespace Crpg.Application.UTest.Battles;

public class GetBattleQueryTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfBattleNotFound()
    {
        GetBattleQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetBattleQuery
        {
            BattleId = 99,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.BattleNotFound));
    }

    [Test]
    public async Task ShouldReturnErrorIfBattleInPreparation()
    {
        Battle battle = new() { Phase = BattlePhase.Preparation };
        ArrangeDb.Battles.Add(battle);
        await ArrangeDb.SaveChangesAsync();

        GetBattleQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetBattleQuery
        {
            BattleId = battle.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.BattleInvalidPhase));
    }

    [Test]
    public async Task ShouldGetTheBattle()
    {
        Battle battle = new() { Phase = BattlePhase.Hiring };
        ArrangeDb.Battles.Add(battle);
        await ArrangeDb.SaveChangesAsync();

        GetBattleQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetBattleQuery
        {
            BattleId = battle.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data!.Id, Is.EqualTo(battle.Id));
    }
}
