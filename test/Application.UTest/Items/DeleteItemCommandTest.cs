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
            var i = Db.Items.Add(new Item
            {
                Name = "sword",
                Value = 100,
                Type = ItemType.BodyArmor,
            });
            await Db.SaveChangesAsync();

            var handler = new DeleteItemCommand.Handler(Db);
            await handler.Handle(new DeleteItemCommand { ItemId = i.Entity.Id }, CancellationToken.None);

            Assert.IsNull(await Db.Items.FindAsync(i.Entity.Id));
        }

        [Test]
        public void WhenItemDoesntExist()
        {
            var handler = new DeleteItemCommand.Handler(Db);
            Assert.ThrowsAsync<NotFoundException>(() =>
                handler.Handle(new DeleteItemCommand { ItemId = 1 }, CancellationToken.None));
        }
    }
}