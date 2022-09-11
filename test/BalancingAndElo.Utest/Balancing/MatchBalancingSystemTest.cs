using System.Collections.Generic;
using Crpg.Module.Balancing;
using NUnit.Framework;

namespace Crpg.Module.UTest.Balancing
{
    public class MatchBalancingSystemTest
    {
        [Test]
        public void CaptainShouldNotBeThatBad()
        {
            var matchBalancer = new MatchBalancingSystem();
            GameMatch balancedGame = matchBalancer.NaiveCaptainBalancing(game1);
            int teamAMeanRating = RatingHelpers.ComputeTeamRatingPowerMean(balancedGame.TeamA, 1);
            int teamBMeanRating = RatingHelpers.ComputeTeamRatingPowerMean(balancedGame.TeamB, 1);
            double meanRatingRatio = (double)teamAMeanRating / (double)teamBMeanRating;
            Assert.AreEqual(meanRatingRatio, 1, 0.2);
            int teamAQuadraticMeanRating = RatingHelpers.ComputeTeamRatingPowerMean(balancedGame.TeamA, 2);
            int teamBQuadraticMeanRating = RatingHelpers.ComputeTeamRatingPowerMean(balancedGame.TeamB, 2);
            double quadraticMeanRatingRatio = (double)teamAQuadraticMeanRating / (double)teamBQuadraticMeanRating;
            Assert.AreEqual(quadraticMeanRatingRatio, 1, 0.2);
        }
        [Test]
        public void KKbalancingRatingsShouldNotBeThatBad()
        {
            var matchBalancer = new MatchBalancingSystem();
            GameMatch balancedGame = matchBalancer.KKBalancing(game1);
            int teamAMeanRating = RatingHelpers.ComputeTeamRatingPowerMean(balancedGame.TeamA, 1);
            int teamBMeanRating = RatingHelpers.ComputeTeamRatingPowerMean(balancedGame.TeamB, 1);
            double meanRatingRatio = (double)teamAMeanRating / (double)teamBMeanRating;
            Assert.AreEqual(meanRatingRatio, 1, 0.2);
            int teamAQuadraticMeanRating = RatingHelpers.ComputeTeamRatingPowerMean(balancedGame.TeamA, 2);
            int teamBQuadraticMeanRating = RatingHelpers.ComputeTeamRatingPowerMean(balancedGame.TeamB, 2);
            double quadraticMeanRatingRatio = (double)teamAQuadraticMeanRating / (double)teamBQuadraticMeanRating;
            Assert.AreEqual(quadraticMeanRatingRatio, 1, 0.2);
        }
        [Test]
        public void KKMakeTeamOfSimilarSizesShouldNotBeThatBad()
        {
            var matchBalancer = new MatchBalancingSystem();
            GameMatch balancedGame = matchBalancer.KKMakeTeamOfSimilarSizes(game1);
            int teamASize = balancedGame.TeamA.Count;
            int teamBSize = balancedGame.TeamB.Count;
            double sizeRatio = (double)teamASize / (double)teamBSize;
            Assert.AreEqual(sizeRatio, 1, 0.2);
        }



        private static Clan lOTR = new(){ Id = 1, Name = "LOTR" };
        private static Clan dBZ =new(){ Id = 2, Name = "DBZ" };
        private static Clan gilead =new(){ Id = 3, Name = "Gilead" };
        private static Clan poudlard =new(){ Id = 4, Name = "Poudlard" };
        private static Clan xMen =new(){ Id = 5, Name = "X-MEN" };
        private static Clan xMenVillain =new(){ Id = 6, Name = "X-MEN Villains" };

        private static User arwen =new(){ Id = 1, Rating = 2000, ClanMembership = new ClanMember { UserId = 1, User = arwen, ClanId = 1, Clan = lOTR } };
        private static User frodon =new(){ Id = 2, Rating = 1600, ClanMembership = new ClanMember { UserId = 2, User = frodon, ClanId = 1, Clan = lOTR } };
        private static User sam =new(){ Id = 3, Rating = 1500, ClanMembership = new ClanMember { UserId = 3, User = sam, ClanId = 1, Clan = lOTR } };
        private static User sangoku =new(){ Id = 4, Rating = 2000, ClanMembership = new ClanMember { UserId = 4, User = sangoku, ClanId = 2, Clan = dBZ } };
        private static User krilin =new(){ Id = 5, Rating = 1000, ClanMembership = new ClanMember { UserId = 5, User = krilin, ClanId = 2, Clan = dBZ } };
        private static User rolandDeschain =new(){ Id = 6, Rating = 2800, ClanMembership = new ClanMember { UserId = 6, User = rolandDeschain, ClanId = 3, Clan = gilead } };
        private static User harryPotter =new(){ Id = 7, Rating = 2000, ClanMembership = new ClanMember { UserId = 7, User = harryPotter, ClanId = 4, Clan = poudlard } };
        private static User ronWeasley = new() { Id = 7, Rating = 600, ClanMembership = new ClanMember { UserId = 7, User = ronWeasley, ClanId = 4, Clan = poudlard } };
        private static User magneto =new(){ Id = 8, Rating = 2700, ClanMembership = new ClanMember { UserId = 8, User = magneto, ClanId = 6, Clan = xMenVillain } };
        private static User profCharles =new(){ Id = 9, Rating = 2800, ClanMembership = new ClanMember { UserId = 9, User = profCharles, ClanId = 5, Clan = xMen } };
        private static User usainBolt =new(){ Id = 10, Rating = 1200 };
        private static User agent007 =new(){ Id = 11, Rating = 1300 };
        private static User spongeBob =new(){ Id = 12, Rating = 800 };
        private static User patrick =new(){ Id = 13, Rating = 500 };
        private static User madonna =new(){ Id = 14, Rating = 1100 };
        private static User laraCroft =new(){ Id = 15, Rating = 3500 };
        private static User jeanneDArc =new(){ Id = 16, Rating = 2400 };
        private static User merlin =new(){ Id = 17, Rating = 2700 };
        private static User bob =new(){ Id = 18, Rating = 1100 };
        private static User thomas =new(){ Id = 19, Rating = 2400 };
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
