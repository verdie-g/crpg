using Crpg.Application.Restrictions.Queries;
using Crpg.Domain.Entities.Restrictions;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Restrictions;

public class GetRestrictionsQueryTest : TestBase
{
    [Test]
    public async Task Basic()
    {
        User user1 = new();
        User user2 = new();

        List<Restriction> restrictions = new()
        {
            new() { RestrictedUser = user1, RestrictedByUser = user2 },
            new() { RestrictedUser = user2, RestrictedByUser = user1 },
        };
        ArrangeDb.Restrictions.AddRange(restrictions);
        await ArrangeDb.SaveChangesAsync();

        var result = await new GetRestrictionsQuery.Handler(ActDb, Mapper).Handle(
            new GetRestrictionsQuery(), CancellationToken.None);
        Assert.AreEqual(2, result.Data!.Count);
    }
}
