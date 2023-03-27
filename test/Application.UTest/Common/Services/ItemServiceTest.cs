using Crpg.Application.Common;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace Crpg.Application.UTest.Common.Services;

public class ItemServiceTest : TestBase
{
    private static readonly Constants Constants = new() { ItemSellCostCoefs = new[] { 0.5f, 0.0f } };

    [Theory]
    public async Task SellItemUnequipped(bool recentlyBought)
    {
        User user = new()
        {
            Gold = 0,
            Items = new List<UserItem>
            {
                new()
                {
                    BaseItem = new Item { Price = 100 },
                    CreatedAt = recentlyBought ? new DateTime(2000, 01, 02) : new DateTime(2000, 01, 01),
                },
            },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        Mock<IDateTime> dateTimeMock = new();
        dateTimeMock.Setup(dt => dt.UtcNow).Returns(new DateTime(2000, 01, 02));

        ItemService itemService = new(dateTimeMock.Object, Constants);
        var userItem = await ActDb.UserItems
            .Include(ui => ui.User)
            .Include(ui => ui.BaseItem)
            .Include(ui => ui.EquippedItems)
            .FirstAsync();
        itemService.SellUserItem(ActDb, userItem);
        await ActDb.SaveChangesAsync();

        user = await AssertDb.Users
            .Include(u => u.Items)
            .FirstAsync(u => u.Id == user.Id);
        Assert.That(recentlyBought ? 100 : 50, Is.EqualTo(user.Gold));
        Assert.That(user.Items, Has.None.Matches<UserItem>(ui => ui.Id == user.Items[0].Id));
    }

    [Test]
    public async Task SellItemEquipped()
    {
        Item item = new() { Id = "0", Price = 100 };
        UserItem userItem = new() { Rank = 1, BaseItem = item };
        List<Character> characters = new()
        {
            new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.Head } } },
            new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.Shoulder } } },
            new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.Body } } },
            new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.Hand } } },
            new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.Leg } } },
            new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.MountHarness } } },
            new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.Mount } } },
            new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.Weapon0 } } },
            new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.Weapon1 } } },
            new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.Weapon2 } } },
            new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.Weapon3 } } },
            new() { EquippedItems = { new EquippedItem { UserItem = userItem, Slot = ItemSlot.WeaponExtra } } },
        };
        User user = new()
        {
            Gold = 0,
            Items = { userItem },
            Characters = characters,
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        Mock<IDateTime> dateTimeMock = new();
        dateTimeMock.Setup(dt => dt.UtcNow).Returns(new DateTime(2000, 01, 01));

        ItemService itemService = new(dateTimeMock.Object, Constants);
        userItem = await ActDb.UserItems
            .Include(ui => ui.User)
            .Include(ui => ui.BaseItem)
            .Include(ui => ui.EquippedItems)
            .FirstAsync();
        itemService.SellUserItem(ActDb, userItem);
        await ActDb.SaveChangesAsync();

        user = await AssertDb.Users
            .Include(u => u.Characters)
            .Include(u => u.Items)
            .FirstAsync(u => u.Id == user.Id);
        Assert.That(user.Gold, Is.EqualTo(50));
        Assert.That(user.Items, Has.None.Matches<UserItem>(ui => ui.Id == userItem.Id));
        Assert.That(user.Characters[0].EquippedItems, Is.Empty);
        Assert.That(user.Characters[1].EquippedItems, Is.Empty);
        Assert.That(user.Characters[2].EquippedItems, Is.Empty);
        Assert.That(user.Characters[3].EquippedItems, Is.Empty);
        Assert.That(user.Characters[4].EquippedItems, Is.Empty);
        Assert.That(user.Characters[5].EquippedItems, Is.Empty);
        Assert.That(user.Characters[6].EquippedItems, Is.Empty);
        Assert.That(user.Characters[7].EquippedItems, Is.Empty);
        Assert.That(user.Characters[8].EquippedItems, Is.Empty);
        Assert.That(user.Characters[9].EquippedItems, Is.Empty);
        Assert.That(user.Characters[10].EquippedItems, Is.Empty);
        Assert.That(user.Characters[11].EquippedItems, Is.Empty);
    }
}
