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
        private static readonly ILogger<SellItemCommand> Logger = Mock.Of<ILogger<SellItemCommand>>();
        private static readonly Constants Constants = new Constants { ItemSellCostCoefs = new[] { 0.5f, 0.0f } };

        [Test]
        public async Task SellItemUnequipped()
        {
            var user = new User
            {
                Gold = 0,
                OwnedItems = new List<OwnedItem>
                {
                    new OwnedItem
                    {
                        Item = new Item { Value = 100 },
                    }
                },
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            await new SellItemCommand.Handler(ActDb, Constants, Logger).Handle(new SellItemCommand
            {
                ItemId = user.OwnedItems[0].ItemId,
                UserId = user.Id,
            }, CancellationToken.None);

            user = await AssertDb.Users
                .Include(u => u.OwnedItems)
                .FirstAsync(u => u.Id == user.Id);
            Assert.AreEqual(50, user.Gold);
            Assert.False(user.OwnedItems.Any(oi => oi.ItemId == user.OwnedItems[0].ItemId));
        }

        [Test]
        public async Task SellItemEquipped()
        {
            var item = new Item { Value = 100 };
            var ownedItem = new OwnedItem { Item = item };
            var characters = new List<Character>
            {
                new Character { EquippedItems = { new EquippedItem { OwnedItem = ownedItem, Slot = ItemSlot.Head } } },
                new Character { EquippedItems = { new EquippedItem { OwnedItem = ownedItem, Slot = ItemSlot.Shoulder } } },
                new Character { EquippedItems = { new EquippedItem { OwnedItem = ownedItem, Slot = ItemSlot.Body } } },
                new Character { EquippedItems = { new EquippedItem { OwnedItem = ownedItem, Slot = ItemSlot.Hand } } },
                new Character { EquippedItems = { new EquippedItem { OwnedItem = ownedItem, Slot = ItemSlot.Leg } } },
                new Character { EquippedItems = { new EquippedItem { OwnedItem = ownedItem, Slot = ItemSlot.MountHarness } } },
                new Character { EquippedItems = { new EquippedItem { OwnedItem = ownedItem, Slot = ItemSlot.Mount } } },
                new Character { EquippedItems = { new EquippedItem { OwnedItem = ownedItem, Slot = ItemSlot.Weapon0 } } },
                new Character { EquippedItems = { new EquippedItem { OwnedItem = ownedItem, Slot = ItemSlot.Weapon1 } } },
                new Character { EquippedItems = { new EquippedItem { OwnedItem = ownedItem, Slot = ItemSlot.Weapon2 } } },
                new Character { EquippedItems = { new EquippedItem { OwnedItem = ownedItem, Slot = ItemSlot.Weapon3 } } },
            };
            var user = new User
            {
                Gold = 0,
                OwnedItems = { ownedItem },
                Characters = characters,
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            await new SellItemCommand.Handler(ActDb, Constants, Logger).Handle(new SellItemCommand
            {
                ItemId = item.Id,
                UserId = user.Id,
            }, CancellationToken.None);

            user = await AssertDb.Users
                .Include(u => u.Characters)
                .Include(u => u.OwnedItems)
                .FirstAsync(u => u.Id == user.Id);
            Assert.AreEqual(50, user.Gold);
            Assert.False(user.OwnedItems.Any(oi => oi.ItemId == item.Id));
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

            var result = await new SellItemCommand.Handler(ActDb, Constants, Logger).Handle(
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

            var result = await new SellItemCommand.Handler(ActDb, Constants, Logger).Handle(
                new SellItemCommand
                {
                    ItemId = item.Entity.Id,
                    UserId = 1,
                }, CancellationToken.None);
            Assert.AreEqual(ErrorCode.ItemNotOwned, result.Errors![0].Code);
        }
    }
}
