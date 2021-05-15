using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Results;
using Crpg.Application.Settlements.Commands;
using Crpg.Domain.Entities.Heroes;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Settlements
{
    public class UpdateSettlementCommandTest : TestBase
    {
        [Test]
        public async Task ShouldReturnErrorIfHeroIsNotFound()
        {
            UpdateSettlementCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new UpdateSettlementCommand
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
            Hero hero = new() { Status = HeroStatus.Idle, User = new User() };
            ArrangeDb.Heroes.Add(hero);
            await ArrangeDb.SaveChangesAsync();

            UpdateSettlementCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new UpdateSettlementCommand
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
            Settlement settlement = new();
            ArrangeDb.Settlements.Add(settlement);

            Hero hero = new()
            {
                Status = HeroStatus.IdleInSettlement,
                TargetedSettlement = settlement,
                User = new User(),
            };
            ArrangeDb.Heroes.Add(hero);

            await ArrangeDb.SaveChangesAsync();

            UpdateSettlementCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new UpdateSettlementCommand
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
            Settlement settlement = new();
            ArrangeDb.Settlements.Add(settlement);

            Hero hero = new()
            {
                Status = HeroStatus.IdleInSettlement,
                Troops = 5,
                TargetedSettlement = settlement,
                User = new User(),
            };
            ArrangeDb.Heroes.Add(hero);

            await ArrangeDb.SaveChangesAsync();

            UpdateSettlementCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new UpdateSettlementCommand
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
            Settlement settlement = new() { Troops = 10 };
            ArrangeDb.Settlements.Add(settlement);

            Hero hero = new()
            {
                Status = HeroStatus.IdleInSettlement,
                Troops = 5,
                TargetedSettlement = settlement,
                User = new User(),
            };
            ArrangeDb.Heroes.Add(hero);

            await ArrangeDb.SaveChangesAsync();

            UpdateSettlementCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new UpdateSettlementCommand
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
            Settlement settlement = new()
            {
                Troops = 20,
            };
            ArrangeDb.Settlements.Add(settlement);

            Hero hero = new()
            {
                Status = HeroStatus.RecruitingInSettlement,
                Troops = 10,
                TargetedSettlement = settlement,
                User = new User(),
            };
            ArrangeDb.Heroes.Add(hero);

            await ArrangeDb.SaveChangesAsync();

            UpdateSettlementCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new UpdateSettlementCommand
            {
                HeroId = hero.Id,
                SettlementId = settlement.Id,
                Troops = 30,
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            var settlementVm = res.Data!;
            Assert.AreEqual(settlement.Id, settlementVm.Id);
            Assert.AreEqual(30, AssertDb.Settlements.Find(settlement.Id).Troops);
        }

        [Test]
        public async Task ShouldTakeTroopsFromSettlement()
        {
            Settlement settlement = new()
            {
                Troops = 20,
            };
            ArrangeDb.Settlements.Add(settlement);

            Hero hero = new()
            {
                Status = HeroStatus.IdleInSettlement,
                Troops = 10,
                TargetedSettlement = settlement,
                User = new User(),
            };
            ArrangeDb.Heroes.Add(hero);

            settlement.Owner = hero;
            await ArrangeDb.SaveChangesAsync();

            UpdateSettlementCommand.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new UpdateSettlementCommand
            {
                HeroId = hero.Id,
                SettlementId = settlement.Id,
                Troops = 10,
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            var settlementVm = res.Data!;
            Assert.AreEqual(settlement.Id, settlementVm.Id);
            Assert.AreEqual(20, AssertDb.Heroes.Find(hero.Id).Troops);
            Assert.AreEqual(10, AssertDb.Settlements.Find(settlement.Id).Troops);
        }
    }
}
