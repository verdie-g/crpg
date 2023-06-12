using Crpg.Application.Items.Queries;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Items;

public class GetItemUpgradesQueryTest : TestBase
{
    [Test]
    public async Task Basic()
    {
        Item[] items =
        {
            new()
            {
                Id = "1",
                Name = "a_h0",
                BaseId = "a",
                Rank = 0,
                Price = 100,
                Type = ItemType.BodyArmor,
                Enabled = true,
            },
            new()
            {
                Id = "2",
                Name = "a_h1",
                BaseId = "a",
                Rank = 1,
                Price = 100,
                Type = ItemType.ShoulderArmor,
                Enabled = true,
            },
            new()
            {
                Id = "3",
                Name = "a_h2",
                BaseId = "a",
                Rank = 2,
                Price = 200,
                Type = ItemType.HandArmor,
                Enabled = true,
            },
            new()
            {
                Id = "4",
                Name = "a_h3",
                BaseId = "a",
                Rank = 3,
                Price = 300,
                Type = ItemType.Bolts,
                Enabled = true,
            },
            new()
            {
                Id = "5",
                Name = "b_h0",
                BaseId = "b",
                Rank = 0,
                Price = 300,
                Type = ItemType.Bolts,
                Enabled = true,
            },
            new()
            {
                Id = "6",
                Name = "b_h1",
                BaseId = "b",
                Rank = 1,
                Price = 300,
                Type = ItemType.Bolts,
                Enabled = true,
            },
            new()
            {
                Id = "7",
                Name = "b_h2",
                BaseId = "b",
                Rank = 2,
                Price = 300,
                Type = ItemType.Bolts,
                Enabled = true,
            },
            new()
            {
                Id = "8",
                Name = "b_h3",
                BaseId = "b",
                Rank = 3,
                Price = 300,
                Type = ItemType.Bolts,
                Enabled = true,
            },
        };

        ArrangeDb.Items.AddRange(items);
        await ArrangeDb.SaveChangesAsync();

        var result = await new GetItemUpgradesQuery.Handler(ActDb, Mapper).Handle(
            new GetItemUpgradesQuery { BaseId = "a" }, CancellationToken.None);

        Assert.That(result.Data!.Count, Is.EqualTo(4));
    }
}
