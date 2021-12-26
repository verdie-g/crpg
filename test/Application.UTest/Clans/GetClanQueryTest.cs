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

        Assert.NotNull(result.Data);
    }

    [Test]
    public async Task ShouldReturnErrorIfClanDoesntExist()
    {
        var result = await new GetClanQuery.Handler(ActDb, Mapper).Handle(new GetClanQuery
        {
            ClanId = 1,
        }, CancellationToken.None);

        Assert.Null(result.Data);
        Assert.IsNotEmpty(result.Errors!);
    }
}
