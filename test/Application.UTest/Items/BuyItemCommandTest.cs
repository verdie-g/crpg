using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Commands;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Items
{
    public class BuyItemCommandTest : TestBase
    {
        [Test]
        public async Task Basic()
        {
            var user = ArrangeDb.Users.Add(new User { Gold = 100 });
            var item = ArrangeDb.Items.Add(new Item { Value = 100 });
            await ArrangeDb.SaveChangesAsync();

            var handler = new BuyItemCommand.Handler(ActDb, Mapper);
            var result = await handler.Handle(new BuyItemCommand
            {
                ItemId = item.Entity.Id,
                UserId = user.Entity.Id,
            }, CancellationToken.None);

            var userDb = await AssertDb.Users
                .Include(u => u.OwnedItems)
                .FirstAsync(u => u.Id == user.Entity.Id);

            var boughtItem = result.Data!;
            Assert.AreEqual(item.Entity.Id, boughtItem.Id);
            Assert.AreEqual(0, userDb.Gold);
            Assert.IsTrue(userDb.OwnedItems.Any(i => i.ItemId == boughtItem.Id));
        }

        [Test]
        public async Task NotFoundItem()
        {
            var user = ArrangeDb.Users.Add(new User { Gold = 100 });
            await ArrangeDb.SaveChangesAsync();

            var handler = new BuyItemCommand.Handler(ActDb, Mapper);
            var result = await handler.Handle(new BuyItemCommand
            {
                ItemId = 1,
                UserId = user.Entity.Id,
            }, CancellationToken.None);

            Assert.AreEqual(ErrorCode.ItemNotFound, result.Errors![0].Code);
        }

        [Test]
        public async Task ErrorIfItemNotRank0()
        {
            var user = new User();
            ArrangeDb.Users.Add(user);
            var item = new Item { Value = 100, Rank = 1 };
            ArrangeDb.Items.Add(item);
            await ArrangeDb.SaveChangesAsync();

            var handler = new BuyItemCommand.Handler(ActDb, Mapper);
            var result = await handler.Handle(new BuyItemCommand
            {
                ItemId = item.Id,
                UserId = user.Id,
            }, CancellationToken.None);

            Assert.AreEqual(ErrorCode.ItemNotBuyable, result.Errors![0].Code);
        }

        [Test]
        public async Task NotFoundUser()
        {
            var item = ArrangeDb.Items.Add(new Item { Value = 100 });
            await ArrangeDb.SaveChangesAsync();

            var handler = new BuyItemCommand.Handler(ActDb, Mapper);
            var result = await handler.Handle(new BuyItemCommand
            {
                ItemId = item.Entity.Id,
                UserId = 1,
            }, CancellationToken.None);
            Assert.AreEqual(ErrorCode.UserNotFound, result.Errors![0].Code);
        }

        [Test]
        public async Task NotEnoughGold()
        {
            var user = ArrangeDb.Users.Add(new User { Gold = 100 });
            var item = ArrangeDb.Items.Add(new Item { Value = 101 });
            await ArrangeDb.SaveChangesAsync();

            var handler = new BuyItemCommand.Handler(ActDb, Mapper);
            var result = await handler.Handle(new BuyItemCommand
            {
                ItemId = item.Entity.Id,
                UserId = user.Entity.Id,
            }, CancellationToken.None);
            Assert.AreEqual(ErrorCode.NotEnoughGold, result.Errors![0].Code);
        }

        [Test]
        public async Task AlreadyOwningItem()
        {
            var item = ArrangeDb.Items.Add(new Item { Value = 100 });
            var user = ArrangeDb.Users.Add(new User
            {
                Gold = 100,
                OwnedItems = new List<OwnedItem> { new() { ItemId = item.Entity.Id } },
            });
            await ArrangeDb.SaveChangesAsync();

            var handler = new BuyItemCommand.Handler(ActDb, Mapper);
            var result = await handler.Handle(new BuyItemCommand
            {
                ItemId = item.Entity.Id,
                UserId = user.Entity.Id,
            }, CancellationToken.None);
            Assert.AreEqual(ErrorCode.ItemAlreadyOwned, result.Errors![0].Code);
        }
    }
}
