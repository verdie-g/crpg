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
            float teamAMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(balancedGame.TeamA, 1);
            float teamBMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(balancedGame.TeamB, 1);
            double meanRatingRatio = (double)teamAMeanRating / (double)teamBMeanRating;
            Assert.AreEqual(meanRatingRatio, 1, 0.2);
            Console.WriteLine("balanced rating ratio = " + meanRatingRatio);
            float teamAQuadraticMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(balancedGame.TeamA, 2);
            float teamBQuadraticMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(balancedGame.TeamB, 2);
            double quadraticMeanRatingRatio = (double)teamAQuadraticMeanRating / (double)teamBQuadraticMeanRating;
            Assert.AreEqual(quadraticMeanRatingRatio, 1, 0.2);
        }
        [Test]
        public void KKbalancingRatingsShouldNotBeThatBad()
        {
            var matchBalancer = new MatchBalancingSystem();
            GameMatch balancedGame = matchBalancer.KKBalancing(game1);
            float teamAMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(balancedGame.TeamA, 1);
            float teamBMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(balancedGame.TeamB, 1);
            double meanRatingRatio = (double)teamAMeanRating / (double)teamBMeanRating;
            Assert.AreEqual(meanRatingRatio, 1, 0.2);
            Console.WriteLine("balanced rating ratio = " + meanRatingRatio);
            float teamAQuadraticMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(balancedGame.TeamA, 2);
            float teamBQuadraticMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(balancedGame.TeamB, 2);
            double quadraticMeanRatingRatio = (double)teamAQuadraticMeanRating / (double)teamBQuadraticMeanRating;
            Assert.AreEqual(quadraticMeanRatingRatio, 1, 0.2);
        }
        [Test]
        public void KKMakeTeamOfSimilarSizesShouldNotBeThatBad()
        {

            var matchBalancer = new MatchBalancingSystem();
            GameMatch balancedGame = matchBalancer.KKMakeTeamOfSimilarSizesWithBannerBalance(game1);
            float teamASize = balancedGame.TeamA.Count;
            float teamBSize = balancedGame.TeamB.Count;
            double sizeRatio = (double)teamASize / (double)teamBSize;
            float teamAMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(balancedGame.TeamA, 1);
            float teamBMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(balancedGame.TeamB, 1);
            double meanRatingRatio = (double)teamAMeanRating / (double)teamBMeanRating;
            Console.WriteLine("balanced rating ratio = " + meanRatingRatio);
            Assert.AreEqual(sizeRatio, 1, 0.2);
        }

        [Test]
        public void BannerBalancing()
        {
            var matchBalancer = new MatchBalancingSystem();

            float unbalancedTeamAMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(game1.TeamA, 1);
            float unbalancedTeamBMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(game1.TeamB, 1);
            double unbalancedMeanRatingRatio = (double)unbalancedTeamAMeanRating / (double)unbalancedTeamBMeanRating;
            Console.WriteLine("unbalanced rating ratio = " + unbalancedMeanRatingRatio);

            GameMatch balancedGame = matchBalancer.BannerBalancing(game1);
            float teamASize = balancedGame.TeamA.Count;
            float teamBSize = balancedGame.TeamB.Count;
            double sizeRatio = (double)teamASize / (double)teamBSize;
            //Assert.AreEqual(sizeRatio, 1, 0.2);
            Console.WriteLine("balanced size ratio = " + sizeRatio);
            float teamARating = RatingHelpers.ComputeTeamRatingPowerSum(balancedGame.TeamA, 1);
            float teamBRating = RatingHelpers.ComputeTeamRatingPowerSum(balancedGame.TeamB, 1);
            double RatingRatio = (double)teamARating / (double)teamBRating;
            Console.WriteLine("teamASize = " + teamASize + " teamBSize = " + teamBSize);
            Console.WriteLine("teamARating = " + teamARating + " teamBRating = " + teamBRating);
            Assert.AreEqual(RatingRatio, 1, 0.2);
        }
        [Test]
        public void BannerBalancingShouldNotSeperateClanMember()
        {
            var matchBalancer = new MatchBalancingSystem();

            float unbalancedTeamAMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(game1.TeamA, 1);
            float unbalancedTeamBMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(game1.TeamB, 1);
            double unbalancedMeanRatingRatio = (double)unbalancedTeamAMeanRating / (double)unbalancedTeamBMeanRating;
            Console.WriteLine("unbalanced rating ratio = " + unbalancedMeanRatingRatio);

            GameMatch balancedGame = matchBalancer.BannerBalancing(game1);
            foreach (User u in game1.TeamA)
            {
                if(u.ClanMembership != null)
                {
                    foreach (User u2 in game1.TeamB)
                    {
                        if(u2.ClanMembership != null)
                        {
                            Assert.AreNotEqual(u.ClanMembership.ClanId, u2.ClanMembership.ClanId);
                        }
                    }
                }
            }
        }


        [Test]
        public void PowerMeanShouldWork()
        {
            List<float> floats = new()
            {
                0,0,5,5,10,10
            };
            Console.WriteLine(MathHelper.PowerMean(floats,1f));
            Assert.AreEqual(MathHelper.PowerMean(floats, 1f), 5, 0.01);

        }


        private static Clan lOTR = new() { Id = 1, Name = "LOTR" };
        private static Clan dBZ = new() { Id = 2, Name = "DBZ" };
        private static Clan gilead = new() { Id = 3, Name = "Gilead" };
        private static Clan poudlard = new() { Id = 4, Name = "Poudlard" };
        private static Clan xMen = new() { Id = 5, Name = "X-MEN" };
        private static Clan xMenVillain = new() { Id = 6, Name = "X-MEN Villains" };
        private static Clan JeanJean = new() { Id = 7, Name = "JeanJean" };
        private static Clan Glut = new() { Id = 8, Name = "Glut" };
        private static Clan Vlex = new() { Id = 9, Name = "Vlex" };
        private static Clan Hudahut = new() { Id = 10, Name = "Hudahut" };


        private static User arwen = new() { Id = 1, Rating = 2000, ClanMembership = new ClanMember { UserId = 1, User = arwen, ClanId = 1, Clan = lOTR } };
        private static User frodon = new() { Id = 2, Rating = 1600, ClanMembership = new ClanMember { UserId = 2, User = frodon, ClanId = 1, Clan = lOTR } };
        private static User sam = new() { Id = 3, Rating = 1500, ClanMembership = new ClanMember { UserId = 3, User = sam, ClanId = 1, Clan = lOTR } };
        private static User sangoku = new() { Id = 4, Rating = 2000, ClanMembership = new ClanMember { UserId = 4, User = sangoku, ClanId = 2, Clan = dBZ } };
        private static User krilin = new() { Id = 5, Rating = 1000, ClanMembership = new ClanMember { UserId = 5, User = krilin, ClanId = 2, Clan = dBZ } };
        private static User rolandDeschain = new() { Id = 6, Rating = 2800, ClanMembership = new ClanMember { UserId = 6, User = rolandDeschain, ClanId = 3, Clan = gilead } };
        private static User harryPotter = new() { Id = 7, Rating = 2000, ClanMembership = new ClanMember { UserId = 7, User = harryPotter, ClanId = 4, Clan = poudlard } };
        private static User magneto = new() { Id = 8, Rating = 2700, ClanMembership = new ClanMember { UserId = 8, User = magneto, ClanId = 6, Clan = xMenVillain } };
        private static User profCharles = new() { Id = 9, Rating = 2800, ClanMembership = new ClanMember { UserId = 9, User = profCharles, ClanId = 5, Clan = xMen } };
        private static User usainBolt = new() { Id = 10, Rating = 1200 };
        private static User agent007 = new() { Id = 11, Rating = 1300 };
        private static User spongeBob = new() { Id = 12, Rating = 800 };
        private static User patrick = new() { Id = 13, Rating = 500 };
        private static User madonna = new() { Id = 14, Rating = 1100 };
        private static User laraCroft = new() { Id = 15, Rating = 3500 };
        private static User jeanneDArc = new() { Id = 16, Rating = 2400 };
        private static User merlin = new() { Id = 17, Rating = 2700 };
        private static User bob = new() { Id = 18, Rating = 1100 };
        private static User thomas = new() { Id = 19, Rating = 2400 };
        private static User ronWeasley = new() { Id = 20, Rating = 600, ClanMembership = new ClanMember { UserId = 7, User = ronWeasley, ClanId = 4, Clan = poudlard } };
        private static User Jean_01 = new() { Id = 21, Rating = 3000, ClanMembership = new ClanMember { UserId = 21, User = Jean_01, ClanId = 7, Clan = JeanJean } };
        private static User Jean_02 = new() { Id = 22, Rating = 2500, ClanMembership = new ClanMember { UserId = 22, User = Jean_02, ClanId = 7, Clan = JeanJean } };
        private static User Jean_03 = new() { Id = 23, Rating = 2100, ClanMembership = new ClanMember { UserId = 23, User = Jean_03, ClanId = 7, Clan = JeanJean } };
        private static User Jean_04 = new() { Id = 24, Rating = 1200, ClanMembership = new ClanMember { UserId = 24, User = Jean_04, ClanId = 7, Clan = JeanJean } };
        private static User Jean_05 = new() { Id = 25, Rating = 800, ClanMembership = new ClanMember { UserId = 25, User = Jean_05, ClanId = 7, Clan = JeanJean } };
        private static User Glutentag_01 = new() { Id = 26, Rating = 900, ClanMembership = new ClanMember { UserId = 26, User = Glutentag_01, ClanId = 8, Clan = Glut } };
        private static User Glutentag_02 = new() { Id = 27, Rating = 200, ClanMembership = new ClanMember { UserId = 27, User = Glutentag_02, ClanId = 8, Clan = Glut } };
        private static User Glutentag_03 = new() { Id = 28, Rating = 2200, ClanMembership = new ClanMember { UserId = 28, User = Glutentag_03, ClanId = 8, Clan = Glut } };
        private static User Glutentag_04 = new() { Id = 29, Rating = 400, ClanMembership = new ClanMember { UserId = 29, User = Glutentag_04, ClanId = 8, Clan = Glut } };
        private static User Glutentag_05 = new() { Id = 30, Rating = 800, ClanMembership = new ClanMember { UserId = 30, User = Glutentag_05, ClanId = 8, Clan = Glut } };
        private static User Vlexance_01 = new() { Id = 31, Rating = 2600, ClanMembership = new ClanMember { UserId = 31, User = Vlexance_01, ClanId = 9, Clan = Vlex } };
        private static User Vlexance_02 = new() { Id = 32, Rating = 2300, ClanMembership = new ClanMember { UserId = 32, User = Vlexance_02, ClanId = 9, Clan = Vlex } };
        private static User Vlexance_03 = new() { Id = 33, Rating = 1300, ClanMembership = new ClanMember { UserId = 33, User = Vlexance_03, ClanId = 9, Clan = Vlex } };
        private static User Vlexance_04 = new() { Id = 34, Rating = 1100, ClanMembership = new ClanMember { UserId = 34, User = Vlexance_04, ClanId = 9, Clan = Vlex } };
        private static User Vlexance_05 = new() { Id = 35, Rating = 300, ClanMembership = new ClanMember { UserId = 35, User = Vlexance_05, ClanId = 9, Clan = Vlex } };
        private static User Hudax_01 = new() { Id = 36, Rating = 1900, ClanMembership = new ClanMember { UserId = 36, User = Hudax_01, ClanId = 10, Clan = Hudahut } };
        private static User Hudax_02 = new() { Id = 37, Rating = 1700, ClanMembership = new ClanMember { UserId = 37, User = Hudax_02, ClanId = 10, Clan = Hudahut } };
        private static User Hudax_03 = new() { Id = 38, Rating = 1300, ClanMembership = new ClanMember { UserId = 38, User = Hudax_03, ClanId = 10, Clan = Hudahut } };
        private static User Hudax_04 = new() { Id = 39, Rating = 1400, ClanMembership = new ClanMember { UserId = 39, User = Hudax_04, ClanId = 10, Clan = Hudahut } };
        private static User Hudax_05 = new() { Id = 40, Rating = 700, ClanMembership = new ClanMember { UserId = 40, User = Hudax_05, ClanId = 10, Clan = Hudahut } };
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
                ronWeasley,
                Jean_01,
                Jean_02,
                Jean_03,
                Jean_04,
                Jean_05,
                Glutentag_01,
                Glutentag_02,
                Glutentag_03,
                Glutentag_04,
                Glutentag_05,
                Vlexance_01,
                Vlexance_02,
                Vlexance_03,
                Vlexance_04,
                Vlexance_05,
                Hudax_01,
                Hudax_02,
                Hudax_03,
                Hudax_04,
                Hudax_05
            },
        };
    }
}
