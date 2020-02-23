using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Items.Queries;
using Crpg.Domain.Entities;

namespace Crpg.Application.UTest.Items
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
                Value = 100,
                Type = ItemType.BodyArmor,
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
