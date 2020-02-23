using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Items.Commands;
using Crpg.Domain.Entities;

namespace Crpg.Application.UTest.Items
{
    public class SellItemCommandTest : TestBase
    {
        [Test]
        public async Task Basic()
        {
            var user = _db.Users.Add(new User
            {
                Money = 0,
                UserItems = new List<UserItem>
                {
                    new UserItem
                    {
                        Item = new Item { Value = 100 },
                    }
                },
            });
            await _db.SaveChangesAsync();

            await new SellItemCommand.Handler(_db, _mapper).Handle(new SellItemCommand
            {
                ItemId = user.Entity.UserItems[0].ItemId,
                UserId = user.Entity.Id,
            }, CancellationToken.None);

            Assert.AreEqual(66, user.Entity.Money);
            Assert.IsTrue(!user.Entity.UserItems.Any(ui =>
                ui.ItemId == user.Entity.UserItems[0].ItemId));
        }

        [Test]
        public async Task NotFoundItem()
        {
            var user = _db.Users.Add(new User());
            await _db.SaveChangesAsync();

            Assert.ThrowsAsync<NotFoundException>(() => new SellItemCommand.Handler(_db, _mapper).Handle(
                new SellItemCommand
                {
                    ItemId = 1,
                    UserId = user.Entity.Id,
                }, CancellationToken.None));
        }

        [Test]
        public async Task NotFoundUser()
        {
            var item = _db.Items.Add(new Item());
            await _db.SaveChangesAsync();

            Assert.ThrowsAsync<NotFoundException>(() => new SellItemCommand.Handler(_db, _mapper).Handle(
                new SellItemCommand
                {
                    ItemId = item.Entity.Id,
                    UserId = 1,
                }, CancellationToken.None));
        }
    }
}