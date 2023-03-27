using Crpg.Application.Clans.Queries;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Clans;

public class GetUserClanQueryTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfClanDoesntExist()
    {
        GetUserClanQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetUserClanQuery
        {
            UserId = 1,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotFound));
    }

    [Test]
    public async Task ShouldReturnNullIfUserNotInAClan()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        GetUserClanQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetUserClanQuery
        {
            UserId = user.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data, Is.Null);
    }

    [Test]
    public async Task ShouldReturnClanIfUserInAClan()
    {
        User user = new() { ClanMembership = new ClanMember { Clan = new Clan() } };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        GetUserClanQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetUserClanQuery
        {
            UserId = user.Id,
        }, CancellationToken.None);

        var clan = res.Data!;
        Assert.That(res.Errors, Is.Null);
        Assert.That(clan.Id, Is.EqualTo(user.ClanMembership.ClanId));
    }
}
