using Crpg.Application.Common.Results;
using Crpg.Application.Users.Queries;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Users;

public class GetUserQueryTest : TestBase
{
    [Test]
    public async Task TestWhenUserDoesntExist()
    {
        GetUserQuery.Handler handler = new(ActDb, Mapper);
        var result = await handler.Handle(new GetUserQuery
        {
            UserId = 1,
        }, CancellationToken.None);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotFound));
    }

    [Test]
    public async Task TestWhenUserExists()
    {
        User dbUser = new()
        {
            PlatformUserId = "13948192759205810",
            Name = "def",
            Role = Role.Admin,
            Avatar = new Uri("http://ghi.klm"),
        };
        ArrangeDb.Users.Add(dbUser);
        await ArrangeDb.SaveChangesAsync();

        GetUserQuery.Handler handler = new(ActDb, Mapper);
        var user = await handler.Handle(new GetUserQuery
        {
            UserId = dbUser.Id,
        }, CancellationToken.None);

        Assert.That(user, Is.Not.Null);
    }
}
