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
        Assert.That(restrictions.Count, Is.EqualTo(2));
        Assert.That(restrictions[0].RestrictedByUser!.PlatformUserId, Is.EqualTo("123"));
        Assert.That(restrictions[1].RestrictedByUser!.PlatformUserId, Is.EqualTo("456"));
    }

    [Test]
    public async Task NotFoundUser()
    {
        GetUserRestrictionsQuery.Handler handler = new(ActDb, Mapper);
        var result = await handler.Handle(new GetUserRestrictionsQuery { UserId = 1 }, CancellationToken.None);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotFound));
    }
}
