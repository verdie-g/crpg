using Crpg.Application.ActivityLogs.Commands;
using Crpg.Domain.Entities.ActivityLogs;
using Crpg.Sdk;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.ActivityLogs;

public class DeleteOldActivityLogsCommandTest : TestBase
{
    [Test]
    public async Task ShoulDeleteOldLogs()
    {
        ArrangeDb.ActivityLogs.AddRange(new ActivityLog[]
        {
            new() { CreatedAt = DateTime.Now.AddDays(-1) },
            new() { CreatedAt = DateTime.Now.AddDays(-10) },
            new() { CreatedAt = DateTime.Now.AddDays(-29) },
            new() { CreatedAt = DateTime.Now.AddDays(-31), Metadata = { new ActivityLogMetadata("a", "b") } },
            new() { CreatedAt = DateTime.Now.AddDays(-50), Metadata = { new ActivityLogMetadata("c", "d") } },
        });
        await ArrangeDb.SaveChangesAsync();

        DeleteOldActivityLogsCommand.Handler handler = new(ActDb, new MachineDateTime());
        await handler.Handle(new DeleteOldActivityLogsCommand(), CancellationToken.None);

        Assert.That(await AssertDb.ActivityLogs.CountAsync(), Is.EqualTo(3));
        Assert.That(await AssertDb.ActivityLogMetadata.CountAsync(), Is.EqualTo(0));
    }
}
