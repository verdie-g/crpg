using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Trpg.Application.Common.Exceptions;
using Trpg.Application.Equipments.Commands;
using Trpg.Domain.Entities;

namespace Trpg.Application.UTest.Equipments
{
    public class DeleteEquipmentCommandTest : TestBase
    {
        [Test]
        public async Task WhenEquipmentExists()
        {
            var e = _db.Equipments.Add(new Equipment
            {
                Name = "sword",
                Price = 100,
                Type = EquipmentType.Body,
            });
            await _db.SaveChangesAsync();

            var handler = new DeleteEquipmentCommand.Handler(_db);
            await handler.Handle(new DeleteEquipmentCommand {EquipmentId = e.Entity.Id}, CancellationToken.None);

            Assert.IsNull(await _db.Equipments.FindAsync(e.Entity.Id));
        }

        [Test]
        public void WhenEquipmentDoesntExist()
        {
            var handler = new DeleteEquipmentCommand.Handler(_db);
            Assert.ThrowsAsync<NotFoundException>(() =>
                handler.Handle(new DeleteEquipmentCommand {EquipmentId = 1}, CancellationToken.None));
        }
    }
}