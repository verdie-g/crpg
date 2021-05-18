using System;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Battles.Commands;
using Crpg.Application.Common;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Battles;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Battles
{
    public class UpdateBattlePhasesCommandTest : TestBase
    {
        private static readonly Constants Constants = new()
        {
            StrategusBattleInitiationDurationHours = 24,
            StrategusBattleHiringDurationHours = 12,
        };

        [Test]
        public async Task ShouldSwitchPreparationBattlesToHiringAfterSomeTime()
        {
            var battles = new[]
            {
                new Battle
                {
                    Phase = BattlePhase.Preparation,
                    CreatedAt = new DateTimeOffset(new DateTime(2010, 12, 3)),
                },
                new Battle
                {
                    Phase = BattlePhase.Preparation,
                    CreatedAt = new DateTimeOffset(new DateTime(2010, 12, 4, 12, 0, 0)),
                },
            };
            ArrangeDb.Battles.AddRange(battles);
            await ArrangeDb.SaveChangesAsync();

            var dateTimeOffsetMock = new Mock<IDateTimeOffset>();
            dateTimeOffsetMock.Setup(dt => dt.Now).Returns(new DateTimeOffset(new DateTime(2010, 12, 5)));
            var handler = new UpdateBattlePhasesCommand.Handler(ActDb, Mock.Of<IBattleScheduler>(),
                dateTimeOffsetMock.Object, Constants);
            await handler.Handle(new UpdateBattlePhasesCommand(), CancellationToken.None);

            battles = await AssertDb.Battles.ToArrayAsync();
            Assert.AreEqual(BattlePhase.Hiring, battles[0].Phase);
            Assert.AreEqual(BattlePhase.Preparation, battles[1].Phase);
        }

        [Test]
        public async Task ShouldScheduleHiringBattlesAfterSomeTime()
        {
            var battles = new[]
            {
                new Battle
                {
                    Phase = BattlePhase.Hiring,
                    CreatedAt = new DateTimeOffset(new DateTime(2010, 12, 3, 10, 0, 0)),
                },
                new Battle
                {
                    Phase = BattlePhase.Hiring,
                    CreatedAt = new DateTimeOffset(new DateTime(2010, 12, 4, 20, 0, 0)),
                },
            };
            ArrangeDb.Battles.AddRange(battles);
            await ArrangeDb.SaveChangesAsync();

            var dateTimeOffsetMock = new Mock<IDateTimeOffset>();
            dateTimeOffsetMock.Setup(dt => dt.Now).Returns(new DateTimeOffset(new DateTime(2010, 12, 5)));
            var battleSchedulerMock = new Mock<IBattleScheduler>();
            var handler = new UpdateBattlePhasesCommand.Handler(ActDb, battleSchedulerMock.Object,
                dateTimeOffsetMock.Object, Constants);
            await handler.Handle(new UpdateBattlePhasesCommand(), CancellationToken.None);

            battles = await AssertDb.Battles.ToArrayAsync();
            Assert.AreEqual(BattlePhase.Scheduled, battles[0].Phase);
            battleSchedulerMock.Verify(s => s.ScheduleBattle(It.IsAny<Battle>()), Times.Once);
            Assert.AreEqual(BattlePhase.Hiring, battles[1].Phase);
        }

        [Test]
        public async Task ShouldSwitchScheduledBattlesToLiveAfterScheduledDate()
        {
            var battles = new[]
            {
                new Battle
                {
                    Phase = BattlePhase.Scheduled,
                    ScheduledFor = new DateTimeOffset(new DateTime(2010, 12, 4)),
                },
                new Battle
                {
                    Phase = BattlePhase.Scheduled,
                    ScheduledFor = new DateTimeOffset(new DateTime(2010, 12, 6)),
                },
            };
            ArrangeDb.Battles.AddRange(battles);
            await ArrangeDb.SaveChangesAsync();

            var dateTimeOffsetMock = new Mock<IDateTimeOffset>();
            dateTimeOffsetMock.Setup(dt => dt.Now).Returns(new DateTimeOffset(new DateTime(2010, 12, 5)));

            var handler = new UpdateBattlePhasesCommand.Handler(ActDb, Mock.Of<IBattleScheduler>(),
                dateTimeOffsetMock.Object, Constants);
            await handler.Handle(new UpdateBattlePhasesCommand(), CancellationToken.None);

            battles = await AssertDb.Battles.ToArrayAsync();
            Assert.AreEqual(BattlePhase.Live, battles[0].Phase);
            Assert.AreEqual(BattlePhase.Scheduled, battles[1].Phase);
        }
    }
}
