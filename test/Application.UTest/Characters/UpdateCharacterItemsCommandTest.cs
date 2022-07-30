using Crpg.Application.Characters.Commands;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters;

public class UpdateCharacterItemsCommandTest : TestBase
{
    [Test]
    public async Task FullUpdate()
    {
        User user = new();

        UserItem headOld = new() { User = user, BaseItem = new Item { Id = "1", Type = ItemType.HeadArmor } };
        UserItem headNew = new() { User = user, BaseItem = new Item { Id = "2", Type = ItemType.HeadArmor } };
        UserItem shoulderOld = new() { User = user, BaseItem = new Item { Id = "3", Type = ItemType.ShoulderArmor } };
        UserItem shoulderNew = new() { User = user, BaseItem = new Item { Id = "4", Type = ItemType.ShoulderArmor } };
        UserItem bodyOld = new() { User = user, BaseItem = new Item { Id = "5", Type = ItemType.BodyArmor } };
        UserItem bodyNew = new() { User = user, BaseItem = new Item { Id = "6", Type = ItemType.BodyArmor } };
        UserItem handOld = new() { User = user, BaseItem = new Item { Id = "7", Type = ItemType.HandArmor } };
        UserItem handNew = new() { User = user, BaseItem = new Item { Id = "8", Type = ItemType.HandArmor } };
        UserItem legOld = new() { User = user, BaseItem = new Item { Id = "9", Type = ItemType.LegArmor } };
        UserItem legNew = new() { User = user, BaseItem = new Item { Id = "10", Type = ItemType.LegArmor } };
        UserItem mountHarnessOld = new() { User = user, BaseItem = new Item { Id = "11", Type = ItemType.MountHarness } };
        UserItem mountHarnessNew = new() { User = user, BaseItem = new Item { Id = "12", Type = ItemType.MountHarness } };
        UserItem mountOld = new() { User = user, BaseItem = new Item { Id = "13", Type = ItemType.Mount } };
        UserItem mountNew = new() { User = user, BaseItem = new Item { Id = "14", Type = ItemType.Mount } };
        UserItem weapon0Old = new() { User = user, BaseItem = new Item { Id = "15", Type = ItemType.Arrows } };
        UserItem weapon0New = new() { User = user, BaseItem = new Item { Id = "16", Type = ItemType.Bolts } };
        UserItem weapon1Old = new() { User = user, BaseItem = new Item { Id = "17", Type = ItemType.Bow } };
        UserItem weapon1New = new() { User = user, BaseItem = new Item { Id = "18", Type = ItemType.Crossbow } };
        UserItem weapon2Old = new() { User = user, BaseItem = new Item { Id = "19", Type = ItemType.Polearm } };
        UserItem weapon2New = new() { User = user, BaseItem = new Item { Id = "20", Type = ItemType.Shield } };
        UserItem weapon3Old = new() { User = user, BaseItem = new Item { Id = "21", Type = ItemType.OneHandedWeapon } };
        UserItem weapon3New = new() { User = user, BaseItem = new Item { Id = "22", Type = ItemType.TwoHandedWeapon } };

        Character character = new()
        {
            Name = "name",
            EquippedItems =
            {
                new EquippedItem { UserItem = headOld, Slot = ItemSlot.Head },
                new EquippedItem { UserItem = shoulderOld, Slot = ItemSlot.Shoulder },
                new EquippedItem { UserItem = bodyOld, Slot = ItemSlot.Body },
                new EquippedItem { UserItem = handOld, Slot = ItemSlot.Hand },
                new EquippedItem { UserItem = legOld, Slot = ItemSlot.Leg },
                new EquippedItem { UserItem = mountHarnessOld, Slot = ItemSlot.MountHarness },
                new EquippedItem { UserItem = mountOld, Slot = ItemSlot.Mount },
                new EquippedItem { UserItem = weapon0Old, Slot = ItemSlot.Weapon0 },
                new EquippedItem { UserItem = weapon1Old, Slot = ItemSlot.Weapon1 },
                new EquippedItem { UserItem = weapon2Old, Slot = ItemSlot.Weapon2 },
                new EquippedItem { UserItem = weapon3Old, Slot = ItemSlot.Weapon3 },
            },
        };

        user.Characters.Add(character);
        ArrangeDb.Users.Add(user);
        ArrangeDb.UserItems.AddRange(headNew, shoulderNew, bodyNew, handNew, legNew, mountHarnessNew, mountNew,
            weapon0New, weapon1New, weapon2New, weapon3New);
        await ArrangeDb.SaveChangesAsync();

        UpdateCharacterItemsCommand.Handler handler = new(ActDb, Mapper);
        UpdateCharacterItemsCommand cmd = new()
        {
            CharacterId = character.Id,
            UserId = user.Id,
            Items = new List<EquippedItemIdViewModel>
            {
                new() { UserItemId = headNew.Id, Slot = ItemSlot.Head },
                new() { UserItemId = shoulderNew.Id, Slot = ItemSlot.Shoulder },
                new() { UserItemId = bodyNew.Id, Slot = ItemSlot.Body },
                new() { UserItemId = handNew.Id, Slot = ItemSlot.Hand },
                new() { UserItemId = legNew.Id, Slot = ItemSlot.Leg },
                new() { UserItemId = mountHarnessNew.Id, Slot = ItemSlot.MountHarness },
                new() { UserItemId = mountNew.Id, Slot = ItemSlot.Mount },
                new() { UserItemId = weapon0New.Id, Slot = ItemSlot.Weapon0 },
                new() { UserItemId = weapon1New.Id, Slot = ItemSlot.Weapon1 },
                new() { UserItemId = weapon2New.Id, Slot = ItemSlot.Weapon2 },
                new() { UserItemId = weapon3New.Id, Slot = ItemSlot.Weapon3 },
            },
        };
        var result = await handler.Handle(cmd, CancellationToken.None);

        var userItemIdBySlot = result.Data!.ToDictionary(i => i.Slot, ei => ei.UserItem.Id);
        Assert.AreEqual(headNew.Id, userItemIdBySlot[ItemSlot.Head]);
        Assert.AreEqual(shoulderNew.Id, userItemIdBySlot[ItemSlot.Shoulder]);
        Assert.AreEqual(bodyNew.Id, userItemIdBySlot[ItemSlot.Body]);
        Assert.AreEqual(handNew.Id, userItemIdBySlot[ItemSlot.Hand]);
        Assert.AreEqual(legNew.Id, userItemIdBySlot[ItemSlot.Leg]);
        Assert.AreEqual(mountHarnessNew.Id, userItemIdBySlot[ItemSlot.MountHarness]);
        Assert.AreEqual(mountNew.Id, userItemIdBySlot[ItemSlot.Mount]);
        Assert.AreEqual(weapon0New.Id, userItemIdBySlot[ItemSlot.Weapon0]);
        Assert.AreEqual(weapon1New.Id, userItemIdBySlot[ItemSlot.Weapon1]);
        Assert.AreEqual(weapon2New.Id, userItemIdBySlot[ItemSlot.Weapon2]);
        Assert.AreEqual(weapon3New.Id, userItemIdBySlot[ItemSlot.Weapon3]);
    }

