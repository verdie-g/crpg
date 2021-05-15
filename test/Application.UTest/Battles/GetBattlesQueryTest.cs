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
                            Hero = new Hero { Troops = 20, User = new User() },
                        },
                        new BattleFighter
                        {
                            Side = BattleSide.Attacker,
                            Commander = false,
                            Hero = new Hero { Troops = 15, User = new User() },
                        },
                        new BattleFighter
                        {
                            Side = BattleSide.Defender,
                            Commander = false,
                            Hero = new Hero { Troops = 35, User = new User() },
                        },
                        new BattleFighter
                        {
                            Side = BattleSide.Defender,
                            Commander = true,
                            Hero = new Hero { Troops = 10, User = new User() },
                        },
                    },
                },
                new()
                {
                    Region = Region.NorthAmerica,
                    Phase = BattlePhase.Battle,
                    Fighters =
                    {
                        new BattleFighter
                        {
                            Side = BattleSide.Attacker,
                            Commander = true,
                            Hero = new Hero { Troops = 100, User = new User() },
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
                            Hero = new Hero { Troops = 35, User = new User() },
                        },
                    },
                },
                new() { Region = Region.NorthAmerica, Phase = BattlePhase.Preparation },
                new() { Region = Region.Europe, Phase = BattlePhase.Hiring },
                new() { Region = Region.Asia, Phase = BattlePhase.Battle },
                new() { Region = Region.NorthAmerica, Phase = BattlePhase.End },
            };
            ArrangeDb.Battles.AddRange(battles);
            await ArrangeDb.SaveChangesAsync();

            GetBattlesQuery.Handler handler = new(ActDb, Mapper);
            var res = await handler.Handle(new GetBattlesQuery
            {
                Region = Region.NorthAmerica,
                Phases = new[] { BattlePhase.Hiring, BattlePhase.Battle },
            }, CancellationToken.None);

            Assert.IsNull(res.Errors);

            var battlesVm = res.Data!;
            Assert.AreEqual(2, battlesVm.Count);

            Assert.AreEqual(Region.NorthAmerica, battlesVm[0].Region);
            Assert.AreEqual(BattlePhase.Hiring, battlesVm[0].Phase);
            Assert.IsNotNull(battlesVm[0].Attacker);
            Assert.IsNotNull(battlesVm[0].Attacker.Hero);
            Assert.AreEqual(35, battlesVm[0].AttackerTotalTroops);
            Assert.IsNotNull(battlesVm[0].Defender);
            Assert.IsNotNull(battlesVm[0].Defender!.Hero);
            Assert.AreEqual(45, battlesVm[0].DefenderTotalTroops);

            Assert.AreEqual(Region.NorthAmerica, battlesVm[1].Region);
            Assert.AreEqual(BattlePhase.Battle, battlesVm[1].Phase);
            Assert.IsNotNull(battlesVm[1].Attacker);
            Assert.IsNotNull(battlesVm[1].Attacker.Hero);
            Assert.AreEqual(100, battlesVm[1].AttackerTotalTroops);
            Assert.AreEqual(47, battlesVm[1].DefenderTotalTroops);
            Assert.IsNotNull(battlesVm[1].Defender);
            Assert.IsNotNull(battlesVm[1].Defender!.Settlement);
        }
    }
}
