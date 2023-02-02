using Crpg.Application.ActivityLogs.Queries;
using Crpg.Domain.Entities.ActivityLogs;
using NUnit.Framework;

namespace Crpg.Application.UTest.ActivityLogs;

public class GetActivityLogsQueryTest : TestBase
{
    [Test]
    public async Task ShouldReturnLastLogsWithNoAfterId()
    {
        ArrangeDb.ActivityLogs.AddRange(new ActivityLog[]
        {
            new() { CreatedAt = DateTime.UtcNow },
            new() { CreatedAt = DateTime.UtcNow.AddMinutes(-1) },
            new() { CreatedAt = DateTime.UtcNow.AddMinutes(-2) },
            new() { CreatedAt = DateTime.UtcNow.AddMinutes(-3) },
        });
        await ArrangeDb.SaveChangesAsync();

        GetActivityLogsQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetActivityLogsQuery
        {
            Count = 3,
            AfterId = null,
        }, CancellationToken.None);

        Assert.AreEqual(3, res.Data!.Count);
        Assert.AreEqual(1, res.Data[0].Id);
        Assert.AreEqual(2, res.Data[1].Id);
        Assert.AreEqual(3, res.Data[2].Id);
    }

    [Test]
    public async Task ShouldReturnLogsAfterId()
    {
        ArrangeDb.ActivityLogs.AddRange(new ActivityLog[]
        {
            new() { CreatedAt = DateTime.UtcNow },
            new() { CreatedAt = DateTime.UtcNow.AddMinutes(-1) },
            new() { CreatedAt = DateTime.UtcNow.AddMinutes(-2) },
            new() { CreatedAt = DateTime.UtcNow.AddMinutes(-3) },
        });
        await ArrangeDb.SaveChangesAsync();

        GetActivityLogsQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetActivityLogsQuery
        {
            Count = 50,
            AfterId = 2,
        }, CancellationToken.None);

        Assert.AreEqual(2, res.Data!.Count);
        Assert.AreEqual(3, res.Data[0].Id);
        Assert.AreEqual(4, res.Data[1].Id);
    }
}
