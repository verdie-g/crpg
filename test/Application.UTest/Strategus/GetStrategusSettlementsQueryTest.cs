using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Strategus.Queries;
using Crpg.Domain.Entities.Clans;
using Crpg.Domain.Entities.Strategus;
using Crpg.Domain.Entities.Users;
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
                    Owner = new StrategusUser
                    {
                        User = new User
                        {
                            Name = "u",
                            ClanMembership = new ClanMember { Clan = new Clan { Name = "c" } },
                        },
                    },
                },
                new StrategusSettlement
                {
                    Name = "def",
                    Type = StrategusSettlementType.Castle,
                    Position = new Point(7, 8),
                    Scene = "sturgia_castle",
                }
            };
            ArrangeDb.StrategusSettlements.AddRange(settlements);
            await ArrangeDb.SaveChangesAsync();

            var handler = new GetStrategusSettlementsQuery.Handler(ActDb, Mapper);
            var res = await handler.Handle(new GetStrategusSettlementsQuery(), CancellationToken.None);
            var settlementViews = res.Data!;
            Assert.IsNotNull(settlementViews);
            Assert.AreEqual(2, settlementViews.Count);

            Assert.AreEqual("abc", settlementViews[0].Name);
            Assert.AreEqual("u", settlementViews[0].Owner!.Name);
            Assert.AreEqual("c", settlementViews[0].Owner!.Clan!.Name);


            Assert.AreEqual("def", settlementViews[1].Name);
            Assert.IsNull(settlementViews[1].Owner);
        }
    }
}
