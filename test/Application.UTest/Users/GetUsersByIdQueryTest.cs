using Crpg.Application.Users.Queries;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Users;

public class GetUsersByIdQueryTest : TestBase
{
    [Test]
    public async Task ShouldReturnEmptyIfUserDoesNotExist()
    {
        GetUsersByIdQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetUsersByIdQuery
        {
            UserIds = new[] { 1 },
        }, CancellationToken.None);

        Assert.NotNull(res.Data);
        Assert.AreEqual(0, res.Data!.Count);
    }

    [Test]
    public async Task TestWhenUsersExists()
    {
        User user0 = new();
        User user1 = new();
        ArrangeDb.Users.AddRange(user0, user1);
        await ArrangeDb.SaveChangesAsync();

        GetUsersByIdQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetUsersByIdQuery
        {
            UserIds = new[] { user0.Id, user1.Id },
        }, CancellationToken.None);

        Assert.NotNull(res.Data);
        Assert.AreEqual(2, res.Data!.Count);
    }
}
