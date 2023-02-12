using Crpg.Application.ActivityLogs.Queries;
using Crpg.Domain.Entities.ActivityLogs;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.ActivityLogs;

public class GetActivityLogsQueryTest : TestBase
{
    [Test]
    public async Task ShouldReturnAllLogsWithNoUserId()
    {
        ArrangeDb.ActivityLogs.AddRange(new ActivityLog[]
        {
            new() { CreatedAt = DateTime.UtcNow.AddMinutes(-2) },
            new() { CreatedAt = DateTime.UtcNow.AddMinutes(-4) },
            new() { CreatedAt = DateTime.UtcNow.AddMinutes(-6) },
            new() { CreatedAt = DateTime.UtcNow.AddMinutes(-8) },
        });
        await ArrangeDb.SaveChangesAsync();

        GetActivityLogsQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetActivityLogsQuery
        {
            From = DateTime.UtcNow.AddMinutes(-7),
            To = DateTime.UtcNow.AddMinutes(-3),
            UserId = null,
        }, CancellationToken.None);

        Assert.AreEqual(2, res.Data!.Count);
        Assert.AreEqual(2, res.Data[0].Id);
        Assert.AreEqual(3, res.Data[1].Id);
    }

    [Test]
    public async Task ShouldReturnLogsForUserId()
    {
        User user = new();
        ArrangeDb.Users.Add(user);

        ArrangeDb.ActivityLogs.AddRange(new ActivityLog[]
        {
            new() { User = user, CreatedAt = DateTime.UtcNow.AddMinutes(-2) },
            new() { User = user, CreatedAt = DateTime.UtcNow.AddMinutes(-4) },
            new() { User = new User(), CreatedAt = DateTime.UtcNow.AddMinutes(-6) },
            new() { User = user, CreatedAt = DateTime.UtcNow.AddMinutes(-8) },
        });
        await ArrangeDb.SaveChangesAsync();

        GetActivityLogsQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetActivityLogsQuery
        {
            From = DateTime.UtcNow.AddMinutes(-10),
            To = DateTime.UtcNow,
            UserId = user.Id,
        }, CancellationToken.None);

        Assert.AreEqual(3, res.Data!.Count);
        Assert.AreEqual(1, res.Data[0].Id);
        Assert.AreEqual(2, res.Data[1].Id);
        Assert.AreEqual(4, res.Data[2].Id);
    }
}
