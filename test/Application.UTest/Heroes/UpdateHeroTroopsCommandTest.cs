using System;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common;
using Crpg.Application.Heroes.Commands;
using Crpg.Domain.Entities.Heroes;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.Heroes
{
    public class UpdateHeroTroopsCommandTest : TestBase
    {
        private static readonly Constants Constants = new()
        {
            StrategusTroopRecruitmentPerHour = 5,
            StrategusMaxHeroTroops = 10,
        };

        [Test]
        public async Task ShouldIncreaseTroopsOfHeroesRecruiting()
        {
            Hero hero1 = new() { Troops = 0, Status = HeroStatus.RecruitingInSettlement, User = new User() };
            Hero hero2 = new() { Troops = 5, Status = HeroStatus.RecruitingInSettlement, User = new User() };
            Hero hero3 = new() { Troops = 9, Status = HeroStatus.RecruitingInSettlement, User = new User() };
            Hero hero4 = new() { Troops = 2, Status = HeroStatus.IdleInSettlement, User = new User() };
            ArrangeDb.Heroes.AddRange(hero1, hero2, hero3, hero4);
            await ArrangeDb.SaveChangesAsync();

            UpdateHeroTroopsCommand.Handler handler = new(ActDb, Constants);
            await handler.Handle(new UpdateHeroTroopsCommand
            {
                DeltaTime = TimeSpan.FromHours(1),
            }, CancellationToken.None);

            hero1 = await AssertDb.Heroes.FirstAsync(h => h.Id == hero1.Id);
            Assert.AreEqual(5, hero1.Troops);
            hero2 = await AssertDb.Heroes.FirstAsync(h => h.Id == hero2.Id);
            Assert.AreEqual(10, hero2.Troops);
            hero3 = await AssertDb.Heroes.FirstAsync(h => h.Id == hero3.Id);
            Assert.AreEqual(10, hero3.Troops);
            hero4 = await AssertDb.Heroes.FirstAsync(h => h.Id == hero4.Id);
            Assert.AreEqual(2, hero4.Troops);
        }
    }
}
