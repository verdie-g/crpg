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
                Enabled = true,
            },
            new()
            {
                Id = "2",
                Name = "toto",
                Price = 100,
                Type = ItemType.ShoulderArmor,
                Enabled = true,
            },
            new()
            {
                Id = "3",
                Name = "tata",
                Price = 200,
                Type = ItemType.HandArmor,
                Enabled = true,
            },
            new()
            {
                Id = "4",
                Name = "titi",
                Price = 300,
                Type = ItemType.Bolts,
                Enabled = false,
            },
            new()
            {
                Id = "5",
                Name = "titi_1",
                Price = 300,
                Type = ItemType.Bolts,
                Enabled = true,
                Rank = 1,
            },
        };
        ArrangeDb.Items.AddRange(items);
        await ArrangeDb.SaveChangesAsync();

        GetItemsQuery.Handler handler = new(ActDb, Mapper);
        var result = await handler.Handle(new GetItemsQuery(), CancellationToken.None);

        Assert.That(result.Data!.Count, Is.EqualTo(3));
    }
}
