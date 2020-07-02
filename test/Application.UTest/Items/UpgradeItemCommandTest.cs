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
    public class UpgradeItemCommandTest : TestBase
    {
        private Item[] _items = default!;

        [SetUp]
        public override async Task SetUp()
        {
            await base.SetUp();

            _items = Enumerable.Range(-3, 7).Select(r => new Item { Rank = r, Value = (r + 4) * 100 }).ToArray();
            Db.Items.AddRange(_items);
            await Db.SaveChangesAsync();
        }

        [Test]
        public async Task ShouldRepairIfRankLessThanZero([Values(0, 1, 2)] int itemIdx)
        {
            var user = Db.Users.Add(new User
            {
                Gold = 1000,
                UserItems = new List<UserItem> { new UserItem { ItemId = _items[itemIdx].Id } }
            });
            await Db.SaveChangesAsync();

            var upgradedItem = await new UpgradeItemCommand.Handler(Db, Mapper).Handle(new UpgradeItemCommand
            {
                ItemId = _items[itemIdx].Id,
                UserId = user.Entity.Id,
            }, CancellationToken.None);

            Assert.AreEqual(itemIdx - 2, upgradedItem.Rank);
            Assert.Less(user.Entity.Gold, 1000);
        }

        [Test]
        public async Task ShouldNotRepairIfRankLessThanZeroButNotEnoughGold([Values(0, 1, 2)] int itemIdx)
        {
            var user = Db.Users.Add(new User
            {
                Gold = 0,
                UserItems = new List<UserItem> { new UserItem { ItemId = _items[itemIdx].Id } }
            });
            await Db.SaveChangesAsync();

            var handler = new UpgradeItemCommand.Handler(Db, Mapper);
            Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(new UpgradeItemCommand
            {
                ItemId = _items[itemIdx].Id,
                UserId = user.Entity.Id,
            }, CancellationToken.None));
        }

        [Test]
        public async Task ShouldLoomIfRankGreatorOrEqualThanZero([Values(3, 4, 5)] int itemIdx)
        {
            var user = Db.Users.Add(new User
            {
                HeirloomPoints = 1,
                UserItems = new List<UserItem> { new UserItem { ItemId = _items[itemIdx].Id } }
            });
            await Db.SaveChangesAsync();

            var upgradedItem = await new UpgradeItemCommand.Handler(Db, Mapper).Handle(new UpgradeItemCommand
            {
                ItemId = _items[itemIdx].Id,
                UserId = user.Entity.Id,
            }, CancellationToken.None);

            Assert.AreEqual(itemIdx - 2, upgradedItem.Rank);
            Assert.AreEqual(0, user.Entity.HeirloomPoints);
        }

        [Test]
        public async Task ShouldNotLoomIfRankGreatorOrEqualThanZeroButNotEnoughLoomPoints([Values(3, 4, 5)] int itemIdx)
        {
            var user = Db.Users.Add(new User
            {
                HeirloomPoints = 0,
                UserItems = new List<UserItem> { new UserItem { ItemId = _items[itemIdx].Id } }
            });
            await Db.SaveChangesAsync();

            var handler = new UpgradeItemCommand.Handler(Db, Mapper);
            Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(new UpgradeItemCommand
            {
                ItemId = _items[itemIdx].Id,
                UserId = user.Entity.Id,
            }, CancellationToken.None));
        }

        [Test]
        public async Task ShouldReplaceCharacterItemWithUpgradeOne([Values(0, 1, 2, 3, 4, 5)] int itemIdx)
        {
            var user = Db.Users.Add(new User
            {
                Gold = 1000,
                HeirloomPoints = 3,
                UserItems = new List<UserItem> { new UserItem { ItemId = _items[itemIdx].Id } },
                Characters = new List<Character>
                {
                    new Character { Items = new CharacterItems { Weapon1ItemId = _items[itemIdx].Id } },
                    new Character { Items = new CharacterItems { Weapon2ItemId = _items[itemIdx].Id } },
                },
            });
            await Db.SaveChangesAsync();

            var upgradedItem = await new UpgradeItemCommand.Handler(Db, Mapper).Handle(new UpgradeItemCommand
            {
                ItemId = _items[itemIdx].Id,
                UserId = user.Entity.Id,
            }, CancellationToken.None);

            Assert.AreEqual(upgradedItem.Id, user.Entity.Characters[0].Items.Weapon1ItemId);
            Assert.AreEqual(upgradedItem.Id, user.Entity.Characters[1].Items.Weapon2ItemId);
        }

        [Test]
        public async Task ShouldThrowOnInvalidItemRank()
        {
            var user = Db.Users.Add(new User { UserItems = new List<UserItem> { new UserItem { ItemId = _items[6].Id } } });
            await Db.SaveChangesAsync();

            Assert.ThrowsAsync<BadRequestException>(() => new UpgradeItemCommand.Handler(Db, Mapper).Handle(
                new UpgradeItemCommand
                {
                    ItemId = _items[6].Id,
                    UserId = user.Entity.Id,
                }, CancellationToken.None));
        }

        [Test]
        public async Task ShouldThrowIfUserDoesntOwnItem()
        {
            var user = Db.Users.Add(new User());
            var item = Db.Items.Add(new Item());
            await Db.SaveChangesAsync();

            Assert.ThrowsAsync<NotFoundException>(() => new UpgradeItemCommand.Handler(Db, Mapper).Handle(
                new UpgradeItemCommand
                {
                    ItemId = item.Entity.Id,
                    UserId = user.Entity.Id,
                }, CancellationToken.None));
        }

        [Test]
        public async Task ShouldThrowIfItemDoesntExist()
        {
            var user = Db.Users.Add(new User());
            await Db.SaveChangesAsync();

            Assert.ThrowsAsync<NotFoundException>(() => new UpgradeItemCommand.Handler(Db, Mapper).Handle(
                new UpgradeItemCommand
                {
                    ItemId = 1,
                    UserId = user.Entity.Id,
                }, CancellationToken.None));
        }

        [Test]
        public async Task ShouldThrowIfUserDoesntExist()
        {
            var item = Db.Items.Add(new Item());
            await Db.SaveChangesAsync();

            Assert.ThrowsAsync<NotFoundException>(() => new UpgradeItemCommand.Handler(Db, Mapper).Handle(
                new UpgradeItemCommand
                {
                    ItemId = item.Entity.Id,
                    UserId = 1,
                }, CancellationToken.None));
        }
    }
}