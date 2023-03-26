using Crpg.Application.Common;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Commands;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.Items;

public class UpgradeUserItemCommandTest : TestBase
{
    private static readonly Constants Constants = new()
    {
        ItemRepairCostPerSecond = 0.0001f,
        BrokenItemRepairPenaltySeconds = 100,
    };

    [Test]
    public async Task ItemNotFound()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        UpgradeUserItemCommand.Handler handler = new(ActDb, Mapper, Constants);
        var result = await handler.Handle(new UpgradeUserItemCommand
        {
            UserItemId = 1,
            UserId = user.Id,
        }, CancellationToken.None);

        Assert.IsNotNull(result.Errors);
        Assert.AreEqual(ErrorCode.UserItemNotFound, result.Errors![0].Code);
    }

    [Test]
    public async Task NotEnoughGoldToRepair()
    {
        UserItem userItem = new() { Rank = -1, BaseItem = new Item { Id = "0", Price = 33333 } };
        User user = new()
        {
            Gold = 100,
            Items = { userItem },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        UpgradeUserItemCommand.Handler handler = new(ActDb, Mapper, Constants);
        var result = await handler.Handle(new UpgradeUserItemCommand
        {
            UserItemId = userItem.Id,
            UserId = user.Id,
        }, CancellationToken.None);

        Assert.IsNotNull(result.Errors);
        Assert.AreEqual(ErrorCode.NotEnoughGold, result.Errors![0].Code);
    }

    [Test]
    public async Task Repair()
    {
        UserItem userItem = new() { Rank = -1, BaseItem = new Item { Id = "0", Price = 33333 } };
        User user = new()
        {
            Gold = 1000,
            Items = { userItem },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        UpgradeUserItemCommand.Handler handler = new(ActDb, Mapper, Constants);
        var result = await handler.Handle(new UpgradeUserItemCommand
        {
            UserItemId = userItem.Id,
            UserId = user.Id,
        }, CancellationToken.None);

        Assert.IsNull(result.Errors);
        Assert.AreEqual(0, result.Data!.Rank);

        var userDb = await AssertDb.Users.FirstAsync(u => u.Id == user.Id);
        Assert.AreEqual(667, userDb.Gold);
    }
}
