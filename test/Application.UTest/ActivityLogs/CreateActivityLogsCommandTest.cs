using Crpg.Application.ActivityLogs.Commands;
using Crpg.Application.ActivityLogs.Models;
using Crpg.Domain.Entities.ActivityLogs;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.ActivityLogs;

public class CreateActivityLogsCommandTest : TestBase
{
    [Test]
    public async Task Basic()
    {
        CreateActivityLogsCommand.Handler handler = new(ActDb);
        await handler.Handle(new CreateActivityLogsCommand
        {
            ActivityLogs = new ActivityLogViewModel[]
            {
                new() { Type = ActivityLogType.ItemBought, Metadata = { ["a"] = "b" } },
                new() { Type = ActivityLogType.ItemSold },
            },
        }, CancellationToken.None);

        Assert.AreEqual(2, await AssertDb.ActivityLogs.CountAsync());
        Assert.AreEqual(1, await AssertDb.ActivityLogMetadata.CountAsync());
    }
}
