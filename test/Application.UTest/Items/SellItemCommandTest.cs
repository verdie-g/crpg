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
            var user = _db.Users.Add(new User
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
            await _db.SaveChangesAsync();

            await new SellItemCommand.Handler(_db).Handle(new SellItemCommand
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
                new Character { HeadItem = item },
                new Character { CapeItem = item },
                new Character { BodyItem = item },
                new Character { HandItem = item },
                new Character { LegItem = item },
                new Character { HorseHarnessItem = item },
                new Character { HorseItem = item },
                new Character { Weapon1Item = item },
                new Character { Weapon2Item = item },
                new Character { Weapon3Item = item },
                new Character { Weapon4Item = item },
            };
            var user = _db.Users.Add(new User
            {
                Gold = 0,
                UserItems = new List<UserItem> { new UserItem { Item = item } },
                Characters = characters,
            });
            await _db.SaveChangesAsync();

            await new SellItemCommand.Handler(_db).Handle(new SellItemCommand
            {
                ItemId = item.Id,
                UserId = user.Entity.Id,
            }, CancellationToken.None);

            Assert.AreEqual(66, user.Entity.Gold);
            Assert.False(user.Entity.UserItems.Any(ui => ui.ItemId == item.Id));
            Assert.Null(characters[0].HeadItem);
            Assert.Null(characters[1].CapeItem);
            Assert.Null(characters[2].BodyItem);
            Assert.Null(characters[3].HeadItem);
            Assert.Null(characters[4].LegItem);
            Assert.Null(characters[5].HorseHarnessItem);
            Assert.Null(characters[6].HorseItem);
            Assert.Null(characters[7].Weapon1Item);
            Assert.Null(characters[8].Weapon1Item);
            Assert.Null(characters[9].Weapon1Item);
            Assert.Null(characters[10].Weapon1Item);
        }

        [Test]
        public async Task NotFoundItem()
        {
            var user = _db.Users.Add(new User());
            await _db.SaveChangesAsync();

            Assert.ThrowsAsync<NotFoundException>(() => new SellItemCommand.Handler(_db).Handle(
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

            Assert.ThrowsAsync<NotFoundException>(() => new SellItemCommand.Handler(_db).Handle(
                new SellItemCommand
                {
                    ItemId = item.Entity.Id,
                    UserId = 1,
                }, CancellationToken.None));
        }
    }
}