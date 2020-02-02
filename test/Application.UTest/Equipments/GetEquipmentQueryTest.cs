using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Trpg.Application.Common.Exceptions;
using Trpg.Application.Equipments.Queries;
using Trpg.Domain.Entities;

namespace Trpg.Application.UTest.Equipments
{
    public class GetEquipmentQueryTest : TestBase
    {
        [Test]
        public void WhenEquipmentDoesntExist()
        {
            var handler = new GetEquipmentQuery.Handler(_db, _mapper);
            Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(new GetEquipmentQuery
            {
                EquipmentId = 1,
            }, CancellationToken.None));
        }

        [Test]
        public async Task WhenEquipmentExists()
        {
            var dbEquipment = new Equipment
            {
                Name = "toto",
                Price = 100,
                Type = EquipmentType.Body,
            };
            _db.Equipments.Add(dbEquipment);
            await _db.SaveChangesAsync();

            var handler = new GetEquipmentQuery.Handler(_db, _mapper);
            var equipment = await handler.Handle(new GetEquipmentQuery
            {
                EquipmentId = dbEquipment.Id,
            }, CancellationToken.None);

            Assert.NotNull(equipment);
        }
    }
}
