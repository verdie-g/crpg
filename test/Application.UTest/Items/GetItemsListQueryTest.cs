using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Trpg.Application.Items.Queries;
using Trpg.Domain.Entities;

namespace Trpg.Application.UTest.Items
{
    public class GetItemsListQueryTest : TestBase
    {
        [Test]
        public async Task Basic()
        {
            _db.AddRange(
                new Item
                {
                    Name = "toto",
                    Price = 100,
                    Type = ItemType.Body,
                },
                new Item
                {
                    Name = "tata",
                    Price = 200,
                    Type = ItemType.Gloves,
                });
            await _db.SaveChangesAsync();

            var handler = new GetItemsListQuery.Handler(_db, _mapper);
            var items = await handler.Handle(new GetItemsListQuery(), CancellationToken.None);

            Assert.AreEqual(2, items.Count);
        }
    }
}