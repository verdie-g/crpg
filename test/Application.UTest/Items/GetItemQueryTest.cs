using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Trpg.Application.Common.Exceptions;
using Trpg.Application.Items.Queries;
using Trpg.Domain.Entities;

namespace Trpg.Application.UTest.Items
{
    public class GetItemQueryTest : TestBase
    {
        [Test]
        public void WhenItemDoesntExist()
        {
            var handler = new GetItemQuery.Handler(_db, _mapper);
            Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(new GetItemQuery
            {
                ItemId = 1,
            }, CancellationToken.None));
        }

        [Test]
        public async Task WhenItemExists()
        {
            var dbItem = new Item
            {
                Name = "toto",
                Price = 100,
                Type = ItemType.Body,
            };
            _db.Items.Add(dbItem);
            await _db.SaveChangesAsync();

            var handler = new GetItemQuery.Handler(_db, _mapper);
            var item = await handler.Handle(new GetItemQuery
            {
                ItemId = dbItem.Id,
            }, CancellationToken.None);

            Assert.NotNull(item);
        }
    }
}
