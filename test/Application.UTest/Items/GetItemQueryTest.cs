using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Items.Queries;
using Crpg.Domain.Entities;
using NUnit.Framework;

namespace Crpg.Application.UTest.Items
{
    public class GetItemQueryTest : TestBase
    {
        [Test]
        public void WhenItemDoesntExist()
        {
            var handler = new GetItemQuery.Handler(Db, Mapper);
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
            Db.Items.Add(dbItem);
            await Db.SaveChangesAsync();

            var handler = new GetItemQuery.Handler(Db, Mapper);
            var item = await handler.Handle(new GetItemQuery
            {
                ItemId = dbItem.Id,
            }, CancellationToken.None);

            Assert.NotNull(item);
        }
    }
}
