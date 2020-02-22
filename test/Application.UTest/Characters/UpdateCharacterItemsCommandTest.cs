using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Trpg.Application.Characters.Commands;
using Trpg.Application.Common.Exceptions;
using Trpg.Domain.Entities;

namespace Trpg.Application.UTest.Characters
{
    public class UpdateCharacterItemsCommandTest : TestBase
    {
        [Test]
        public async Task FullUpdate()
        {
            var headOld = _db.Items.Add(new Item {Type = ItemType.HeadArmor});
            var headNew = _db.Items.Add(new Item {Type = ItemType.HeadArmor});
            var bodyOld = _db.Items.Add(new Item {Type = ItemType.BodyArmor});
            var bodyNew = _db.Items.Add(new Item {Type = ItemType.BodyArmor});
            var legsOld = _db.Items.Add(new Item {Type = ItemType.LegArmor});
            var legsNew = _db.Items.Add(new Item {Type = ItemType.LegArmor});
            var glovesOld = _db.Items.Add(new Item {Type = ItemType.HandArmor});
            var glovesNew = _db.Items.Add(new Item {Type = ItemType.HandArmor});
            var weapon1Old = _db.Items.Add(new Item {Type = ItemType.Arrows});
            var weapon1New = _db.Items.Add(new Item {Type = ItemType.Bolts});
            var weapon2Old = _db.Items.Add(new Item {Type = ItemType.Bow});
            var weapon2New = _db.Items.Add(new Item {Type = ItemType.Crossbow});
            var weapon3Old = _db.Items.Add(new Item {Type = ItemType.Polearm});
            var weapon3New = _db.Items.Add(new Item {Type = ItemType.Shield});
            var weapon4Old = _db.Items.Add(new Item {Type = ItemType.OneHandedWeapon});
            var weapon4New = _db.Items.Add(new Item {Type = ItemType.TwoHandedWeapon});
            var character = _db.Characters.Add(new Character
            {
                Name = "name",
                HeadItem = headOld.Entity,
                BodyItem = bodyOld.Entity,
                LegsItem = legsOld.Entity,
                GlovesItem = glovesOld.Entity,
                Weapon1Item = weapon1Old.Entity,
            });
            var user = _db.Users.Add(new User
            {
                UserItems = new List<UserItem>
                {
                    new UserItem {Item = headOld.Entity},
                    new UserItem {Item = headNew.Entity},
                    new UserItem {Item = bodyOld.Entity},
                    new UserItem {Item = bodyNew.Entity},
                    new UserItem {Item = legsOld.Entity},
                    new UserItem {Item = legsNew.Entity},
                    new UserItem {Item = glovesOld.Entity},
                    new UserItem {Item = glovesNew.Entity},
                    new UserItem {Item = weapon1Old.Entity},
                    new UserItem {Item = weapon1New.Entity},
                    new UserItem {Item = weapon2Old.Entity},
                    new UserItem {Item = weapon2New.Entity},
                    new UserItem {Item = weapon3Old.Entity},
                    new UserItem {Item = weapon3New.Entity},
                    new UserItem {Item = weapon4Old.Entity},
                    new UserItem {Item = weapon4New.Entity},
                },
                Characters = new List<Character> {character.Entity}
            });
            await _db.SaveChangesAsync();

            var handler = new UpdateCharacterItemsCommand.Handler(_db, _mapper);
            var cmd = new UpdateCharacterItemsCommand
            {
                CharacterId = character.Entity.Id,
                UserId = user.Entity.Id,
                HeadItemId = headNew.Entity.Id,
                BodyItemId = bodyNew.Entity.Id,
                LegsItemId = legsNew.Entity.Id,
                GlovesItemId = glovesNew.Entity.Id,
                Weapon1ItemId = weapon1New.Entity.Id,
                Weapon2ItemId = weapon2New.Entity.Id,
                Weapon3ItemId = weapon3New.Entity.Id,
                Weapon4ItemId = weapon4New.Entity.Id,
            };
            var c = await handler.Handle(cmd, CancellationToken.None);

            Assert.AreEqual(cmd.CharacterId, c.Id);
            Assert.AreEqual(character.Entity.Name, c.Name);
            Assert.AreEqual(cmd.HeadItemId, c.HeadItem.Id);
            Assert.AreEqual(cmd.BodyItemId, c.BodyItem.Id);
            Assert.AreEqual(cmd.LegsItemId, c.LegsItem.Id);
            Assert.AreEqual(cmd.GlovesItemId, c.GlovesItem.Id);
            Assert.AreEqual(cmd.Weapon1ItemId, c.Weapon1Item.Id);
            Assert.AreEqual(cmd.Weapon2ItemId, c.Weapon2Item.Id);
            Assert.AreEqual(cmd.Weapon3ItemId, c.Weapon3Item.Id);
            Assert.AreEqual(cmd.Weapon4ItemId, c.Weapon4Item.Id);
        }

