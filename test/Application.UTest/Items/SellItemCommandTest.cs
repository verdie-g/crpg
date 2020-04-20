using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Items.Commands;
using Crpg.Domain.Entities;
using NUnit.Framework;

namespace Crpg.Application.UTest.Items
{
    public class SellItemCommandTest : TestBase
    {
        [Test]
        public async Task SellItemUnequipped()
        {
            var user = Db.Users.Add(new User
            {
                Gold = 0,
                UserItems = new List<UserItem>
                {
                    new UserItem
                    {
                        Item = new Item { Value = 100 },
                    }
                },
            });
            await Db.SaveChangesAsync();

            await new SellItemCommand.Handler(Db).Handle(new SellItemCommand
            {
                ItemId = user.Entity.UserItems[0].ItemId,
                UserId = user.Entity.Id,
            }, CancellationToken.None);

            Assert.AreEqual(66, user.Entity.Gold);
            Assert.False(user.Entity.UserItems.Any(ui => ui.ItemId == user.Entity.UserItems[0].ItemId));
        }

        [Test]
        public async Task SellItemEquipped()
        {
            var item = new Item { Value = 100 };
            var characters = new List<Character>
            {
                new Character { Items = { HeadItem = item } },
                new Character { Items = { CapeItem = item } },
                new Character { Items = { BodyItem = item } },
                new Character { Items = { HandItem = item } },
                new Character { Items = { LegItem = item } },
                new Character { Items = { HorseHarnessItem = item } },
                new Character { Items = { HorseItem = item } },
                new Character { Items = { Weapon1Item = item } },
                new Character { Items = { Weapon2Item = item } },
                new Character { Items = { Weapon3Item = item } },
                new Character { Items = { Weapon4Item = item } },
            };
            var user = Db.Users.Add(new User
            {
                Gold = 0,
                UserItems = new List<UserItem> { new UserItem { Item = item } },
                Characters = characters,
            });
            await Db.SaveChangesAsync();

            await new SellItemCommand.Handler(Db).Handle(new SellItemCommand
            {
                ItemId = item.Id,
                UserId = user.Entity.Id,
            }, CancellationToken.None);

            Assert.AreEqual(66, user.Entity.Gold);
            Assert.False(user.Entity.UserItems.Any(ui => ui.ItemId == item.Id));
            Assert.Null(characters[0].Items.HeadItem);
            Assert.Null(characters[1].Items.CapeItem);
            Assert.Null(characters[2].Items.BodyItem);
            Assert.Null(characters[3].Items.HeadItem);
            Assert.Null(characters[4].Items.LegItem);
            Assert.Null(characters[5].Items.HorseHarnessItem);
            Assert.Null(characters[6].Items.HorseItem);
            Assert.Null(characters[7].Items.Weapon1Item);
            Assert.Null(characters[8].Items.Weapon1Item);
            Assert.Null(characters[9].Items.Weapon1Item);
            Assert.Null(characters[10].Items.Weapon1Item);
        }

        [Test]
        public async Task NotFoundItem()
        {
            var user = Db.Users.Add(new User());
            await Db.SaveChangesAsync();

            Assert.ThrowsAsync<NotFoundException>(() => new SellItemCommand.Handler(Db).Handle(
                new SellItemCommand
                {
                    ItemId = 1,
                    UserId = user.Entity.Id,
                }, CancellationToken.None));
        }

        [Test]
        public async Task NotFoundUser()
        {
            var item = Db.Items.Add(new Item());
            await Db.SaveChangesAsync();

            Assert.ThrowsAsync<NotFoundException>(() => new SellItemCommand.Handler(Db).Handle(
                new SellItemCommand
                {
                    ItemId = item.Entity.Id,
                    UserId = 1,
                }, CancellationToken.None));
        }
    }
}