using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Trpg.Application.Characters.Commands;
using Trpg.Application.Common.Exceptions;
using Trpg.Domain.Entities;

namespace Trpg.Application.UTest.Characters
{
    public class UpdateCharacterCommandTest : TestBase
    {
        [Test]
        public async Task FullUpdate()
        {
            var headOld = _db.Equipments.Add(new Equipment {Type = EquipmentType.Head});
            var headNew = _db.Equipments.Add(new Equipment {Type = EquipmentType.Head});
            var bodyOld = _db.Equipments.Add(new Equipment {Type = EquipmentType.Body});
            var bodyNew = _db.Equipments.Add(new Equipment {Type = EquipmentType.Body});
            var legsOld = _db.Equipments.Add(new Equipment {Type = EquipmentType.Legs});
            var legsNew = _db.Equipments.Add(new Equipment {Type = EquipmentType.Legs});
            var glovesOld = _db.Equipments.Add(new Equipment {Type = EquipmentType.Gloves});
            var glovesNew = _db.Equipments.Add(new Equipment {Type = EquipmentType.Gloves});
            var weapon1Old = _db.Equipments.Add(new Equipment {Type = EquipmentType.Weapon});
            var weapon1New = _db.Equipments.Add(new Equipment {Type = EquipmentType.Weapon});
            var weapon2Old = _db.Equipments.Add(new Equipment {Type = EquipmentType.Weapon});
            var weapon2New = _db.Equipments.Add(new Equipment {Type = EquipmentType.Weapon});
            var weapon3Old = _db.Equipments.Add(new Equipment {Type = EquipmentType.Weapon});
            var weapon3New = _db.Equipments.Add(new Equipment {Type = EquipmentType.Weapon});
            var weapon4Old = _db.Equipments.Add(new Equipment {Type = EquipmentType.Weapon});
            var weapon4New = _db.Equipments.Add(new Equipment {Type = EquipmentType.Weapon});
            var character = _db.Characters.Add(new Character
            {
                Name = "Old name",
                HeadEquipment = headOld.Entity,
                BodyEquipment = bodyOld.Entity,
                LegsEquipment = legsOld.Entity,
                GlovesEquipment = glovesOld.Entity,
                Weapon1Equipment = weapon1Old.Entity,
            });
            var user = _db.Users.Add(new User
            {
                UserEquipments = new List<UserEquipment>
                {
                    new UserEquipment {Equipment = headOld.Entity},
                    new UserEquipment {Equipment = headNew.Entity},
                    new UserEquipment {Equipment = bodyOld.Entity},
                    new UserEquipment {Equipment = bodyNew.Entity},
                    new UserEquipment {Equipment = legsOld.Entity},
                    new UserEquipment {Equipment = legsNew.Entity},
                    new UserEquipment {Equipment = glovesOld.Entity},
                    new UserEquipment {Equipment = glovesNew.Entity},
                    new UserEquipment {Equipment = weapon1Old.Entity},
                    new UserEquipment {Equipment = weapon1New.Entity},
                    new UserEquipment {Equipment = weapon2Old.Entity},
                    new UserEquipment {Equipment = weapon2New.Entity},
                    new UserEquipment {Equipment = weapon3Old.Entity},
                    new UserEquipment {Equipment = weapon3New.Entity},
                    new UserEquipment {Equipment = weapon4Old.Entity},
                    new UserEquipment {Equipment = weapon4New.Entity},
                },
                Characters = new List<Character> {character.Entity}
            });
            await _db.SaveChangesAsync();

            var handler = new UpdateCharacterCommand.Handler(_db, _mapper);
            var cmd = new UpdateCharacterCommand
            {
                CharacterId = character.Entity.Id,
                UserId = user.Entity.Id,
                Name = "New name",
                HeadEquipmentId = headNew.Entity.Id,
                BodyEquipmentId = bodyNew.Entity.Id,
                LegsEquipmentId = legsNew.Entity.Id,
                GlovesEquipmentId = glovesNew.Entity.Id,
                Weapon1EquipmentId = weapon1New.Entity.Id,
                Weapon2EquipmentId = weapon2New.Entity.Id,
                Weapon3EquipmentId = weapon3New.Entity.Id,
                Weapon4EquipmentId = weapon4New.Entity.Id,
            };
            var c = await handler.Handle(cmd, CancellationToken.None);

            Assert.AreEqual(cmd.CharacterId, c.Id);
            Assert.AreEqual(cmd.Name, c.Name);
            Assert.AreEqual(cmd.HeadEquipmentId, c.HeadEquipment.Id);
            Assert.AreEqual(cmd.BodyEquipmentId, c.BodyEquipment.Id);
            Assert.AreEqual(cmd.LegsEquipmentId, c.LegsEquipment.Id);
            Assert.AreEqual(cmd.GlovesEquipmentId, c.GlovesEquipment.Id);
            Assert.AreEqual(cmd.Weapon1EquipmentId, c.Weapon1Equipment.Id);
            Assert.AreEqual(cmd.Weapon2EquipmentId, c.Weapon2Equipment.Id);
            Assert.AreEqual(cmd.Weapon3EquipmentId, c.Weapon3Equipment.Id);
            Assert.AreEqual(cmd.Weapon4EquipmentId, c.Weapon4Equipment.Id);
        }

