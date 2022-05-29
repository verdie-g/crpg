using Crpg.Application.Common;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Commands;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.Items;

public class UpgradeItemCommandTest : TestBase
{
    private static readonly Constants Constants = new()
    {
        ItemRepairCostCoefs = new[] { 0.07f, 0 },
    };

    private Item[] _items = default!;

    [SetUp]
    public override async Task SetUp()
    {
        await base.SetUp();

        _items = Enumerable.Range(-3, 7).Select(r => new Item { Rank = r, Price = (r + 4) * 100 }).ToArray();
        ArrangeDb.Items.AddRange(_items);
        await ArrangeDb.SaveChangesAsync();
    }

    [Test]
    public async Task ShouldRepairIfRankLessThanZero([Values(0, 1, 2)] int itemIdx)
    {
        User user = new()
        {
            Gold = 1000,
            Items = new List<UserItem> { new() { ItemId = _items[itemIdx].Id } },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        var result = await new UpgradeItemCommand.Handler(ActDb, Mapper, Constants).Handle(new UpgradeItemCommand
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
            Items = new List<UserItem> { new() { ItemId = _items[itemIdx].Id } },
        });
        await ArrangeDb.SaveChangesAsync();

        UpgradeItemCommand.Handler handler = new(ActDb, Mapper, Constants);
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
        User user = new()
        {
            HeirloomPoints = 1,
            Items = new List<UserItem> { new() { ItemId = _items[itemIdx].Id } },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        var result = await new UpgradeItemCommand.Handler(ActDb, Mapper, Constants).Handle(new UpgradeItemCommand
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
            Items = new List<UserItem> { new() { ItemId = _items[itemIdx].Id } },
        });
        await ArrangeDb.SaveChangesAsync();

        UpgradeItemCommand.Handler handler = new(ActDb, Mapper, Constants);
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
        UserItem userItem = new() { ItemId = _items[itemIdx].Id };
        User user = new()
        {
            Gold = 1000,
            HeirloomPoints = 3,
            Items = new List<UserItem> { userItem },
            Characters = new List<Character>
            {
                new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.Weapon0 } } },
                new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.Weapon1 } } },
            },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        var upgradedItem = (await new UpgradeItemCommand.Handler(ActDb, Mapper, Constants).Handle(new UpgradeItemCommand
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

        var result = await new UpgradeItemCommand.Handler(ActDb, Mapper, Constants).Handle(
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

        var result = await new UpgradeItemCommand.Handler(ActDb, Mapper, Constants).Handle(
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

        var result = await new UpgradeItemCommand.Handler(ActDb, Mapper, Constants).Handle(
            new UpgradeItemCommand
            {
                ItemId = item.Entity.Id,
                UserId = 1,
            }, CancellationToken.None);
        Assert.AreEqual(ErrorCode.ItemNotOwned, result.Errors![0].Code);
    }
}
