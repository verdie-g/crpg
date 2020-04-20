using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Characters.Commands;
using Crpg.Application.Common.Exceptions;
using Crpg.Domain.Entities;
using NUnit.Framework;

namespace Crpg.Application.UTest.Characters
{
    public class UpdateCharacterItemsCommandTest : TestBase
    {
        [Test]
        public async Task FullUpdate()
        {
            var headOld = Db.Items.Add(new Item { Type = ItemType.HeadArmor });
            var headNew = Db.Items.Add(new Item { Type = ItemType.HeadArmor });
            var capeOld = Db.Items.Add(new Item { Type = ItemType.Cape });
            var capeNew = Db.Items.Add(new Item { Type = ItemType.Cape });
            var bodyOld = Db.Items.Add(new Item { Type = ItemType.BodyArmor });
            var bodyNew = Db.Items.Add(new Item { Type = ItemType.BodyArmor });
            var handOld = Db.Items.Add(new Item { Type = ItemType.HandArmor });
            var handNew = Db.Items.Add(new Item { Type = ItemType.HandArmor });
            var legOld = Db.Items.Add(new Item { Type = ItemType.LegArmor });
            var legNew = Db.Items.Add(new Item { Type = ItemType.LegArmor });
            var horseHarnessOld = Db.Items.Add(new Item { Type = ItemType.HorseHarness });
            var horseHarnessNew = Db.Items.Add(new Item { Type = ItemType.HorseHarness });
            var horseOld = Db.Items.Add(new Item { Type = ItemType.Horse });
            var horseNew = Db.Items.Add(new Item { Type = ItemType.Horse });
            var weapon1Old = Db.Items.Add(new Item { Type = ItemType.Arrows });
            var weapon1New = Db.Items.Add(new Item { Type = ItemType.Bolts });
            var weapon2Old = Db.Items.Add(new Item { Type = ItemType.Bow });
            var weapon2New = Db.Items.Add(new Item { Type = ItemType.Crossbow });
            var weapon3Old = Db.Items.Add(new Item { Type = ItemType.Polearm });
            var weapon3New = Db.Items.Add(new Item { Type = ItemType.Shield });
            var weapon4Old = Db.Items.Add(new Item { Type = ItemType.OneHandedWeapon });
            var weapon4New = Db.Items.Add(new Item { Type = ItemType.TwoHandedWeapon });
            var character = Db.Characters.Add(new Character
            {
                Name = "name",
                Items = new CharacterItems
                {
                    HeadItem = headOld.Entity,
                    CapeItem = capeOld.Entity,
                    BodyItem = bodyOld.Entity,
                    HandItem = handOld.Entity,
                    LegItem = legOld.Entity,
                    HorseHarnessItem = horseHarnessOld.Entity,
                    HorseItem = horseOld.Entity,
                    Weapon1Item = weapon1Old.Entity,
                    Weapon2Item = weapon2Old.Entity,
                    Weapon3Item = weapon3Old.Entity,
                    Weapon4Item = weapon4Old.Entity,
                },
            });
            var user = Db.Users.Add(new User
            {
                UserItems = new List<UserItem>
                {
                    new UserItem { Item = headOld.Entity },
                    new UserItem { Item = headNew.Entity },
                    new UserItem { Item = capeOld.Entity },
                    new UserItem { Item = capeNew.Entity },
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
            await Db.SaveChangesAsync();

            var handler = new UpdateCharacterItemsCommand.Handler(Db, Mapper);
            var cmd = new UpdateCharacterItemsCommand
            {
                CharacterId = character.Entity.Id,
                UserId = user.Entity.Id,
                HeadItemId = headNew.Entity.Id,
                CapeItemId = capeNew.Entity.Id,
                BodyItemId = bodyNew.Entity.Id,
                HandItemId = handNew.Entity.Id,
                LegItemId = legNew.Entity.Id,
                HorseHarnessItemId = horseHarnessNew.Entity.Id,
                HorseItemId = horseNew.Entity.Id,
                Weapon1ItemId = weapon1New.Entity.Id,
                Weapon2ItemId = weapon2New.Entity.Id,
                Weapon3ItemId = weapon3New.Entity.Id,
                Weapon4ItemId = weapon4New.Entity.Id,
            };
            var c = await handler.Handle(cmd, CancellationToken.None);

            Assert.AreEqual(cmd.HeadItemId, c.HeadItem!.Id);
            Assert.AreEqual(cmd.CapeItemId, c.CapeItem!.Id);
            Assert.AreEqual(cmd.BodyItemId, c.BodyItem!.Id);
            Assert.AreEqual(cmd.HandItemId, c.HandItem!.Id);
            Assert.AreEqual(cmd.LegItemId, c.LegItem!.Id);
            Assert.AreEqual(cmd.HorseHarnessItemId, c.HorseHarnessItem!.Id);
            Assert.AreEqual(cmd.HorseItemId, c.HorseItem!.Id);
            Assert.AreEqual(cmd.Weapon1ItemId, c.Weapon1Item!.Id);
            Assert.AreEqual(cmd.Weapon2ItemId, c.Weapon2Item!.Id);
            Assert.AreEqual(cmd.Weapon3ItemId, c.Weapon3Item!.Id);
            Assert.AreEqual(cmd.Weapon4ItemId, c.Weapon4Item!.Id);
        }

        [Test]
        public async Task PartialUpdate()
        {
            var headOld = Db.Items.Add(new Item { Type = ItemType.HeadArmor });
            var headNew = Db.Items.Add(new Item { Type = ItemType.HeadArmor });
            var bodyNew = Db.Items.Add(new Item { Type = ItemType.BodyArmor });
            var legOld = Db.Items.Add(new Item { Type = ItemType.LegArmor });
            var character = Db.Characters.Add(new Character
            {
                Items = new CharacterItems
                {
                    HeadItem = headOld.Entity,
                    CapeItem = null,
                    BodyItem = null,
                    HandItem = null,
                    LegItem = legOld.Entity,
                    HorseHarnessItem = null,
                    HorseItem = null,
                    Weapon1Item = null,
                    Weapon2Item = null,
                    Weapon3Item = null,
                    Weapon4Item = null,
                },
            });
            var user = Db.Users.Add(new User
            {
                UserItems = new List<UserItem>
                {
                    new UserItem { Item = headOld.Entity },
                    new UserItem { Item = headNew.Entity },
                    new UserItem { Item = bodyNew.Entity },
                    new UserItem { Item = legOld.Entity },
                },
                Characters = new List<Character> { character.Entity }
            });
            await Db.SaveChangesAsync();

            var handler = new UpdateCharacterItemsCommand.Handler(Db, Mapper);
            var cmd = new UpdateCharacterItemsCommand
            {
                CharacterId = character.Entity.Id,
                UserId = user.Entity.Id,
                HeadItemId = headNew.Entity.Id,
                CapeItemId = null,
                BodyItemId = bodyNew.Entity.Id,
                HandItemId = null,
                LegItemId = null,
                HorseHarnessItemId = null,
                HorseItemId = null,
                Weapon1ItemId = null,
                Weapon2ItemId = null,
                Weapon3ItemId = null,
                Weapon4ItemId = null,
            };
            var c = await handler.Handle(cmd, CancellationToken.None);

            Assert.AreEqual(cmd.HeadItemId, c.HeadItem!.Id);
            Assert.IsNull(c.CapeItem);
            Assert.AreEqual(cmd.BodyItemId, c.BodyItem!.Id);
            Assert.IsNull(c.HandItem);
            Assert.IsNull(c.LegItem);
            Assert.IsNull(c.HorseHarnessItem);
            Assert.IsNull(c.HorseItem);
            Assert.IsNull(c.Weapon1Item);
            Assert.IsNull(c.Weapon2Item);
            Assert.IsNull(c.Weapon3Item);
            Assert.IsNull(c.Weapon4Item);
        }

        [Test]
        public async Task CharacterNotFound()
        {
            var user = Db.Users.Add(new User());
            await Db.SaveChangesAsync();

            var handler = new UpdateCharacterItemsCommand.Handler(Db, Mapper);
            var cmd = new UpdateCharacterItemsCommand
            {
                CharacterId = 1,
                UserId = user.Entity.Id,
            };

            Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(cmd, CancellationToken.None));
        }

        [Test]
        public async Task CharacterNotOwned()
        {
            var character = Db.Characters.Add(new Character());
            var user = Db.Users.Add(new User());
            await Db.SaveChangesAsync();

            var handler = new UpdateCharacterItemsCommand.Handler(Db, Mapper);
            var cmd = new UpdateCharacterItemsCommand
            {
                CharacterId = character.Entity.Id,
                UserId = user.Entity.Id,
            };

            Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(cmd, CancellationToken.None));
        }

        [Test]
        public async Task UserNotFound()
        {
            var character = Db.Characters.Add(new Character());
            await Db.SaveChangesAsync();

            var handler = new UpdateCharacterItemsCommand.Handler(Db, Mapper);
            var cmd = new UpdateCharacterItemsCommand
            {
                CharacterId = character.Entity.Id,
                UserId = 1,
            };

            Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(cmd, CancellationToken.None));
        }

