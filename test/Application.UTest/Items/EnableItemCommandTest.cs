using Crpg.Application.Common.Results;
using Crpg.Application.Items.Commands;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Crpg.Application.UTest.Items;

public class EnableItemCommandTest : TestBase
{
    [Test]
    public async Task ShouldReturnErrorIfItemIsNotFound()
    {
        User user = new();
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        var result = await new EnableItemCommand.Handler(ActDb).Handle(new EnableItemCommand
        {
            ItemId = "a",
            Enable = true,
            UserId = user.Id,
        }, CancellationToken.None);
        Assert.AreEqual(ErrorCode.ItemNotFound, result.Errors![0].Code);
    }

    [Test]
    public async Task ShouldEnableItem()
    {
        Item item = new() { Id = "a", Enabled = false };
        ArrangeDb.Items.Add(item);
        User user = new();
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        var result = await new EnableItemCommand.Handler(ActDb).Handle(new EnableItemCommand
        {
            ItemId = item.Id,
            Enable = true,
            UserId = user.Id,
        }, CancellationToken.None);

        Assert.IsNull(result.Errors);
        var dbItem = AssertDb.Items.First(i => i.Id == "a");
        Assert.IsTrue(dbItem.Enabled);
    }

    [Test]
    public async Task ShouldDisableItem()
    {
        Item item = new() { Id = "a", Enabled = false };
        ArrangeDb.Items.Add(item);

        User user0 = new();
        UserItem user0Item = new() { BaseItem = item };
        user0.Items.Add(user0Item);
        Character user0Character = new()
        {
            EquippedItems =
            {
                new EquippedItem { UserItem = user0Item },
            },
        };
        user0.Characters.Add(user0Character);

        User user1 = new();
        UserItem user1Item = new() { BaseItem = item };
        user1.Items.Add(user1Item);
        Character user1Character = new()
        {
            EquippedItems =
            {
                new EquippedItem { Slot = ItemSlot.Head, UserItem = user1Item },
                new EquippedItem { Slot = ItemSlot.Body, UserItem = new UserItem { BaseItem = new Item { Id = "b" } } },
            },
        };
        user1.Characters.Add(user1Character);
        ArrangeDb.Users.AddRange(user0, user1);
        await ArrangeDb.SaveChangesAsync();

        var result = await new EnableItemCommand.Handler(ActDb).Handle(new EnableItemCommand
        {
            ItemId = item.Id,
            Enable = false,
            UserId = user0.Id,
        }, CancellationToken.None);

        Assert.IsNull(result.Errors);
        var dbItem = AssertDb.Items.First(i => i.Id == "a");
        Assert.IsFalse(dbItem.Enabled);

        var equippedItems = await AssertDb.EquippedItems.Include(ei => ei.UserItem).ToArrayAsync();
        Assert.AreEqual(1, equippedItems.Length);
        Assert.AreEqual("b", equippedItems[0].UserItem!.BaseItemId);
    }
}
