using Crpg.Application.ActivityLogs.Queries;
using Crpg.Domain.Entities.ActivityLogs;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.ActivityLogs;

public class GetActivityLogsQueryTest : TestBase
{
    [Test]
    public async Task ShouldReturnAllLogsWithNoUserIdsAndNoTypes()
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
            UserIds = Array.Empty<int>(),
            Types = Array.Empty<ActivityLogType>(),
        }, CancellationToken.None);

        Assert.That(res.Data!.Count, Is.EqualTo(2));
        Assert.That(res.Data[0].Id, Is.EqualTo(2));
        Assert.That(res.Data[1].Id, Is.EqualTo(3));
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
            UserIds = new[] { user.Id },
            Types = Array.Empty<ActivityLogType>(),
        }, CancellationToken.None);

        Assert.That(res.Data!.Count, Is.EqualTo(3));
        Assert.That(res.Data[0].Id, Is.EqualTo(1));
        Assert.That(res.Data[1].Id, Is.EqualTo(2));
        Assert.That(res.Data[2].Id, Is.EqualTo(4));
    }

    [Test]
    public async Task ShouldReturnLogsForType()
    {
        User user = new();
        ArrangeDb.Users.Add(user);

        ArrangeDb.ActivityLogs.AddRange(new ActivityLog[]
        {
            new() { User = user, Type = ActivityLogType.UserCreated, CreatedAt = DateTime.UtcNow.AddMinutes(-2) },
            new() { User = user, Type = ActivityLogType.UserCreated, CreatedAt = DateTime.UtcNow.AddMinutes(-4) },
            new() { User = user, Type = ActivityLogType.UserDeleted, CreatedAt = DateTime.UtcNow.AddMinutes(-6) },
            new() { User = user, Type = ActivityLogType.UserCreated, CreatedAt = DateTime.UtcNow.AddMinutes(-8) },
        });
        await ArrangeDb.SaveChangesAsync();

        GetActivityLogsQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetActivityLogsQuery
        {
            From = DateTime.UtcNow.AddMinutes(-10),
            To = DateTime.UtcNow,
            UserIds = new[] { user.Id },
            Types = new[] { ActivityLogType.UserCreated },
        }, CancellationToken.None);

        Assert.That(res.Data!.Count, Is.EqualTo(3));
        Assert.That(res.Data[0].Id, Is.EqualTo(1));
        Assert.That(res.Data[1].Id, Is.EqualTo(2));
        Assert.That(res.Data[2].Id, Is.EqualTo(4));
    }

    [Test]
    public async Task ShouldReturnLogsForUserAndType()
    {
        User user = new();
        ArrangeDb.Users.Add(user);

        ArrangeDb.ActivityLogs.AddRange(new ActivityLog[]
        {
            new() { User = new User(), Type = ActivityLogType.UserCreated, CreatedAt = DateTime.UtcNow.AddMinutes(-2) },
            new() { User = user, Type = ActivityLogType.UserCreated, CreatedAt = DateTime.UtcNow.AddMinutes(-4) },
            new() { User = user, Type = ActivityLogType.UserDeleted, CreatedAt = DateTime.UtcNow.AddMinutes(-6) },
            new() { User = new User(), Type = ActivityLogType.UserCreated, CreatedAt = DateTime.UtcNow.AddMinutes(-8) },
        });
        await ArrangeDb.SaveChangesAsync();

        GetActivityLogsQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetActivityLogsQuery
        {
            From = DateTime.UtcNow.AddMinutes(-10),
            To = DateTime.UtcNow,
            UserIds = new[] { user.Id },
            Types = new[] { ActivityLogType.UserDeleted },
        }, CancellationToken.None);

        Assert.That(res.Data!.Count, Is.EqualTo(1));
        Assert.That(res.Data[0].Id, Is.EqualTo(3));
    }
}
