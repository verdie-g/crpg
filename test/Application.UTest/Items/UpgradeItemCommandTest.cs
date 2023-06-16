using System.Threading;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Items.Commands;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Items;

public class UpgradeItemCommandTest : TestBase
{
    [Test]
    public async Task Basic()
    {
        Item item0 = new() { Id = "a_h0", BaseId = "a", Price = 100, Enabled = true, Rank = 0 };
        Item item1 = new() { Id = "a_h1", BaseId = "a", Price = 100, Enabled = true, Rank = 1 };
        User user = new()
        {
            Gold = 100,
            Items = { new UserItem { Item = item0 } },
            HeirloomPoints = 5,
        };
        ArrangeDb.Users.Add(user);
        ArrangeDb.Items.Add(item0);
        ArrangeDb.Items.Add(item1);
        await ArrangeDb.SaveChangesAsync();

        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

        UpgradeUserItemCommand.Handler handler = new(ActDb, Mapper, activityLogServiceMock.Object);
        var result = await handler.Handle(new UpgradeUserItemCommand
        {
            UserItemId = user.Items[0].Id,
            UserId = user.Id,
        }, CancellationToken.None);

        var userDb = await AssertDb.Users
            .Include(u => u.Items)
            .FirstAsync(u => u.Id == user.Id);

        var equippedItems = await AssertDb.EquippedItems
                .ToListAsync();

        var upgradedUserItem = result.Data!;
        Assert.That(upgradedUserItem.Item.Rank, Is.EqualTo(1));
        Assert.That(upgradedUserItem.Item.BaseId, Is.EqualTo(item0.BaseId));
        Assert.That(userDb.HeirloomPoints, Is.EqualTo(4));
        Assert.That(userDb.Items, Has.Some.Matches<UserItem>(ui => ui.Id == upgradedUserItem.Id));
        Assert.That(!equippedItems.Any(ei => ei.UserItemId == user.Items[0].Id));
    }

    [Test]
    public async Task CannotUpgradeNonExistingItem()
    {
        Item item0 = new() { Id = "a_h12", BaseId = "a", Price = 100, Enabled = true, Rank = 12 };
        User user = new()
        {
            Gold = 100,
            Items = { new() },
            HeirloomPoints = 10,
        };
        ArrangeDb.Users.Add(user);
        ArrangeDb.Items.Add(item0);
        await ArrangeDb.SaveChangesAsync();

        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

        UpgradeUserItemCommand.Handler handler = new(ActDb, Mapper, activityLogServiceMock.Object);
        var result = await handler.Handle(new UpgradeUserItemCommand
        {
            UserItemId = 15,
            UserId = user.Id,
        }, CancellationToken.None);

        var errorCode = result.Errors![0].Code;
        Assert.That(errorCode, Is.EqualTo(ErrorCode.UserItemNotFound));
    }

    [Test]
    public async Task NotFoundUser()
    {
        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };
        UpgradeUserItemCommand.Handler handler = new(ActDb, Mapper, activityLogServiceMock.Object);
        var result = await handler.Handle(new UpgradeUserItemCommand
        {
            UserItemId = 50,
            UserId = 1,
        }, CancellationToken.None);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotFound));
    }

    [Test]
    public async Task BannerCannotBeUpgraded()
    {
        Item item0 = new() { Id = "a_h0", BaseId = "a", Price = 100, Enabled = true, Rank = 0, Type = ItemType.Banner };
        User user = new()
        {
            Gold = 100,
            Items = { new UserItem { Item = item0 } },
            HeirloomPoints = 5,
        };

        ArrangeDb.Users.Add(user);
        ArrangeDb.Items.Add(item0);
        await ArrangeDb.SaveChangesAsync();

        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

        UpgradeUserItemCommand.Handler handler = new(ActDb, Mapper, activityLogServiceMock.Object);
        var result = await handler.Handle(new UpgradeUserItemCommand
        {
            UserItemId = user.Items[0].Id,
            UserId = user.Id,
        }, CancellationToken.None);

        var userDb = await AssertDb.Users
            .Include(u => u.Items)
            .FirstAsync(u => u.Id == user.Id);

        var errorCode = result.Errors![0].Code;
        Assert.That(errorCode, Is.EqualTo(ErrorCode.ItemNotUpgradable));
    }

    [Test]
    public async Task AlreadyOwnedHeirloom()
    {
        Item item0 = new() { Id = "a_h0", BaseId = "a", Price = 100, Enabled = true, Rank = 0 };
        Item item1 = new() { Id = "a_h1", BaseId = "a", Price = 100, Enabled = true, Rank = 1 };
        User user = new()
        {
            Gold = 100,
            Items = { new() { Item = item0 }, new() { Item = item1 } },
            HeirloomPoints = 5,
        };
        ArrangeDb.Users.Add(user);
        ArrangeDb.Items.Add(item0);
        ArrangeDb.Items.Add(item1);
        await ArrangeDb.SaveChangesAsync();

        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

        UpgradeUserItemCommand.Handler handler = new(ActDb, Mapper, activityLogServiceMock.Object);
        var result = await handler.Handle(new UpgradeUserItemCommand
        {
            UserItemId = user.Items[0].Id,
            UserId = user.Id,
        }, CancellationToken.None);

        var errorCode = result.Errors![0].Code;
        Assert.That(errorCode, Is.EqualTo(ErrorCode.ItemAlreadyOwned));
    }

    [Test]
    public async Task CannotUpgradeWithNoHeirloomPoints()
    {
        Item item0 = new() { Id = "a_h0", BaseId = "a", Price = 100, Enabled = true, Rank = 0 };
        Item item1 = new() { Id = "a_h1", BaseId = "a", Price = 100, Enabled = true, Rank = 1 };
        User user = new()
        {
            Gold = 100,
            Items = { new() { Item = item0 } },
            HeirloomPoints = 0,
        };
        ArrangeDb.Users.Add(user);
        ArrangeDb.Items.Add(item0);
        ArrangeDb.Items.Add(item1);
        await ArrangeDb.SaveChangesAsync();

        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

        UpgradeUserItemCommand.Handler handler = new(ActDb, Mapper, activityLogServiceMock.Object);
        var result = await handler.Handle(new UpgradeUserItemCommand
        {
            UserItemId = user.Items[0].Id,
            UserId = user.Id,
        }, CancellationToken.None);

        var errorCode = result.Errors![0].Code;
        Assert.That(errorCode, Is.EqualTo(ErrorCode.NotEnoughHeirloomPoints));
    }

    [Test]
    public async Task CannotUpgradeMaxRankItem()
    {
        Item item0 = new() { Id = "a_h12", BaseId = "a", Price = 100, Enabled = true, Rank = 12 };
        User user = new()
        {
            Gold = 100,
            Items = { new() { Item = item0 } },
            HeirloomPoints = 10,
        };
        ArrangeDb.Users.Add(user);
        ArrangeDb.Items.Add(item0);
        await ArrangeDb.SaveChangesAsync();

        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

        UpgradeUserItemCommand.Handler handler = new(ActDb, Mapper, activityLogServiceMock.Object);
        var result = await handler.Handle(new UpgradeUserItemCommand
        {
            UserItemId = user.Items[0].Id,
            UserId = user.Id,
        }, CancellationToken.None);

        var errorCode = result.Errors![0].Code;
        Assert.That(errorCode, Is.EqualTo(ErrorCode.UserItemMaxRankReached));
    }
}