        [Test]
        public async Task PartialUpdate()
        {
             var headOld = _db.Equipments.Add(new Equipment {Type = EquipmentType.Head});
             var headNew = _db.Equipments.Add(new Equipment {Type = EquipmentType.Head});
             var bodyNew = _db.Equipments.Add(new Equipment {Type = EquipmentType.Body});
             var legsOld = _db.Equipments.Add(new Equipment {Type = EquipmentType.Legs});
             var character = _db.Characters.Add(new Character
             {
                 Name = "Old name",
                 HeadEquipment = headOld.Entity,
                 BodyEquipment = null,
                 LegsEquipment = legsOld.Entity,
                 GlovesEquipment = null,
                 Weapon1Equipment = null,
                 Weapon2Equipment = null,
                 Weapon3Equipment = null,
                 Weapon4Equipment = null,
             });
             var user = _db.Users.Add(new User
             {
                 UserEquipments = new List<UserEquipment>
                 {
                     new UserEquipment {Equipment = headOld.Entity},
                     new UserEquipment {Equipment = headNew.Entity},
                     new UserEquipment {Equipment = bodyNew.Entity},
                     new UserEquipment {Equipment = legsOld.Entity},
                 },
                 Characters = new List<Character> {character.Entity}
             });
             await _db.SaveChangesAsync();

             var handler = new UpdateCharacterCommand.Handler(_db, _mapper);
             var cmd = new UpdateCharacterCommand
             {
                 CharacterId = character.Entity.Id,
                 UserId = user.Entity.Id,
                 Name = "New name",
                 HeadEquipmentId = headNew.Entity.Id,
                 BodyEquipmentId = bodyNew.Entity.Id,
                 LegsEquipmentId = null,
                 GlovesEquipmentId = null,
                 Weapon1EquipmentId = null,
                 Weapon2EquipmentId = null,
                 Weapon3EquipmentId = null,
                 Weapon4EquipmentId = null,
             };
             var c = await handler.Handle(cmd, CancellationToken.None);

             Assert.AreEqual(cmd.CharacterId, c.Id);
             Assert.AreEqual(cmd.Name, c.Name);
             Assert.AreEqual(cmd.HeadEquipmentId, c.HeadEquipment.Id);
             Assert.AreEqual(cmd.BodyEquipmentId, c.BodyEquipment.Id);
             Assert.IsNull(c.LegsEquipment);
             Assert.IsNull(c.GlovesEquipment);
             Assert.IsNull(c.Weapon1Equipment);
             Assert.IsNull(c.Weapon2Equipment);
             Assert.IsNull(c.Weapon3Equipment);
             Assert.IsNull(c.Weapon4Equipment);
        }

