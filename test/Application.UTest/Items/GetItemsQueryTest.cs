using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Items.Queries;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Items;
using NUnit.Framework;

namespace Crpg.Application.UTest.Items
{
    public class GetItemsQueryTest : TestBase
    {
        [Test]
        public async Task BaseItems()
        {
            var items = new[]
            {
                new Item
                {
                    Name = "toto",
                    Value = 100,
                    Type = ItemType.BodyArmor,
                    Rank = 0,
                },
                new Item
                {
                    Name = "toto",
                    Value = 100,
                    Type = ItemType.ShoulderArmor,
                    Rank = 3,
                },
                new Item
                {
                    Name = "tata",
                    Value = 200,
                    Type = ItemType.HandArmor,
                    Rank = 0,
                },
            };
            ArrangeDb.Items.AddRange(items);
            await ArrangeDb.SaveChangesAsync();

            GetItemsQuery.Handler handler = new(ActDb, Mapper);
            var result = await handler.Handle(new GetItemsQuery { BaseItems = true }, CancellationToken.None);

            Assert.AreEqual(2, result.Data!.Count);
        }

        [Test]
        public async Task AllItems()
        {
            var items = new[]
            {
                new Item
                {
                    Name = "toto",
                    Value = 100,
                    Type = ItemType.BodyArmor,
                    Rank = 0,
                },
                new Item
                {
                    Name = "toto",
                    Value = 100,
                    Type = ItemType.ShoulderArmor,
                    Rank = 3,
                },
                new Item
                {
                    Name = "tata",
                    Value = 200,
                    Type = ItemType.HandArmor,
                    Rank = 0,
                },
            };
            ArrangeDb.Items.AddRange(items);
            await ArrangeDb.SaveChangesAsync();

            GetItemsQuery.Handler handler = new(ActDb, Mapper);
            var result = await handler.Handle(new GetItemsQuery { BaseItems = false }, CancellationToken.None);

            Assert.AreEqual(3, result.Data!.Count);
        }
    }
}
