using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Strategus.Queries;
using Crpg.Domain.Entities.Strategus;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Strategus
{
    public class GetStrategusSettlementsQueryTest : TestBase
    {
        [Test]
        public async Task ShouldReturnAllSettlements()
        {
            var settlements = new[]
            {
                new StrategusSettlement
                {
                    Name = "abc",
                    Type = StrategusSettlementType.Village,
                    Position = new Point(5, 6),
                    Scene = "battania_village",
                },
                new StrategusSettlement
                {
                    Name = "def",
                    Type = StrategusSettlementType.Castle,
                    Position = new Point(7, 8),
                    Scene = "sturgia_castle",
                },
            };
            ArrangeDb.StrategusSettlements.AddRange(settlements);
            await ArrangeDb.SaveChangesAsync();

            var handler = new GetStrategusSettlementsQuery.Handler(ActDb, Mapper);
            var res = await handler.Handle(new GetStrategusSettlementsQuery(), CancellationToken.None);
            var settlementViews = res.Data!;
            Assert.IsNotNull(settlementViews);
            Assert.AreEqual(2, settlementViews.Count);

            Assert.AreEqual("abc", settlementViews[0].Name);
            Assert.AreEqual("def", settlementViews[1].Name);
        }
    }
}
