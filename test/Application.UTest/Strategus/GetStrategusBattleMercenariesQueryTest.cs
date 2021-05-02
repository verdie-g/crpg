using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Strategus.Queries;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Strategus.Battles;
using Crpg.Domain.Entities.Users;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Strategus
{
    public class GetStrategusBattleMercenariesQueryTest : TestBase
    {
        [Test]
        public async Task ShouldReturnErrorIfBattleNotFound()
        {
            GetStrategusBattleMercenariesQuery.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassModel>());
            var res = await handler.Handle(new GetStrategusBattleMercenariesQuery
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
            StrategusBattle battle = new() { Phase = StrategusBattlePhase.Preparation };
            ArrangeDb.StrategusBattles.Add(battle);
            await ArrangeDb.SaveChangesAsync();

            GetStrategusBattleMercenariesQuery.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassModel>());
            var res = await handler.Handle(new GetStrategusBattleMercenariesQuery
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
            StrategusBattle battle = new() { Phase = StrategusBattlePhase.Hiring };
            ArrangeDb.StrategusBattles.Add(battle);
            await ArrangeDb.SaveChangesAsync();

            GetStrategusBattleMercenariesQuery.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassModel>());
            var res = await handler.Handle(new GetStrategusBattleMercenariesQuery
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
            StrategusBattle battle = new()
            {
                Phase = StrategusBattlePhase.Hiring,
                Fighters = { new StrategusBattleFighter { HeroId = 20, Side = StrategusBattleSide.Defender } },
                Mercenaries =
                {
                    new StrategusBattleMercenary
                    {
                        Character = new Character { User = new User() },
                        Side = StrategusBattleSide.Attacker,
                    },
                    new StrategusBattleMercenary
                    {
                        Character = new Character { User = new User() },
                        Side = StrategusBattleSide.Defender,
                    },
                },
            };
            ArrangeDb.StrategusBattles.Add(battle);
            await ArrangeDb.SaveChangesAsync();

            GetStrategusBattleMercenariesQuery.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassModel>());
            var res = await handler.Handle(new GetStrategusBattleMercenariesQuery
            {
                UserId = 20,
                BattleId = battle.Id,
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            var mercenaries = res.Data!;
            Assert.AreEqual(1, mercenaries.Count);
            Assert.AreEqual(StrategusBattleSide.Defender, mercenaries[0].Side);
        }

        [TestCase(StrategusBattlePhase.Battle)]
        [TestCase(StrategusBattlePhase.End)]
        public async Task ShouldOnlyReturnBothSidesDuringOtherPhases(StrategusBattlePhase battlePhase)
        {
            StrategusBattle battle = new()
            {
                Phase = battlePhase,
                Mercenaries =
                {
                    new StrategusBattleMercenary
                    {
                        Character = new Character { User = new User() },
                        Side = StrategusBattleSide.Attacker,
                    },
                    new StrategusBattleMercenary
                    {
                        Character = new Character { User = new User() },
                        Side = StrategusBattleSide.Defender,
                    },
                },
            };
            ArrangeDb.StrategusBattles.Add(battle);
            await ArrangeDb.SaveChangesAsync();

            GetStrategusBattleMercenariesQuery.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassModel>());
            var res = await handler.Handle(new GetStrategusBattleMercenariesQuery
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