    [Test]
    public async Task PartialUpdate()
    {
        User user = new();
        ArrangeDb.Users.Add(user);

        UserItem headOld = new() { User = user, BaseItem = new Item { Id = "1", Type = ItemType.HeadArmor } };
        UserItem headNew = new() { User = user, BaseItem = new Item { Id = "2", Type = ItemType.HeadArmor } };
        UserItem bodyNew = new() { User = user, BaseItem = new Item { Id = "3", Type = ItemType.BodyArmor } };
        UserItem handOld = new() { User = user, BaseItem = new Item { Id = "4", Type = ItemType.HandArmor } };
        UserItem legOld = new() { User = user, BaseItem = new Item { Id = "5", Type = ItemType.LegArmor } };

        Character character = new()
        {
            Name = "name",
            EquippedItems =
            {
                new EquippedItem { UserItem = headOld, Slot = ItemSlot.Head },
                new EquippedItem { UserItem = handOld, Slot = ItemSlot.Hand },
                new EquippedItem { UserItem = legOld, Slot = ItemSlot.Leg },
            },
        };

        user.Characters.Add(character);
        ArrangeDb.UserItems.AddRange(headNew, bodyNew);
        await ArrangeDb.SaveChangesAsync();

        UpdateCharacterItemsCommand.Handler handler = new(ActDb, Mapper);
        UpdateCharacterItemsCommand cmd = new()
        {
            CharacterId = character.Id,
            UserId = user.Id,
            Items = new List<EquippedItemIdViewModel>
            {
                new() { UserItemId = headNew.Id, Slot = ItemSlot.Head },
                new() { UserItemId = bodyNew.Id, Slot = ItemSlot.Body },
                new() { UserItemId = null, Slot = ItemSlot.Hand },
            },
        };
        var result = await handler.Handle(cmd, CancellationToken.None);

        var userItemIdBySlot = result.Data!.ToDictionary(i => i.Slot, i => i.UserItem.Id);
        Assert.AreEqual(headNew.Id, userItemIdBySlot[ItemSlot.Head]);
        Assert.That(userItemIdBySlot, Does.Not.ContainKey(ItemSlot.Shoulder));
        Assert.AreEqual(bodyNew.Id, userItemIdBySlot[ItemSlot.Body]);
        Assert.That(userItemIdBySlot, Does.Not.ContainKey(ItemSlot.Shoulder));
        Assert.That(userItemIdBySlot, Does.Not.ContainKey(ItemSlot.Hand));
        Assert.AreEqual(legOld.Id, userItemIdBySlot[ItemSlot.Leg]);
        Assert.That(userItemIdBySlot, Does.Not.ContainKey(ItemSlot.MountHarness));
        Assert.That(userItemIdBySlot, Does.Not.ContainKey(ItemSlot.Mount));
        Assert.That(userItemIdBySlot, Does.Not.ContainKey(ItemSlot.Weapon0));
        Assert.That(userItemIdBySlot, Does.Not.ContainKey(ItemSlot.Weapon1));
        Assert.That(userItemIdBySlot, Does.Not.ContainKey(ItemSlot.Weapon2));
        Assert.That(userItemIdBySlot, Does.Not.ContainKey(ItemSlot.Weapon3));
    }

