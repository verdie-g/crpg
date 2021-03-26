using System;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common;
using Crpg.Application.Common.Services;
using Crpg.Application.Strategus.Commands;
using Crpg.Domain.Entities.Strategus;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Strategus
{
    public class UpdateStrategusBattleStatusesCommandTest : TestBase
    {
        private static readonly Constants Constants = new Constants
        {
            StrategusBattleInitiationDurationHours = 24,
            StrategusBattleHiringDurationHours = 12,
        };

        [Test]
        public async Task ShouldSwitchedInitiatedBattlesToHiringAfterSomeTime()
        {
            var battles = new[]
            {
                new StrategusBattle
                {
                    Status = StrategusBattleStatus.Initiated,
                    CreatedAt = new DateTimeOffset(new DateTime(2010, 12, 3)),
                },
                new StrategusBattle
                {
                    Status = StrategusBattleStatus.Initiated,
                    CreatedAt = new DateTimeOffset(new DateTime(2010, 12, 4, 12, 0, 0)),
                },
            };
            ArrangeDb.StrategusBattles.AddRange(battles);
            await ArrangeDb.SaveChangesAsync();

            var dateTimeOffsetMock = new Mock<IDateTimeOffset>();
            dateTimeOffsetMock.Setup(dt => dt.Now).Returns(new DateTimeOffset(new DateTime(2010, 12, 5)));
            var handler = new UpdateStrategusBattleStatusesCommand.Handler(ActDb, Mock.Of<IStrategusBattleScheduler>(),
                dateTimeOffsetMock.Object, Constants);
            await handler.Handle(new UpdateStrategusBattleStatusesCommand(), CancellationToken.None);

            battles = await AssertDb.StrategusBattles.ToArrayAsync();
            Assert.AreEqual(StrategusBattleStatus.Hiring, battles[0].Status);
            Assert.AreEqual(StrategusBattleStatus.Initiated, battles[1].Status);
        }

        [Test]
        public async Task ShouldScheduleHiringBattlesAfterSomeTime()
        {
            var battles = new[]
            {
                new StrategusBattle
                {
                    Status = StrategusBattleStatus.Hiring,
                    CreatedAt = new DateTimeOffset(new DateTime(2010, 12, 3, 10, 0, 0)),
                },
                new StrategusBattle
                {
                    Status = StrategusBattleStatus.Hiring,
                    CreatedAt = new DateTimeOffset(new DateTime(2010, 12, 4, 20, 0, 0)),
                },
            };
            ArrangeDb.StrategusBattles.AddRange(battles);
            await ArrangeDb.SaveChangesAsync();

            var dateTimeOffsetMock = new Mock<IDateTimeOffset>();
            dateTimeOffsetMock.Setup(dt => dt.Now).Returns(new DateTimeOffset(new DateTime(2010, 12, 5)));
            var battleSchedulerMock = new Mock<IStrategusBattleScheduler>();
            var handler = new UpdateStrategusBattleStatusesCommand.Handler(ActDb, battleSchedulerMock.Object,
                dateTimeOffsetMock.Object, Constants);
            await handler.Handle(new UpdateStrategusBattleStatusesCommand(), CancellationToken.None);

            battles = await AssertDb.StrategusBattles.ToArrayAsync();
            Assert.AreEqual(StrategusBattleStatus.Live, battles[0].Status);
            battleSchedulerMock.Verify(s => s.ScheduleBattle(It.IsAny<StrategusBattle>()), Times.Once);
            Assert.AreEqual(StrategusBattleStatus.Hiring, battles[1].Status);
        }
    }
}
