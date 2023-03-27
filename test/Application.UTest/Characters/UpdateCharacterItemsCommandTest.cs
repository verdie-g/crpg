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

        UserItem headOld = new() { User = user, BaseItem = new Item { Id = "1", Type = ItemType.HeadArmor, Enabled = true } };
        UserItem headNew = new() { User = user, BaseItem = new Item { Id = "2", Type = ItemType.HeadArmor, Enabled = true } };
        UserItem shoulderOld = new() { User = user, BaseItem = new Item { Id = "3", Type = ItemType.ShoulderArmor, Enabled = true } };
        UserItem shoulderNew = new() { User = user, BaseItem = new Item { Id = "4", Type = ItemType.ShoulderArmor, Enabled = true } };
        UserItem bodyOld = new() { User = user, BaseItem = new Item { Id = "5", Type = ItemType.BodyArmor, Enabled = true } };
        UserItem bodyNew = new() { User = user, BaseItem = new Item { Id = "6", Type = ItemType.BodyArmor, Enabled = true } };
        UserItem handOld = new() { User = user, BaseItem = new Item { Id = "7", Type = ItemType.HandArmor, Enabled = true } };
        UserItem handNew = new() { User = user, BaseItem = new Item { Id = "8", Type = ItemType.HandArmor, Enabled = true } };
        UserItem legOld = new() { User = user, BaseItem = new Item { Id = "9", Type = ItemType.LegArmor, Enabled = true } };
        UserItem legNew = new() { User = user, BaseItem = new Item { Id = "10", Type = ItemType.LegArmor, Enabled = true } };
        UserItem mountHarnessOld = new() { User = user, BaseItem = new Item { Id = "11", Type = ItemType.MountHarness, Enabled = true } };
        UserItem mountHarnessNew = new() { User = user, BaseItem = new Item { Id = "12", Type = ItemType.MountHarness, Enabled = true } };
        UserItem mountOld = new() { User = user, BaseItem = new Item { Id = "13", Type = ItemType.Mount, Enabled = true } };
        UserItem mountNew = new() { User = user, BaseItem = new Item { Id = "14", Type = ItemType.Mount, Enabled = true } };
        UserItem weapon0Old = new() { User = user, BaseItem = new Item { Id = "15", Type = ItemType.Arrows, Enabled = true } };
        UserItem weapon0New = new() { User = user, BaseItem = new Item { Id = "16", Type = ItemType.Bolts, Enabled = true } };
        UserItem weapon1Old = new() { User = user, BaseItem = new Item { Id = "17", Type = ItemType.Bow, Enabled = true } };
        UserItem weapon1New = new() { User = user, BaseItem = new Item { Id = "18", Type = ItemType.Crossbow, Enabled = true } };
        UserItem weapon2Old = new() { User = user, BaseItem = new Item { Id = "19", Type = ItemType.Polearm, Enabled = true } };
        UserItem weapon2New = new() { User = user, BaseItem = new Item { Id = "20", Type = ItemType.Shield, Enabled = true } };
        UserItem weapon3Old = new() { User = user, BaseItem = new Item { Id = "21", Type = ItemType.OneHandedWeapon, Enabled = true } };
        UserItem weapon3New = new() { User = user, BaseItem = new Item { Id = "22", Type = ItemType.TwoHandedWeapon, Enabled = true } };
        UserItem weaponExtraOld = new() { User = user, BaseItem = new Item { Id = "23", Type = ItemType.Banner, Enabled = true } };
        UserItem weaponExtraNew = new() { User = user, BaseItem = new Item { Id = "24", Type = ItemType.Banner, Enabled = true } };

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
                new EquippedItem { UserItem = weaponExtraOld, Slot = ItemSlot.WeaponExtra },
            },
        };

        user.Characters.Add(character);
        ArrangeDb.Users.Add(user);
        ArrangeDb.UserItems.AddRange(headNew, shoulderNew, bodyNew, handNew, legNew, mountHarnessNew, mountNew,
            weapon0New, weapon1New, weapon2New, weapon3New, weaponExtraNew);
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
                new() { UserItemId = weaponExtraNew.Id, Slot = ItemSlot.WeaponExtra },
            },
        };
        var result = await handler.Handle(cmd, CancellationToken.None);
        Assert.That(result.Errors, Is.Null);

        var userItemIdBySlot = result.Data!.ToDictionary(i => i.Slot, ei => ei.UserItem.Id);
        Assert.That(userItemIdBySlot[ItemSlot.Head], Is.EqualTo(headNew.Id));
        Assert.That(userItemIdBySlot[ItemSlot.Shoulder], Is.EqualTo(shoulderNew.Id));
        Assert.That(userItemIdBySlot[ItemSlot.Body], Is.EqualTo(bodyNew.Id));
        Assert.That(userItemIdBySlot[ItemSlot.Hand], Is.EqualTo(handNew.Id));
        Assert.That(userItemIdBySlot[ItemSlot.Leg], Is.EqualTo(legNew.Id));
        Assert.That(userItemIdBySlot[ItemSlot.MountHarness], Is.EqualTo(mountHarnessNew.Id));
        Assert.That(userItemIdBySlot[ItemSlot.Mount], Is.EqualTo(mountNew.Id));
        Assert.That(userItemIdBySlot[ItemSlot.Weapon0], Is.EqualTo(weapon0New.Id));
        Assert.That(userItemIdBySlot[ItemSlot.Weapon1], Is.EqualTo(weapon1New.Id));
        Assert.That(userItemIdBySlot[ItemSlot.Weapon2], Is.EqualTo(weapon2New.Id));
        Assert.That(userItemIdBySlot[ItemSlot.Weapon3], Is.EqualTo(weapon3New.Id));
        Assert.That(userItemIdBySlot[ItemSlot.WeaponExtra], Is.EqualTo(weaponExtraNew.Id));
    }

    [Test]
    public async Task PartialUpdate()
    {
        User user = new();
        ArrangeDb.Users.Add(user);

        UserItem headOld = new() { User = user, BaseItem = new Item { Id = "1", Type = ItemType.HeadArmor, Enabled = true } };
        UserItem headNew = new() { User = user, BaseItem = new Item { Id = "2", Type = ItemType.HeadArmor, Enabled = true } };
        UserItem bodyNew = new() { User = user, BaseItem = new Item { Id = "3", Type = ItemType.BodyArmor, Enabled = true } };
        UserItem handOld = new() { User = user, BaseItem = new Item { Id = "4", Type = ItemType.HandArmor, Enabled = true } };
        UserItem legOld = new() { User = user, BaseItem = new Item { Id = "5", Type = ItemType.LegArmor, Enabled = true } };

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
        Assert.That(userItemIdBySlot[ItemSlot.Head], Is.EqualTo(headNew.Id));
        Assert.That(userItemIdBySlot, Does.Not.ContainKey(ItemSlot.Shoulder));
        Assert.That(userItemIdBySlot[ItemSlot.Body], Is.EqualTo(bodyNew.Id));
        Assert.That(userItemIdBySlot, Does.Not.ContainKey(ItemSlot.Shoulder));
        Assert.That(userItemIdBySlot, Does.Not.ContainKey(ItemSlot.Hand));
        Assert.That(userItemIdBySlot[ItemSlot.Leg], Is.EqualTo(legOld.Id));
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
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.CharacterNotFound));
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
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.CharacterNotFound));
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
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.CharacterNotFound));
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
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.UserItemNotFound));
    }

    [Test]
    public async Task ItemDisabled()
    {
        UserItem userItem = new() { BaseItem = new Item { Type = ItemType.HeadArmor, Enabled = false } };
        Character character = new();
        User user = new()
        {
            Characters = { character },
            Items = { userItem },
        };
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
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.ItemDisabled));
    }

    [Test]
    public async Task ItemBroken()
    {
        UserItem userItem = new() { Rank = -1, BaseItem = new Item { Type = ItemType.HeadArmor, Enabled = true } };
        Character character = new();
        User user = new()
        {
            Characters = { character },
            Items = { userItem },
        };
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
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.ItemBroken));
    }

    [Test]
    public async Task ItemNotOwned()
    {
        UserItem userItem = new() { BaseItem = new Item { Type = ItemType.HeadArmor, Enabled = true } };
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
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.UserItemNotFound));
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
        UserItem userItem = new() { BaseItem = new Item { Type = itemType, Enabled = true } };
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
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.ItemBadSlot));
    }

    [TestCase(ItemFlags.DropOnAnyAction)]
    [TestCase(ItemFlags.DropOnWeaponChange)]
    [TestCase(ItemFlags.DropOnWeaponChange | ItemFlags.DropOnAnyAction)]
    public async Task DropOnWeaponChangeItemOnWeaponSlot(ItemFlags itemFlags)
    {
        Character character = new();
        UserItem userItem = new()
        {
            BaseItem = new Item
            {
                Type = ItemType.Banner,
                Flags = itemFlags,
                Enabled = true,
            },
        };
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
            Items = new List<EquippedItemIdViewModel> { new() { UserItemId = userItem.Id, Slot = ItemSlot.Weapon0 } },
        };

        var result = await handler.Handle(cmd, CancellationToken.None);
        Assert.That(result.Errors![0].Code, Is.EqualTo(ErrorCode.ItemBadSlot));
    }
}
