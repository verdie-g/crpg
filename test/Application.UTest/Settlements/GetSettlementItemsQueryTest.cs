using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Results;
using Crpg.Application.Settlements.Queries;
using Crpg.Domain.Entities.Heroes;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Settlements
{
    public class GetSettlementItemsQueryTest : TestBase
    {
        [Test]
        public async Task ShouldReturnErrorIfHeroNotFound()
        {
            GetSettlementItemsQuery.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new GetSettlementItemsQuery
            {
                HeroId = 99,
                SettlementId = 99,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.HeroNotFound, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfHeroNotInASettlement()
        {
            Settlement settlement = new();
            ArrangeDb.Settlements.Add(settlement);
            Hero hero = new()
            {
                Status = HeroStatus.Idle,
                User = new User(),
            };
            ArrangeDb.Heroes.Add(hero);
            await ArrangeDb.SaveChangesAsync();

            GetSettlementItemsQuery.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new GetSettlementItemsQuery
            {
                HeroId = hero.Id,
                SettlementId = settlement.Id,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.HeroNotInASettlement, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfHeroNotInTheSettlement()
        {
            Settlement settlement = new();
            ArrangeDb.Settlements.Add(settlement);
            Hero hero = new()
            {
                Status = HeroStatus.IdleInSettlement,
                TargetedSettlement = new Settlement(),
                User = new User(),
            };
            ArrangeDb.Heroes.Add(hero);
            await ArrangeDb.SaveChangesAsync();

            GetSettlementItemsQuery.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new GetSettlementItemsQuery
            {
                HeroId = hero.Id,
                SettlementId = settlement.Id,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.HeroNotInASettlement, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnErrorIfHeroIsNotSettlementOwner()
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

            GetSettlementItemsQuery.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new GetSettlementItemsQuery
            {
                HeroId = hero.Id,
                SettlementId = settlement.Id,
            }, CancellationToken.None);

            Assert.IsNotNull(res.Errors);
            Assert.AreEqual(ErrorCode.HeroNotSettlementOwner, res.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReturnSettlementItems()
        {
            Settlement settlement = new()
            {
                Items =
                {
                    new SettlementItem { Item = new Item() },
                    new SettlementItem { Item = new Item() },
                },
            };
            ArrangeDb.Settlements.Add(settlement);

            Hero hero = new()
            {
                Status = HeroStatus.IdleInSettlement,
                TargetedSettlement = settlement,
                User = new User(),
                OwnedSettlements = { settlement },
            };
            ArrangeDb.Heroes.Add(hero);
            await ArrangeDb.SaveChangesAsync();

            GetSettlementItemsQuery.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new GetSettlementItemsQuery
            {
                HeroId = hero.Id,
                SettlementId = settlement.Id,
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);
            Assert.AreEqual(2, res.Data!.Count);
        }
    }
}
