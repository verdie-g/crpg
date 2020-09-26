using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Items.Queries;
using Crpg.Domain.Entities;
using NUnit.Framework;

namespace Crpg.Application.UTest.Items
{
    public class GetItemsListQueryTest : TestBase
    {
        [Test]
        public async Task Basic()
        {
            ArrangeDb.AddRange(
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
                    Type = ItemType.Cape,
                    Rank = 3,
                },
                new Item
                {
                    Name = "tata",
                    Value = 200,
                    Type = ItemType.HandArmor,
                    Rank = 0,
                });
            await ArrangeDb.SaveChangesAsync();

            var handler = new GetItemsListQuery.Handler(ActDb, Mapper);
            var items = await handler.Handle(new GetItemsListQuery(), CancellationToken.None);

            Assert.AreEqual(2, items.Count);
        }
    }
}