        [Test]
        public async Task UserNotFound()
        {
             var character = _db.Characters.Add(new Character { Name = "" });
             await _db.SaveChangesAsync();

             var handler = new UpdateCharacterCommand.Handler(_db, _mapper);
             var cmd = new UpdateCharacterCommand
             {
                 CharacterId = character.Entity.Id,
                 UserId = 1,
                 Name = "",
             };

             Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(cmd, CancellationToken.None));
        }

        [Test]
        public async Task EquipmentNotFound()
        {
             var character = _db.Characters.Add(new Character { Name = "" });
             var user = _db.Users.Add(new User
             {
                 Characters = new List<Character> {character.Entity}
             });
             await _db.SaveChangesAsync();

             var handler = new UpdateCharacterCommand.Handler(_db, _mapper);
             var cmd = new UpdateCharacterCommand
             {
                 CharacterId = character.Entity.Id,
                 UserId = user.Entity.Id,
                 Name = "New name",
                 HeadEquipmentId = 1,
             };

             Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(cmd, CancellationToken.None));
        }

        [Test]
        public async Task EquipmentNotOwned()
        {
            var head = _db.Equipments.Add(new Equipment {Type = EquipmentType.Head});
            var character = _db.Characters.Add(new Character
            {
                Name = "",
            });
            var user = _db.Users.Add(new User
            {
                Characters = new List<Character> {character.Entity}
            });
            await _db.SaveChangesAsync();

            var handler = new UpdateCharacterCommand.Handler(_db, _mapper);
            var cmd = new UpdateCharacterCommand
            {
                CharacterId = character.Entity.Id,
                UserId = user.Entity.Id,
                Name = "New name",
                HeadEquipmentId = head.Entity.Id,
            };

            Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(cmd, CancellationToken.None));
        }

        [Theory]
        public async Task WrongEquipmentType(EquipmentType equipmentType)
        {
             var head = _db.Equipments.Add(new Equipment {Type = EquipmentType.Head});
             var body = _db.Equipments.Add(new Equipment {Type = EquipmentType.Body});
             var legs = _db.Equipments.Add(new Equipment {Type = EquipmentType.Legs});
             var gloves = _db.Equipments.Add(new Equipment {Type = EquipmentType.Gloves});
             var weapon = _db.Equipments.Add(new Equipment {Type = EquipmentType.Weapon});
             var character = _db.Characters.Add(new Character { Name = "" });
             var user = _db.Users.Add(new User
             {
                 UserEquipments = new List<UserEquipment>
                 {
                     new UserEquipment {Equipment = head.Entity},
                     new UserEquipment {Equipment = body.Entity},
                     new UserEquipment {Equipment = legs.Entity},
                     new UserEquipment {Equipment = gloves.Entity},
                     new UserEquipment {Equipment = weapon.Entity},
                 },
                 Characters = new List<Character> {character.Entity}
             });
             await _db.SaveChangesAsync();

             var handler = new UpdateCharacterCommand.Handler(_db, _mapper);
             var cmd = new UpdateCharacterCommand
             {
                 CharacterId = character.Entity.Id,
                 UserId = user.Entity.Id,
                 Name = "New name",
                 HeadEquipmentId = equipmentType == EquipmentType.Head ? null : (int?) body.Entity.Id,
                 BodyEquipmentId = equipmentType == EquipmentType.Body ? null : (int?) legs.Entity.Id,
                 LegsEquipmentId = equipmentType == EquipmentType.Legs ? null : (int?) gloves.Entity.Id,
                 GlovesEquipmentId = equipmentType == EquipmentType.Gloves ? null : (int?) weapon.Entity.Id,
                 Weapon1EquipmentId = equipmentType == EquipmentType.Weapon ? null : (int?) head.Entity.Id,
             };

             Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(cmd, CancellationToken.None));
        }

        [Test]
        public void BadName()
        {
            var validator = new UpdateCharacterCommand.Validator();
            var res = validator.Validate(new UpdateCharacterCommand {Name = ""});
            Assert.AreEqual(1, res.Errors.Count);
        }
    }
}