        [Test]
        public async Task ItemNotFound()
        {
            var character = Db.Characters.Add(new Character());
            var user = Db.Users.Add(new User
            {
                Characters = new List<Character> { character.Entity }
            });
            await Db.SaveChangesAsync();

            var handler = new UpdateCharacterItemsCommand.Handler(Db, Mapper);
            var cmd = new UpdateCharacterItemsCommand
            {
                CharacterId = character.Entity.Id,
                UserId = user.Entity.Id,
                HeadItemId = 1,
            };

            Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(cmd, CancellationToken.None));
        }

        [Test]
        public async Task ItemNotOwned()
        {
            var head = Db.Items.Add(new Item { Type = ItemType.HeadArmor });
            var character = Db.Characters.Add(new Character());
            var user = Db.Users.Add(new User
            {
                Characters = new List<Character> { character.Entity }
            });
            await Db.SaveChangesAsync();

            var handler = new UpdateCharacterItemsCommand.Handler(Db, Mapper);
            var cmd = new UpdateCharacterItemsCommand
            {
                CharacterId = character.Entity.Id,
                UserId = user.Entity.Id,
                HeadItemId = head.Entity.Id,
            };

            Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(cmd, CancellationToken.None));
        }

        [Theory]
        public async Task WrongItemType(ItemType itemType)
        {
            var head = Db.Items.Add(new Item { Type = ItemType.HeadArmor });
            var cape = Db.Items.Add(new Item { Type = ItemType.Cape });
            var body = Db.Items.Add(new Item { Type = ItemType.BodyArmor });
            var hand = Db.Items.Add(new Item { Type = ItemType.HandArmor });
            var leg = Db.Items.Add(new Item { Type = ItemType.LegArmor });
            var horseHarness = Db.Items.Add(new Item { Type = ItemType.HorseHarness });
            var horse = Db.Items.Add(new Item { Type = ItemType.Horse });
            var weapon = Db.Items.Add(new Item { Type = ItemType.OneHandedWeapon });
            var character = Db.Characters.Add(new Character());
            var user = Db.Users.Add(new User
            {
                UserItems = new List<UserItem>
                {
                    new UserItem { Item = head.Entity },
                    new UserItem { Item = cape.Entity },
                    new UserItem { Item = body.Entity },
                    new UserItem { Item = hand.Entity },
                    new UserItem { Item = leg.Entity },
                    new UserItem { Item = horseHarness.Entity },
                    new UserItem { Item = horse.Entity },
                    new UserItem { Item = weapon.Entity },
                },
                Characters = new List<Character> { character.Entity }
            });
            await Db.SaveChangesAsync();

            var handler = new UpdateCharacterItemsCommand.Handler(Db, Mapper);
            var cmd = new UpdateCharacterItemsCommand
            {
                CharacterId = character.Entity.Id,
                UserId = user.Entity.Id,
                HeadItemId = itemType == ItemType.HeadArmor ? null : (int?)weapon.Entity.Id,
                CapeItemId = itemType == ItemType.Cape ? null : (int?)head.Entity.Id,
                BodyItemId = itemType == ItemType.BodyArmor ? null : (int?)cape.Entity.Id,
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
                    : (int?)cape.Entity.Id,
                Weapon4ItemId = itemType == ItemType.Thrown || itemType == ItemType.TwoHandedWeapon
                    ? null
                    : (int?)body.Entity.Id,
            };

            Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(cmd, CancellationToken.None));
        }
    }
}