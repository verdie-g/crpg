using Crpg.Application.Clans.Queries;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Clans;

public class GetClanMembersQueryTest : TestBase
{
    [Test]
    public async Task ShouldGetClanMembersIfClanExists()
    {
        Clan clan = new()
        {
            Members =
            {
                new ClanMember { Role = ClanMemberRole.Leader, User = new User() },
                new ClanMember { Role = ClanMemberRole.Officer, User = new User() },
                new ClanMember { Role = ClanMemberRole.Member, User = new User() },
            },
        };
        ArrangeDb.Clans.Add(clan);
        await ArrangeDb.SaveChangesAsync();

        GetClanMembersQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetClanMembersQuery
        {
            ClanId = clan.Id,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Null);
        Assert.That(res.Data!.Count, Is.EqualTo(3));
    }

    [Test]
    public async Task ShouldReturnErrorIfClanDoesntExist()
    {
        GetClanMembersQuery.Handler handler = new(ActDb, Mapper);
        var res = await handler.Handle(new GetClanMembersQuery
        {
            ClanId = 1,
        }, CancellationToken.None);

        Assert.That(res.Errors, Is.Not.Null);
        Assert.That(res.Errors![0].Code, Is.EqualTo(ErrorCode.ClanNotFound));
    }
}
