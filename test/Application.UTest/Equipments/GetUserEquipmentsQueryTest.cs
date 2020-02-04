using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Trpg.Application.Equipments.Queries;
using Trpg.Domain.Entities;

namespace Trpg.Application.UTest.Equipments
{
    public class GetUserEquipmentsQueryTest : TestBase
    {
        [Test]
        public async Task Basic()
        {
            var user = _db.Users.Add(new User
            {
                UserEquipments = new List<UserEquipment>
                {
                    new UserEquipment {Equipment = new Equipment()},
                    new UserEquipment {Equipment = new Equipment()},
                }
            });
            await _db.SaveChangesAsync();

            var equipments = await new GetUserEquipmentsQuery.Handler(_db, _mapper).Handle(
                new GetUserEquipmentsQuery {UserId = user.Entity.Id}, CancellationToken.None);

            Assert.AreEqual(2, equipments.Count);
        }
    }
}