    [Test]
    public async Task CharacterNotFound()
    {
        var user = ArrangeDb.Users.Add(new User());
        await ArrangeDb.SaveChangesAsync();

        UpdateCharacterItemsCommand.Handler handler = new(ActDb, Mapper);
        UpdateCharacterItemsCommand cmd = new()
        {
            CharacterId = 1,
            UserId = user.Entity.Id,
        };

        var result = await handler.Handle(cmd, CancellationToken.None);
        Assert.AreEqual(ErrorCode.CharacterNotFound, result.Errors![0].Code);
    }

    [Test]
    public async Task CharacterNotOwned()
    {
        var character = ArrangeDb.Characters.Add(new Character());
        var user = ArrangeDb.Users.Add(new User());
        await ArrangeDb.SaveChangesAsync();

        UpdateCharacterItemsCommand.Handler handler = new(ActDb, Mapper);
        UpdateCharacterItemsCommand cmd = new()
        {
            CharacterId = character.Entity.Id,
            UserId = user.Entity.Id,
        };

        var result = await handler.Handle(cmd, CancellationToken.None);
        Assert.AreEqual(ErrorCode.CharacterNotFound, result.Errors![0].Code);
    }

    [Test]
    public async Task UserNotFound()
    {
        var character = ArrangeDb.Characters.Add(new Character());
        await ArrangeDb.SaveChangesAsync();

        UpdateCharacterItemsCommand.Handler handler = new(ActDb, Mapper);
        UpdateCharacterItemsCommand cmd = new()
        {
            CharacterId = character.Entity.Id,
            UserId = 1,
        };

        var result = await handler.Handle(cmd, CancellationToken.None);
        Assert.AreEqual(ErrorCode.CharacterNotFound, result.Errors![0].Code);
    }

