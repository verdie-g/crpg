using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Characters.Commands;
using Crpg.Application.Common.Results;
using Crpg.Application.Items.Models;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters
{
    public class UpdateCharacterItemsCommandTest : TestBase
    {
        [Test]
        public async Task FullUpdate()
        {
            var user = new User();

            var headOld = new UserItem { User = user, Item = new Item { Type = ItemType.HeadArmor } };
            var headNew = new UserItem { User = user, Item = new Item { Type = ItemType.HeadArmor } };
            var shoulderOld = new UserItem { User = user, Item = new Item { Type = ItemType.ShoulderArmor } };
            var shoulderNew = new UserItem { User = user, Item = new Item { Type = ItemType.ShoulderArmor } };
            var bodyOld = new UserItem { User = user, Item = new Item { Type = ItemType.BodyArmor } };
            var bodyNew = new UserItem { User = user, Item = new Item { Type = ItemType.BodyArmor } };
            var handOld = new UserItem { User = user, Item = new Item { Type = ItemType.HandArmor } };
            var handNew = new UserItem { User = user, Item = new Item { Type = ItemType.HandArmor } };
            var legOld = new UserItem { User = user, Item = new Item { Type = ItemType.LegArmor } };
            var legNew = new UserItem { User = user, Item = new Item { Type = ItemType.LegArmor } };
            var mountHarnessOld = new UserItem { User = user, Item = new Item { Type = ItemType.MountHarness } };
            var mountHarnessNew = new UserItem { User = user, Item = new Item { Type = ItemType.MountHarness } };
            var mountOld = new UserItem { User = user, Item = new Item { Type = ItemType.Mount } };
            var mountNew = new UserItem { User = user, Item = new Item { Type = ItemType.Mount } };
            var weapon0Old = new UserItem { User = user, Item = new Item { Type = ItemType.Arrows } };
            var weapon0New = new UserItem { User = user, Item = new Item { Type = ItemType.Bolts } };
            var weapon1Old = new UserItem { User = user, Item = new Item { Type = ItemType.Bow } };
            var weapon1New = new UserItem { User = user, Item = new Item { Type = ItemType.Crossbow } };
            var weapon2Old = new UserItem { User = user, Item = new Item { Type = ItemType.Polearm } };
            var weapon2New = new UserItem { User = user, Item = new Item { Type = ItemType.Shield } };
            var weapon3Old = new UserItem { User = user, Item = new Item { Type = ItemType.OneHandedWeapon } };
            var weapon3New = new UserItem { User = user, Item = new Item { Type = ItemType.TwoHandedWeapon } };

            var character = new Character
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
            var cmd = new UpdateCharacterItemsCommand
            {
                CharacterId = character.Id,
                UserId = user.Id,
                Items = new List<EquippedItemIdViewModel>
                {
                    new() { ItemId = headNew.ItemId, Slot = ItemSlot.Head },
                    new() { ItemId = shoulderNew.ItemId, Slot = ItemSlot.Shoulder },
                    new() { ItemId = bodyNew.ItemId, Slot = ItemSlot.Body },
                    new() { ItemId = handNew.ItemId, Slot = ItemSlot.Hand },
                    new() { ItemId = legNew.ItemId, Slot = ItemSlot.Leg },
                    new() { ItemId = mountHarnessNew.ItemId, Slot = ItemSlot.MountHarness },
                    new() { ItemId = mountNew.ItemId, Slot = ItemSlot.Mount },
                    new() { ItemId = weapon0New.ItemId, Slot = ItemSlot.Weapon0 },
                    new() { ItemId = weapon1New.ItemId, Slot = ItemSlot.Weapon1 },
                    new() { ItemId = weapon2New.ItemId, Slot = ItemSlot.Weapon2 },
                    new() { ItemId = weapon3New.ItemId, Slot = ItemSlot.Weapon3 },
                },
            };
            var result = await handler.Handle(cmd, CancellationToken.None);

            var itemIdBySlot = result.Data!.ToDictionary(i => i.Slot, i => i.Item.Id);
            Assert.AreEqual(headNew.ItemId, itemIdBySlot[ItemSlot.Head]);
            Assert.AreEqual(shoulderNew.ItemId, itemIdBySlot[ItemSlot.Shoulder]);
            Assert.AreEqual(bodyNew.ItemId, itemIdBySlot[ItemSlot.Body]);
            Assert.AreEqual(handNew.ItemId, itemIdBySlot[ItemSlot.Hand]);
            Assert.AreEqual(legNew.ItemId, itemIdBySlot[ItemSlot.Leg]);
            Assert.AreEqual(mountHarnessNew.ItemId, itemIdBySlot[ItemSlot.MountHarness]);
            Assert.AreEqual(mountNew.ItemId, itemIdBySlot[ItemSlot.Mount]);
            Assert.AreEqual(weapon0New.ItemId, itemIdBySlot[ItemSlot.Weapon0]);
            Assert.AreEqual(weapon1New.ItemId, itemIdBySlot[ItemSlot.Weapon1]);
            Assert.AreEqual(weapon2New.ItemId, itemIdBySlot[ItemSlot.Weapon2]);
            Assert.AreEqual(weapon3New.ItemId, itemIdBySlot[ItemSlot.Weapon3]);
        }

        [Test]
        public async Task PartialUpdate()
        {
            var user = new User();
            ArrangeDb.Users.Add(user);

            var headOld = new UserItem { User = user, Item = new Item { Type = ItemType.HeadArmor } };
            var headNew = new UserItem { User = user, Item = new Item { Type = ItemType.HeadArmor } };
            var bodyNew = new UserItem { User = user, Item = new Item { Type = ItemType.BodyArmor } };
            var handOld = new UserItem { User = user, Item = new Item { Type = ItemType.HandArmor } };
            var legOld = new UserItem { User = user, Item = new Item { Type = ItemType.LegArmor } };

            var character = new Character
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
            var cmd = new UpdateCharacterItemsCommand
            {
                CharacterId = character.Id,
                UserId = user.Id,
                Items = new List<EquippedItemIdViewModel>
                {
                    new() { ItemId = headNew.ItemId, Slot = ItemSlot.Head },
                    new() { ItemId = bodyNew.ItemId, Slot = ItemSlot.Body },
                    new() { ItemId = null, Slot = ItemSlot.Hand },
                },
            };
            var result = await handler.Handle(cmd, CancellationToken.None);

            var itemIdBySlot = result.Data!.ToDictionary(i => i.Slot, i => i.Item.Id);
            Assert.AreEqual(headNew.ItemId, itemIdBySlot[ItemSlot.Head]);
            Assert.That(itemIdBySlot, Does.Not.ContainKey(ItemSlot.Shoulder));
            Assert.AreEqual(bodyNew.ItemId, itemIdBySlot[ItemSlot.Body]);
            Assert.That(itemIdBySlot, Does.Not.ContainKey(ItemSlot.Shoulder));
            Assert.That(itemIdBySlot, Does.Not.ContainKey(ItemSlot.Hand));
            Assert.AreEqual(legOld.ItemId, itemIdBySlot[ItemSlot.Leg]);
            Assert.That(itemIdBySlot, Does.Not.ContainKey(ItemSlot.MountHarness));
            Assert.That(itemIdBySlot, Does.Not.ContainKey(ItemSlot.Mount));
            Assert.That(itemIdBySlot, Does.Not.ContainKey(ItemSlot.Weapon0));
            Assert.That(itemIdBySlot, Does.Not.ContainKey(ItemSlot.Weapon1));
            Assert.That(itemIdBySlot, Does.Not.ContainKey(ItemSlot.Weapon2));
            Assert.That(itemIdBySlot, Does.Not.ContainKey(ItemSlot.Weapon3));
        }

        [Test]
        public async Task CharacterNotFound()
        {
            var user = ArrangeDb.Users.Add(new User());
            await ArrangeDb.SaveChangesAsync();

            UpdateCharacterItemsCommand.Handler handler = new(ActDb, Mapper);
            var cmd = new UpdateCharacterItemsCommand
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
            var cmd = new UpdateCharacterItemsCommand
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
            var cmd = new UpdateCharacterItemsCommand
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
            var cmd = new UpdateCharacterItemsCommand
            {
                CharacterId = character.Entity.Id,
                UserId = user.Entity.Id,
                Items = new List<EquippedItemIdViewModel> { new() { ItemId = 1, Slot = ItemSlot.Head } },
            };

            var result = await handler.Handle(cmd, CancellationToken.None);
            Assert.AreEqual(ErrorCode.ItemNotOwned, result.Errors![0].Code);
        }

        [Test]
        public async Task ItemNotOwned()
        {
            var head = ArrangeDb.Items.Add(new Item { Type = ItemType.HeadArmor });
            var character = ArrangeDb.Characters.Add(new Character());
            var user = ArrangeDb.Users.Add(new User
            {
                Characters = new List<Character> { character.Entity },
            });
            await ArrangeDb.SaveChangesAsync();

            UpdateCharacterItemsCommand.Handler handler = new(ActDb, Mapper);
            var cmd = new UpdateCharacterItemsCommand
            {
                CharacterId = character.Entity.Id,
                UserId = user.Entity.Id,
                Items = new List<EquippedItemIdViewModel> { new() { ItemId = head.Entity.Id, Slot = ItemSlot.Head } },
            };

            var result = await handler.Handle(cmd, CancellationToken.None);
            Assert.AreEqual(ErrorCode.ItemNotOwned, result.Errors![0].Code);
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
            var character = new Character();
            var userItem = new UserItem { Item = new Item { Type = itemType } };
            var user = new User
            {
                Items = { userItem },
                Characters = { character },
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            UpdateCharacterItemsCommand.Handler handler = new(ActDb, Mapper);
            var cmd = new UpdateCharacterItemsCommand
            {
                CharacterId = character.Id,
                UserId = user.Id,
                Items = new List<EquippedItemIdViewModel> { new() { ItemId = userItem.ItemId, Slot = itemSlot } },
            };

            var result = await handler.Handle(cmd, CancellationToken.None);
            Assert.AreEqual(ErrorCode.ItemBadSlot, result.Errors![0].Code);
        }
    }
}
