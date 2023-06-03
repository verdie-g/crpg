using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Items.Commands;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Items;

public class BuyItemCommandTest : TestBase
{
    [Test]
    public async Task Basic()
    {
        Item item = new() { Id = "0", Price = 100, Enabled = true };
        User user = new()
        {
            Gold = 100,
            Items = { new UserItem { Rank = 1, Item = item } },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };

        BuyItemCommand.Handler handler = new(ActDb, Mapper, activityLogServiceMock.Object);
        var result = await handler.Handle(new BuyItemCommand
        {
            ItemId = item.Id,
            UserId = user.Id,
        }, CancellationToken.None);

        var userDb = await AssertDb.Users
            .Include(u => u.Items)
            .FirstAsync(u => u.Id == user.Id);

        var boughtUserItem = result.Data!;
        Assert.That(boughtUserItem.Rank, Is.EqualTo(0));
        Assert.That(boughtUserItem.BaseItem.Id, Is.EqualTo(item.Id));
        Assert.That(userDb.Gold, Is.EqualTo(0));
        Assert.That(userDb.Items, Has.Some.Matches<UserItem>(ui => ui.Id == boughtUserItem.Id));
    }

    [Test]
    public async Task NotFoundItem()
    {
        var user = ArrangeDb.Users.Add(new User { Gold = 100 });
        await ArrangeDb.SaveChangesAsync();

        BuyItemCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IActivityLogService>());
        var result = await handler.Handle(new BuyItemCommand
        {
            ItemId = "1",
            UserId = user.Entity.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.ItemNotFound));
    }

    [Test]
    public async Task NotFoundUser()
    {
        var item = ArrangeDb.Items.Add(new Item { Price = 100, Enabled = true });
        await ArrangeDb.SaveChangesAsync();

        BuyItemCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IActivityLogService>());
        var result = await handler.Handle(new BuyItemCommand
        {
            ItemId = item.Entity.Id,
            UserId = 1,
        }, CancellationToken.None);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.UserNotFound));
    }

    [Test]
    public async Task DisabledItem()
    {
        var user = ArrangeDb.Users.Add(new User { Gold = 100 });
        var item = ArrangeDb.Items.Add(new Item { Price = 100, Enabled = false });
        await ArrangeDb.SaveChangesAsync();

        BuyItemCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IActivityLogService>());
        var result = await handler.Handle(new BuyItemCommand
        {
            ItemId = item.Entity.Id,
            UserId = user.Entity.Id,
        }, CancellationToken.None);

        Assert.That(result.Errors, Is.Not.Null);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.ItemDisabled));
    }

    [Theory]
    public async Task BannerItem(bool isDonor, Role role)
    {
        var user = ArrangeDb.Users.Add(new User { Gold = 100, Role = role, IsDonor = isDonor });
        var item = ArrangeDb.Items.Add(new Item { Type = ItemType.Banner, Price = 100, Enabled = true });
        await ArrangeDb.SaveChangesAsync();

        Mock<IActivityLogService> activityLogServiceMock = new() { DefaultValue = DefaultValue.Mock };
        BuyItemCommand.Handler handler = new(ActDb, Mapper, activityLogServiceMock.Object);
        var result = await handler.Handle(new BuyItemCommand
        {
            ItemId = item.Entity.Id,
            UserId = user.Entity.Id,
        }, CancellationToken.None);

        if (isDonor || role != Role.User)
        {
            Assert.That(result.Errors, Is.Null);
        }
        else
        {
            Assert.That(result.Errors, Is.Not.Null);
            Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.ItemNotBuyable));
        }
    }

    [Test]
    public async Task NotEnoughGold()
    {
        var user = ArrangeDb.Users.Add(new User { Gold = 100 });
        var item = ArrangeDb.Items.Add(new Item { Price = 101, Enabled = true });
        await ArrangeDb.SaveChangesAsync();

        BuyItemCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IActivityLogService>());
        var result = await handler.Handle(new BuyItemCommand
        {
            ItemId = item.Entity.Id,
            UserId = user.Entity.Id,
        }, CancellationToken.None);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.NotEnoughGold));
    }

    [Test]
    public async Task AlreadyOwningItem()
    {
        Item item = new() { Price = 100, Enabled = true };
        User user = new()
        {
            Gold = 100,
            Items = new List<UserItem> { new() { Rank = 0, ItemId = item.Id } },
        };
        ArrangeDb.Items.Add(item);
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        BuyItemCommand.Handler handler = new(ActDb, Mapper, Mock.Of<IActivityLogService>());
        var result = await handler.Handle(new BuyItemCommand
        {
            ItemId = item.Id,
            UserId = user.Id,
        }, CancellationToken.None);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.ItemAlreadyOwned));
    }
}
