using Crpg.Application.Common.Results;
using Crpg.Application.Items.Commands;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.Items;

public class BuyItemCommandTest : TestBase
{
    [Test]
    public async Task Basic()
    {
        Item item = new() { Id = "0", Price = 100 };
        User user = new()
        {
            Gold = 100,
            Items = { new UserItem { Rank = 1, BaseItem = item } },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        BuyItemCommand.Handler handler = new(ActDb, Mapper);
        var result = await handler.Handle(new BuyItemCommand
        {
            ItemId = item.Id,
            UserId = user.Id,
        }, CancellationToken.None);

        var userDb = await AssertDb.Users
            .Include(u => u.Items)
            .FirstAsync(u => u.Id == user.Id);

        var boughtUserItem = result.Data!;
        Assert.AreEqual(0, boughtUserItem.Rank);
        Assert.AreEqual(item.Id, boughtUserItem.BaseItem.Id);
        Assert.AreEqual(0, userDb.Gold);
        Assert.IsTrue(userDb.Items.Any(ui => ui.Id == boughtUserItem.Id));
    }

    [Test]
    public async Task NotFoundItem()
    {
        var user = ArrangeDb.Users.Add(new User { Gold = 100 });
        await ArrangeDb.SaveChangesAsync();

        BuyItemCommand.Handler handler = new(ActDb, Mapper);
        var result = await handler.Handle(new BuyItemCommand
        {
            ItemId = "1",
            UserId = user.Entity.Id,
        }, CancellationToken.None);

        Assert.AreEqual(ErrorCode.ItemNotFound, result.Errors![0].Code);
    }

    [Test]
    public async Task NotFoundUser()
    {
        var item = ArrangeDb.Items.Add(new Item { Price = 100 });
        await ArrangeDb.SaveChangesAsync();

        BuyItemCommand.Handler handler = new(ActDb, Mapper);
        var result = await handler.Handle(new BuyItemCommand
        {
            ItemId = item.Entity.Id,
            UserId = 1,
        }, CancellationToken.None);
        Assert.AreEqual(ErrorCode.UserNotFound, result.Errors![0].Code);
    }

    [Test]
    public async Task NotEnoughGold()
    {
        var user = ArrangeDb.Users.Add(new User { Gold = 100 });
        var item = ArrangeDb.Items.Add(new Item { Price = 101 });
        await ArrangeDb.SaveChangesAsync();

        BuyItemCommand.Handler handler = new(ActDb, Mapper);
        var result = await handler.Handle(new BuyItemCommand
        {
            ItemId = item.Entity.Id,
            UserId = user.Entity.Id,
        }, CancellationToken.None);
        Assert.AreEqual(ErrorCode.NotEnoughGold, result.Errors![0].Code);
    }

    [Test]
    public async Task AlreadyOwningItem()
    {
        Item item = new() { Price = 100 };
        User user = new()
        {
            Gold = 100,
            Items = new List<UserItem> { new() { Rank = 0, BaseItemId = item.Id } },
        };
        ArrangeDb.Items.Add(item);
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        BuyItemCommand.Handler handler = new(ActDb, Mapper);
        var result = await handler.Handle(new BuyItemCommand
        {
            ItemId = item.Id,
            UserId = user.Id,
        }, CancellationToken.None);
        Assert.AreEqual(ErrorCode.ItemAlreadyOwned, result.Errors![0].Code);
    }
}
