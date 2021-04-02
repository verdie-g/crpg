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

            var headOld = new OwnedItem { User = user, Item = new Item { Type = ItemType.HeadArmor } };
            var headNew = new OwnedItem { User = user, Item = new Item { Type = ItemType.HeadArmor } };
            var shoulderOld = new OwnedItem { User = user, Item = new Item { Type = ItemType.ShoulderArmor } };
            var shoulderNew = new OwnedItem { User = user, Item = new Item { Type = ItemType.ShoulderArmor } };
            var bodyOld = new OwnedItem { User = user, Item = new Item { Type = ItemType.BodyArmor } };
            var bodyNew = new OwnedItem { User = user, Item = new Item { Type = ItemType.BodyArmor } };
            var handOld = new OwnedItem { User = user, Item = new Item { Type = ItemType.HandArmor } };
            var handNew = new OwnedItem { User = user, Item = new Item { Type = ItemType.HandArmor } };
            var legOld = new OwnedItem { User = user, Item = new Item { Type = ItemType.LegArmor } };
            var legNew = new OwnedItem { User = user, Item = new Item { Type = ItemType.LegArmor } };
            var mountHarnessOld = new OwnedItem { User = user, Item = new Item { Type = ItemType.MountHarness } };
            var mountHarnessNew = new OwnedItem { User = user, Item = new Item { Type = ItemType.MountHarness } };
            var mountOld = new OwnedItem { User = user, Item = new Item { Type = ItemType.Mount } };
            var mountNew = new OwnedItem { User = user, Item = new Item { Type = ItemType.Mount } };
            var weapon0Old = new OwnedItem { User = user, Item = new Item { Type = ItemType.Arrows } };
            var weapon0New = new OwnedItem { User = user, Item = new Item { Type = ItemType.Bolts } };
            var weapon1Old = new OwnedItem { User = user, Item = new Item { Type = ItemType.Bow } };
            var weapon1New = new OwnedItem { User = user, Item = new Item { Type = ItemType.Crossbow } };
            var weapon2Old = new OwnedItem { User = user, Item = new Item { Type = ItemType.Polearm } };
            var weapon2New = new OwnedItem { User = user, Item = new Item { Type = ItemType.Shield } };
            var weapon3Old = new OwnedItem { User = user, Item = new Item { Type = ItemType.OneHandedWeapon } };
            var weapon3New = new OwnedItem { User = user, Item = new Item { Type = ItemType.TwoHandedWeapon } };

            var character = new Character
            {
                Name = "name",
                EquippedItems =
                {
                    new EquippedItem { OwnedItem = headOld, Slot = ItemSlot.Head },
                    new EquippedItem { OwnedItem = shoulderOld, Slot = ItemSlot.Shoulder },
                    new EquippedItem { OwnedItem = bodyOld, Slot = ItemSlot.Body },
                    new EquippedItem { OwnedItem = handOld, Slot = ItemSlot.Hand },
                    new EquippedItem { OwnedItem = legOld, Slot = ItemSlot.Leg },
                    new EquippedItem { OwnedItem = mountHarnessOld, Slot = ItemSlot.MountHarness },
                    new EquippedItem { OwnedItem = mountOld, Slot = ItemSlot.Mount },
                    new EquippedItem { OwnedItem = weapon0Old, Slot = ItemSlot.Weapon0 },
                    new EquippedItem { OwnedItem = weapon1Old, Slot = ItemSlot.Weapon1 },
                    new EquippedItem { OwnedItem = weapon2Old, Slot = ItemSlot.Weapon2 },
                    new EquippedItem { OwnedItem = weapon3Old, Slot = ItemSlot.Weapon3 },
                },
            };

            user.Characters.Add(character);
            ArrangeDb.Users.Add(user);
            ArrangeDb.OwnedItems.AddRange(headNew, shoulderNew, bodyNew, handNew, legNew, mountHarnessNew, mountNew,
                weapon0New, weapon1New, weapon2New, weapon3New);
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateCharacterItemsCommand.Handler(ActDb, Mapper);
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

            var headOld = new OwnedItem { User = user, Item = new Item { Type = ItemType.HeadArmor } };
            var headNew = new OwnedItem { User = user, Item = new Item { Type = ItemType.HeadArmor } };
            var bodyNew = new OwnedItem { User = user, Item = new Item { Type = ItemType.BodyArmor } };
            var handOld = new OwnedItem { User = user, Item = new Item { Type = ItemType.HandArmor } };
            var legOld = new OwnedItem { User = user, Item = new Item { Type = ItemType.LegArmor } };

            var character = new Character
            {
                Name = "name",
                EquippedItems =
                {
                    new EquippedItem { OwnedItem = headOld, Slot = ItemSlot.Head },
                    new EquippedItem { OwnedItem = handOld, Slot = ItemSlot.Hand },
                    new EquippedItem { OwnedItem = legOld, Slot = ItemSlot.Leg },
                },
            };

            user.Characters.Add(character);
            ArrangeDb.OwnedItems.AddRange(headNew, bodyNew);
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateCharacterItemsCommand.Handler(ActDb, Mapper);
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

            var handler = new UpdateCharacterItemsCommand.Handler(ActDb, Mapper);
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

            var handler = new UpdateCharacterItemsCommand.Handler(ActDb, Mapper);
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

            var handler = new UpdateCharacterItemsCommand.Handler(ActDb, Mapper);
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
                Characters = new List<Character> { character.Entity }
            });
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateCharacterItemsCommand.Handler(ActDb, Mapper);
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
                Characters = new List<Character> { character.Entity }
            });
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateCharacterItemsCommand.Handler(ActDb, Mapper);
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
            var ownedItem = new OwnedItem { Item = new Item { Type = itemType } };
            var user = new User
            {
                OwnedItems = { ownedItem },
                Characters = { character },
            };
            ArrangeDb.Users.Add(user);
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateCharacterItemsCommand.Handler(ActDb, Mapper);
            var cmd = new UpdateCharacterItemsCommand
            {
                CharacterId = character.Id,
                UserId = user.Id,
                Items = new List<EquippedItemIdViewModel> { new() { ItemId = ownedItem.ItemId, Slot = itemSlot } },
            };

            var result = await handler.Handle(cmd, CancellationToken.None);
            Assert.AreEqual(ErrorCode.ItemBadSlot, result.Errors![0].Code);
        }
    }
}
