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

        var boughtUserItem = result.Data!;
        Assert.That(boughtUserItem.Item.Rank, Is.EqualTo(1));
        Assert.That(boughtUserItem.Item.BaseId, Is.EqualTo(item0.BaseId));
        Assert.That(userDb.Gold, Is.EqualTo(100));
        Assert.That(userDb.Items, Has.Some.Matches<UserItem>(ui => ui.Id == boughtUserItem.Id));
    }

    [Test]
    public async Task BannerCannotBeUpgraded()
    {
        Item item0 = new() { Id = "a_h0", BaseId = "a", Price = 100, Enabled = true, Rank = 0, Type = ItemType.Banner };
        User user = new()
        {
            Gold = 100,
            Items = { new UserItem { Item = item0 } },
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
    public async Task AlreadyOwnedHeirloom()
    {
        Item item0 = new() { Id = "a_h0", BaseId = "a", Price = 100, Enabled = true, Rank = 0 };
        Item item1 = new() { Id = "a_h1", BaseId = "a", Price = 100, Enabled = true, Rank = 1 };
        UserItem userItem0 = new()
        { Item = item0 };
        UserItem userItem1 = new()
        { Item = item1 };

        User user = new()
        {
            Gold = 100,
            Items = { userItem0, userItem1 },
        };
        ArrangeDb.Users.Add(user);
        ArrangeDb.Items.Add(item0);
        ArrangeDb.Items.Add(item1);
        ArrangeDb.UserItems.Add(userItem0);
        ArrangeDb.UserItems.Add(userItem1);
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
}
