using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Items.Commands;
using Crpg.Domain.Entities;
using NUnit.Framework;

namespace Crpg.Application.UTest.Items
{
    public class DeleteItemCommandTest : TestBase
    {
        [Test]
        public async Task WhenItemExists()
        {
            var i = _db.Items.Add(new Item
            {
                Name = "sword",
                Value = 100,
                Type = ItemType.BodyArmor,
            });
            await _db.SaveChangesAsync();

            var handler = new DeleteItemCommand.Handler(_db);
            await handler.Handle(new DeleteItemCommand { ItemId = i.Entity.Id }, CancellationToken.None);

            Assert.IsNull(await _db.Items.FindAsync(i.Entity.Id));
        }

        [Test]
        public void WhenItemDoesntExist()
        {
            var handler = new DeleteItemCommand.Handler(_db);
            Assert.ThrowsAsync<NotFoundException>(() =>
                handler.Handle(new DeleteItemCommand { ItemId = 1 }, CancellationToken.None));
        }
    }
}