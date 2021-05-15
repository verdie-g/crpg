using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Battles.Queries;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Battles;
using NUnit.Framework;

namespace Crpg.Application.UTest.Battles
{
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

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.BattleNotFound, res.Errors![0].Code);
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

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.BattleInvalidPhase, res.Errors![0].Code);
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

            Assert.IsNull(res.Errors);
            Assert.AreEqual(battle.Id, res.Data!.Id);
        }
    }
}
