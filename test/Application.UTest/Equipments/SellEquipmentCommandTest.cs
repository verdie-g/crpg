using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Trpg.Application.Common.Exceptions;
using Trpg.Application.Equipments.Commands;
using Trpg.Domain.Entities;

namespace Trpg.Application.UTest.Equipments
{
    public class SellEquipmentCommandTest : TestBase
    {
        [Test]
        public async Task Basic()
        {
            var user = _db.Users.Add(new User
            {
                Money = 0,
                UserEquipments = new List<UserEquipment>
                {
                    new UserEquipment
                    {
                        Equipment = new Equipment {Price = 100},
                    }
                },
            });
            await _db.SaveChangesAsync();

            await new SellEquipmentCommand.Handler(_db, _mapper).Handle(new SellEquipmentCommand
            {
                EquipmentId = user.Entity.UserEquipments[0].EquipmentId,
                UserId = user.Entity.Id,
            }, CancellationToken.None);

            Assert.AreEqual(66, user.Entity.Money);
            Assert.IsTrue(!user.Entity.UserEquipments.Any(ue =>
                ue.EquipmentId == user.Entity.UserEquipments[0].EquipmentId));
        }

        [Test]
        public async Task NotFoundEquipment()
        {
            var user = _db.Users.Add(new User());
            await _db.SaveChangesAsync();

            Assert.ThrowsAsync<NotFoundException>(() => new SellEquipmentCommand.Handler(_db, _mapper).Handle(
                new SellEquipmentCommand
                {
                    EquipmentId = 1,
                    UserId = user.Entity.Id,
                }, CancellationToken.None));
        }

        [Test]
        public async Task NotFoundUser()
        {
            var equipment = _db.Equipments.Add(new Equipment());
            await _db.SaveChangesAsync();

            Assert.ThrowsAsync<NotFoundException>(() => new SellEquipmentCommand.Handler(_db, _mapper).Handle(
                new SellEquipmentCommand
                {
                    EquipmentId = equipment.Entity.Id,
                    UserId = 1,
                }, CancellationToken.None));
        }
    }
}