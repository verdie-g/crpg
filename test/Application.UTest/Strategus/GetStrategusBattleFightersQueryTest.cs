using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Results;
using Crpg.Application.Strategus.Queries;
using Crpg.Domain.Entities.Strategus;
using Crpg.Domain.Entities.Strategus.Battles;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Strategus
{
    public class GetStrategusBattleFightersQueryTest : TestBase
    {
        [Test]
        public async Task ShouldReturnErrorIfBattleNotFound()
        {
            GetStrategusBattleFightersQuery.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new GetStrategusBattleFightersQuery
            {
                BattleId = 99,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.BattleNotFound, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfBattleIsInPreparation()
        {
            StrategusBattle battle = new() { Phase = StrategusBattlePhase.Preparation };
            ArrangeDb.StrategusBattles.Add(battle);
            await ArrangeDb.SaveChangesAsync();

            GetStrategusBattleFightersQuery.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new GetStrategusBattleFightersQuery
            {
                BattleId = battle.Id,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.BattleInvalidPhase, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnFighters()
        {
            StrategusBattle battle = new()
            {
                Phase = StrategusBattlePhase.Hiring,
                Fighters =
                {
                    new StrategusBattleFighter
                    {
                        Settlement = new StrategusSettlement { Name = "a" },
                    },
                    new StrategusBattleFighter
                    {
                        Hero = new StrategusHero { User = new User { Name = "b" } },
                    },
                },
            };
            ArrangeDb.StrategusBattles.Add(battle);
            await ArrangeDb.SaveChangesAsync();

            GetStrategusBattleFightersQuery.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new GetStrategusBattleFightersQuery
            {
                BattleId = battle.Id,
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            var fighters = res.Data!;
            Assert.AreEqual(2, fighters.Count);
            Assert.AreEqual("a", fighters[0].Settlement!.Name);
            Assert.AreEqual("b", fighters[1].Hero!.Name);
        }
    }
}
