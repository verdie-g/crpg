using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Trpg.Application.Common.Exceptions;
using Trpg.Application.Equipments.Commands;
using Trpg.Domain.Entities;

namespace Trpg.Application.UTest.Equipments
{
    public class BuyEquipmentCommandTest : TestBase
    {
        [Test]
        public async Task Basic()
        {
            var user = _db.Users.Add(new User { Money = 100 });
            var equipment = _db.Equipments.Add(new Equipment { Price = 100 });
            await _db.SaveChangesAsync();

            var handler = new BuyEquipmentCommand.Handler(_db, _mapper);
            var boughtEquipment = await handler.Handle(new BuyEquipmentCommand
            {
                EquipmentId = equipment.Entity.Id,
                UserId = user.Entity.Id,
            }, CancellationToken.None);

            var userDb = await _db.Users
                .Include(u => u.UserEquipments)
                .FirstAsync(u => u.Id == user.Entity.Id);

            Assert.AreEqual(equipment.Entity.Id, boughtEquipment.Id);
            Assert.AreEqual(0, userDb.Money);
            Assert.IsTrue(userDb.UserEquipments.Any(e => e.EquipmentId == boughtEquipment.Id));
        }

        [Test]
        public async Task NotFoundEquipment()
        {
            var user = _db.Users.Add(new User { Money = 100 });
            await _db.SaveChangesAsync();

            var handler = new BuyEquipmentCommand.Handler(_db, _mapper);
            Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(new BuyEquipmentCommand
            {
                EquipmentId = 1,
                UserId = user.Entity.Id,
            }, CancellationToken.None));
        }

        [Test]
        public async Task NotFoundUser()
        {
            var equipment = _db.Equipments.Add(new Equipment { Price = 100 });
            await _db.SaveChangesAsync();

            var handler = new BuyEquipmentCommand.Handler(_db, _mapper);
            Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(new BuyEquipmentCommand
            {
                EquipmentId = equipment.Entity.Id,
                UserId = 1,
            }, CancellationToken.None));
        }

        [Test]
        public async Task NotEnoughMoney()
        {
            var user = _db.Users.Add(new User { Money = 100 });
            var equipment = _db.Equipments.Add(new Equipment { Price = 101 });
            await _db.SaveChangesAsync();

            var handler = new BuyEquipmentCommand.Handler(_db, _mapper);
            Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(new BuyEquipmentCommand
            {
                EquipmentId = equipment.Entity.Id,
                UserId = user.Entity.Id,
            }, CancellationToken.None));
        }

        [Test]
        public async Task AlreadyOwningEquipment()
        {
            var equipment = _db.Equipments.Add(new Equipment {Price = 100});
            var user = _db.Users.Add(new User
            {
                Money = 100,
                UserEquipments = new List<UserEquipment> {new UserEquipment {EquipmentId = equipment.Entity.Id}}
            });
            await _db.SaveChangesAsync();

            var handler = new BuyEquipmentCommand.Handler(_db, _mapper);
            Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(new BuyEquipmentCommand
            {
                EquipmentId = equipment.Entity.Id,
                UserId = user.Entity.Id,
            }, CancellationToken.None));
        }
    }
}