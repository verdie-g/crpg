using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Characters.Commands;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities;
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
            var headOld = ArrangeDb.Items.Add(new Item { Type = ItemType.HeadArmor });
            var headNew = ArrangeDb.Items.Add(new Item { Type = ItemType.HeadArmor });
            var shoulderOld = ArrangeDb.Items.Add(new Item { Type = ItemType.ShoulderArmor });
            var shoulderNew = ArrangeDb.Items.Add(new Item { Type = ItemType.ShoulderArmor });
            var bodyOld = ArrangeDb.Items.Add(new Item { Type = ItemType.BodyArmor });
            var bodyNew = ArrangeDb.Items.Add(new Item { Type = ItemType.BodyArmor });
            var handOld = ArrangeDb.Items.Add(new Item { Type = ItemType.HandArmor });
            var handNew = ArrangeDb.Items.Add(new Item { Type = ItemType.HandArmor });
            var legOld = ArrangeDb.Items.Add(new Item { Type = ItemType.LegArmor });
            var legNew = ArrangeDb.Items.Add(new Item { Type = ItemType.LegArmor });
            var horseHarnessOld = ArrangeDb.Items.Add(new Item { Type = ItemType.HorseHarness });
            var horseHarnessNew = ArrangeDb.Items.Add(new Item { Type = ItemType.HorseHarness });
            var horseOld = ArrangeDb.Items.Add(new Item { Type = ItemType.Horse });
            var horseNew = ArrangeDb.Items.Add(new Item { Type = ItemType.Horse });
            var weapon1Old = ArrangeDb.Items.Add(new Item { Type = ItemType.Arrows });
            var weapon1New = ArrangeDb.Items.Add(new Item { Type = ItemType.Bolts });
            var weapon2Old = ArrangeDb.Items.Add(new Item { Type = ItemType.Bow });
            var weapon2New = ArrangeDb.Items.Add(new Item { Type = ItemType.Crossbow });
            var weapon3Old = ArrangeDb.Items.Add(new Item { Type = ItemType.Polearm });
            var weapon3New = ArrangeDb.Items.Add(new Item { Type = ItemType.Shield });
            var weapon4Old = ArrangeDb.Items.Add(new Item { Type = ItemType.OneHandedWeapon });
            var weapon4New = ArrangeDb.Items.Add(new Item { Type = ItemType.TwoHandedWeapon });
            var character = ArrangeDb.Characters.Add(new Character
            {
                Name = "name",
                Items = new CharacterItems
                {
                    HeadItem = headOld.Entity,
                    ShoulderItem = shoulderOld.Entity,
                    BodyItem = bodyOld.Entity,
                    HandItem = handOld.Entity,
                    LegItem = legOld.Entity,
                    HorseHarnessItem = horseHarnessOld.Entity,
                    HorseItem = horseOld.Entity,
                    Weapon1Item = weapon1Old.Entity,
                    Weapon2Item = weapon2Old.Entity,
                    Weapon3Item = weapon3Old.Entity,
                    Weapon4Item = weapon4Old.Entity,
                    AutoRepair = false,
                },
            });
            var user = ArrangeDb.Users.Add(new User
            {
                OwnedItems = new List<UserItem>
                {
                    new UserItem { Item = headOld.Entity },
                    new UserItem { Item = headNew.Entity },
                    new UserItem { Item = shoulderOld.Entity },
                    new UserItem { Item = shoulderNew.Entity },
                    new UserItem { Item = bodyOld.Entity },
                    new UserItem { Item = bodyNew.Entity },
                    new UserItem { Item = handOld.Entity },
                    new UserItem { Item = handNew.Entity },
                    new UserItem { Item = legOld.Entity },
                    new UserItem { Item = legNew.Entity },
                    new UserItem { Item = horseHarnessOld.Entity },
                    new UserItem { Item = horseHarnessNew.Entity },
                    new UserItem { Item = horseOld.Entity },
                    new UserItem { Item = horseNew.Entity },
                    new UserItem { Item = weapon1Old.Entity },
                    new UserItem { Item = weapon1New.Entity },
                    new UserItem { Item = weapon2Old.Entity },
                    new UserItem { Item = weapon2New.Entity },
                    new UserItem { Item = weapon3Old.Entity },
                    new UserItem { Item = weapon3New.Entity },
                    new UserItem { Item = weapon4Old.Entity },
                    new UserItem { Item = weapon4New.Entity },
                },
                Characters = new List<Character> { character.Entity }
            });
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateCharacterItemsCommand.Handler(ActDb, Mapper);
            var cmd = new UpdateCharacterItemsCommand
            {
                CharacterId = character.Entity.Id,
                UserId = user.Entity.Id,
                HeadItemId = headNew.Entity.Id,
                ShoulderItemId = shoulderNew.Entity.Id,
                BodyItemId = bodyNew.Entity.Id,
                HandItemId = handNew.Entity.Id,
                LegItemId = legNew.Entity.Id,
                HorseHarnessItemId = horseHarnessNew.Entity.Id,
                HorseItemId = horseNew.Entity.Id,
                Weapon1ItemId = weapon1New.Entity.Id,
                Weapon2ItemId = weapon2New.Entity.Id,
                Weapon3ItemId = weapon3New.Entity.Id,
                Weapon4ItemId = weapon4New.Entity.Id,
                AutoRepair = true,
            };
            var result = await handler.Handle(cmd, CancellationToken.None);

            var c = result.Data!;
            Assert.AreEqual(cmd.HeadItemId, c.HeadItem!.Id);
            Assert.AreEqual(cmd.ShoulderItemId, c.ShoulderItem!.Id);
            Assert.AreEqual(cmd.BodyItemId, c.BodyItem!.Id);
            Assert.AreEqual(cmd.HandItemId, c.HandItem!.Id);
            Assert.AreEqual(cmd.LegItemId, c.LegItem!.Id);
            Assert.AreEqual(cmd.HorseHarnessItemId, c.HorseHarnessItem!.Id);
            Assert.AreEqual(cmd.HorseItemId, c.HorseItem!.Id);
            Assert.AreEqual(cmd.Weapon1ItemId, c.Weapon1Item!.Id);
            Assert.AreEqual(cmd.Weapon2ItemId, c.Weapon2Item!.Id);
            Assert.AreEqual(cmd.Weapon3ItemId, c.Weapon3Item!.Id);
            Assert.AreEqual(cmd.Weapon4ItemId, c.Weapon4Item!.Id);
            Assert.IsTrue(c.AutoRepair);
        }

        [Test]
        public async Task PartialUpdate()
        {
            var headOld = ArrangeDb.Items.Add(new Item { Type = ItemType.HeadArmor });
            var headNew = ArrangeDb.Items.Add(new Item { Type = ItemType.HeadArmor });
            var bodyNew = ArrangeDb.Items.Add(new Item { Type = ItemType.BodyArmor });
            var legOld = ArrangeDb.Items.Add(new Item { Type = ItemType.LegArmor });
            var character = ArrangeDb.Characters.Add(new Character
            {
                Items = new CharacterItems
                {
                    HeadItem = headOld.Entity,
                    ShoulderItem = null,
                    BodyItem = null,
                    HandItem = null,
                    LegItem = legOld.Entity,
                    HorseHarnessItem = null,
                    HorseItem = null,
                    Weapon1Item = null,
                    Weapon2Item = null,
                    Weapon3Item = null,
                    Weapon4Item = null,
                    AutoRepair = true,
                },
            });
            var user = ArrangeDb.Users.Add(new User
            {
                OwnedItems = new List<UserItem>
                {
                    new UserItem { Item = headOld.Entity },
                    new UserItem { Item = headNew.Entity },
                    new UserItem { Item = bodyNew.Entity },
                    new UserItem { Item = legOld.Entity },
                },
                Characters = new List<Character> { character.Entity }
            });
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateCharacterItemsCommand.Handler(ActDb, Mapper);
            var cmd = new UpdateCharacterItemsCommand
            {
                CharacterId = character.Entity.Id,
                UserId = user.Entity.Id,
                HeadItemId = headNew.Entity.Id,
                ShoulderItemId = null,
                BodyItemId = bodyNew.Entity.Id,
                HandItemId = null,
                LegItemId = null,
                HorseHarnessItemId = null,
                HorseItemId = null,
                Weapon1ItemId = null,
                Weapon2ItemId = null,
                Weapon3ItemId = null,
                Weapon4ItemId = null,
                AutoRepair = false,
            };
            var result = await handler.Handle(cmd, CancellationToken.None);

            var c = result.Data!;
            Assert.AreEqual(cmd.HeadItemId, c.HeadItem!.Id);
            Assert.IsNull(c.ShoulderItem);
            Assert.AreEqual(cmd.BodyItemId, c.BodyItem!.Id);
            Assert.IsNull(c.HandItem);
            Assert.IsNull(c.LegItem);
            Assert.IsNull(c.HorseHarnessItem);
            Assert.IsNull(c.HorseItem);
            Assert.IsNull(c.Weapon1Item);
            Assert.IsNull(c.Weapon2Item);
            Assert.IsNull(c.Weapon3Item);
            Assert.IsNull(c.Weapon4Item);
            Assert.IsFalse(c.AutoRepair);
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
                HeadItemId = 1,
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
                HeadItemId = head.Entity.Id,
            };

            var result = await handler.Handle(cmd, CancellationToken.None);
            Assert.AreEqual(ErrorCode.ItemNotOwned, result.Errors![0].Code);
        }

        [Theory]
        public async Task WrongItemType(ItemType itemType)
        {
            var head = ArrangeDb.Items.Add(new Item { Type = ItemType.HeadArmor });
            var shoulder = ArrangeDb.Items.Add(new Item { Type = ItemType.ShoulderArmor });
            var body = ArrangeDb.Items.Add(new Item { Type = ItemType.BodyArmor });
            var hand = ArrangeDb.Items.Add(new Item { Type = ItemType.HandArmor });
            var leg = ArrangeDb.Items.Add(new Item { Type = ItemType.LegArmor });
            var horseHarness = ArrangeDb.Items.Add(new Item { Type = ItemType.HorseHarness });
            var horse = ArrangeDb.Items.Add(new Item { Type = ItemType.Horse });
            var weapon = ArrangeDb.Items.Add(new Item { Type = ItemType.OneHandedWeapon });
            var character = ArrangeDb.Characters.Add(new Character());
            var user = ArrangeDb.Users.Add(new User
            {
                OwnedItems = new List<UserItem>
                {
                    new UserItem { Item = head.Entity },
                    new UserItem { Item = shoulder.Entity },
                    new UserItem { Item = body.Entity },
                    new UserItem { Item = hand.Entity },
                    new UserItem { Item = leg.Entity },
                    new UserItem { Item = horseHarness.Entity },
                    new UserItem { Item = horse.Entity },
                    new UserItem { Item = weapon.Entity },
                },
                Characters = new List<Character> { character.Entity }
            });
            await ArrangeDb.SaveChangesAsync();

            var handler = new UpdateCharacterItemsCommand.Handler(ActDb, Mapper);
            var cmd = new UpdateCharacterItemsCommand
            {
                CharacterId = character.Entity.Id,
                UserId = user.Entity.Id,
                HeadItemId = itemType == ItemType.HeadArmor ? null : (int?)weapon.Entity.Id,
                ShoulderItemId = itemType == ItemType.ShoulderArmor ? null : (int?)head.Entity.Id,
                BodyItemId = itemType == ItemType.BodyArmor ? null : (int?)shoulder.Entity.Id,
                HandItemId = itemType == ItemType.HandArmor ? null : (int?)body.Entity.Id,
                LegItemId = itemType == ItemType.LegArmor ? null : (int?)hand.Entity.Id,
                HorseHarnessItemId = itemType == ItemType.HorseHarness ? null : (int?)leg.Entity.Id,
                HorseItemId = itemType == ItemType.Horse ? null : (int?)horseHarness.Entity.Id,
                Weapon1ItemId = itemType == ItemType.Arrows || itemType == ItemType.Bolts || itemType == ItemType.Bow
                    ? null
                    : (int?)horse.Entity.Id,
                Weapon2ItemId = itemType == ItemType.Crossbow || itemType == ItemType.OneHandedWeapon
                    ? null
                    : (int?)head.Entity.Id,
                Weapon3ItemId = itemType == ItemType.Polearm || itemType == ItemType.Shield
                    ? null
                    : (int?)shoulder.Entity.Id,
                Weapon4ItemId = itemType == ItemType.Thrown || itemType == ItemType.TwoHandedWeapon
                    ? null
                    : (int?)body.Entity.Id,
            };

            var result = await handler.Handle(cmd, CancellationToken.None);
            Assert.AreEqual(ErrorCode.ItemBadType, result.Errors![0].Code);
        }
    }
}
