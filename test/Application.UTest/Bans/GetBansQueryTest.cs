using Crpg.Application.Bans.Queries;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Bans;

public class GetBansQueryTest : TestBase
{
    [Test]
    public async Task Basic()
    {
        User user1 = new();
        User user2 = new();

        List<Ban> bans = new()
        {
            new() { BannedUser = user1, BannedByUser = user2 },
            new() { BannedUser = user2, BannedByUser = user1 },
        };
        ArrangeDb.Bans.AddRange(bans);
        await ArrangeDb.SaveChangesAsync();

        var result = await new GetBansQuery.Handler(ActDb, Mapper).Handle(
            new GetBansQuery(), CancellationToken.None);
        Assert.AreEqual(2, result.Data!.Count);
    }
}
