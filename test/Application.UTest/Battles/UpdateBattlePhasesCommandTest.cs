using Crpg.Application.Battles.Commands;
using Crpg.Application.Common;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Battles;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Battles;

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
        Battle[] battles =
        {
            new()
            {
                Phase = BattlePhase.Preparation,
                CreatedAt = new DateTime(2010, 12, 3),
            },
            new()
            {
                Phase = BattlePhase.Preparation,
                CreatedAt = new DateTime(2010, 12, 4, 12, 0, 0),
            },
        };
        ArrangeDb.Battles.AddRange(battles);
        await ArrangeDb.SaveChangesAsync();

        Mock<IBattleMercenaryDistributionModel> battleMercenaryDistributionModelMock = new();

        Mock<IDateTime> dateTimeMock = new();
        dateTimeMock.Setup(dt => dt.UtcNow).Returns(new DateTime(2010, 12, 5));

        UpdateBattlePhasesCommand.Handler handler = new(ActDb, battleMercenaryDistributionModelMock.Object,
            Mock.Of<IBattleScheduler>(), dateTimeMock.Object, Constants);
        await handler.Handle(new UpdateBattlePhasesCommand(), CancellationToken.None);

        battles = await AssertDb.Battles.ToArrayAsync();
        Assert.AreEqual(BattlePhase.Hiring, battles[0].Phase);
        Assert.AreEqual(BattlePhase.Preparation, battles[1].Phase);

        battleMercenaryDistributionModelMock.Verify(m =>
            m.DistributeMercenaries(It.IsAny<IList<BattleFighter>>(), It.IsAny<int>()), Times.Once);
    }

    [Test]
    public async Task ShouldScheduleHiringBattlesAfterSomeTime()
    {
        Battle[] battles =
        {
            new()
            {
                Phase = BattlePhase.Hiring,
                CreatedAt = new DateTime(2010, 12, 3, 10, 0, 0),
            },
            new()
            {
                Phase = BattlePhase.Hiring,
                CreatedAt = new DateTime(2010, 12, 4, 20, 0, 0),
            },
        };
        ArrangeDb.Battles.AddRange(battles);
        await ArrangeDb.SaveChangesAsync();

        Mock<IDateTime> dateTimeMock = new();
        dateTimeMock.Setup(dt => dt.UtcNow).Returns(new DateTime(2010, 12, 5));
        Mock<IBattleScheduler> battleSchedulerMock = new();
        UpdateBattlePhasesCommand.Handler handler = new(ActDb, Mock.Of<IBattleMercenaryDistributionModel>(),
            battleSchedulerMock.Object, dateTimeMock.Object, Constants);
        await handler.Handle(new UpdateBattlePhasesCommand(), CancellationToken.None);

        battles = await AssertDb.Battles.ToArrayAsync();
        Assert.AreEqual(BattlePhase.Scheduled, battles[0].Phase);
        battleSchedulerMock.Verify(s => s.ScheduleBattle(It.IsAny<Battle>()), Times.Once);
        Assert.AreEqual(BattlePhase.Hiring, battles[1].Phase);
    }

    [Test]
    public async Task ShouldSwitchScheduledBattlesToLiveAfterScheduledDate()
    {
        Battle[] battles =
        {
            new()
            {
                Phase = BattlePhase.Scheduled,
                ScheduledFor = new DateTime(2010, 12, 4),
            },
            new()
            {
                Phase = BattlePhase.Scheduled,
                ScheduledFor = new DateTime(2010, 12, 6),
            },
        };
        ArrangeDb.Battles.AddRange(battles);
        await ArrangeDb.SaveChangesAsync();

        Mock<IDateTime> dateTimeMock = new();
        dateTimeMock.Setup(dt => dt.UtcNow).Returns(new DateTime(2010, 12, 5));

        UpdateBattlePhasesCommand.Handler handler = new(ActDb, Mock.Of<IBattleMercenaryDistributionModel>(),
            Mock.Of<IBattleScheduler>(), dateTimeMock.Object, Constants);
        await handler.Handle(new UpdateBattlePhasesCommand(), CancellationToken.None);

        battles = await AssertDb.Battles.ToArrayAsync();
        Assert.AreEqual(BattlePhase.Live, battles[0].Phase);
        Assert.AreEqual(BattlePhase.Scheduled, battles[1].Phase);
    }
}
