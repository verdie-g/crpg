using System;
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
    public class GetStrategusBattleMercenaryApplicationsQueryTest : TestBase
    {
        [Test]
        public async Task ShouldReturnErrorIfBattleNotFound()
        {
            GetStrategusBattleMercenaryApplicationsQuery.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassModel>());
            var res = await handler.Handle(new GetStrategusBattleMercenaryApplicationsQuery
            {
                UserId = 99,
                BattleId = 99,
                Statuses = Array.Empty<StrategusBattleMercenaryApplicationStatus>(),
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

            GetStrategusBattleMercenaryApplicationsQuery.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassModel>());
            var res = await handler.Handle(new GetStrategusBattleMercenaryApplicationsQuery
            {
                UserId = 99,
                BattleId = battle.Id,
                Statuses = Array.Empty<StrategusBattleMercenaryApplicationStatus>(),
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.BattleInvalidPhase, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldOnlyReturnFighterSideIfUserIsAFighter()
        {
            StrategusBattle battle = new()
            {
                Phase = StrategusBattlePhase.Hiring,
                Fighters = { new StrategusBattleFighter { HeroId = 20, Side = StrategusBattleSide.Defender } },
                MercenaryApplications =
                {
                    new StrategusBattleMercenaryApplication
                    {
                        Character = new Character { User = new User() },
                        Side = StrategusBattleSide.Attacker,
                        Status = StrategusBattleMercenaryApplicationStatus.Pending,
                    },
                    new StrategusBattleMercenaryApplication
                    {
                        Character = new Character { User = new User() },
                        Side = StrategusBattleSide.Attacker,
                        Status = StrategusBattleMercenaryApplicationStatus.Declined,
                    },
                    new StrategusBattleMercenaryApplication
                    {
                        Character = new Character { User = new User() },
                        Side = StrategusBattleSide.Attacker,
                        Status = StrategusBattleMercenaryApplicationStatus.Accepted,
                    },
                    new StrategusBattleMercenaryApplication
                    {
                        Character = new Character { User = new User() },
                        Side = StrategusBattleSide.Defender,
                        Status = StrategusBattleMercenaryApplicationStatus.Pending,
                    },
                    new StrategusBattleMercenaryApplication
                    {
                        Character = new Character { User = new User() },
                        Side = StrategusBattleSide.Defender,
                        Status = StrategusBattleMercenaryApplicationStatus.Declined,
                    },
                    new StrategusBattleMercenaryApplication
                    {
                        Character = new Character { User = new User() },
                        Side = StrategusBattleSide.Defender,
                        Status = StrategusBattleMercenaryApplicationStatus.Accepted,
                    },
                },
            };
            ArrangeDb.StrategusBattles.Add(battle);
            await ArrangeDb.SaveChangesAsync();

            GetStrategusBattleMercenaryApplicationsQuery.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassModel>());
            var res = await handler.Handle(new GetStrategusBattleMercenaryApplicationsQuery
            {
                UserId = 20,
                BattleId = battle.Id,
                Statuses = new[]
                {
                    StrategusBattleMercenaryApplicationStatus.Pending,
                    StrategusBattleMercenaryApplicationStatus.Declined,
                    StrategusBattleMercenaryApplicationStatus.Accepted,
                },
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            var applications = res.Data!;
            Assert.AreEqual(3, applications.Count);
            Assert.AreEqual(StrategusBattleSide.Defender, applications[0].Side);
        }

        [Test]
        public async Task ShouldOnlyReturnUserApplicationsIfUserIsNotAFighter()
        {
            User user = new();
            ArrangeDb.Users.Add(user);

            StrategusBattle battle = new()
            {
                Phase = StrategusBattlePhase.Hiring,
                Fighters = { new StrategusBattleFighter { HeroId = 99, Side = StrategusBattleSide.Defender } },
                MercenaryApplications =
                {
                    new StrategusBattleMercenaryApplication
                    {
                        Character = new Character { User = new User() },
                        Side = StrategusBattleSide.Attacker,
                        Status = StrategusBattleMercenaryApplicationStatus.Pending,
                    },
                    new StrategusBattleMercenaryApplication
                    {
                        Character = new Character { User = user },
                        Side = StrategusBattleSide.Attacker,
                        Status = StrategusBattleMercenaryApplicationStatus.Pending,
                    },
                    new StrategusBattleMercenaryApplication
                    {
                        Character = new Character { User = user },
                        Side = StrategusBattleSide.Defender,
                        Status = StrategusBattleMercenaryApplicationStatus.Pending,
                    },
                    new StrategusBattleMercenaryApplication
                    {
                        Character = new Character { User = user },
                        Side = StrategusBattleSide.Defender,
                        Status = StrategusBattleMercenaryApplicationStatus.Declined,
                    },
                },
            };
            ArrangeDb.StrategusBattles.Add(battle);
            await ArrangeDb.SaveChangesAsync();

            GetStrategusBattleMercenaryApplicationsQuery.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassModel>());
            var res = await handler.Handle(new GetStrategusBattleMercenaryApplicationsQuery
            {
                UserId = user.Id,
                BattleId = battle.Id,
                Statuses = new[]
                {
                    StrategusBattleMercenaryApplicationStatus.Pending,
                },
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            var applications = res.Data!;
            Assert.AreEqual(2, applications.Count);
            Assert.AreEqual(StrategusBattleSide.Attacker, applications[0].Side);
            Assert.AreEqual(StrategusBattleSide.Defender, applications[1].Side);
        }
    }
}
