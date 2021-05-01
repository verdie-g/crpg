using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Results;
using Crpg.Application.Strategus.Commands;
using Crpg.Domain.Entities.Strategus;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Strategus
{
    public class UpdateStrategusSettlementCommandTest : TestBase
    {
        [Test]
        public async Task ShouldReturnErrorIfHeroIsNotFound()
        {
            UpdateStrategusSettlementCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new UpdateStrategusSettlementCommand
            {
                HeroId = 1,
                SettlementId = 2,
                Troops = 0,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.HeroNotFound, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfHeroNotInASettlement()
        {
            StrategusHero hero = new() { Status = StrategusHeroStatus.Idle, User = new User() };
            ArrangeDb.StrategusHeroes.Add(hero);
            await ArrangeDb.SaveChangesAsync();

            UpdateStrategusSettlementCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new UpdateStrategusSettlementCommand
            {
                HeroId = hero.Id,
                SettlementId = 1,
                Troops = 0,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.HeroNotInASettlement, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfHeroNotInTheSpecifiedSettlement()
        {
            StrategusSettlement settlement = new();
            ArrangeDb.StrategusSettlements.Add(settlement);

            StrategusHero hero = new()
            {
                Status = StrategusHeroStatus.IdleInSettlement,
                TargetedSettlement = settlement,
                User = new User(),
            };
            ArrangeDb.StrategusHeroes.Add(hero);

            await ArrangeDb.SaveChangesAsync();

            UpdateStrategusSettlementCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new UpdateStrategusSettlementCommand
            {
                HeroId = hero.Id,
                SettlementId = 99,
                Troops = 0,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.HeroNotInASettlement, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfHeroIsGivingTroopTheyDontHave()
        {
            StrategusSettlement settlement = new();
            ArrangeDb.StrategusSettlements.Add(settlement);

            StrategusHero hero = new()
            {
                Status = StrategusHeroStatus.IdleInSettlement,
                Troops = 5,
                TargetedSettlement = settlement,
                User = new User(),
            };
            ArrangeDb.StrategusHeroes.Add(hero);

            await ArrangeDb.SaveChangesAsync();

            UpdateStrategusSettlementCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new UpdateStrategusSettlementCommand
            {
                HeroId = hero.Id,
                SettlementId = settlement.Id,
                Troops = 6,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.HeroNotEnoughTroops, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfHeroIsTakingTroopsFromANotOwnedSettlement()
        {
            StrategusSettlement settlement = new() { Troops = 10 };
            ArrangeDb.StrategusSettlements.Add(settlement);

            StrategusHero hero = new()
            {
                Status = StrategusHeroStatus.IdleInSettlement,
                Troops = 5,
                TargetedSettlement = settlement,
                User = new User(),
            };
            ArrangeDb.StrategusHeroes.Add(hero);

            await ArrangeDb.SaveChangesAsync();

            UpdateStrategusSettlementCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new UpdateStrategusSettlementCommand
            {
                HeroId = hero.Id,
                SettlementId = settlement.Id,
                Troops = 5,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.HeroNotSettlementOwner, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldGiveTroopsToSettlement()
        {
            StrategusSettlement settlement = new()
            {
                Troops = 20,
            };
            ArrangeDb.StrategusSettlements.Add(settlement);

            StrategusHero hero = new()
            {
                Status = StrategusHeroStatus.RecruitingInSettlement,
                Troops = 10,
                TargetedSettlement = settlement,
                User = new User(),
            };
            ArrangeDb.StrategusHeroes.Add(hero);

            await ArrangeDb.SaveChangesAsync();

            UpdateStrategusSettlementCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new UpdateStrategusSettlementCommand
            {
                HeroId = hero.Id,
                SettlementId = settlement.Id,
                Troops = 30,
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            var settlementVm = res.Data!;
            Assert.AreEqual(settlement.Id, settlementVm.Id);
            Assert.AreEqual(30, AssertDb.StrategusSettlements.Find(settlement.Id).Troops);
        }

        [Test]
        public async Task ShouldTakeTroopsFromSettlement()
        {
            StrategusSettlement settlement = new()
            {
                Troops = 20,
            };
            ArrangeDb.StrategusSettlements.Add(settlement);

            StrategusHero hero = new()
            {
                Status = StrategusHeroStatus.IdleInSettlement,
                Troops = 10,
                TargetedSettlement = settlement,
                User = new User(),
            };
            ArrangeDb.StrategusHeroes.Add(hero);

            settlement.Owner = hero;
            await ArrangeDb.SaveChangesAsync();

            UpdateStrategusSettlementCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new UpdateStrategusSettlementCommand
            {
                HeroId = hero.Id,
                SettlementId = settlement.Id,
                Troops = 10,
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            var settlementVm = res.Data!;
            Assert.AreEqual(settlement.Id, settlementVm.Id);
            Assert.AreEqual(20, AssertDb.StrategusHeroes.Find(hero.Id).Troops);
            Assert.AreEqual(10, AssertDb.StrategusSettlements.Find(settlement.Id).Troops);
        }
    }
}
