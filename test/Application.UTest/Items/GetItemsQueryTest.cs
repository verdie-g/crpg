using Crpg.Application.Items.Queries;
using Crpg.Domain.Entities.Items;
using NUnit.Framework;

namespace Crpg.Application.UTest.Items;

public class GetItemsQueryTest : TestBase
{
    [Test]
    public async Task Basic()
    {
        Item[] items =
        {
            new()
            {
                Id = "1",
                Name = "toto",
                Price = 100,
                Type = ItemType.BodyArmor,
            },
            new()
            {
                Id = "2",
                Name = "toto",
                Price = 100,
                Type = ItemType.ShoulderArmor,
            },
            new()
            {
                Id = "3",
                Name = "tata",
                Price = 200,
                Type = ItemType.HandArmor,
            },
        };
        ArrangeDb.Items.AddRange(items);
        await ArrangeDb.SaveChangesAsync();

        GetItemsQuery.Handler handler = new(ActDb, Mapper);
        var result = await handler.Handle(new GetItemsQuery(), CancellationToken.None);

        Assert.AreEqual(3, result.Data!.Count);
    }
}
