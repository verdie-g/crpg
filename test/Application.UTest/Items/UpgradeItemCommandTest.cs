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

            var upgradedItem = await new UpgradeItemCommand.Handler(ActDb, Mapper).Handle(new UpgradeItemCommand
            {
                ItemId = _items[itemIdx].Id,
                UserId = user.Id,
            }, CancellationToken.None);

            user = await AssertDb.Users.FirstAsync(u => u.Id == user.Id);
            Assert.AreEqual(itemIdx - 2, upgradedItem.Rank);
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
            Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(new UpgradeItemCommand
            {
                ItemId = _items[itemIdx].Id,
                UserId = user.Entity.Id,
            }, CancellationToken.None));
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

            var upgradedItem = await new UpgradeItemCommand.Handler(ActDb, Mapper).Handle(new UpgradeItemCommand
            {
                ItemId = _items[itemIdx].Id,
                UserId = user.Id,
            }, CancellationToken.None);

            user = await AssertDb.Users.FirstAsync(u => u.Id == user.Id);
            Assert.AreEqual(itemIdx - 2, upgradedItem.Rank);
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
            Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(new UpgradeItemCommand
            {
                ItemId = _items[itemIdx].Id,
                UserId = user.Entity.Id,
            }, CancellationToken.None));
        }

        [Test]
        public async Task ShouldReplaceCharacterItemWithUpgradeOne([Values(0, 1, 2, 3, 4, 5)] int itemIdx)
        {
            var user = new User
            {
                Gold = 1000,
                HeirloomPoints = 3,
                OwnedItems = new List<UserItem> { new UserItem { ItemId = _items[itemIdx].Id } },
                Characters = new List<Character>
                {
                    new Character { Items = new CharacterItems { Weapon1ItemId = _items[itemIdx].Id } },
                    new Character { Items = new CharacterItems { Weapon2ItemId = _items[itemIdx].Id } },
                },
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var upgradedItem = await new UpgradeItemCommand.Handler(ActDb, Mapper).Handle(new UpgradeItemCommand
            {
                ItemId = _items[itemIdx].Id,
                UserId = user.Id,
            }, CancellationToken.None);

            user = await AssertDb.Users
                .Include(u => u.Characters)
                .FirstAsync(u => u.Id == user.Id);
            Assert.AreEqual(upgradedItem.Id, user.Characters[0].Items.Weapon1ItemId);
            Assert.AreEqual(upgradedItem.Id, user.Characters[1].Items.Weapon2ItemId);
        }

        [Test]
        public async Task ShouldThrowOnInvalidItemRank()
        {
            var user = ArrangeDb.Users.Add(new User { OwnedItems = new List<UserItem> { new UserItem { ItemId = _items[6].Id } } });
            await ArrangeDb.SaveChangesAsync();

            Assert.ThrowsAsync<BadRequestException>(() => new UpgradeItemCommand.Handler(ActDb, Mapper).Handle(
                new UpgradeItemCommand
                {
                    ItemId = _items[6].Id,
                    UserId = user.Entity.Id,
                }, CancellationToken.None));
        }

        [Test]
        public async Task ShouldThrowIfUserDoesntOwnItem()
        {
            var user = ArrangeDb.Users.Add(new User());
            var item = ArrangeDb.Items.Add(new Item());
            await ArrangeDb.SaveChangesAsync();

            Assert.ThrowsAsync<NotFoundException>(() => new UpgradeItemCommand.Handler(ActDb, Mapper).Handle(
                new UpgradeItemCommand
                {
                    ItemId = item.Entity.Id,
                    UserId = user.Entity.Id,
                }, CancellationToken.None));
        }

        [Test]
        public async Task ShouldThrowIfItemDoesntExist()
        {
            var user = ArrangeDb.Users.Add(new User());
            await ArrangeDb.SaveChangesAsync();

            Assert.ThrowsAsync<NotFoundException>(() => new UpgradeItemCommand.Handler(ActDb, Mapper).Handle(
                new UpgradeItemCommand
                {
                    ItemId = 1,
                    UserId = user.Entity.Id,
                }, CancellationToken.None));
        }

        [Test]
        public async Task ShouldThrowIfUserDoesntExist()
        {
            var item = ArrangeDb.Items.Add(new Item());
            await ArrangeDb.SaveChangesAsync();

            Assert.ThrowsAsync<NotFoundException>(() => new UpgradeItemCommand.Handler(ActDb, Mapper).Handle(
                new UpgradeItemCommand
                {
                    ItemId = item.Entity.Id,
                    UserId = 1,
                }, CancellationToken.None));
        }
    }
}