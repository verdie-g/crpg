using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Crpg.Application.Items.Queries;
using Crpg.Domain.Entities;

namespace Crpg.Application.UTest.Items
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
                    Value = 100,
                    Type = ItemType.BodyArmor,
                },
                new Item
                {
                    Name = "tata",
                    Value = 200,
                    Type = ItemType.HandArmor,
                });
            await _db.SaveChangesAsync();

            var handler = new GetItemsListQuery.Handler(_db, _mapper);
            var items = await handler.Handle(new GetItemsListQuery(), CancellationToken.None);

            Assert.AreEqual(2, items.Count);
        }
    }
}