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

        Item upgradedItem = new() { Id = "0", Price = 150 };
        Mock<IItemModifierService> itemModifierServiceMock = new();
        itemModifierServiceMock
            .Setup(m => m.ModifyItem(It.IsAny<Item>(), It.IsAny<int>()))
            .Returns(upgradedItem);

        Mock<IDateTime> dateTimeMock = new();
        dateTimeMock.Setup(dt => dt.UtcNow).Returns(new DateTime(2000, 01, 02));

        ItemService itemService = new(itemModifierServiceMock.Object, dateTimeMock.Object, Constants);
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
        Assert.AreEqual(recentlyBought ? 150 : 75, user.Gold);
        Assert.False(user.Items.Any(ui => ui.Id == user.Items[0].Id));
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

        Item upgradedItem = new() { Id = "0", Price = 150 };
        Mock<IItemModifierService> itemModifierServiceMock = new();
        itemModifierServiceMock
            .Setup(m => m.ModifyItem(It.IsAny<Item>(), It.IsAny<int>()))
            .Returns(upgradedItem);

        Mock<IDateTime> dateTimeMock = new();
        dateTimeMock.Setup(dt => dt.UtcNow).Returns(new DateTime(2000, 01, 01));

        ItemService itemService = new(itemModifierServiceMock.Object, dateTimeMock.Object, Constants);
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
        Assert.AreEqual(75, user.Gold);
        Assert.False(user.Items.Any(ui => ui.Id == userItem.Id));
        Assert.IsEmpty(user.Characters[0].EquippedItems);
        Assert.IsEmpty(user.Characters[1].EquippedItems);
        Assert.IsEmpty(user.Characters[2].EquippedItems);
        Assert.IsEmpty(user.Characters[3].EquippedItems);
        Assert.IsEmpty(user.Characters[4].EquippedItems);
        Assert.IsEmpty(user.Characters[5].EquippedItems);
        Assert.IsEmpty(user.Characters[6].EquippedItems);
        Assert.IsEmpty(user.Characters[7].EquippedItems);
        Assert.IsEmpty(user.Characters[8].EquippedItems);
        Assert.IsEmpty(user.Characters[9].EquippedItems);
        Assert.IsEmpty(user.Characters[10].EquippedItems);
        Assert.IsEmpty(user.Characters[11].EquippedItems);
    }
}