        [Test]
        public async Task PartialUpdate()
        {
            var headOld = _db.Items.Add(new Item {Type = ItemType.HeadArmor});
            var headNew = _db.Items.Add(new Item {Type = ItemType.HeadArmor});
            var bodyNew = _db.Items.Add(new Item {Type = ItemType.BodyArmor});
            var legsOld = _db.Items.Add(new Item {Type = ItemType.LegArmor});
            var character = _db.Characters.Add(new Character
            {
                HeadItem = headOld.Entity,
                BodyItem = null,
                LegsItem = legsOld.Entity,
                GlovesItem = null,
                Weapon1Item = null,
                Weapon2Item = null,
                Weapon3Item = null,
                Weapon4Item = null,
            });
            var user = _db.Users.Add(new User
            {
                UserItems = new List<UserItem>
                {
                    new UserItem {Item = headOld.Entity},
                    new UserItem {Item = headNew.Entity},
                    new UserItem {Item = bodyNew.Entity},
                    new UserItem {Item = legsOld.Entity},
                },
                Characters = new List<Character> {character.Entity}
            });
            await _db.SaveChangesAsync();

            var handler = new UpdateCharacterItemsCommand.Handler(_db, _mapper);
            var cmd = new UpdateCharacterItemsCommand
            {
                CharacterId = character.Entity.Id,
                UserId = user.Entity.Id,
                HeadItemId = headNew.Entity.Id,
                BodyItemId = bodyNew.Entity.Id,
                LegsItemId = null,
                GlovesItemId = null,
                Weapon1ItemId = null,
                Weapon2ItemId = null,
                Weapon3ItemId = null,
                Weapon4ItemId = null,
            };
            var c = await handler.Handle(cmd, CancellationToken.None);

            Assert.AreEqual(cmd.CharacterId, c.Id);
            Assert.AreEqual(cmd.HeadItemId, c.HeadItem.Id);
            Assert.AreEqual(cmd.BodyItemId, c.BodyItem.Id);
            Assert.IsNull(c.LegsItem);
            Assert.IsNull(c.GlovesItem);
            Assert.IsNull(c.Weapon1Item);
            Assert.IsNull(c.Weapon2Item);
            Assert.IsNull(c.Weapon3Item);
            Assert.IsNull(c.Weapon4Item);
        }

        [Test]
        public async Task CharacterNotFound()
        {
            var user = _db.Users.Add(new User());
            await _db.SaveChangesAsync();

            var handler = new UpdateCharacterItemsCommand.Handler(_db, _mapper);
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
            var character = _db.Characters.Add(new Character());
            var user = _db.Users.Add(new User());
            await _db.SaveChangesAsync();

            var handler = new UpdateCharacterItemsCommand.Handler(_db, _mapper);
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
            var character = _db.Characters.Add(new Character());
            await _db.SaveChangesAsync();

            var handler = new UpdateCharacterItemsCommand.Handler(_db, _mapper);
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
            var character = _db.Characters.Add(new Character());
            var user = _db.Users.Add(new User
            {
                Characters = new List<Character> {character.Entity}
            });
            await _db.SaveChangesAsync();

            var handler = new UpdateCharacterItemsCommand.Handler(_db, _mapper);
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
            var head = _db.Items.Add(new Item {Type = ItemType.HeadArmor});
            var character = _db.Characters.Add(new Character());
            var user = _db.Users.Add(new User
            {
                Characters = new List<Character> {character.Entity}
            });
            await _db.SaveChangesAsync();

            var handler = new UpdateCharacterItemsCommand.Handler(_db, _mapper);
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
            var head = _db.Items.Add(new Item {Type = ItemType.HeadArmor});
            var body = _db.Items.Add(new Item {Type = ItemType.BodyArmor});
            var legs = _db.Items.Add(new Item {Type = ItemType.LegArmor});
            var gloves = _db.Items.Add(new Item {Type = ItemType.HandArmor});
            var weapon = _db.Items.Add(new Item {Type = ItemType.OneHandedWeapon});
            var character = _db.Characters.Add(new Character());
            var user = _db.Users.Add(new User
            {
                UserItems = new List<UserItem>
                {
                    new UserItem {Item = head.Entity},
                    new UserItem {Item = body.Entity},
                    new UserItem {Item = legs.Entity},
                    new UserItem {Item = gloves.Entity},
                    new UserItem {Item = weapon.Entity},
                },
                Characters = new List<Character> {character.Entity}
            });
            await _db.SaveChangesAsync();

            var handler = new UpdateCharacterItemsCommand.Handler(_db, _mapper);
            var cmd = new UpdateCharacterItemsCommand
            {
                CharacterId = character.Entity.Id,
                UserId = user.Entity.Id,
                HeadItemId = itemType == ItemType.HeadArmor ? null : (int?) body.Entity.Id,
                BodyItemId = itemType == ItemType.BodyArmor ? null : (int?) legs.Entity.Id,
                LegsItemId = itemType == ItemType.LegArmor ? null : (int?) gloves.Entity.Id,
                GlovesItemId = itemType == ItemType.HandArmor ? null : (int?) weapon.Entity.Id,
                Weapon1ItemId = itemType == ItemType.Arrows || itemType == ItemType.Bolts || itemType == ItemType.Bow ? null : (int?) head.Entity.Id,
                Weapon2ItemId = itemType == ItemType.Crossbow || itemType == ItemType.OneHandedWeapon ? null : (int?) head.Entity.Id,
                Weapon3ItemId = itemType == ItemType.Polearm || itemType == ItemType.Shield  ? null : (int?) head.Entity.Id,
                Weapon4ItemId = itemType == ItemType.Thrown || itemType == ItemType.TwoHandedWeapon  ? null : (int?) head.Entity.Id,
            };

            Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(cmd, CancellationToken.None));
        }
    }
}