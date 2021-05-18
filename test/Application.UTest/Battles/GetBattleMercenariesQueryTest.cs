using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Battles.Queries;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Users;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Battles
{
    public class GetBattleMercenariesQueryTest : TestBase
    {
        [Test]
        public async Task ShouldReturnErrorIfBattleNotFound()
        {
            GetBattleMercenariesQuery.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassModel>());
            var res = await handler.Handle(new GetBattleMercenariesQuery
            {
                UserId = 99,
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

            GetBattleMercenariesQuery.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassModel>());
            var res = await handler.Handle(new GetBattleMercenariesQuery
            {
                UserId = 99,
                BattleId = battle.Id,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.BattleInvalidPhase, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfBattleIsInHiringPhaseAndUserNotAFighter()
        {
            Battle battle = new() { Phase = BattlePhase.Hiring };
            ArrangeDb.Battles.Add(battle);
            await ArrangeDb.SaveChangesAsync();

            GetBattleMercenariesQuery.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassModel>());
            var res = await handler.Handle(new GetBattleMercenariesQuery
            {
                UserId = 99,
                BattleId = battle.Id,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.HeroNotAFighter, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldOnlyReturnOneSideDuringHiringPhase()
        {
            Battle battle = new()
            {
                Phase = BattlePhase.Hiring,
                Fighters = { new BattleFighter { HeroId = 20, Side = BattleSide.Defender } },
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

            GetBattleMercenariesQuery.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassModel>());
            var res = await handler.Handle(new GetBattleMercenariesQuery
            {
                UserId = 20,
                BattleId = battle.Id,
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            var mercenaries = res.Data!;
            Assert.AreEqual(1, mercenaries.Count);
            Assert.AreEqual(BattleSide.Defender, mercenaries[0].Side);
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

            GetBattleMercenariesQuery.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassModel>());
            var res = await handler.Handle(new GetBattleMercenariesQuery
            {
                UserId = 20,
                BattleId = battle.Id,
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            var mercenaries = res.Data!;
            Assert.AreEqual(2, mercenaries.Count);
        }
    }
}