    [Test]
    public async Task ItemNotFound()
    {
        var character = ArrangeDb.Characters.Add(new Character());
        var user = ArrangeDb.Users.Add(new User
        {
            Characters = new List<Character> { character.Entity },
        });
        await ArrangeDb.SaveChangesAsync();

        UpdateCharacterItemsCommand.Handler handler = new(ActDb, Mapper);
        UpdateCharacterItemsCommand cmd = new()
        {
            CharacterId = character.Entity.Id,
            UserId = user.Entity.Id,
            Items = new List<EquippedItemIdViewModel> { new() { UserItemId = 1, Slot = ItemSlot.Head } },
        };

        var result = await handler.Handle(cmd, CancellationToken.None);
        Assert.AreEqual(ErrorCode.UserItemNotFound, result.Errors![0].Code);
    }

    [Test]
    public async Task ItemNotOwned()
    {
        UserItem userItem = new() { BaseItem = new Item { Type = ItemType.HeadArmor } };
        Character character = new();
        User user = new() { Characters = { character } };
        ArrangeDb.UserItems.Add(userItem);
        ArrangeDb.Characters.Add(character);
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        UpdateCharacterItemsCommand.Handler handler = new(ActDb, Mapper);
        UpdateCharacterItemsCommand cmd = new()
        {
            CharacterId = character.Id,
            UserId = user.Id,
            Items = new List<EquippedItemIdViewModel> { new() { UserItemId = userItem.Id, Slot = ItemSlot.Head } },
        };

        var result = await handler.Handle(cmd, CancellationToken.None);
        Assert.AreEqual(ErrorCode.UserItemNotFound, result.Errors![0].Code);
    }

    [TestCase(ItemType.HeadArmor, ItemSlot.Shoulder)]
    [TestCase(ItemType.ShoulderArmor, ItemSlot.Body)]
    [TestCase(ItemType.BodyArmor, ItemSlot.Hand)]
    [TestCase(ItemType.HandArmor, ItemSlot.Leg)]
    [TestCase(ItemType.LegArmor, ItemSlot.MountHarness)]
    [TestCase(ItemType.MountHarness, ItemSlot.Mount)]
    [TestCase(ItemType.Mount, ItemSlot.Weapon0)]
    [TestCase(ItemType.Shield, ItemSlot.Head)]
    [TestCase(ItemType.Bow, ItemSlot.Shoulder)]
    [TestCase(ItemType.Crossbow, ItemSlot.Body)]
    [TestCase(ItemType.OneHandedWeapon, ItemSlot.Hand)]
    [TestCase(ItemType.TwoHandedWeapon, ItemSlot.Leg)]
    [TestCase(ItemType.Polearm, ItemSlot.MountHarness)]
    [TestCase(ItemType.Thrown, ItemSlot.Mount)]
    [TestCase(ItemType.Arrows, ItemSlot.Head)]
    [TestCase(ItemType.Bolts, ItemSlot.Shoulder)]
    [TestCase(ItemType.Pistol, ItemSlot.Body)]
    [TestCase(ItemType.Musket, ItemSlot.Hand)]
    [TestCase(ItemType.Bullets, ItemSlot.Leg)]
    [TestCase(ItemType.Banner, ItemSlot.MountHarness)]
    public async Task WrongSlotForItemType(ItemType itemType, ItemSlot itemSlot)
    {
        Character character = new();
        UserItem userItem = new() { BaseItem = new Item { Type = itemType } };
        User user = new()
        {
            Items = { userItem },
            Characters = { character },
        };
        ArrangeDb.Users.Add(user);
        await ArrangeDb.SaveChangesAsync();

        UpdateCharacterItemsCommand.Handler handler = new(ActDb, Mapper);
        UpdateCharacterItemsCommand cmd = new()
        {
            CharacterId = character.Id,
            UserId = user.Id,
            Items = new List<EquippedItemIdViewModel> { new() { UserItemId = userItem.Id, Slot = itemSlot } },
        };

        var result = await handler.Handle(cmd, CancellationToken.None);
        Assert.AreEqual(ErrorCode.ItemBadSlot, result.Errors![0].Code);
    }
}
