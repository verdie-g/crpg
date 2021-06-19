using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Battles.Commands;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Heroes;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.Battles
{
    public class RespondToBattleFighterApplicationCommandTest : TestBase
    {
        [Test]
        public async Task ShouldReturnErrorIfHeroIsNotFound()
        {
            RespondToBattleFighterApplicationCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new RespondToBattleFighterApplicationCommand
            {
                HeroId = 99,
                FighterApplicationId = 99,
                Accept = true,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.HeroNotFound, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfApplicationIsNotFound()
        {
            Hero hero = new() { User = new User() };
            ArrangeDb.Heroes.Add(hero);
            await ArrangeDb.SaveChangesAsync();

            RespondToBattleFighterApplicationCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new RespondToBattleFighterApplicationCommand
            {
                HeroId = hero.Id,
                FighterApplicationId = 99,
                Accept = true,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.ApplicationNotFound, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfHeroIsNotAFighter()
        {
            Hero hero = new() { User = new User() };
            ArrangeDb.Heroes.Add(hero);

            Battle battle = new()
            {
                Phase = BattlePhase.Preparation,
            };
            ArrangeDb.Battles.Add(battle);

            BattleFighterApplication application = new()
            {
                Side = BattleSide.Attacker,
                Status = BattleFighterApplicationStatus.Pending,
                Battle = battle,
                Hero = new Hero { User = new User() },
            };
            ArrangeDb.BattleFighterApplications.Add(application);
            await ArrangeDb.SaveChangesAsync();

            RespondToBattleFighterApplicationCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new RespondToBattleFighterApplicationCommand
            {
                HeroId = hero.Id,
                FighterApplicationId = application.Id,
                Accept = true,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.HeroNotAFighter, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfHeroIsNotACommander()
        {
            Hero hero = new() { User = new User() };
            ArrangeDb.Heroes.Add(hero);

            Battle battle = new()
            {
                Phase = BattlePhase.Preparation,
                Fighters =
                {
                    new BattleFighter
                    {
                        Side = BattleSide.Attacker,
                        Commander = false,
                        Hero = hero,
                    },
                },
            };
            ArrangeDb.Battles.Add(battle);

            BattleFighterApplication application = new()
            {
                Side = BattleSide.Attacker,
                Status = BattleFighterApplicationStatus.Pending,
                Battle = battle,
                Hero = new Hero { User = new User() },
            };
            ArrangeDb.BattleFighterApplications.Add(application);
            await ArrangeDb.SaveChangesAsync();

            RespondToBattleFighterApplicationCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new RespondToBattleFighterApplicationCommand
            {
                HeroId = hero.Id,
                FighterApplicationId = application.Id,
                Accept = true,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.FighterNotACommander, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfFightersNotOnTheSameSide()
        {
            Hero hero = new() { User = new User() };
            ArrangeDb.Heroes.Add(hero);

            Battle battle = new()
            {
                Phase = BattlePhase.Preparation,
                Fighters =
                {
                    new BattleFighter
                    {
                        Side = BattleSide.Defender,
                        Commander = true,
                        Hero = hero,
                    },
                },
            };
            ArrangeDb.Battles.Add(battle);

            BattleFighterApplication application = new()
            {
                Side = BattleSide.Attacker,
                Status = BattleFighterApplicationStatus.Pending,
                Battle = battle,
                Hero = new Hero { User = new User() },
            };
            ArrangeDb.BattleFighterApplications.Add(application);
            await ArrangeDb.SaveChangesAsync();

            RespondToBattleFighterApplicationCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new RespondToBattleFighterApplicationCommand
            {
                HeroId = hero.Id,
                FighterApplicationId = application.Id,
                Accept = true,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.FightersNotOnTheSameSide, res.Errors![0].Code);
        }

        [TestCase(BattlePhase.Hiring)]
        [TestCase(BattlePhase.Scheduled)]
        [TestCase(BattlePhase.Live)]
        [TestCase(BattlePhase.End)]
        public async Task ShouldReturnErrorIfBattleIsNotInPreparation(BattlePhase battlePhase)
        {
            Hero hero = new() { User = new User() };
            ArrangeDb.Heroes.Add(hero);

            Battle battle = new()
            {
                Phase = battlePhase,
                Fighters =
                {
                    new BattleFighter
                    {
                        Side = BattleSide.Attacker,
                        Commander = true,
                        Hero = hero,
                    },
                },
            };
            ArrangeDb.Battles.Add(battle);

            BattleFighterApplication application = new()
            {
                Side = BattleSide.Attacker,
                Status = BattleFighterApplicationStatus.Pending,
                Battle = battle,
                Hero = new Hero { User = new User() },
            };
            ArrangeDb.BattleFighterApplications.Add(application);
            await ArrangeDb.SaveChangesAsync();

            RespondToBattleFighterApplicationCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new RespondToBattleFighterApplicationCommand
            {
                HeroId = hero.Id,
                FighterApplicationId = application.Id,
                Accept = true,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.BattleInvalidPhase, res.Errors![0].Code);
        }

        [TestCase(BattleFighterApplicationStatus.Declined)]
        [TestCase(BattleFighterApplicationStatus.Accepted)]
        public async Task ShouldReturnErrorIfApplicationIsClosed(BattleFighterApplicationStatus applicationStatus)
        {
            Hero hero = new() { User = new User() };
            ArrangeDb.Heroes.Add(hero);

            Battle battle = new()
            {
                Phase = BattlePhase.Preparation,
                Fighters =
                {
                    new BattleFighter
                    {
                        Side = BattleSide.Attacker,
                        Commander = true,
                        Hero = hero,
                    },
                },
            };
            ArrangeDb.Battles.Add(battle);

            BattleFighterApplication application = new()
            {
                Side = BattleSide.Attacker,
                Status = applicationStatus,
                Battle = battle,
                Hero = new Hero { User = new User() },
            };
            ArrangeDb.BattleFighterApplications.Add(application);
            await ArrangeDb.SaveChangesAsync();

            RespondToBattleFighterApplicationCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new RespondToBattleFighterApplicationCommand
            {
                HeroId = hero.Id,
                FighterApplicationId = application.Id,
                Accept = true,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.ApplicationClosed, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldDeclineApplication()
        {
            Hero hero = new() { User = new User() };
            ArrangeDb.Heroes.Add(hero);

            Battle battle = new()
            {
                Phase = BattlePhase.Preparation,
                Fighters =
                {
                    new BattleFighter
                    {
                        Side = BattleSide.Attacker,
                        Commander = true,
                        Hero = hero,
                    },
                },
            };
            ArrangeDb.Battles.Add(battle);

            BattleFighterApplication application = new()
            {
                Side = BattleSide.Attacker,
                Status = BattleFighterApplicationStatus.Pending,
                Battle = battle,
                Hero = new Hero { User = new User() },
            };
            ArrangeDb.BattleFighterApplications.Add(application);
            await ArrangeDb.SaveChangesAsync();

            RespondToBattleFighterApplicationCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new RespondToBattleFighterApplicationCommand
            {
                HeroId = hero.Id,
                FighterApplicationId = application.Id,
                Accept = false,
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            var applicationVm = res.Data!;
            Assert.AreEqual(application.Id, applicationVm.Id);
            Assert.AreEqual(BattleFighterApplicationStatus.Declined, applicationVm.Status);

            Assert.AreEqual(1, await AssertDb.BattleFighters.CountAsync());
        }

        [Test]
        public async Task ShouldAcceptApplication()
        {
            Hero hero = new() { User = new User() };
            ArrangeDb.Heroes.Add(hero);

            Battle battle = new()
            {
                Phase = BattlePhase.Preparation,
                Fighters =
                {
                    new BattleFighter
                    {
                        Side = BattleSide.Attacker,
                        Commander = true,
                        Hero = hero,
                    },
                },
            };
            ArrangeDb.Battles.Add(battle);

            BattleFighterApplication application = new()
            {
                Side = BattleSide.Attacker,
                Status = BattleFighterApplicationStatus.Pending,
                Battle = battle,
                Hero = new Hero { User = new User() },
            };
            ArrangeDb.BattleFighterApplications.Add(application);
            await ArrangeDb.SaveChangesAsync();

            RespondToBattleFighterApplicationCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new RespondToBattleFighterApplicationCommand
            {
                HeroId = hero.Id,
                FighterApplicationId = application.Id,
                Accept = true,
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            var applicationVm = res.Data!;
            Assert.AreEqual(application.Id, applicationVm.Id);
            Assert.AreEqual(BattleFighterApplicationStatus.Accepted, applicationVm.Status);

            Assert.AreEqual(2, await AssertDb.BattleFighters.CountAsync());
        }
    }
}
