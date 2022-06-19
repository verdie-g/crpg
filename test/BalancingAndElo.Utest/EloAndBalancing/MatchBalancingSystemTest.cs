using System.Collections.Generic;
using Crpg.BalancingAndRating.Balancing;
using NUnit.Framework;

namespace Crpg.BalancingAndRating.UTest.Balancing
{
    public class MatchBalancingSystemTest
    {
        [Test]
        public void CaptainShouldNotBeThatBad()
        {
            var matchBalancer = new MatchBalancingSystem();
            var eloSystem = new Rating();
            GameMatch balancedGame = matchBalancer.NaiveCaptainBalancing(game1);
            int teamAMeanElo = eloSystem.ComputeTeamEloPowerMean(balancedGame.TeamA, 1);
            int teamBMeanElo = eloSystem.ComputeTeamEloPowerMean(balancedGame.TeamB, 1);
            double meanEloRatio = (double)teamAMeanElo / (double)teamBMeanElo;
            Assert.AreEqual(meanEloRatio, 1, 0.2);
            int teamAQuadraticMeanElo = eloSystem.ComputeTeamEloPowerMean(balancedGame.TeamA, 2);
            int teamBQuadraticMeanElo = eloSystem.ComputeTeamEloPowerMean(balancedGame.TeamB, 2);
            double quadraticMeanEloRatio = (double)teamAQuadraticMeanElo / (double)teamBQuadraticMeanElo;
            Assert.AreEqual(quadraticMeanEloRatio, 1, 0.2);
        }


        private static Clan lOTR = new(){ Id = 1, Name = "LOTR" };
        private static Clan dBZ =new(){ Id = 2, Name = "DBZ" };
        private static Clan gilead =new(){ Id = 3, Name = "Gilead" };
        private static Clan poudlard =new(){ Id = 4, Name = "Poudlard" };
        private static Clan xMen =new(){ Id = 5, Name = "X-MEN" };
        private static Clan xMenVillain =new(){ Id = 6, Name = "X-MEN Villains" };

        private static User arwen =new(){ Id = 1, Elo = 2000, ClanMembership = new ClanMember { UserId = 1, User = arwen, ClanId = 1, Clan = lOTR } };
        private static User frodon =new(){ Id = 2, Elo = 1600, ClanMembership = new ClanMember { UserId = 2, User = frodon, ClanId = 1, Clan = lOTR } };
        private static User sam =new(){ Id = 3, Elo = 1500, ClanMembership = new ClanMember { UserId = 3, User = sam, ClanId = 1, Clan = lOTR } };
        private static User sangoku =new(){ Id = 4, Elo = 2000, ClanMembership = new ClanMember { UserId = 4, User = sangoku, ClanId = 1, Clan = lOTR } };
        private static User krilin =new(){ Id = 5, Elo = 1000, ClanMembership = new ClanMember { UserId = 5, User = krilin, ClanId = 2, Clan = dBZ } };
        private static User rolandDeschain =new(){ Id = 6, Elo = 2800, ClanMembership = new ClanMember { UserId = 6, User = rolandDeschain, ClanId = 3, Clan = gilead } };
        private static User harryPotter =new(){ Id = 7, Elo = 2000, ClanMembership = new ClanMember { UserId = 7, User = krilin, ClanId = 2, Clan = dBZ } };
        private static User magneto =new(){ Id = 8, Elo = 2700, ClanMembership = new ClanMember { UserId = 8, User = magneto, ClanId = 6, Clan = xMenVillain } };
        private static User profCharles =new(){ Id = 9, Elo = 2800, ClanMembership = new ClanMember { UserId = 9, User = profCharles, ClanId = 5, Clan = xMen } };
        private static User usainBolt =new(){ Id = 10, Elo = 1200 };
        private static User agent007 =new(){ Id = 11, Elo = 1300 };
        private static User spongeBob =new(){ Id = 12, Elo = 800 };
        private static User patrick =new(){ Id = 13, Elo = 500 };
        private static User madonna =new(){ Id = 14, Elo = 1100 };
        private static User laraCroft =new(){ Id = 15, Elo = 3500 };
        private static User jeanneDArc =new(){ Id = 16, Elo = 2400 };
        private static User merlin =new(){ Id = 17, Elo = 2700 };
        private static User bob =new(){ Id = 18, Elo = 1100 };
        private static User thomas =new(){ Id = 19, Elo = 2400 };
        private GameMatch game1 = new()
        {
            TeamA = new List<User> { },
            TeamB = new List<User> { },
            Waiting = new List<User>
            {
                arwen,
                frodon,
                sam,
                sangoku,
                krilin,
                rolandDeschain,
                harryPotter,
                magneto,
                profCharles,
                usainBolt,
                agent007,
                spongeBob,
                patrick,
                madonna,
                laraCroft,
                jeanneDArc,
                merlin,
                bob,
                thomas,
            },
        };
    }
}
