using System;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common;
using Crpg.Application.Strategus.Commands;
using Crpg.Domain.Entities.Strategus;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.Strategus
{
    public class UpdateStrategusHeroTroopsCommandTest : TestBase
    {
        private static readonly Constants Constants = new()
        {
            StrategusTroopRecruitmentPerHour = 5,
            StrategusMaxHeroTroops = 10,
        };

        [Test]
        public async Task ShouldIncreaseTroopsOfHeroesRecruiting()
        {
            var hero1 = new StrategusHero { Troops = 0, Status = StrategusHeroStatus.RecruitingInSettlement, User = new User() };
            var hero2 = new StrategusHero { Troops = 5, Status = StrategusHeroStatus.RecruitingInSettlement, User = new User() };
            var hero3 = new StrategusHero { Troops = 9, Status = StrategusHeroStatus.RecruitingInSettlement, User = new User() };
            var hero4 = new StrategusHero { Troops = 2, Status = StrategusHeroStatus.IdleInSettlement, User = new User() };
            ArrangeDb.StrategusHeroes.AddRange(hero1, hero2, hero3, hero4);
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateStrategusHeroTroopsCommand.Handler(ActDb, Constants);
            await handler.Handle(new UpdateStrategusHeroTroopsCommand
            {
                DeltaTime = TimeSpan.FromHours(1),
            }, CancellationToken.None);

            hero1 = await AssertDb.StrategusHeroes.FirstAsync(h => h.Id == hero1.Id);
            Assert.AreEqual(5, hero1.Troops);
            hero2 = await AssertDb.StrategusHeroes.FirstAsync(h => h.Id == hero2.Id);
            Assert.AreEqual(10, hero2.Troops);
            hero3 = await AssertDb.StrategusHeroes.FirstAsync(h => h.Id == hero3.Id);
            Assert.AreEqual(10, hero3.Troops);
            hero4 = await AssertDb.StrategusHeroes.FirstAsync(h => h.Id == hero4.Id);
            Assert.AreEqual(2, hero4.Troops);
        }
    }
}
