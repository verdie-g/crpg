using Crpg.Application.Items.Queries;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Items;

public class GetUserItemsQueryTest : TestBase
{
    [Test]
    public async Task Basic()
    {
        var user = ArrangeDb.Users.Add(new User
        {
            Items = new List<UserItem>
            {
                new() { BaseItem = new Item { Id = "1", Enabled = true } },
                new() { BaseItem = new Item { Id = "2", Enabled = true } },
                new() { BaseItem = new Item { Id = "3", Enabled = false } },
            },
        });
        await ArrangeDb.SaveChangesAsync();

        var result = await new GetUserItemsQuery.Handler(ActDb, Mapper).Handle(
            new GetUserItemsQuery { UserId = user.Entity.Id }, CancellationToken.None);

        Assert.That(result.Data!.Count, Is.EqualTo(2));
    }
}
