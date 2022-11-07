using Crpg.Application.Common.Results;
using Crpg.Application.Users.Queries;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Users;

public class GetUserByPlatformIdQueryTest : TestBase
{
    [Test]
    public async Task TestWhenUserExists()
    {
        User user = new()
        {
            Platform = Platform.Steam,
            PlatformUserId = "13948192759205810",
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        GetUserByPlatformIdQuery.Handler handler = new(ActDb, Mapper);
        var userVm = await handler.Handle(new GetUserByPlatformIdQuery
        {
            Platform = Platform.Steam,
            PlatformUserId = "13948192759205810",
        }, CancellationToken.None);

        Assert.NotNull(userVm);
    }
}
