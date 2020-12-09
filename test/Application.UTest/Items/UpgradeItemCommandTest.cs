using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Commands;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.Items
{
    public class UpgradeItemCommandTest : TestBase
    {
        private Item[] _items = default!;

        [SetUp]
        public override async Task SetUp()
        {
            await base.SetUp();

            _items = Enumerable.Range(-3, 7).Select(r => new Item { Rank = r, Value = (r + 4) * 100 }).ToArray();
            ArrangeDb.Items.AddRange(_items);
            await ArrangeDb.SaveChangesAsync();
        }

        [Test]
        public async Task ShouldRepairIfRankLessThanZero([Values(0, 1, 2)] int itemIdx)
        {
            var user = new User
            {
                Gold = 1000,
                OwnedItems = new List<UserItem> { new UserItem { ItemId = _items[itemIdx].Id } }
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var result = await new UpgradeItemCommand.Handler(ActDb, Mapper).Handle(new UpgradeItemCommand
            {
                ItemId = _items[itemIdx].Id,
                UserId = user.Id,
            }, CancellationToken.None);

            user = await AssertDb.Users.FirstAsync(u => u.Id == user.Id);
            Assert.AreEqual(itemIdx - 2, result.Data!.Rank);
            Assert.Less(user.Gold, 1000);
        }

        [Test]
        public async Task ShouldNotRepairIfRankLessThanZeroButNotEnoughGold([Values(0, 1, 2)] int itemIdx)
        {
            var user = ArrangeDb.Users.Add(new User
            {
                Gold = 0,
                OwnedItems = new List<UserItem> { new UserItem { ItemId = _items[itemIdx].Id } }
            });
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpgradeItemCommand.Handler(ActDb, Mapper);
            var result = await handler.Handle(new UpgradeItemCommand
            {
                ItemId = _items[itemIdx].Id,
                UserId = user.Entity.Id,
            }, CancellationToken.None);
            Assert.AreEqual(ErrorCode.NotEnoughGold, result.Errors![0].Code);
        }

        [Test]
        public async Task ShouldLoomIfRankGreatorOrEqualThanZero([Values(3, 4, 5)] int itemIdx)
        {
            var user = new User
            {
                HeirloomPoints = 1,
                OwnedItems = new List<UserItem> { new UserItem { ItemId = _items[itemIdx].Id } }
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var result = await new UpgradeItemCommand.Handler(ActDb, Mapper).Handle(new UpgradeItemCommand
            {
                ItemId = _items[itemIdx].Id,
                UserId = user.Id,
            }, CancellationToken.None);

            user = await AssertDb.Users.FirstAsync(u => u.Id == user.Id);
            Assert.AreEqual(itemIdx - 2, result.Data!.Rank);
            Assert.AreEqual(0, user.HeirloomPoints);
        }

        [Test]
        public async Task ShouldNotLoomIfRankGreatorOrEqualThanZeroButNotEnoughLoomPoints([Values(3, 4, 5)] int itemIdx)
        {
            var user = ArrangeDb.Users.Add(new User
            {
                HeirloomPoints = 0,
                OwnedItems = new List<UserItem> { new UserItem { ItemId = _items[itemIdx].Id } }
            });
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpgradeItemCommand.Handler(ActDb, Mapper);
            var result = await handler.Handle(new UpgradeItemCommand
            {
                ItemId = _items[itemIdx].Id,
                UserId = user.Entity.Id,
            }, CancellationToken.None);
            Assert.AreEqual(ErrorCode.NotEnoughHeirloomPoints, result.Errors![0].Code);
        }

        [Test]
        public async Task ShouldReplaceCharacterItemWithUpgradeOne([Values(0, 1, 2, 3, 4, 5)] int itemIdx)
        {
            var userItem = new UserItem { ItemId = _items[itemIdx].Id };
            var user = new User
            {
                Gold = 1000,
                HeirloomPoints = 3,
                OwnedItems = new List<UserItem> { userItem },
                Characters = new List<Character>
                {
                    new Character { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.Weapon0 } } },
                    new Character { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.Weapon1 } } },
                },
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var upgradedItem = (await new UpgradeItemCommand.Handler(ActDb, Mapper).Handle(new UpgradeItemCommand
            {
                ItemId = _items[itemIdx].Id,
                UserId = user.Id,
            }, CancellationToken.None)).Data!;

            user = await AssertDb.Users
                .Include(u => u.Characters).ThenInclude(c => c.EquippedItems)
                .FirstAsync(u => u.Id == user.Id);
            Assert.AreEqual(upgradedItem.Id, user.Characters[0].EquippedItems[0].ItemId);
            Assert.AreEqual(upgradedItem.Id, user.Characters[1].EquippedItems[0].ItemId);
        }

        [Test]
        public async Task ShouldThrowIfUserDoesntOwnItem()
        {
            var user = ArrangeDb.Users.Add(new User());
            var item = ArrangeDb.Items.Add(new Item());
            await ArrangeDb.SaveChangesAsync();

            var result = await new UpgradeItemCommand.Handler(ActDb, Mapper).Handle(
                new UpgradeItemCommand
                {
                    ItemId = item.Entity.Id,
                    UserId = user.Entity.Id,
                }, CancellationToken.None);
            Assert.AreEqual(ErrorCode.ItemNotOwned, result.Errors![0].Code);
        }

        [Test]
        public async Task ShouldThrowIfItemDoesntExist()
        {
            var user = ArrangeDb.Users.Add(new User());
            await ArrangeDb.SaveChangesAsync();

            var result = await new UpgradeItemCommand.Handler(ActDb, Mapper).Handle(
                new UpgradeItemCommand
                {
                    ItemId = 1,
                    UserId = user.Entity.Id,
                }, CancellationToken.None);
            Assert.AreEqual(ErrorCode.ItemNotOwned, result.Errors![0].Code);
        }

        [Test]
        public async Task ShouldThrowIfUserDoesntExist()
        {
            var item = ArrangeDb.Items.Add(new Item());
            await ArrangeDb.SaveChangesAsync();

            var result = await new UpgradeItemCommand.Handler(ActDb, Mapper).Handle(
                new UpgradeItemCommand
                {
                    ItemId = item.Entity.Id,
                    UserId = 1,
                }, CancellationToken.None);
            Assert.AreEqual(ErrorCode.ItemNotOwned, result.Errors![0].Code);
        }
    }
}
