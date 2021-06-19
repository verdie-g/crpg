using System;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Battles.Queries;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Heroes;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Battles
{
    public class GetBattleFighterApplicationsQueryTest : TestBase
    {
        [Test]
        public async Task ShouldReturnErrorIfBattleNotFound()
        {
            GetBattleFighterApplicationsQuery.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new GetBattleFighterApplicationsQuery
            {
                HeroId = 99,
                BattleId = 99,
                Statuses = Array.Empty<BattleFighterApplicationStatus>(),
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.BattleNotFound, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldOnlyReturnFighterSideIfUserIsACommander()
        {
            Battle battle = new()
            {
                Phase = BattlePhase.Hiring,
                Fighters = { new BattleFighter { HeroId = 20, Side = BattleSide.Defender, Commander = true } },
                FighterApplications =
                {
                    new BattleFighterApplication
                    {
                        Hero = new Hero { User = new User() },
                        Side = BattleSide.Attacker,
                        Status = BattleFighterApplicationStatus.Pending,
                    },
                    new BattleFighterApplication
                    {
                        Hero = new Hero { User = new User() },
                        Side = BattleSide.Attacker,
                        Status = BattleFighterApplicationStatus.Declined,
                    },
                    new BattleFighterApplication
                    {
                        Hero = new Hero { User = new User() },
                        Side = BattleSide.Attacker,
                        Status = BattleFighterApplicationStatus.Accepted,
                    },
                    new BattleFighterApplication
                    {
                        Hero = new Hero { User = new User() },
                        Side = BattleSide.Defender,
                        Status = BattleFighterApplicationStatus.Pending,
                    },
                    new BattleFighterApplication
                    {
                        Hero = new Hero { User = new User() },
                        Side = BattleSide.Defender,
                        Status = BattleFighterApplicationStatus.Declined,
                    },
                    new BattleFighterApplication
                    {
                        Hero = new Hero { User = new User() },
                        Side = BattleSide.Defender,
                        Status = BattleFighterApplicationStatus.Accepted,
                    },
                },
            };
            ArrangeDb.Battles.Add(battle);
            await ArrangeDb.SaveChangesAsync();

            GetBattleFighterApplicationsQuery.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new GetBattleFighterApplicationsQuery
            {
                HeroId = 20,
                BattleId = battle.Id,
                Statuses = new[]
                {
                    BattleFighterApplicationStatus.Pending,
                    BattleFighterApplicationStatus.Declined,
                    BattleFighterApplicationStatus.Accepted,
                },
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            var applications = res.Data!;
            Assert.AreEqual(3, applications.Count);
            Assert.AreEqual(BattleSide.Defender, applications[0].Side);
        }

        [Theory]
        public async Task ShouldOnlyReturnHeroApplicationsIfHeroIsNotACommanderOrFighter(bool isFighter)
        {
            Hero hero = new() { User = new User() };
            ArrangeDb.Heroes.Add(hero);

            Battle battle = new()
            {
                Phase = BattlePhase.Hiring,
                Fighters = { new BattleFighter { HeroId = 99, Side = BattleSide.Defender } },
                FighterApplications =
                {
                    new BattleFighterApplication
                    {
                        Hero = new Hero { User = new User() },
                        Side = BattleSide.Attacker,
                        Status = BattleFighterApplicationStatus.Pending,
                    },
                    new BattleFighterApplication
                    {
                        Hero = hero,
                        Side = BattleSide.Attacker,
                        Status = BattleFighterApplicationStatus.Pending,
                    },
                    new BattleFighterApplication
                    {
                        Hero = hero,
                        Side = BattleSide.Defender,
                        Status = BattleFighterApplicationStatus.Pending,
                    },
                    new BattleFighterApplication
                    {
                        Hero = hero,
                        Side = BattleSide.Defender,
                        Status = BattleFighterApplicationStatus.Declined,
                    },
                },
            };
            if (isFighter)
            {
                battle.Fighters.Add(new BattleFighter
                {
                    Hero = hero,
                    Side = BattleSide.Defender,
                    Commander = false,
                });
            }

            ArrangeDb.Battles.Add(battle);
            await ArrangeDb.SaveChangesAsync();

            GetBattleFighterApplicationsQuery.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new GetBattleFighterApplicationsQuery
            {
                HeroId = hero.Id,
                BattleId = battle.Id,
                Statuses = new[]
                {
                    BattleFighterApplicationStatus.Pending,
                },
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            var applications = res.Data!;
            Assert.AreEqual(2, applications.Count);
            Assert.AreEqual(BattleSide.Attacker, applications[0].Side);
            Assert.AreEqual(BattleSide.Defender, applications[1].Side);
        }
    }
}
