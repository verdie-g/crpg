using Crpg.Application.Clans.Queries;
using Crpg.Domain.Entities.Clans;
using NUnit.Framework;

namespace Crpg.Application.UTest.Clans;

public class GetClanQueryTest : TestBase
{
    [Test]
    public async Task ShouldGetClanIfExists()
    {
        Clan clan = new();
        ArrangeDb.Clans.Add(clan);
        await ArrangeDb.SaveChangesAsync();

        var result = await new GetClanQuery.Handler(ActDb, Mapper).Handle(new GetClanQuery
        {
            ClanId = clan.Id,
        }, CancellationToken.None);

        Assert.That(result.Data, Is.Not.Null);
    }

    [Test]
    public async Task ShouldReturnErrorIfClanDoesntExist()
    {
        var result = await new GetClanQuery.Handler(ActDb, Mapper).Handle(new GetClanQuery
        {
            ClanId = 1,
        }, CancellationToken.None);

        Assert.That(result.Data, Is.Null);
        Assert.That(result.Errors!, Is.Not.Empty);
    }
}
