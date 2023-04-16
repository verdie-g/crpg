using Crpg.Application.Restrictions.Queries;
using Crpg.Domain.Entities.Restrictions;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk;
using NUnit.Framework;

namespace Crpg.Application.UTest.Restrictions;

internal class IsUserBannedQueryTest : TestBase
{
    [Test]
    public async Task RestrictionNotFound()
    {
        IsUserBannedQuery.Handler handler = new(ActDb, new MachineDateTime());
        var res = await handler.Handle(new IsUserBannedQuery { UserId = 1 }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data, Is.False);
    }

    [Theory]
    public async Task RestrictionFound(RestrictionType type, bool active)
    {
        User user = new();
        Restriction restriction = new()
        {
            Type = type,
            Duration = active ? TimeSpan.FromHours(5) : TimeSpan.Zero,
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            RestrictedUser = user,
        };
        ArrangeDb.Restrictions.Add(restriction);
        await ArrangeDb.SaveChangesAsync();

        IsUserBannedQuery.Handler handler = new(ActDb, new MachineDateTime());
        var res = await handler.Handle(new IsUserBannedQuery
        {
            UserId = user.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        if (type == RestrictionType.All && active)
        {
            Assert.That(res.Data, Is.True);
        }
        else
        {
            Assert.That(res.Data, Is.False);
        }
    }
}
