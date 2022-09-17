using Crpg.Application.Common.Results;
using Crpg.Application.Restrictions.Queries;
using Crpg.Domain.Entities.Restrictions;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Restrictions;

public class GetUserRestrictionsQueryTest : TestBase
{
    [Test]
    public async Task Basic()
    {
        User user = new()
        {
            Restrictions = new List<Restriction>
            {
                new() { RestrictedByUser = new User { PlatformUserId = "123" } },
                new() { RestrictedByUser = new User { PlatformUserId = "456" } },
            },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        var result = await new GetUserRestrictionsQuery.Handler(ActDb, Mapper).Handle(
            new GetUserRestrictionsQuery { UserId = user.Id }, CancellationToken.None);
        var restrictions = result.Data!;
        Assert.AreEqual(2, restrictions.Count);
        Assert.AreEqual("123", restrictions[0].RestrictedByUser!.PlatformUserId);
        Assert.AreEqual("456", restrictions[1].RestrictedByUser!.PlatformUserId);
    }

    [Test]
    public async Task NotFoundUser()
    {
        GetUserRestrictionsQuery.Handler handler = new(ActDb, Mapper);
        var result = await handler.Handle(new GetUserRestrictionsQuery { UserId = 1 }, CancellationToken.None);
        Assert.AreEqual(ErrorCode.UserNotFound, result.Errors![0].Code);
    }
}
