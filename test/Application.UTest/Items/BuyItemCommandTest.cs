using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Items.Commands;
using Crpg.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.Items
{
    public class BuyItemCommandTest : TestBase
    {
        [Test]
        public async Task Basic()
        {
            var user = Db.Users.Add(new User { Gold = 100 });
            var item = Db.Items.Add(new Item { Value = 100 });
            await Db.SaveChangesAsync();

            var handler = new BuyItemCommand.Handler(Db, Mapper);
            var boughtItem = await handler.Handle(new BuyItemCommand
            {
                ItemId = item.Entity.Id,
                UserId = user.Entity.Id,
            }, CancellationToken.None);

            var userDb = await Db.Users
                .Include(u => u.OwnedItems)
                .FirstAsync(u => u.Id == user.Entity.Id);

            Assert.AreEqual(item.Entity.Id, boughtItem.Id);
            Assert.AreEqual(0, userDb.Gold);
            Assert.IsTrue(userDb.OwnedItems.Any(i => i.ItemId == boughtItem.Id));
        }

        [Test]
        public async Task NotFoundItem()
        {
            var user = Db.Users.Add(new User { Gold = 100 });
            await Db.SaveChangesAsync();

            var handler = new BuyItemCommand.Handler(Db, Mapper);
            Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(new BuyItemCommand
            {
                ItemId = 1,
                UserId = user.Entity.Id,
            }, CancellationToken.None));
        }

        [Test]
        public async Task NotFoundUser()
        {
            var item = Db.Items.Add(new Item { Value = 100 });
            await Db.SaveChangesAsync();

            var handler = new BuyItemCommand.Handler(Db, Mapper);
            Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(new BuyItemCommand
            {
                ItemId = item.Entity.Id,
                UserId = 1,
            }, CancellationToken.None));
        }

        [Test]
        public async Task NotEnoughGold()
        {
            var user = Db.Users.Add(new User { Gold = 100 });
            var item = Db.Items.Add(new Item { Value = 101 });
            await Db.SaveChangesAsync();

            var handler = new BuyItemCommand.Handler(Db, Mapper);
            Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(new BuyItemCommand
            {
                ItemId = item.Entity.Id,
                UserId = user.Entity.Id,
            }, CancellationToken.None));
        }

        [Test]
        public async Task AlreadyOwningItem()
        {
            var item = Db.Items.Add(new Item { Value = 100 });
            var user = Db.Users.Add(new User
            {
                Gold = 100,
                OwnedItems = new List<UserItem> { new UserItem { ItemId = item.Entity.Id } }
            });
            await Db.SaveChangesAsync();

            var handler = new BuyItemCommand.Handler(Db, Mapper);
            Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(new BuyItemCommand
            {
                ItemId = item.Entity.Id,
                UserId = user.Entity.Id,
            }, CancellationToken.None));
        }
    }
}