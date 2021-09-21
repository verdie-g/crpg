using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Battles.Queries;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Heroes;
using Crpg.Domain.Entities.Settlements;
using Crpg.Domain.Entities.Users;
using NUnit.Framework;

namespace Crpg.Application.UTest.Battles
{
    public class GetBattlesQueryTest : TestBase
    {
        [Test]
        public async Task ShouldGetBattlesMatchingThePhases()
        {
            Battle[] battles =
            {
                new()
                {
                    Region = Region.NorthAmerica,
                    Phase = BattlePhase.Hiring,
                    Fighters =
                    {
                        new BattleFighter
                        {
                            Side = BattleSide.Attacker,
                            Commander = true,
                            Hero = new Hero { Troops = 20.9f, User = new User() },
                        },
                        new BattleFighter
                        {
                            Side = BattleSide.Attacker,
                            Commander = false,
                            Hero = new Hero { Troops = 15.8f, User = new User() },
                        },
                        new BattleFighter
                        {
                            Side = BattleSide.Defender,
                            Commander = false,
                            Hero = new Hero { Troops = 35.7f, User = new User() },
                        },
                        new BattleFighter
                        {
                            Side = BattleSide.Defender,
                            Commander = true,
                            Hero = new Hero { Troops = 10.6f, User = new User() },
                        },
                    },
                },
                new()
                {
                    Region = Region.NorthAmerica,
                    Phase = BattlePhase.Live,
                    Fighters =
                    {
                        new BattleFighter
                        {
                            Side = BattleSide.Attacker,
                            Commander = true,
                            Hero = new Hero { Troops = 100.5f, User = new User() },
                        },
                        new BattleFighter
                        {
                            Side = BattleSide.Defender,
                            Commander = true,
                            Settlement = new Settlement
                            {
                                Name = "toto",
                                Troops = 12,
                            },
                        },
                        new BattleFighter
                        {
                            Side = BattleSide.Defender,
                            Commander = false,
                            Hero = new Hero { Troops = 35.6f, User = new User() },
                        },
                    },
                },
                new() { Region = Region.NorthAmerica, Phase = BattlePhase.Preparation },
                new() { Region = Region.Europe, Phase = BattlePhase.Hiring },
                new() { Region = Region.Asia, Phase = BattlePhase.Live },
                new() { Region = Region.NorthAmerica, Phase = BattlePhase.End },
            };
            ArrangeDb.Battles.AddRange(battles);
            await ArrangeDb.SaveChangesAsync();

            GetBattlesQuery.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new GetBattlesQuery
            {
                Region = Region.NorthAmerica,
                Phases = new[] { BattlePhase.Hiring, BattlePhase.Live },
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);

            var battlesVm = res.Data!;
            Assert.AreEqual(2, battlesVm.Count);

            Assert.AreEqual(Region.NorthAmerica, battlesVm[0].Region);
            Assert.AreEqual(BattlePhase.Hiring, battlesVm[0].Phase);
            Assert.IsNotNull(battlesVm[0].Attackers);

            foreach (var a in battlesVm[0].Attackers)
            {
                Assert.IsNotNull(a.Hero);
            }

            Assert.AreEqual(35, battlesVm[0].AttackerTotalTroops);
            Assert.IsNotNull(battlesVm[0].Defenders);

            foreach (var d in battlesVm[0].Defenders)
            {
                Assert.IsNotNull(d.Hero);
            }

            Assert.AreEqual(45, battlesVm[0].DefenderTotalTroops);

            Assert.AreEqual(Region.NorthAmerica, battlesVm[1].Region);
            Assert.AreEqual(BattlePhase.Live, battlesVm[1].Phase);
            Assert.IsNotNull(battlesVm[1].Attackers);

            foreach (var a in battlesVm[1].Attackers)
            {
                Assert.IsNotNull(a.Hero);
            }

            Assert.AreEqual(100, battlesVm[1].AttackerTotalTroops);
            Assert.AreEqual(47, battlesVm[1].DefenderTotalTroops);
            Assert.IsNotNull(battlesVm[1].Defenders);

            int defendersSettlementCount = 0;
            int defendersHeroCount = 0;

            foreach (var d in battlesVm[1].Defenders)
            {
                if (d.Settlement != null)
                {
                    defendersSettlementCount++;
                }

                if (d.Hero != null)
                {
                    defendersHeroCount++;
                }
            }

            Assert.That(defendersSettlementCount == 1 && defendersHeroCount == 1);
        }
    }
}
