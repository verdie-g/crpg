using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Battles.Queries;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Heroes;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Battles
{
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

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.BattleNotFound, res.Errors![0].Code);
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

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.BattleInvalidPhase, res.Errors![0].Code);
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
                        Hero = new Hero { User = new User { Name = "b" } },
                        Commander = true,
                    },
                    new BattleFighter
                    {
                        Hero = new Hero { User = new User { Name = "c" } },
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

            Assert.IsNull(res.Errors);
            var fighters = res.Data!;
            Assert.AreEqual(3, fighters.Count);
            Assert.AreEqual("a", fighters[0].Settlement!.Name);
            Assert.AreEqual("b", fighters[1].Hero!.Name);
            Assert.AreEqual(true, fighters[1].Commander);
            Assert.AreEqual("c", fighters[2].Hero!.Name);
            Assert.AreEqual(false, fighters[2].Commander);
        }
    }
}
