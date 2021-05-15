using System;
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
    public class GetBattleMercenaryApplicationsQueryTest : TestBase
    {
        [Test]
        public async Task ShouldReturnErrorIfBattleNotFound()
        {
            GetBattleMercenaryApplicationsQuery.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassModel>());
            var res = await handler.Handle(new GetBattleMercenaryApplicationsQuery
            {
                UserId = 99,
                BattleId = 99,
                Statuses = Array.Empty<BattleMercenaryApplicationStatus>(),
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

            GetBattleMercenaryApplicationsQuery.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassModel>());
            var res = await handler.Handle(new GetBattleMercenaryApplicationsQuery
            {
                UserId = 99,
                BattleId = battle.Id,
                Statuses = Array.Empty<BattleMercenaryApplicationStatus>(),
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.BattleInvalidPhase, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldOnlyReturnFighterSideIfUserIsAFighter()
        {
            Battle battle = new()
            {
                Phase = BattlePhase.Hiring,
                Fighters = { new BattleFighter { HeroId = 20, Side = BattleSide.Defender } },
                MercenaryApplications =
                {
                    new BattleMercenaryApplication
                    {
                        Character = new Character { User = new User() },
                        Side = BattleSide.Attacker,
                        Status = BattleMercenaryApplicationStatus.Pending,
                    },
                    new BattleMercenaryApplication
                    {
                        Character = new Character { User = new User() },
                        Side = BattleSide.Attacker,
                        Status = BattleMercenaryApplicationStatus.Declined,
                    },
                    new BattleMercenaryApplication
                    {
                        Character = new Character { User = new User() },
                        Side = BattleSide.Attacker,
                        Status = BattleMercenaryApplicationStatus.Accepted,
                    },
                    new BattleMercenaryApplication
                    {
                        Character = new Character { User = new User() },
                        Side = BattleSide.Defender,
                        Status = BattleMercenaryApplicationStatus.Pending,
                    },
                    new BattleMercenaryApplication
                    {
                        Character = new Character { User = new User() },
                        Side = BattleSide.Defender,
                        Status = BattleMercenaryApplicationStatus.Declined,
                    },
                    new BattleMercenaryApplication
                    {
                        Character = new Character { User = new User() },
                        Side = BattleSide.Defender,
                        Status = BattleMercenaryApplicationStatus.Accepted,
                    },
                },
            };
            ArrangeDb.Battles.Add(battle);
            await ArrangeDb.SaveChangesAsync();

            GetBattleMercenaryApplicationsQuery.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassModel>());
            var res = await handler.Handle(new GetBattleMercenaryApplicationsQuery
            {
                UserId = 20,
                BattleId = battle.Id,
                Statuses = new[]
                {
                    BattleMercenaryApplicationStatus.Pending,
                    BattleMercenaryApplicationStatus.Declined,
                    BattleMercenaryApplicationStatus.Accepted,
                },
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            var applications = res.Data!;
            Assert.AreEqual(3, applications.Count);
            Assert.AreEqual(BattleSide.Defender, applications[0].Side);
        }

        [Test]
        public async Task ShouldOnlyReturnUserApplicationsIfUserIsNotAFighter()
        {
            User user = new();
            ArrangeDb.Users.Add(user);

            Battle battle = new()
            {
                Phase = BattlePhase.Hiring,
                Fighters = { new BattleFighter { HeroId = 99, Side = BattleSide.Defender } },
                MercenaryApplications =
                {
                    new BattleMercenaryApplication
                    {
                        Character = new Character { User = new User() },
                        Side = BattleSide.Attacker,
                        Status = BattleMercenaryApplicationStatus.Pending,
                    },
                    new BattleMercenaryApplication
                    {
                        Character = new Character { User = user },
                        Side = BattleSide.Attacker,
                        Status = BattleMercenaryApplicationStatus.Pending,
                    },
                    new BattleMercenaryApplication
                    {
                        Character = new Character { User = user },
                        Side = BattleSide.Defender,
                        Status = BattleMercenaryApplicationStatus.Pending,
                    },
                    new BattleMercenaryApplication
                    {
                        Character = new Character { User = user },
                        Side = BattleSide.Defender,
                        Status = BattleMercenaryApplicationStatus.Declined,
                    },
                },
            };
            ArrangeDb.Battles.Add(battle);
            await ArrangeDb.SaveChangesAsync();

            GetBattleMercenaryApplicationsQuery.Handler handler = new(ActDb, Mapper, Mock.Of<ICharacterClassModel>());
            var res = await handler.Handle(new GetBattleMercenaryApplicationsQuery
            {
                UserId = user.Id,
                BattleId = battle.Id,
                Statuses = new[]
                {
                    BattleMercenaryApplicationStatus.Pending,
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
