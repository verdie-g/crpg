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
        Assert.AreEqual(ErrorCode.UserNotFound, result.Errors![0].Code);
    }

    [Test]
    public async Task TestWhenUserExists()
    {
        User dbUser = new()
        {
            PlatformUserId = "13948192759205810",
            Name = "def",
            Role = Role.Admin,
            AvatarSmall = new Uri("http://ghi.klm"),
            AvatarMedium = new Uri("http://mno.pqr"),
            AvatarFull = new Uri("http://stu.vwx"),
        };
        ArrangeDb.Users.Add(dbUser);
        await ArrangeDb.SaveChangesAsync();

        GetUserQuery.Handler handler = new(ActDb, Mapper);
        var user = await handler.Handle(new GetUserQuery
        {
            UserId = dbUser.Id,
        }, CancellationToken.None);

        Assert.NotNull(user);
    }
}
