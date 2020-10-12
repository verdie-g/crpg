using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Commands;
using Crpg.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.Items
{
    public class SellItemCommandTest : TestBase
    {
        [Test]
        public async Task SellItemUnequipped()
        {
            var user = new User
            {
                Gold = 0,
                OwnedItems = new List<UserItem>
                {
                    new UserItem
                    {
                        Item = new Item { Value = 100 },
                    }
                },
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            await new SellItemCommand.Handler(ActDb).Handle(new SellItemCommand
            {
                ItemId = user.OwnedItems[0].ItemId,
                UserId = user.Id,
            }, CancellationToken.None);

            user = await AssertDb.Users
                .Include(u => u.OwnedItems)
                .FirstAsync(u => u.Id == user.Id);
            Assert.AreEqual(66, user.Gold);
            Assert.False(user.OwnedItems.Any(oi => oi.ItemId == user.OwnedItems[0].ItemId));
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
            var user = new User
            {
                Gold = 0,
                OwnedItems = new List<UserItem> { new UserItem { Item = item } },
                Characters = characters,
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            await new SellItemCommand.Handler(ActDb).Handle(new SellItemCommand
            {
                ItemId = item.Id,
                UserId = user.Id,
            }, CancellationToken.None);

            user = await AssertDb.Users
                .Include(u => u.Characters)
                .Include(u => u.OwnedItems)
                .FirstAsync(u => u.Id == user.Id);
            Assert.AreEqual(66, user.Gold);
            Assert.False(user.OwnedItems.Any(oi => oi.ItemId == item.Id));
            Assert.Null(user.Characters[0].Items.HeadItemId);
            Assert.Null(user.Characters[1].Items.CapeItemId);
            Assert.Null(user.Characters[2].Items.BodyItemId);
            Assert.Null(user.Characters[3].Items.HeadItemId);
            Assert.Null(user.Characters[4].Items.LegItemId);
            Assert.Null(user.Characters[5].Items.HorseHarnessItemId);
            Assert.Null(user.Characters[6].Items.HorseItemId);
            Assert.Null(user.Characters[7].Items.Weapon1ItemId);
            Assert.Null(user.Characters[8].Items.Weapon2ItemId);
            Assert.Null(user.Characters[9].Items.Weapon3ItemId);
            Assert.Null(user.Characters[10].Items.Weapon4ItemId);
        }

        [Test]
        public async Task NotFoundItem()
        {
            var user = ArrangeDb.Users.Add(new User());
            await ArrangeDb.SaveChangesAsync();

            var result = await new SellItemCommand.Handler(ActDb).Handle(
                new SellItemCommand
                {
                    ItemId = 1,
                    UserId = user.Entity.Id,
                }, CancellationToken.None);
            Assert.AreEqual(ErrorCode.ItemNotOwned, result.Errors![0].Code);
        }

        [Test]
        public async Task NotFoundUser()
        {
            var item = ArrangeDb.Items.Add(new Item());
            await ArrangeDb.SaveChangesAsync();

            var result = await new SellItemCommand.Handler(ActDb).Handle(
                new SellItemCommand
                {
                    ItemId = item.Entity.Id,
                    UserId = 1,
                }, CancellationToken.None);
            Assert.AreEqual(ErrorCode.ItemNotOwned, result.Errors![0].Code);
        }
    }
}
