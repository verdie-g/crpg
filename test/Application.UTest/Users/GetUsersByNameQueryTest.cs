using Crpg.Application.Users.Queries;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Users;

public class GetUsersByNameQueryTest : TestBase
{
    [Test]
    public async Task ShouldReturnEmptyIfUserNotMatched()
    {
        ArrangeDb.Users.AddRange(new User[]
        {
            new() { Name = "ya" },
        });
        await ArrangeDb.SaveChangesAsync();

        GetUsersByNameQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetUsersByNameQuery
        {
            Name = "yo",
        }, CancellationToken.None);

        Assert.That(res.Data, Is.Not.Null);
        Assert.That(res.Data!.Length, Is.EqualTo(0));
    }

    [Test]
    public async Task TestMatched()
    {
        ArrangeDb.Users.AddRange(new User[]
        {
            new() { Name = "yo" },
            new() { Name = "ayo" },
            new() { Name = "Yoa" },
            new() { Name = "ayOa" },
            new() { Name = "ya" },
        });
        await ArrangeDb.SaveChangesAsync();

        GetUsersByNameQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetUsersByNameQuery
        {
            Name = "yo",
        }, CancellationToken.None);

        Assert.That(res.Data, Is.Not.Null);
        Assert.That(res.Data!.Length, Is.EqualTo(4));
    }
}
