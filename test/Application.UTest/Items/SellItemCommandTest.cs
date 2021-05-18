using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Commands;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Items
{
    public class SellItemCommandTest : TestBase
    {
        private static readonly Constants Constants = new() { ItemSellCostCoefs = new[] { 0.5f, 0.0f } };

        [Test]
        public async Task SellItemUnequipped()
        {
            var user = new User
            {
                Gold = 0,
                Items = new List<UserItem>
                {
                    new()
                    {
                        Item = new Item { Value = 100 },
                    },
                },
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            await new SellItemCommand.Handler(ActDb, Constants).Handle(new SellItemCommand
            {
                ItemId = user.Items[0].ItemId,
                UserId = user.Id,
            }, CancellationToken.None);

            user = await AssertDb.Users
                .Include(u => u.Items)
                .FirstAsync(u => u.Id == user.Id);
            Assert.AreEqual(50, user.Gold);
            Assert.False(user.Items.Any(oi => oi.ItemId == user.Items[0].ItemId));
        }

        [Test]
        public async Task SellItemEquipped()
        {
            var item = new Item { Value = 100 };
            var userItem = new UserItem { Item = item };
            var characters = new List<Character>
            {
                new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.Head } } },
                new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.Shoulder } } },
                new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.Body } } },
                new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.Hand } } },
                new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.Leg } } },
                new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.MountHarness } } },
                new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.Mount } } },
                new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.Weapon0 } } },
                new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.Weapon1 } } },
                new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.Weapon2 } } },
                new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.Weapon3 } } },
            };
            var user = new User
            {
                Gold = 0,
                Items = { userItem },
                Characters = characters,
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            await new SellItemCommand.Handler(ActDb, Constants).Handle(new SellItemCommand
            {
                ItemId = item.Id,
                UserId = user.Id,
            }, CancellationToken.None);

            user = await AssertDb.Users
                .Include(u => u.Characters)
                .Include(u => u.Items)
                .FirstAsync(u => u.Id == user.Id);
            Assert.AreEqual(50, user.Gold);
            Assert.False(user.Items.Any(oi => oi.ItemId == item.Id));
            Assert.IsEmpty(user.Characters[0].EquippedItems);
            Assert.IsEmpty(user.Characters[1].EquippedItems);
            Assert.IsEmpty(user.Characters[2].EquippedItems);
            Assert.IsEmpty(user.Characters[3].EquippedItems);
            Assert.IsEmpty(user.Characters[4].EquippedItems);
            Assert.IsEmpty(user.Characters[5].EquippedItems);
            Assert.IsEmpty(user.Characters[6].EquippedItems);
            Assert.IsEmpty(user.Characters[7].EquippedItems);
            Assert.IsEmpty(user.Characters[8].EquippedItems);
            Assert.IsEmpty(user.Characters[9].EquippedItems);
            Assert.IsEmpty(user.Characters[10].EquippedItems);
        }

        [Test]
        public async Task NotFoundItem()
        {
            var user = ArrangeDb.Users.Add(new User());
            await ArrangeDb.SaveChangesAsync();

            var result = await new SellItemCommand.Handler(ActDb, Constants).Handle(
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

            var result = await new SellItemCommand.Handler(ActDb, Constants).Handle(
                new SellItemCommand
                {
                    ItemId = item.Entity.Id,
                    UserId = 1,
                }, CancellationToken.None);
            Assert.AreEqual(ErrorCode.ItemNotOwned, result.Errors![0].Code);
        }
    }
}
