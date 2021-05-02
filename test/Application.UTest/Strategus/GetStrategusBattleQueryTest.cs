using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Results;
using Crpg.Application.Strategus.Queries;
using Crpg.Domain.Entities.Strategus.Battles;
using NUnit.Framework;

namespace Crpg.Application.UTest.Strategus
{
    public class GetStrategusBattleQueryTest : TestBase
    {
        [Test]
        public async Task ShouldReturnErrorIfBattleNotFound()
        {
            GetStrategusBattleQuery.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new GetStrategusBattleQuery
            {
                BattleId = 99,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.BattleNotFound, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfBattleInPreparation()
        {
            StrategusBattle battle = new() { Phase = StrategusBattlePhase.Preparation };
            ArrangeDb.StrategusBattles.Add(battle);
            await ArrangeDb.SaveChangesAsync();

            GetStrategusBattleQuery.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new GetStrategusBattleQuery
            {
                BattleId = battle.Id,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.BattleInvalidPhase, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldGetTheBattle()
        {
            StrategusBattle battle = new() { Phase = StrategusBattlePhase.Hiring };
            ArrangeDb.StrategusBattles.Add(battle);
            await ArrangeDb.SaveChangesAsync();

            GetStrategusBattleQuery.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new GetStrategusBattleQuery
            {
                BattleId = battle.Id,
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            Assert.AreEqual(battle.Id, res.Data!.Id);
        }
    }
}
