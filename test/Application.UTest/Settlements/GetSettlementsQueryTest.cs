using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Settlements.Queries;
using Crpg.Domain.Entities.Settlements;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace Crpg.Application.UTest.Settlements
{
    public class GetSettlementsQueryTest : TestBase
    {
        [Test]
        public async Task ShouldReturnAllSettlements()
        {
            var settlements = new[]
            {
                new Settlement
                {
                    Name = "abc",
                    Type = SettlementType.Village,
                    Position = new Point(5, 6),
                    Scene = "battania_village",
                },
                new Settlement
                {
                    Name = "def",
                    Type = SettlementType.Castle,
                    Position = new Point(7, 8),
                    Scene = "sturgia_castle",
                },
            };
            ArrangeDb.Settlements.AddRange(settlements);
            await ArrangeDb.SaveChangesAsync();

            GetSettlementsQuery.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new GetSettlementsQuery(), CancellationToken.None);
            var settlementViews = res.Data!;
            Assert.IsNotNull(settlementViews);
            Assert.AreEqual(2, settlementViews.Count);

            Assert.AreEqual("abc", settlementViews[0].Name);
            Assert.AreEqual("def", settlementViews[1].Name);
        }
    }
}
