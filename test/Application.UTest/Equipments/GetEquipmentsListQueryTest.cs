using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Trpg.Application.Equipments.Queries;
using Trpg.Domain.Entities;

namespace Trpg.Application.UTest.Equipments
{
    public class GetEquipmentsListQueryTest : TestBase
    {
        [Test]
        public async Task Basic()
        {
            _db.AddRange(
                new Equipment
                {
                    Name = "toto",
                    Price = 100,
                    Type = EquipmentType.Body,
                },
                new Equipment
                {
                    Name = "tata",
                    Price = 200,
                    Type = EquipmentType.Gloves,
                });
            await _db.SaveChangesAsync();

            var handler = new GetEquipmentsListQuery.Handler(_db, _mapper);
            var equipments = await handler.Handle(new GetEquipmentsListQuery(), CancellationToken.None);

            Assert.AreEqual(2, equipments.Count);
        }
    }
}