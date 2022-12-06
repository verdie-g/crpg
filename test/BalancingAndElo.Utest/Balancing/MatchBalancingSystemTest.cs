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

            GameMatch balancedGame = matchBalancer.PureBannerBalancing(game1);
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
        public void BannerBalancingWithEdgeCase()
        {
            var matchBalancer = new MatchBalancingSystem();

            float unbalancedTeamAMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(game1.TeamA, 1);
            float unbalancedTeamBMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(game1.TeamB, 1);
            double unbalancedMeanRatingRatio = (double)unbalancedTeamAMeanRating / (double)unbalancedTeamBMeanRating;
            Console.WriteLine("unbalanced rating ratio = " + unbalancedMeanRatingRatio);

            GameMatch balancedGame = matchBalancer.BannerBalancingWithEdgeCases(game1);
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

            GameMatch balancedGame = matchBalancer.PureBannerBalancing(game1);
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
        private static Clan BlackBaronesses = new() { Id = 11, Name = "Black Baronesses" };
        private static Clan NavyKnights = new() { Id = 12, Name = "Navy Knights" };
        private static Clan PurplePeasants = new() { Id = 13, Name = "Purple Peasants" };
        private static Clan RedRitters = new() { Id = 14, Name = "Red Ritters" };
        private static Clan LemonLevies = new() { Id = 15, Name = "Lemon Levies" };
        private static Clan ScarletShieldmaidens = new() { Id = 16, Name = "Scarlet Shieldmaidens" };
        private static Clan DUMPSTERS = new() { Id = 50, Name = "DUMPSTERS" };
        private static Clan TRASHCANS = new() { Id = 51, Name = "TRASHCANS" };
        private static Clan SCRUBS = new() { Id = 52, Name = "SCRUBS" };
        private static Clan GARBAGE = new() { Id = 53, Name = "GARBAGE" };
        private static Clan BASURA = new() { Id = 54, Name = "BASURA" };
        private static Clan WASTE = new() { Id = 55, Name = "WASTE" };
        private static Clan BADS = new() { Id = 56, Name = "BADS" };
        private static Clan POORS = new() { Id = 57, Name = "POORS" };
        private static Clan PEASANTRY = new() { Id = 58, Name = "PEASANTRY" };
        private static Clan SERFS = new() { Id = 59, Name = "SERFS" };
        private static Clan VAGABONDS = new() { Id = 60, Name = "VAGABONDS" };


        private static User arwen = new() {Name = "Arwen",  Id = 1, Rating = 2000, ClanMembership = new ClanMember { UserId = 1, User = arwen, ClanId = 1, Clan = lOTR } };
        private static User frodon = new() { Name = "Frodon", Id = 2, Rating = 1600, ClanMembership = new ClanMember { UserId = 2, User = frodon, ClanId = 1, Clan = lOTR } };
        private static User sam = new() { Name = "Sam", Id = 3, Rating = 1500, ClanMembership = new ClanMember { UserId = 3, User = sam, ClanId = 1, Clan = lOTR } };
        private static User sangoku = new() { Name = "Sangoku", Id = 4, Rating = 2000, ClanMembership = new ClanMember { UserId = 4, User = sangoku, ClanId = 2, Clan = dBZ } };
        private static User krilin = new() { Name = "Krilin", Id = 5, Rating = 1000, ClanMembership = new ClanMember { UserId = 5, User = krilin, ClanId = 2, Clan = dBZ } };
        private static User rolandDeschain = new() { Name = "Roland Deschain", Id = 6, Rating = 2800, ClanMembership = new ClanMember { UserId = 6, User = rolandDeschain, ClanId = 3, Clan = gilead } };
        private static User harryPotter = new() { Name = "Harry Potter", Id = 7, Rating = 2000, ClanMembership = new ClanMember { UserId = 7, User = harryPotter, ClanId = 4, Clan = poudlard } };
        private static User magneto = new() { Name = "Magneto", Id = 8, Rating = 2700, ClanMembership = new ClanMember { UserId = 8, User = magneto, ClanId = 6, Clan = xMenVillain } };
        private static User profCharles = new() { Name = "Professor Charles", Id = 9, Rating = 2800, ClanMembership = new ClanMember { UserId = 9, User = profCharles, ClanId = 5, Clan = xMen } };
        private static User usainBolt = new() { Name = "Usain Bolt", Id = 10, Rating = 1200 };
        private static User agent007 = new() { Name = "Agent 007", Id = 11, Rating = 1300 };
        private static User spongeBob = new() { Name = "SpongeBob", Id = 12, Rating = 800 };
        private static User patrick = new() { Name = "Patrick", Id = 13, Rating = 500 };
        private static User madonna = new() { Name = "Madonna", Id = 14, Rating = 1100 };
        private static User laraCroft = new() { Name = "Lara Croft", Id = 15, Rating = 3500 };
        private static User jeanneDArc = new() { Name = "Jeanne D'ARC", Id = 16, Rating = 2400 };
        private static User merlin = new() { Name = "Merlin", Id = 17, Rating = 2700 };
        private static User bob = new() { Name = "Bob", Id = 18, Rating = 1100 };
        private static User thomas = new() { Name = "Thomas", Id = 19, Rating = 2400 };
        private static User ronWeasley = new() { Name = "Ron Weasley", Id = 20, Rating = 600, ClanMembership = new ClanMember { UserId = 7, User = ronWeasley, ClanId = 4, Clan = poudlard } };
        private static User Jean_01 = new() { Name = "Jean_01", Id = 21, Rating = 3000, ClanMembership = new ClanMember { UserId = 21, User = Jean_01, ClanId = 7, Clan = JeanJean } };
        private static User Jean_02 = new() { Name = "Jean_02", Id = 22, Rating = 2500, ClanMembership = new ClanMember { UserId = 22, User = Jean_02, ClanId = 7, Clan = JeanJean } };
        private static User Jean_03 = new() { Name = "Jean_03", Id = 23, Rating = 2100, ClanMembership = new ClanMember { UserId = 23, User = Jean_03, ClanId = 7, Clan = JeanJean } };
        private static User Jean_04 = new() { Name = "Jean_04", Id = 24, Rating = 1200, ClanMembership = new ClanMember { UserId = 24, User = Jean_04, ClanId = 7, Clan = JeanJean } };
        private static User Jean_05 = new() { Name = "Jean_05", Id = 25, Rating = 800, ClanMembership = new ClanMember { UserId = 25, User = Jean_05, ClanId = 7, Clan = JeanJean } };
        private static User Glutentag_01 = new() { Name = "Glutentag_01", Id = 26, Rating = 900, ClanMembership = new ClanMember { UserId = 26, User = Glutentag_01, ClanId = 8, Clan = Glut } };
        private static User Glutentag_02 = new() { Name = "Glutentag_02", Id = 27, Rating = 200, ClanMembership = new ClanMember { UserId = 27, User = Glutentag_02, ClanId = 8, Clan = Glut } };
        private static User Glutentag_03 = new() { Name = "Glutentag_03", Id = 28, Rating = 2200, ClanMembership = new ClanMember { UserId = 28, User = Glutentag_03, ClanId = 8, Clan = Glut } };
        private static User Glutentag_04 = new() { Name = "Glutentag_04", Id = 29, Rating = 400, ClanMembership = new ClanMember { UserId = 29, User = Glutentag_04, ClanId = 8, Clan = Glut } };
        private static User Glutentag_05 = new() { Name = "Glutentag_05", Id = 30, Rating = 800, ClanMembership = new ClanMember { UserId = 30, User = Glutentag_05, ClanId = 8, Clan = Glut } };
        private static User Vlexance_01 = new() { Name = "Vlexance_01", Id = 31, Rating = 2600, ClanMembership = new ClanMember { UserId = 31, User = Vlexance_01, ClanId = 9, Clan = Vlex } };
        private static User Vlexance_02 = new() { Name = "Vlexance_02", Id = 32, Rating = 2300, ClanMembership = new ClanMember { UserId = 32, User = Vlexance_02, ClanId = 9, Clan = Vlex } };
        private static User Vlexance_03 = new() { Name = "Vlexance_03", Id = 33, Rating = 1300, ClanMembership = new ClanMember { UserId = 33, User = Vlexance_03, ClanId = 9, Clan = Vlex } };
        private static User Vlexance_04 = new() { Name = "Vlexance_04", Id = 34, Rating = 1100, ClanMembership = new ClanMember { UserId = 34, User = Vlexance_04, ClanId = 9, Clan = Vlex } };
        private static User Vlexance_05 = new() { Name = "Vlexance_05", Id = 35, Rating = 300, ClanMembership = new ClanMember { UserId = 35, User = Vlexance_05, ClanId = 9, Clan = Vlex } };
        private static User Hudax_01 = new() { Name = "Hudax_01", Id = 36, Rating = 1100, ClanMembership = new ClanMember { UserId = 36, User = Hudax_01, ClanId = 10, Clan = Hudahut } };
        private static User Hudax_02 = new() { Name = "Hudax_02", Id = 37, Rating = 2900, ClanMembership = new ClanMember { UserId = 37, User = Hudax_02, ClanId = 10, Clan = Hudahut } };
        private static User Hudax_03 = new() { Name = "Hudax_03", Id = 38, Rating = 1700, ClanMembership = new ClanMember { UserId = 38, User = Hudax_03, ClanId = 10, Clan = Hudahut } };
        private static User Hudax_04 = new() { Name = "Hudax_04", Id = 39, Rating = 1500, ClanMembership = new ClanMember { UserId = 39, User = Hudax_04, ClanId = 10, Clan = Hudahut } };
        private static User Hudax_05 = new() { Name = "Hudax_05", Id = 40, Rating = 2200, ClanMembership = new ClanMember { UserId = 40, User = Hudax_05, ClanId = 10, Clan = Hudahut } };
        private static User Hudax_06 = new() { Name = "", Id = 36, Rating = 1900, ClanMembership = new ClanMember { UserId = 436, User = Hudax_01, ClanId = 10, Clan = Hudahut } };
        private static User Hudax_07 = new() { Name = "", Id = 37, Rating = 1700, ClanMembership = new ClanMember { UserId = 437, User = Hudax_02, ClanId = 10, Clan = Hudahut } };
        private static User Hudax_08 = new() { Name = "", Id = 38, Rating = 1300, ClanMembership = new ClanMember { UserId = 438, User = Hudax_03, ClanId = 10, Clan = Hudahut } };
        private static User Hudax_09 = new() { Name = "", Id = 39, Rating = 1400, ClanMembership = new ClanMember { UserId = 439, User = Hudax_04, ClanId = 10, Clan = Hudahut } };
        private static User Hudax_10 = new() { Name = "", Id = 40, Rating = 700, ClanMembership = new ClanMember { UserId = 440, User = Hudax_05, ClanId = 10, Clan = Hudahut } };
        private static User GerryShepherd = new() { Id = 41, Rating = 2000, ClanMembership = new ClanMember { UserId = 41, User = GerryShepherd, ClanId = 11, Clan = BlackBaronesses } };
        private static User BullyDog = new() { Id = 42, Rating = 1600, ClanMembership = new ClanMember { UserId = 42, User = BullyDog, ClanId = 11, Clan = BlackBaronesses } };
        private static User LabbyRetriever = new() { Id = 43, Rating = 1500, ClanMembership = new ClanMember { UserId = 43, User = LabbyRetriever, ClanId = 11, Clan = BlackBaronesses } };
        private static User GoldyRetriever = new() { Id = 44, Rating = 2000, ClanMembership = new ClanMember { UserId = 44, User = GoldyRetriever, ClanId = 12, Clan = NavyKnights } };
        private static User SibbyHusky = new() { Id = 45, Rating = 1000, ClanMembership = new ClanMember { UserId = 45, User = SibbyHusky, ClanId = 12, Clan = NavyKnights } };
        private static User Poodlums = new() { Id = 46, Rating = 2800, ClanMembership = new ClanMember { UserId = 46, User = Poodlums, ClanId = 13, Clan = PurplePeasants } };
        private static User BordyCollie = new() { Id = 47, Rating = 2000, ClanMembership = new ClanMember { UserId = 47, User = BordyCollie, ClanId = 14, Clan = RedRitters } };
        private static User Rottyweiler = new() { Id = 48, Rating = 2700, ClanMembership = new ClanMember { UserId = 48, User = Rottyweiler, ClanId = 15, Clan = LemonLevies } };
        private static User Daschyhund = new() { Id = 49, Rating = 2800, ClanMembership = new ClanMember { UserId = 49, User = Daschyhund, ClanId = 16, Clan = ScarletShieldmaidens } };
        private static User GreatieDane = new() { Id = 50, Rating = 1200 };
        private static User YorkyTerrier = new() { Id = 51, Rating = 1300 };
        private static User CockySpaniel = new() { Id = 52, Rating = 800 };
        private static User Pomyranian = new() { Id = 53, Rating = 500 };
        private static User Bullymastiff = new() { Id = 54, Rating = 1100 };
        private static User JackyRussell = new() { Id = 55, Rating = 3500 };
        private static User Akitayinu = new() { Id = 56, Rating = 2400 };
        private static User Maltiepoo = new() { Id = 57, Rating = 2700 };
        private static User Doberymann = new() { Id = 58, Rating = 1100 };
        private static User Sheeiitzu = new() { Id = 59, Rating = 2400 };
        private static User BassetyHound = new() { Id = 60, Rating = 600, ClanMembership = new ClanMember { UserId = 60, User = BassetyHound, ClanId = 14, Clan = RedRitters } };
        private static User GopherSnakeWeb = new() { Id = 1000, Rating = 819, ClanMembership = new ClanMember { UserId = 1000, User = GopherSnakeWeb, ClanId = 58, Clan = PEASANTRY } };
        private static User AmbushSword = new() { Id = 1001, Rating = 2019, ClanMembership = new ClanMember { UserId = 1001, User = AmbushSword, ClanId = 50, Clan = DUMPSTERS } };
        private static User FencingPacMan = new() { Id = 1002, Rating = 738, ClanMembership = new ClanMember { UserId = 1002, User = FencingPacMan, ClanId = 53, Clan = GARBAGE } };
        private static User EbonSalient = new() { Id = 1003, Rating = 1381, ClanMembership = new ClanMember { UserId = 1003, User = EbonSalient, ClanId = 52, Clan = SCRUBS } };
        private static User CannonSnaky = new() { Id = 1004, Rating = 2140, ClanMembership = new ClanMember { UserId = 1004, User = CannonSnaky, ClanId = 55, Clan = WASTE } };
        private static User DarklyWine = new() { Id = 1005, Rating = 2295, ClanMembership = new ClanMember { UserId = 1005, User = DarklyWine, ClanId = 52, Clan = SCRUBS } };
        private static User BonfireQuillon = new() { Id = 1006, Rating = 2304, ClanMembership = new ClanMember { UserId = 1006, User = BonfireQuillon, ClanId = 52, Clan = SCRUBS } };
        private static User BunnySlopeStationHouse = new() { Id = 1007, Rating = 2067, ClanMembership = new ClanMember { UserId = 1007, User = BunnySlopeStationHouse, ClanId = 50, Clan = DUMPSTERS } };
        private static User BridgeheadRattlesnake = new() { Id = 1008, Rating = 1809, ClanMembership = new ClanMember { UserId = 1008, User = BridgeheadRattlesnake, ClanId = 54, Clan = BASURA } };
        private static User InfernoSunless = new() { Id = 1009, Rating = 1765, ClanMembership = new ClanMember { UserId = 1009, User = InfernoSunless, ClanId = 56, Clan = BADS } };
        private static User BarricadePrince = new() { Id = 1010, Rating = 2150, ClanMembership = new ClanMember { UserId = 1010, User = BarricadePrince, ClanId = 52, Clan = SCRUBS } };
        private static User FoulKingdom = new() { Id = 1011, Rating = 1718, ClanMembership = new ClanMember { UserId = 1011, User = FoulKingdom, ClanId = 51, Clan = TRASHCANS } };
        private static User DarknessJoeBlake = new() { Id = 1012, Rating = 2499, ClanMembership = new ClanMember { UserId = 1012, User = DarknessJoeBlake, ClanId = 57, Clan = POORS } };
        private static User ExtinguisherPommel = new() { Id = 1013, Rating = 1791, ClanMembership = new ClanMember { UserId = 1013, User = ExtinguisherPommel, ClanId = 60, Clan = VAGABONDS } };
        private static User CaliberKingship = new() { Id = 1014, Rating = 692, ClanMembership = new ClanMember { UserId = 1014, User = CaliberKingship, ClanId = 50, Clan = DUMPSTERS } };
        private static User FirelightSalvo = new() { Id = 1015, Rating = 2226, ClanMembership = new ClanMember { UserId = 1015, User = FirelightSalvo, ClanId = 51, Clan = TRASHCANS } };
        private static User GarnetMonarch = new() { Id = 1016, Rating = 1075, ClanMembership = new ClanMember { UserId = 1016, User = GarnetMonarch, ClanId = 51, Clan = TRASHCANS } };
        private static User EdgedKatana = new() { Id = 1017, Rating = 2335, ClanMembership = new ClanMember { UserId = 1017, User = EdgedKatana, ClanId = 60, Clan = VAGABONDS } };
        private static User AntichristKnife = new() { Id = 1018, Rating = 2743, ClanMembership = new ClanMember { UserId = 1018, User = AntichristKnife, ClanId = 50, Clan = DUMPSTERS } };
        private static User DarkenThrust = new() { Id = 1019, Rating = 1084, ClanMembership = new ClanMember { UserId = 1019, User = DarkenThrust, ClanId = 52, Clan = SCRUBS } };
        private static User AnaphylacticShockLowering = new() { Id = 1020, Rating = 1969, ClanMembership = new ClanMember { UserId = 1020, User = AnaphylacticShockLowering, ClanId = 55, Clan = WASTE } };
        private static User ApprenticeSpottedAdder = new() { Id = 1021, Rating = 2189, ClanMembership = new ClanMember { UserId = 1021, User = ApprenticeSpottedAdder, ClanId = 57, Clan = POORS } };
        private static User DrawTreacle = new() { Id = 1022, Rating = 2215, ClanMembership = new ClanMember { UserId = 1022, User = DrawTreacle, ClanId = 59, Clan = SERFS } };
        private static User AglyphousObscure = new() { Id = 1023, Rating = 2381, ClanMembership = new ClanMember { UserId = 1023, User = AglyphousObscure, ClanId = 52, Clan = SCRUBS } };
        private static User BackfangedWalk = new() { Id = 1024, Rating = 1341, ClanMembership = new ClanMember { UserId = 1024, User = BackfangedWalk, ClanId = 54, Clan = BASURA } };
        private static User ArachnomorphaeScathe = new() { Id = 1025, Rating = 2160, ClanMembership = new ClanMember { UserId = 1025, User = ArachnomorphaeScathe, ClanId = 55, Clan = WASTE } };
        private static User DisenvenomShadowy = new() { Id = 1026, Rating = 2330, ClanMembership = new ClanMember { UserId = 1026, User = DisenvenomShadowy, ClanId = 54, Clan = BASURA } };
        private static User BroadswordKick = new() { Id = 1027, Rating = 2978, ClanMembership = new ClanMember { UserId = 1027, User = BroadswordKick, ClanId = 53, Clan = GARBAGE } };
        private static User DuskNovelist = new() { Id = 1028, Rating = 729, ClanMembership = new ClanMember { UserId = 1028, User = DuskNovelist, ClanId = 50, Clan = DUMPSTERS } };
        private static User PinkPanther = new() { Id = 1029, Rating = 2854, ClanMembership = new ClanMember { UserId = 1029, User = PinkPanther, ClanId = 56, Clan = BADS } };
        private static User DirkSubfusc = new() { Id = 1030, Rating = 2423, ClanMembership = new ClanMember { UserId = 1030, User = DirkSubfusc, ClanId = 57, Clan = POORS } };
        private static User FireServiceProbationer = new() { Id = 1031, Rating = 1800, ClanMembership = new ClanMember { UserId = 1031, User = FireServiceProbationer, ClanId = 60, Clan = VAGABONDS } };
        private static User BetrayPrehensor = new() { Id = 1032, Rating = 2739, ClanMembership = new ClanMember { UserId = 1032, User = BetrayPrehensor, ClanId = 50, Clan = DUMPSTERS } };
        private static User FlaskTigerSnake = new() { Id = 1033, Rating = 1737, ClanMembership = new ClanMember { UserId = 1033, User = FlaskTigerSnake, ClanId = 52, Clan = SCRUBS } };
        private static User BeginnerPlatypus = new() { Id = 1034, Rating = 2472, ClanMembership = new ClanMember { UserId = 1034, User = BeginnerPlatypus, ClanId = 51, Clan = TRASHCANS } };
        private static User BushmasterSteel = new() { Id = 1035, Rating = 1930, ClanMembership = new ClanMember { UserId = 1035, User = BushmasterSteel, ClanId = 58, Clan = PEASANTRY } };
        private static User BreechIron = new() { Id = 1036, Rating = 513, ClanMembership = new ClanMember { UserId = 1036, User = BreechIron, ClanId = 50, Clan = DUMPSTERS } };
        private static User BarbecueLivid = new() { Id = 1037, Rating = 1065, ClanMembership = new ClanMember { UserId = 1037, User = BarbecueLivid, ClanId = 59, Clan = SERFS } };
        private static User InfantRinkhals = new() { Id = 1038, Rating = 1612, ClanMembership = new ClanMember { UserId = 1038, User = InfantRinkhals, ClanId = 51, Clan = TRASHCANS } };
        private static User AtterStranger = new() { Id = 1039, Rating = 2987, ClanMembership = new ClanMember { UserId = 1039, User = AtterStranger, ClanId = 60, Clan = VAGABONDS } };
        private static User BanditKrait = new() { Id = 1040, Rating = 2313, ClanMembership = new ClanMember { UserId = 1040, User = BanditKrait, ClanId = 51, Clan = TRASHCANS } };
        private static User IntelligenceMatchless = new() { Id = 1041, Rating = 2064, ClanMembership = new ClanMember { UserId = 1041, User = IntelligenceMatchless, ClanId = 50, Clan = DUMPSTERS } };
        private static User GrillMuzzle = new() { Id = 1042, Rating = 555, ClanMembership = new ClanMember { UserId = 1042, User = GrillMuzzle, ClanId = 52, Clan = SCRUBS } };
        private static User BombinateTwo = new() { Id = 1043, Rating = 2778, ClanMembership = new ClanMember { UserId = 1043, User = BombinateTwo, ClanId = 58, Clan = PEASANTRY } };
        private static User GunRapid = new() { Id = 1044, Rating = 1269, ClanMembership = new ClanMember { UserId = 1044, User = GunRapid, ClanId = 58, Clan = PEASANTRY } };
        private static User FlameproofReprisal = new() { Id = 1045, Rating = 631, ClanMembership = new ClanMember { UserId = 1045, User = FlameproofReprisal, ClanId = 60, Clan = VAGABONDS } };
        private static User FullerMoccasin = new() { Id = 1046, Rating = 2547, ClanMembership = new ClanMember { UserId = 1046, User = FullerMoccasin, ClanId = 51, Clan = TRASHCANS } };
        private static User HarassSmokeScreen = new() { Id = 1047, Rating = 2266, ClanMembership = new ClanMember { UserId = 1047, User = HarassSmokeScreen, ClanId = 55, Clan = WASTE } };
        private static User CyanoSax = new() { Id = 1048, Rating = 1456, ClanMembership = new ClanMember { UserId = 1048, User = CyanoSax, ClanId = 50, Clan = DUMPSTERS } };
        private static User DarksomeSwivel = new() { Id = 1049, Rating = 1458, ClanMembership = new ClanMember { UserId = 1049, User = DarksomeSwivel, ClanId = 53, Clan = GARBAGE } };
        private static User CounterspyMamba = new() { Id = 1050, Rating = 1223, ClanMembership = new ClanMember { UserId = 1050, User = CounterspyMamba, ClanId = 59, Clan = SERFS } };
        private static User FirewardRingedWaterSnake = new() { Id = 1051, Rating = 2477, ClanMembership = new ClanMember { UserId = 1051, User = FirewardRingedWaterSnake, ClanId = 51, Clan = TRASHCANS } };
        private static User CombustMurky = new() { Id = 1052, Rating = 2812, ClanMembership = new ClanMember { UserId = 1052, User = CombustMurky, ClanId = 53, Clan = GARBAGE } };
        private static User AlightRoyal = new() { Id = 1053, Rating = 1850, ClanMembership = new ClanMember { UserId = 1053, User = AlightRoyal, ClanId = 53, Clan = GARBAGE } };
        private static User HandgunStrafe = new() { Id = 1054, Rating = 1086, ClanMembership = new ClanMember { UserId = 1054, User = HandgunStrafe, ClanId = 52, Clan = SCRUBS } };
        private static User FraternizeTenebrous = new() { Id = 1055, Rating = 1936, ClanMembership = new ClanMember { UserId = 1055, User = FraternizeTenebrous, ClanId = 53, Clan = GARBAGE } };
        private static User CounterespionageReconnaissance = new() { Id = 1056, Rating = 1021, ClanMembership = new ClanMember { UserId = 1056, User = CounterespionageReconnaissance, ClanId = 58, Clan = PEASANTRY } };
        private static User HissRabbit = new() { Id = 1057, Rating = 2537, ClanMembership = new ClanMember { UserId = 1057, User = HissRabbit, ClanId = 57, Clan = POORS } };
        private static User HappyVirulent = new() { Id = 1058, Rating = 2478, ClanMembership = new ClanMember { UserId = 1058, User = HappyVirulent, ClanId = 60, Clan = VAGABONDS } };
        private static User FieryRaspberry = new() { Id = 1059, Rating = 1385, ClanMembership = new ClanMember { UserId = 1059, User = FieryRaspberry, ClanId = 50, Clan = DUMPSTERS } };
        private static User DigeratiOpisthoglyphous = new() { Id = 1060, Rating = 2185, ClanMembership = new ClanMember { UserId = 1060, User = DigeratiOpisthoglyphous, ClanId = 57, Clan = POORS } };
        private static User CongoEelRingSnake = new() { Id = 1061, Rating = 2382, ClanMembership = new ClanMember { UserId = 1061, User = CongoEelRingSnake, ClanId = 53, Clan = GARBAGE } };
        private static User CountermineMopUp = new() { Id = 1062, Rating = 2511, ClanMembership = new ClanMember { UserId = 1062, User = CountermineMopUp, ClanId = 55, Clan = WASTE } };
        private static User InvadeShoot = new() { Id = 1063, Rating = 523, ClanMembership = new ClanMember { UserId = 1063, User = InvadeShoot, ClanId = 54, Clan = BASURA } };
        private static User HouseSnakePrime = new() { Id = 1064, Rating = 2579, ClanMembership = new ClanMember { UserId = 1064, User = HouseSnakePrime, ClanId = 52, Clan = SCRUBS } };
        private static User BurnTaupe = new() { Id = 1065, Rating = 988, ClanMembership = new ClanMember { UserId = 1065, User = BurnTaupe, ClanId = 54, Clan = BASURA } };
        private static User CourtNeophytism = new() { Id = 1066, Rating = 2362, ClanMembership = new ClanMember { UserId = 1066, User = CourtNeophytism, ClanId = 51, Clan = TRASHCANS } };
        private static User EaterSerpentine = new() { Id = 1067, Rating = 1872, ClanMembership = new ClanMember { UserId = 1067, User = EaterSerpentine, ClanId = 55, Clan = WASTE } };
        private static User FiresideLimber = new() { Id = 1068, Rating = 2486, ClanMembership = new ClanMember { UserId = 1068, User = FiresideLimber, ClanId = 59, Clan = SERFS } };
        private static User GunslingerMole = new() { Id = 1069, Rating = 744, ClanMembership = new ClanMember { UserId = 1069, User = GunslingerMole, ClanId = 59, Clan = SERFS } };
        private static User FlameVirulence = new() { Id = 1070, Rating = 810, ClanMembership = new ClanMember { UserId = 1070, User = FlameVirulence, ClanId = 54, Clan = BASURA } };
        private static User IgneousTail = new() { Id = 1071, Rating = 1142, ClanMembership = new ClanMember { UserId = 1071, User = IgneousTail, ClanId = 53, Clan = GARBAGE } };
        private static User GapWalnut = new() { Id = 1072, Rating = 1023, ClanMembership = new ClanMember { UserId = 1072, User = GapWalnut, ClanId = 51, Clan = TRASHCANS } };
        private static User BombardSullen = new() { Id = 1073, Rating = 2013, ClanMembership = new ClanMember { UserId = 1073, User = BombardSullen, ClanId = 56, Clan = BADS } };
        private static User DaggerShooting = new() { Id = 1074, Rating = 639, ClanMembership = new ClanMember { UserId = 1074, User = DaggerShooting, ClanId = 57, Clan = POORS } };
        private static User CimmerianPistol = new() { Id = 1075, Rating = 1753, ClanMembership = new ClanMember { UserId = 1075, User = CimmerianPistol, ClanId = 59, Clan = SERFS } };
        private static User BiteNavy = new() { Id = 1076, Rating = 1845, ClanMembership = new ClanMember { UserId = 1076, User = BiteNavy, ClanId = 52, Clan = SCRUBS } };
        private static User GreenieMelittin = new() { Id = 1077, Rating = 702, ClanMembership = new ClanMember { UserId = 1077, User = GreenieMelittin, ClanId = 55, Clan = WASTE } };
        private static User BlackToxin = new() { Id = 1078, Rating = 2714, ClanMembership = new ClanMember { UserId = 1078, User = BlackToxin, ClanId = 57, Clan = POORS } };
        private static User GirdWaterMoccasin = new() { Id = 1079, Rating = 1876, ClanMembership = new ClanMember { UserId = 1079, User = GirdWaterMoccasin, ClanId = 58, Clan = PEASANTRY } };
        private static User AirGunKingly = new() { Id = 1080, Rating = 1691, ClanMembership = new ClanMember { UserId = 1080, User = AirGunKingly, ClanId = 57, Clan = POORS } };
        private static User FireproofSwarthy = new() { Id = 1081, Rating = 1043, ClanMembership = new ClanMember { UserId = 1081, User = FireproofSwarthy, ClanId = 60, Clan = VAGABONDS } };
        private static User GuardSepia = new() { Id = 1082, Rating = 2588, ClanMembership = new ClanMember { UserId = 1082, User = GuardSepia, ClanId = 60, Clan = VAGABONDS } };
        private static User FairPuttotheSword = new() { Id = 1083, Rating = 1486, ClanMembership = new ClanMember { UserId = 1083, User = FairPuttotheSword, ClanId = 53, Clan = GARBAGE } };
        private static User AbecedarianWaterPistol = new() { Id = 1084, Rating = 2079, ClanMembership = new ClanMember { UserId = 1084, User = AbecedarianWaterPistol, ClanId = 55, Clan = WASTE } };
        private static User EmberSwordplay = new() { Id = 1085, Rating = 1639, ClanMembership = new ClanMember { UserId = 1085, User = EmberSwordplay, ClanId = 55, Clan = WASTE } };
        private static User DuskyScabbard = new() { Id = 1086, Rating = 2837, ClanMembership = new ClanMember { UserId = 1086, User = DuskyScabbard, ClanId = 55, Clan = WASTE } };
        private static User CadetShed = new() { Id = 1087, Rating = 1522, ClanMembership = new ClanMember { UserId = 1087, User = CadetShed, ClanId = 55, Clan = WASTE } };
        private static User BalefireWorm = new() { Id = 1088, Rating = 2132, ClanMembership = new ClanMember { UserId = 1088, User = BalefireWorm, ClanId = 59, Clan = SERFS } };
        private static User EngageSansevieria = new() { Id = 1089, Rating = 1001, ClanMembership = new ClanMember { UserId = 1089, User = EngageSansevieria, ClanId = 51, Clan = TRASHCANS } };
        private static User BrownstoneQuisling = new() { Id = 1090, Rating = 2385, ClanMembership = new ClanMember { UserId = 1090, User = BrownstoneQuisling, ClanId = 56, Clan = BADS } };
        private static User AlexitericalTaipan = new() { Id = 1091, Rating = 720, ClanMembership = new ClanMember { UserId = 1091, User = AlexitericalTaipan, ClanId = 58, Clan = PEASANTRY } };
        private static User BladeShotgun = new() { Id = 1092, Rating = 2797, ClanMembership = new ClanMember { UserId = 1092, User = BladeShotgun, ClanId = 51, Clan = TRASHCANS } };
        private static User AntiaircraftPunk = new() { Id = 1093, Rating = 2236, ClanMembership = new ClanMember { UserId = 1093, User = AntiaircraftPunk, ClanId = 55, Clan = WASTE } };
        private static User GunfireListeningPost = new() { Id = 1094, Rating = 2646, ClanMembership = new ClanMember { UserId = 1094, User = GunfireListeningPost, ClanId = 56, Clan = BADS } };
        private static User BuckFeverScowl = new() { Id = 1095, Rating = 2252, ClanMembership = new ClanMember { UserId = 1095, User = BuckFeverScowl, ClanId = 56, Clan = BADS } };
        private static User ChaseProteroglypha = new() { Id = 1096, Rating = 1069, ClanMembership = new ClanMember { UserId = 1096, User = ChaseProteroglypha, ClanId = 56, Clan = BADS } };
        private static User FoeYataghan = new() { Id = 1097, Rating = 612, ClanMembership = new ClanMember { UserId = 1097, User = FoeYataghan, ClanId = 56, Clan = BADS } };
        private static User BrunetteWadding = new() { Id = 1098, Rating = 1019, ClanMembership = new ClanMember { UserId = 1098, User = BrunetteWadding, ClanId = 52, Clan = SCRUBS } };
        private static User BoomslangYounker = new() { Id = 1099, Rating = 1740, ClanMembership = new ClanMember { UserId = 1099, User = BoomslangYounker, ClanId = 55, Clan = WASTE } };
        private static User BoaScout = new() { Id = 1100, Rating = 1069, ClanMembership = new ClanMember { UserId = 1100, User = BoaScout, ClanId = 51, Clan = TRASHCANS } };
        private static User AlphabetarianSerum = new() { Id = 1101, Rating = 837, ClanMembership = new ClanMember { UserId = 1101, User = AlphabetarianSerum, ClanId = 52, Clan = SCRUBS } };
        private static User EmpoisonSnake = new() { Id = 1102, Rating = 1721, ClanMembership = new ClanMember { UserId = 1102, User = EmpoisonSnake, ClanId = 57, Clan = POORS } };
        private static User InflammablePuffAdder = new() { Id = 1103, Rating = 1292, ClanMembership = new ClanMember { UserId = 1103, User = InflammablePuffAdder, ClanId = 50, Clan = DUMPSTERS } };
        private static User BullfightTiro = new() { Id = 1104, Rating = 579, ClanMembership = new ClanMember { UserId = 1104, User = BullfightTiro, ClanId = 60, Clan = VAGABONDS } };
        private static User FirearmKeelback = new() { Id = 1105, Rating = 2183, ClanMembership = new ClanMember { UserId = 1105, User = FirearmKeelback, ClanId = 56, Clan = BADS } };
        private static User FiringPumpernickel = new() { Id = 1106, Rating = 1124, ClanMembership = new ClanMember { UserId = 1106, User = FiringPumpernickel, ClanId = 58, Clan = PEASANTRY } };
        private static User InimicalVennation = new() { Id = 1107, Rating = 2878, ClanMembership = new ClanMember { UserId = 1107, User = InimicalVennation, ClanId = 53, Clan = GARBAGE } };
        private static User ConeShellPiece = new() { Id = 1108, Rating = 1220, ClanMembership = new ClanMember { UserId = 1108, User = ConeShellPiece, ClanId = 50, Clan = DUMPSTERS } };
        private static User InitiateMarsala = new() { Id = 1109, Rating = 2767, ClanMembership = new ClanMember { UserId = 1109, User = InitiateMarsala, ClanId = 59, Clan = SERFS } };
        private static User BulletRacer = new() { Id = 1110, Rating = 2957, ClanMembership = new ClanMember { UserId = 1110, User = BulletRacer, ClanId = 60, Clan = VAGABONDS } };
        private static User EggplantRifle = new() { Id = 1111, Rating = 930, ClanMembership = new ClanMember { UserId = 1111, User = EggplantRifle, ClanId = 51, Clan = TRASHCANS } };
        private static User EbonyQueen = new() { Id = 1112, Rating = 1050, ClanMembership = new ClanMember { UserId = 1112, User = EbonyQueen, ClanId = 52, Clan = SCRUBS } };
        private static User InflameMorglay = new() { Id = 1113, Rating = 1846, ClanMembership = new ClanMember { UserId = 1113, User = InflameMorglay, ClanId = 53, Clan = GARBAGE } };
        private static User ComeUnlimber = new() { Id = 1114, Rating = 1467, ClanMembership = new ClanMember { UserId = 1114, User = ComeUnlimber, ClanId = 54, Clan = BASURA } };
        private static User FighterRange = new() { Id = 1115, Rating = 1061, ClanMembership = new ClanMember { UserId = 1115, User = FighterRange, ClanId = 53, Clan = GARBAGE } };
        private static User CottonmouthOxblood = new() { Id = 1116, Rating = 2781, ClanMembership = new ClanMember { UserId = 1116, User = CottonmouthOxblood, ClanId = 55, Clan = WASTE } };
        private static User FifthColumnParry = new() { Id = 1117, Rating = 2384, ClanMembership = new ClanMember { UserId = 1117, User = FifthColumnParry, ClanId = 51, Clan = TRASHCANS } };
        private static User CarbuncleParley = new() { Id = 1118, Rating = 1220, ClanMembership = new ClanMember { UserId = 1118, User = CarbuncleParley, ClanId = 56, Clan = BADS } };
        private static User FoibleUnfriend = new() { Id = 1119, Rating = 1287, ClanMembership = new ClanMember { UserId = 1119, User = FoibleUnfriend, ClanId = 57, Clan = POORS } };
        private static User DamascusSteelProfession = new() { Id = 1120, Rating = 1895, ClanMembership = new ClanMember { UserId = 1120, User = DamascusSteelProfession, ClanId = 57, Clan = POORS } };
        private static User AntimissileSap = new() { Id = 1121, Rating = 1022, ClanMembership = new ClanMember { UserId = 1121, User = AntimissileSap, ClanId = 50, Clan = DUMPSTERS } };
        private static User FloretTityus = new() { Id = 1122, Rating = 1596, ClanMembership = new ClanMember { UserId = 1122, User = FloretTityus, ClanId = 54, Clan = BASURA } };
        private static User CoachwhipRapier = new() { Id = 1123, Rating = 1102, ClanMembership = new ClanMember { UserId = 1123, User = CoachwhipRapier, ClanId = 50, Clan = DUMPSTERS } };
        private static User BootySubmachineGun = new() { Id = 1124, Rating = 2262, ClanMembership = new ClanMember { UserId = 1124, User = BootySubmachineGun, ClanId = 52, Clan = SCRUBS } };
        private static User DamoclesProteroglyphous = new() { Id = 1125, Rating = 2610, ClanMembership = new ClanMember { UserId = 1125, User = DamoclesProteroglyphous, ClanId = 56, Clan = BADS } };
        private static User CannonadeStrip = new() { Id = 1126, Rating = 1511, ClanMembership = new ClanMember { UserId = 1126, User = CannonadeStrip, ClanId = 50, Clan = DUMPSTERS } };
        private static User FlammableWildfire = new() { Id = 1127, Rating = 2633, ClanMembership = new ClanMember { UserId = 1127, User = FlammableWildfire, ClanId = 50, Clan = DUMPSTERS } };
        private static User AlexipharmicJohnny = new() { Id = 1128, Rating = 2358, ClanMembership = new ClanMember { UserId = 1128, User = AlexipharmicJohnny, ClanId = 59, Clan = SERFS } };
        private static User DischargeProteroglyph = new() { Id = 1129, Rating = 2145, ClanMembership = new ClanMember { UserId = 1129, User = DischargeProteroglyph, ClanId = 54, Clan = BASURA } };
        private static User InfiltrateKindling = new() { Id = 1130, Rating = 1323, ClanMembership = new ClanMember { UserId = 1130, User = InfiltrateKindling, ClanId = 54, Clan = BASURA } };
        private static User BilboRhasophore = new() { Id = 1131, Rating = 984, ClanMembership = new ClanMember { UserId = 1131, User = BilboRhasophore, ClanId = 60, Clan = VAGABONDS } };
        private static User ChamberOutvenom = new() { Id = 1132, Rating = 892, ClanMembership = new ClanMember { UserId = 1132, User = ChamberOutvenom, ClanId = 56, Clan = BADS } };
        private static User GunmanSlash = new() { Id = 1133, Rating = 678, ClanMembership = new ClanMember { UserId = 1133, User = GunmanSlash, ClanId = 53, Clan = GARBAGE } };
        private static User AblazeRayGun = new() { Id = 1134, Rating = 540, ClanMembership = new ClanMember { UserId = 1134, User = AblazeRayGun, ClanId = 60, Clan = VAGABONDS } };
        private static User ContagionMalihini = new() { Id = 1135, Rating = 1520, ClanMembership = new ClanMember { UserId = 1135, User = ContagionMalihini, ClanId = 52, Clan = SCRUBS } };
        private static User FangNavyBlue = new() { Id = 1136, Rating = 833, ClanMembership = new ClanMember { UserId = 1136, User = FangNavyBlue, ClanId = 56, Clan = BADS } };
        private static User ChocolateSombre = new() { Id = 1137, Rating = 2840, ClanMembership = new ClanMember { UserId = 1137, User = ChocolateSombre, ClanId = 52, Clan = SCRUBS } };
        private static User EnvenomationSheathe = new() { Id = 1138, Rating = 2312, ClanMembership = new ClanMember { UserId = 1138, User = EnvenomationSheathe, ClanId = 58, Clan = PEASANTRY } };
        private static User AflameReign = new() { Id = 1139, Rating = 2654, ClanMembership = new ClanMember { UserId = 1139, User = AflameReign, ClanId = 60, Clan = VAGABONDS } };
        private static User AglyphTang = new() { Id = 1140, Rating = 1677, ClanMembership = new ClanMember { UserId = 1140, User = AglyphTang, ClanId = 56, Clan = BADS } };
        private static User AlexitericMachineGun = new() { Id = 1141, Rating = 826, ClanMembership = new ClanMember { UserId = 1141, User = AlexitericMachineGun, ClanId = 50, Clan = DUMPSTERS } };
        private static User ForteTheriac = new() { Id = 1142, Rating = 706, ClanMembership = new ClanMember { UserId = 1142, User = ForteTheriac, ClanId = 57, Clan = POORS } };
        private static User FlagofTruceNaked = new() { Id = 1143, Rating = 1609, ClanMembership = new ClanMember { UserId = 1143, User = FlagofTruceNaked, ClanId = 50, Clan = DUMPSTERS } };
        private static User HydraRough = new() { Id = 1144, Rating = 2991, ClanMembership = new ClanMember { UserId = 1144, User = HydraRough, ClanId = 51, Clan = TRASHCANS } };
        private static User BaldricOphi = new() { Id = 1145, Rating = 609, ClanMembership = new ClanMember { UserId = 1145, User = BaldricOphi, ClanId = 54, Clan = BASURA } };
        private static User HangerMapepire = new() { Id = 1146, Rating = 1869, ClanMembership = new ClanMember { UserId = 1146, User = HangerMapepire, ClanId = 51, Clan = TRASHCANS } };
        private static User BlankSpittingSnake = new() { Id = 1147, Rating = 2391, ClanMembership = new ClanMember { UserId = 1147, User = BlankSpittingSnake, ClanId = 54, Clan = BASURA } };
        private static User CounteroffensiveShutterbug = new() { Id = 1148, Rating = 637, ClanMembership = new ClanMember { UserId = 1148, User = CounteroffensiveShutterbug, ClanId = 56, Clan = BADS } };
        private static User GlaiveRuby = new() { Id = 1149, Rating = 1795, ClanMembership = new ClanMember { UserId = 1149, User = GlaiveRuby, ClanId = 50, Clan = DUMPSTERS } };
        private static User EelTenderfoot = new() { Id = 1150, Rating = 2384, ClanMembership = new ClanMember { UserId = 1150, User = EelTenderfoot, ClanId = 58, Clan = PEASANTRY } };
        private static User CoffeeSalamander = new() { Id = 1151, Rating = 1604, ClanMembership = new ClanMember { UserId = 1151, User = CoffeeSalamander, ClanId = 55, Clan = WASTE } };
        private static User CastleShadow = new() { Id = 1152, Rating = 1230, ClanMembership = new ClanMember { UserId = 1152, User = CastleShadow, ClanId = 52, Clan = SCRUBS } };
        private static User AnguineMaroon = new() { Id = 1153, Rating = 2287, ClanMembership = new ClanMember { UserId = 1153, User = AnguineMaroon, ClanId = 54, Clan = BASURA } };
        private static User GopherLubber = new() { Id = 1154, Rating = 2166, ClanMembership = new ClanMember { UserId = 1154, User = GopherLubber, ClanId = 52, Clan = SCRUBS } };
        private static User FrontfangedScalp = new() { Id = 1155, Rating = 1969, ClanMembership = new ClanMember { UserId = 1155, User = FrontfangedScalp, ClanId = 53, Clan = GARBAGE } };
        private static User FrontXiphoid = new() { Id = 1156, Rating = 1973, ClanMembership = new ClanMember { UserId = 1156, User = FrontXiphoid, ClanId = 55, Clan = WASTE } };
        private static User BurntRinghals = new() { Id = 1157, Rating = 1243, ClanMembership = new ClanMember { UserId = 1157, User = BurntRinghals, ClanId = 59, Clan = SERFS } };
        private static User FireTruckRegal = new() { Id = 1158, Rating = 1518, ClanMembership = new ClanMember { UserId = 1158, User = FireTruckRegal, ClanId = 55, Clan = WASTE } };
        private static User ArchenemySidearm = new() { Id = 1159, Rating = 599, ClanMembership = new ClanMember { UserId = 1159, User = ArchenemySidearm, ClanId = 54, Clan = BASURA } };
        private static User CarryLandlubber = new() { Id = 1160, Rating = 2970, ClanMembership = new ClanMember { UserId = 1160, User = CarryLandlubber, ClanId = 58, Clan = PEASANTRY } };
        private static User BlacksnakeToledo = new() { Id = 1161, Rating = 1690, ClanMembership = new ClanMember { UserId = 1161, User = BlacksnakeToledo, ClanId = 54, Clan = BASURA } };
        private static User ExcaliburPyrolatry = new() { Id = 1162, Rating = 1279, ClanMembership = new ClanMember { UserId = 1162, User = ExcaliburPyrolatry, ClanId = 58, Clan = PEASANTRY } };
        private static User CounterintelligenceKinglet = new() { Id = 1163, Rating = 2365, ClanMembership = new ClanMember { UserId = 1163, User = CounterintelligenceKinglet, ClanId = 51, Clan = TRASHCANS } };
        private static User IceMiss = new() { Id = 1164, Rating = 1283, ClanMembership = new ClanMember { UserId = 1164, User = IceMiss, ClanId = 50, Clan = DUMPSTERS } };
        private static User BearerPitch = new() { Id = 1165, Rating = 896, ClanMembership = new ClanMember { UserId = 1165, User = BearerPitch, ClanId = 53, Clan = GARBAGE } };
        private static User BackswordSerpent = new() { Id = 1166, Rating = 1537, ClanMembership = new ClanMember { UserId = 1166, User = BackswordSerpent, ClanId = 57, Clan = POORS } };
        private static User HornedViperMusket = new() { Id = 1167, Rating = 2288, ClanMembership = new ClanMember { UserId = 1167, User = HornedViperMusket, ClanId = 55, Clan = WASTE } };
        private static User FoxholePummel = new() { Id = 1168, Rating = 887, ClanMembership = new ClanMember { UserId = 1168, User = FoxholePummel, ClanId = 60, Clan = VAGABONDS } };
        private static User DunRamrod = new() { Id = 1169, Rating = 1296, ClanMembership = new ClanMember { UserId = 1169, User = DunRamrod, ClanId = 51, Clan = TRASHCANS } };
        private static User ClipNeophyte = new() { Id = 1170, Rating = 1907, ClanMembership = new ClanMember { UserId = 1170, User = ClipNeophyte, ClanId = 60, Clan = VAGABONDS } };
        private static User InternshipPilot = new() { Id = 1171, Rating = 1423, ClanMembership = new ClanMember { UserId = 1171, User = InternshipPilot, ClanId = 50, Clan = DUMPSTERS } };
        private static User FoxSnakeMocha = new() { Id = 1172, Rating = 1588, ClanMembership = new ClanMember { UserId = 1172, User = FoxSnakeMocha, ClanId = 51, Clan = TRASHCANS } };
        private static User BungarotoxinSnakeskin = new() { Id = 1173, Rating = 2260, ClanMembership = new ClanMember { UserId = 1173, User = BungarotoxinSnakeskin, ClanId = 51, Clan = TRASHCANS } };
        private static User DoubleTrail = new() { Id = 1174, Rating = 1478, ClanMembership = new ClanMember { UserId = 1174, User = DoubleTrail, ClanId = 58, Clan = PEASANTRY } };
        private static User FalchionPoker = new() { Id = 1175, Rating = 2138, ClanMembership = new ClanMember { UserId = 1175, User = FalchionPoker, ClanId = 51, Clan = TRASHCANS } };
        private static User BbGunScute = new() { Id = 1176, Rating = 2266, ClanMembership = new ClanMember { UserId = 1176, User = BbGunScute, ClanId = 54, Clan = BASURA } };
        private static User HognosedViper = new() { Id = 1177, Rating = 2242, ClanMembership = new ClanMember { UserId = 1177, User = HognosedViper, ClanId = 60, Clan = VAGABONDS } };
        private static User ThompsonSubmachineGun = new() { Id = 1178, Rating = 1534, ClanMembership = new ClanMember { UserId = 1178, User = ThompsonSubmachineGun, ClanId = 52, Clan = SCRUBS } };
        private static User FoemanRegicide = new() { Id = 1179, Rating = 1104, ClanMembership = new ClanMember { UserId = 1179, User = FoemanRegicide, ClanId = 57, Clan = POORS } };
        private static User AdversaryStoke = new() { Id = 1180, Rating = 2027, ClanMembership = new ClanMember { UserId = 1180, User = AdversaryStoke, ClanId = 60, Clan = VAGABONDS } };
        private static User EnsiformOpisthoglyph = new() { Id = 1181, Rating = 1273, ClanMembership = new ClanMember { UserId = 1181, User = EnsiformOpisthoglyph, ClanId = 54, Clan = BASURA } };
        private static User FoxReptile = new() { Id = 1182, Rating = 574, ClanMembership = new ClanMember { UserId = 1182, User = FoxReptile, ClanId = 56, Clan = BADS } };
        private static User BottleGreenVictory = new() { Id = 1183, Rating = 1149, ClanMembership = new ClanMember { UserId = 1183, User = BottleGreenVictory, ClanId = 51, Clan = TRASHCANS } };
        private static User GreenhornTwist = new() { Id = 1184, Rating = 1278, ClanMembership = new ClanMember { UserId = 1184, User = GreenhornTwist, ClanId = 50, Clan = DUMPSTERS } };
        private static User BaselardScimitar = new() { Id = 1185, Rating = 2868, ClanMembership = new ClanMember { UserId = 1185, User = BaselardScimitar, ClanId = 56, Clan = BADS } };
        private static User CobraLunge = new() { Id = 1186, Rating = 2748, ClanMembership = new ClanMember { UserId = 1186, User = CobraLunge, ClanId = 51, Clan = TRASHCANS } };
        private static User AubergineSurly = new() { Id = 1187, Rating = 1283, ClanMembership = new ClanMember { UserId = 1187, User = AubergineSurly, ClanId = 57, Clan = POORS } };
        private static User FirelessUnfledged = new() { Id = 1188, Rating = 1141, ClanMembership = new ClanMember { UserId = 1188, User = FirelessUnfledged, ClanId = 59, Clan = SERFS } };
        private static User CurtanaRoyalty = new() { Id = 1189, Rating = 2297, ClanMembership = new ClanMember { UserId = 1189, User = CurtanaRoyalty, ClanId = 51, Clan = TRASHCANS } };
        private static User FerSally = new() { Id = 1190, Rating = 1408, ClanMembership = new ClanMember { UserId = 1190, User = FerSally, ClanId = 55, Clan = WASTE } };
        private static User GarterSnakeLately = new() { Id = 1191, Rating = 816, ClanMembership = new ClanMember { UserId = 1191, User = GarterSnakeLately, ClanId = 52, Clan = SCRUBS } };
        private static User CalibrateJillaroo = new() { Id = 1192, Rating = 1800, ClanMembership = new ClanMember { UserId = 1192, User = CalibrateJillaroo, ClanId = 51, Clan = TRASHCANS } };
        private static User CollaborateLance = new() { Id = 1193, Rating = 1634, ClanMembership = new ClanMember { UserId = 1193, User = CollaborateLance, ClanId = 51, Clan = TRASHCANS } };
        private static User ArrowrootOphidian = new() { Id = 1194, Rating = 2924, ClanMembership = new ClanMember { UserId = 1194, User = ArrowrootOphidian, ClanId = 57, Clan = POORS } };
        private static User HamadryadTarantula = new() { Id = 1195, Rating = 1455, ClanMembership = new ClanMember { UserId = 1195, User = HamadryadTarantula, ClanId = 50, Clan = DUMPSTERS } };
        private static User AdderMisfire = new() { Id = 1196, Rating = 2734, ClanMembership = new ClanMember { UserId = 1196, User = AdderMisfire, ClanId = 55, Clan = WASTE } };
        private static User IrisTsuba = new() { Id = 1197, Rating = 2552, ClanMembership = new ClanMember { UserId = 1197, User = IrisTsuba, ClanId = 51, Clan = TRASHCANS } };
        private static User AirgunStonefish = new() { Id = 1198, Rating = 2460, ClanMembership = new ClanMember { UserId = 1198, User = AirgunStonefish, ClanId = 53, Clan = GARBAGE } };
        private static User HepaticMustard = new() { Id = 1199, Rating = 2104, ClanMembership = new ClanMember { UserId = 1199, User = HepaticMustard, ClanId = 53, Clan = GARBAGE } };
        private static User CombatPrefire = new() { Id = 1200, Rating = 1030 };
        private static User HolsterSwordsmanship = new() { Id = 1201, Rating = 1576 };
        private static User EscolarSpittingCobra = new() { Id = 1202, Rating = 2246 };
        private static User FiretrapMelano = new() { Id = 1203, Rating = 2741 };
        private static User CheckVinous = new() { Id = 1204, Rating = 752 };
        private static User BeachheadLeaden = new() { Id = 1205, Rating = 1594 };
        private static User ComputerPhobiaNightAdder = new() { Id = 1206, Rating = 690 };
        private static User BothropsMusketry = new() { Id = 1207, Rating = 2419 };
        private static User AntagonistLodgment = new() { Id = 1208, Rating = 1900 };
        private static User CorposantWhinyard = new() { Id = 1209, Rating = 1707 };
        private static User BlackoutMurk = new() { Id = 1210, Rating = 2113 };
        private static User ChassisPrivateer = new() { Id = 1211, Rating = 2613 };
        private static User DeadlySheath = new() { Id = 1212, Rating = 2170 };
        private static User FightSight = new() { Id = 1213, Rating = 1646 };
        private static User FirehousePuny = new() { Id = 1214, Rating = 1198 };
        private static User BlindSnakeUnsheathe = new() { Id = 1215, Rating = 2332 };
        private static User DeMachine = new() { Id = 1216, Rating = 913 };
        private static User FoilRecoil = new() { Id = 1217, Rating = 1480 };
        private static User EnvenomateMatachin = new() { Id = 1218, Rating = 632 };
        private static User CannonryStoker = new() { Id = 1219, Rating = 1146 };
        private static User CarpetSnakeSaber = new() { Id = 1220, Rating = 1166 };
        private static User DubMudSnake = new() { Id = 1221, Rating = 2726 };
        private static User ChelaOverkill = new() { Id = 1222, Rating = 2915 };
        private static User FireplugNoviceship = new() { Id = 1223, Rating = 702 };
        private static User CanVirus = new() { Id = 1224, Rating = 2865 };
        private static User BuckwheaterVenin = new() { Id = 1225, Rating = 1908 };
        private static User AceSwordless = new() { Id = 1226, Rating = 919 };
        private static User AllongePartisan = new() { Id = 1227, Rating = 2804 };
        private static User CampfireNewChum = new() { Id = 1228, Rating = 826 };
        private static User CrotoxinMulberry = new() { Id = 1229, Rating = 1273 };
        private static User DerisionStygian = new() { Id = 1230, Rating = 1008 };
        private static User DarklingTyro = new() { Id = 1231, Rating = 1130 };
        private static User GrassSnakeRekindle = new() { Id = 1232, Rating = 1275 };
        private static User AntagonizePitchy = new() { Id = 1233, Rating = 2149 };
        private static User EmplacementOpisthoglypha = new() { Id = 1234, Rating = 2782 };
        private static User GunshotSomber = new() { Id = 1235, Rating = 1052 };
        private static User BrandSequester = new() { Id = 1236, Rating = 1556 };
        private static User ConflagrationPlat = new() { Id = 1237, Rating = 503 };
        private static User GunnerPitchdark = new() { Id = 1238, Rating = 1514 };
        private static User FlareSlate = new() { Id = 1239, Rating = 2592 };
        private static User AcinacesVictor = new() { Id = 1240, Rating = 1349 };
        private static User InkyStickUp = new() { Id = 1241, Rating = 2306 };
        private static User FriendSponson = new() { Id = 1242, Rating = 790 };
        private static User AnguiformLethal = new() { Id = 1243, Rating = 1280 };
        private static User AttackSovereign = new() { Id = 1244, Rating = 1514 };
        private static User GloomyRookie = new() { Id = 1245, Rating = 1233 };
        private static User AckSwordCane = new() { Id = 1246, Rating = 2727 };
        private static User ConsumeLower = new() { Id = 1247, Rating = 698 };
        private static User ApitherapyUmber = new() { Id = 1248, Rating = 864 };
        private static User BurningKindle = new() { Id = 1249, Rating = 1741 };
        private static User FlagTrigger = new() { Id = 1250, Rating = 2633 };
        private static User InvasionMatador = new() { Id = 1251, Rating = 1582 };
        private static User AntigunMilkSnake = new() { Id = 1252, Rating = 1843 };
        private static User ConstrictorWeapon = new() { Id = 1253, Rating = 2141 };
        private static User GloomSpike = new() { Id = 1254, Rating = 1837 };
        private static User EyedPython = new() { Id = 1255, Rating = 2393 };
        private static User IncendiarySlug = new() { Id = 1256, Rating = 1070 };
        private static User CrownKingsnake = new() { Id = 1257, Rating = 2725 };
        private static User BlackDuckMine = new() { Id = 1258, Rating = 1435 };
        private static User FenceVenom = new() { Id = 1259, Rating = 2088 };
        private static User FireNovitiate = new() { Id = 1260, Rating = 1142 };
        private static User FrogYoungling = new() { Id = 1261, Rating = 2885 };
        private static User IngleMachinePistol = new() { Id = 1262, Rating = 2553 };
        private static User BlunderbussTeal = new() { Id = 1263, Rating = 2716 };
        private static User CopperheadStratagem = new() { Id = 1264, Rating = 914 };
        private static User CubSerpentiform = new() { Id = 1265, Rating = 1261 };
        private static User DragonRingedSnake = new() { Id = 1266, Rating = 2928 };
        private static User AmbuscadePop = new() { Id = 1267, Rating = 2102 };
        private static User HaftStout = new() { Id = 1268, Rating = 1133 };
        private static User FangedOilfish = new() { Id = 1269, Rating = 2176 };
        private static User FreshmanSlither = new() { Id = 1270, Rating = 2107 };
        private static User InnovativeSilencer = new() { Id = 1271, Rating = 898 };
        private static User AugerShot = new() { Id = 1272, Rating = 1088 };
        private static User CollaborationNewbie = new() { Id = 1273, Rating = 977 };
        private static User GladiolusIsToast = new() { Id = 1274, Rating = 2984 };
        private static User DingyTuck = new() { Id = 1275, Rating = 2851 };
        private static User ArchariosSharpshooter = new() { Id = 1276, Rating = 2362 };
        private static User DarkQuadrate = new() { Id = 1277, Rating = 1444 };
        private static User DungeonRam = new() { Id = 1278, Rating = 678 };
        private static User BlazeLight = new() { Id = 1279, Rating = 1449 };
        private static User AutomaticSwordfish = new() { Id = 1280, Rating = 1252 };
        private static User EmpyrosisSad = new() { Id = 1281, Rating = 2620 };
        private static User IgnitePlum = new() { Id = 1282, Rating = 2814 };
        private static User Firebomb = new() { Id = 1283, Rating = 501 };
        private static User RattlesnakeRoot = new() { Id = 1284, Rating = 2437 };
        private static User BackViper = new() { Id = 1285, Rating = 2994 };
        private static User FlintlockSabotage = new() { Id = 1286, Rating = 2694 };
        private static User AspVenomous = new() { Id = 1287, Rating = 1966 };
        private static User GriffinShooter = new() { Id = 1288, Rating = 2282 };
        private static User BlackenMagazine = new() { Id = 1289, Rating = 2335 };
        private static User BeltShell = new() { Id = 1290, Rating = 819 };
        private static User GunpointMate = new() { Id = 1291, Rating = 1938 };
        private static User CastUp = new() { Id = 1292, Rating = 2953 };
        private static User ClockXiphophyllous = new() { Id = 1293, Rating = 2363 };
        private static User FiredrakeRefire = new() { Id = 1294, Rating = 1244 };
        private static User BoreSnakebite = new() { Id = 1295, Rating = 541 };
        private static User CarbylamineNeurotropic = new() { Id = 1296, Rating = 2358 };
        private static User ChapeMalice = new() { Id = 1297, Rating = 2521 };
        private static User HoldUpRedbackSpider = new() { Id = 1298, Rating = 1579 };
        private static User AntiveninTraverse = new() { Id = 1299, Rating = 2973 };
        private static User DeepSword = new() { Id = 1300, Rating = 2405 };
        private static User GunstockZombie = new() { Id = 1301, Rating = 2507 };
        private static User BoaConstrictorRifling = new() { Id = 1302, Rating = 768 };
        private static User ColubrineMilk = new() { Id = 1303, Rating = 2523 };
        private static User EnkindleReload = new() { Id = 1304, Rating = 2265 };
        private static User FirepowerQuarter = new() { Id = 1305, Rating = 2271 };
        private static User ForaySmother = new() { Id = 1306, Rating = 685 };
        private static User ChargeKing = new() { Id = 1307, Rating = 1606 };
        private static User ClaymoreLow = new() { Id = 1308, Rating = 2833 };
        private static User ColubridRex = new() { Id = 1309, Rating = 2814 };
        private static User ImmolateMarksman = new() { Id = 1310, Rating = 1618 };
        private static User HellfirePopgun = new() { Id = 1311, Rating = 1366 };
        private static User HostileMadtom = new() { Id = 1312, Rating = 1042 };
        private static User BlackamoorSable = new() { Id = 1313, Rating = 1426 };
        private static User FlakWaster = new() { Id = 1314, Rating = 2620 };
        private static User CoverVenomosalivary = new() { Id = 1315, Rating = 1268 };
        private static User AccoladeTrain = new() { Id = 1316, Rating = 2860 };
        private static User BackfireLine = new() { Id = 1317, Rating = 2815 };
        private static User ColtRetreat = new() { Id = 1318, Rating = 1579 };
        private static User HolocaustShah = new() { Id = 1319, Rating = 2648 };
        private static User EnvenomOvercast = new() { Id = 1320, Rating = 2482 };
        private static User InterceptorPyromancy = new() { Id = 1321, Rating = 1170 };
        private static User CutlassSwordsman = new() { Id = 1322, Rating = 2727 };
        private static User CollaboratorSwordKnot = new() { Id = 1323, Rating = 1222 };
        private static User ClaretPicket = new() { Id = 1324, Rating = 1978 };
        private static User CatchStarter = new() { Id = 1325, Rating = 2531 };
        private static User FireballSabre = new() { Id = 1326, Rating = 1449 };
        private static User GrapeSurrender = new() { Id = 1327, Rating = 2972 };
        private static User AnacondaTommyGun = new() { Id = 1328, Rating = 2268 };
        private static User CheckmateThundercloud = new() { Id = 1329, Rating = 570 };
        private static User HereMatchlock = new() { Id = 1330, Rating = 1099 };
        private static User AbatisPilotSnake = new() { Id = 1331, Rating = 1161 };
        private static User BarrelSting = new() { Id = 1332, Rating = 2809 };
        private static User CombustibleNight = new() { Id = 1333, Rating = 2443 };
        private static User CommandoLock = new() { Id = 1334, Rating = 1266 };
        private static User BeestingTrench = new() { Id = 1335, Rating = 630 };
        private static User AfireSlough = new() { Id = 1336, Rating = 1619 };
        private static User CoralSnakePyro = new() { Id = 1337, Rating = 615 };
        private static User BloodPhosphodiesterase = new() { Id = 1338, Rating = 695 };
        private static User HiltMurrey = new() { Id = 1339, Rating = 1174 };
        private static User CharcoalPrisonerofWar = new() { Id = 1340, Rating = 1696 };
        private static User GunmetalSwelter = new() { Id = 1341, Rating = 2797 };
        private static User CaliginousPuce = new() { Id = 1342, Rating = 825 };
        private static User BrownSloe = new() { Id = 1343, Rating = 1748 };
        private static User FiredFishQuench = new() { Id = 1344, Rating = 558 };
        private static User CaperLynx = new() { Id = 1345, Rating = 770 };
        private static User TastyCalyx = new() { Id = 1346, Rating = 545 };
        private static User SiameseLavender = new() { Id = 1347, Rating = 1912 };
        private static User BeauChichi = new() { Id = 1348, Rating = 2004 };
        private static User DogPanther = new() { Id = 1349, Rating = 2210 };
        private static User BlossomJelly = new() { Id = 1350, Rating = 2133 };
        private static User SharpPapergirl = new() { Id = 1351, Rating = 2547 };
        private static User MoppetTear = new() { Id = 1352, Rating = 1364 };
        private static User BlowHandsome = new() { Id = 1353, Rating = 1455 };
        private static User SisterSmirk = new() { Id = 1354, Rating = 2864 };
        private static User FelineSweetTooth = new() { Id = 1355, Rating = 742 };
        private static User SealSneak = new() { Id = 1356, Rating = 1227 };
        private static User TinyFetis = new() { Id = 1357, Rating = 1402 };
        private static User LassBloom = new() { Id = 1358, Rating = 751 };
        private static User BoxDinky = new() { Id = 1359, Rating = 1688 };
        private static User BriocheRam = new() { Id = 1360, Rating = 2750 };
        private static User SweetKitty = new() { Id = 1361, Rating = 2678 };
        private static User GrimalkinDelicacy = new() { Id = 1362, Rating = 1057 };
        private static User LionMuscatel = new() { Id = 1363, Rating = 1903 };
        private static User ExtrafloralUnicorn = new() { Id = 1364, Rating = 2923 };
        private static User NewsgirlLitter = new() { Id = 1365, Rating = 2515 };
        private static User CatsBalm = new() { Id = 1366, Rating = 1786 };
        private static User DiscriminateBlancmange = new() { Id = 1367, Rating = 2151 };
        private static User TroopBobcat = new() { Id = 1368, Rating = 502 };
        private static User PunctiliousQuirky = new() { Id = 1369, Rating = 865 };
        private static User GuideyUnpretty = new() { Id = 1370, Rating = 1450 };
        private static User ScurryHuge = new() { Id = 1371, Rating = 754 };
        private static User SlatternLoving = new() { Id = 1372, Rating = 2238 };
        private static User OnlyGirlChild = new() { Id = 1373, Rating = 1249 };
        private static User SwellSomali = new() { Id = 1374, Rating = 2813 };
        private static User LatinaCyme = new() { Id = 1375, Rating = 2221 };
        private static User ScrumptiousKettleofFish = new() { Id = 1376, Rating = 2061 };
        private static User PearlPosy = new() { Id = 1377, Rating = 1745 };
        private static User AlyssumMilkmaid = new() { Id = 1378, Rating = 1251 };
        private static User ChrysanthemumPeachy = new() { Id = 1379, Rating = 1083 };
        private static User CalicoBun = new() { Id = 1380, Rating = 635 };
        private static User BunnyCatPudgy = new() { Id = 1381, Rating = 2810 };
        private static User CandiedFragrant = new() { Id = 1382, Rating = 1004 };
        private static User MadamTomboy = new() { Id = 1383, Rating = 975 };
        private static User GynoeciumFeat = new() { Id = 1384, Rating = 866 };
        private static User PreciseAnthesis = new() { Id = 1385, Rating = 2696 };
        private static User SaccharineLamb = new() { Id = 1386, Rating = 2021 };
        private static User CoquettePleasant = new() { Id = 1387, Rating = 1384 };
        private static User LilacSweetly = new() { Id = 1388, Rating = 782 };
        private static User EmbarrassedMeow = new() { Id = 1389, Rating = 1489 };
        private static User FloweringMissy = new() { Id = 1390, Rating = 896 };
        private static User CuttyClamber = new() { Id = 1391, Rating = 2338 };
        private static User PrettilyThalamus = new() { Id = 1392, Rating = 1346 };
        private static User EncounterPollination = new() { Id = 1393, Rating = 576 };
        private static User PatrolBonbon = new() { Id = 1394, Rating = 2468 };
        private static User PortFem = new() { Id = 1395, Rating = 1659 };
        private static User BudgereePerianth = new() { Id = 1396, Rating = 2764 };
        private static User PsycheStaminate = new() { Id = 1397, Rating = 1035 };
        private static User BatMitzvahQuatrefoil = new () { Id = 1398, Rating = 2009 };
        private static User HoneyedSugar = new() { Id = 1399, Rating = 2216 };
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
                Hudax_05,
                Hudax_06,
                Hudax_07,
                Hudax_08,
                Hudax_09,
                Hudax_10,
                GerryShepherd,
                BullyDog,
                LabbyRetriever,
                GoldyRetriever,
                SibbyHusky,
                Poodlums,
                BordyCollie,
                Rottyweiler,
                Daschyhund,
                GreatieDane,
                YorkyTerrier,
                CockySpaniel,
                Pomyranian,
                Bullymastiff,
                JackyRussell,
                Akitayinu,
                Maltiepoo,
                Doberymann,
                Sheeiitzu,
                BassetyHound,
                GopherSnakeWeb,
                AmbushSword,
                FencingPacMan,
                EbonSalient,
                CannonSnaky,
                DarklyWine,
                BonfireQuillon,
                BunnySlopeStationHouse,
                BridgeheadRattlesnake,
                InfernoSunless,
                BarricadePrince,
                FoulKingdom,
                DarknessJoeBlake,
                ExtinguisherPommel,
                CaliberKingship,
                FirelightSalvo,
                GarnetMonarch,
                EdgedKatana,
                AntichristKnife,
                DarkenThrust,
                AnaphylacticShockLowering,
                ApprenticeSpottedAdder,
                DrawTreacle,
                AglyphousObscure,
                BackfangedWalk,
                ArachnomorphaeScathe,
                DisenvenomShadowy,
                BroadswordKick,
                DuskNovelist,
                PinkPanther,
                DirkSubfusc,
                FireServiceProbationer,
                BetrayPrehensor,
                FlaskTigerSnake,
                BeginnerPlatypus,
                BushmasterSteel,
                BreechIron,
                BarbecueLivid,
                InfantRinkhals,
                AtterStranger,
                BanditKrait,
                IntelligenceMatchless,
                GrillMuzzle,
                BombinateTwo,
                GunRapid,
                FlameproofReprisal,
                FullerMoccasin,
                HarassSmokeScreen,
                CyanoSax,
                DarksomeSwivel,
                CounterspyMamba,
                FirewardRingedWaterSnake,
                CombustMurky,
                AlightRoyal,
                HandgunStrafe,
                FraternizeTenebrous,
                CounterespionageReconnaissance,
                HissRabbit,
                HappyVirulent,
                FieryRaspberry,
                DigeratiOpisthoglyphous,
                CongoEelRingSnake,
                CountermineMopUp,
                InvadeShoot,
                HouseSnakePrime,
                BurnTaupe,
                CourtNeophytism,
                EaterSerpentine,
                FiresideLimber,
                GunslingerMole,
                FlameVirulence,
                IgneousTail,
                GapWalnut,
                BombardSullen,
                DaggerShooting,
                CimmerianPistol,
                BiteNavy,
                GreenieMelittin,
                BlackToxin,
                GirdWaterMoccasin,
                AirGunKingly,
                FireproofSwarthy,
                GuardSepia,
                FairPuttotheSword,
                AbecedarianWaterPistol,
                EmberSwordplay,
                DuskyScabbard,
                CadetShed,
                BalefireWorm,
                EngageSansevieria,
                BrownstoneQuisling,
                AlexitericalTaipan,
                BladeShotgun,
                AntiaircraftPunk,
                GunfireListeningPost,
                BuckFeverScowl,
                ChaseProteroglypha,
                FoeYataghan,
                BrunetteWadding,
                BoomslangYounker,
                BoaScout,
                AlphabetarianSerum,
                EmpoisonSnake,
                InflammablePuffAdder,
                BullfightTiro,
                FirearmKeelback,
                FiringPumpernickel,
                InimicalVennation,
                ConeShellPiece,
                InitiateMarsala,
                BulletRacer,
                EggplantRifle,
                EbonyQueen,
                InflameMorglay,
                ComeUnlimber,
                FighterRange,
                CottonmouthOxblood,
                FifthColumnParry,
                CarbuncleParley,
                FoibleUnfriend,
                DamascusSteelProfession,
                AntimissileSap,
                FloretTityus,
                CoachwhipRapier,
                BootySubmachineGun,
                DamoclesProteroglyphous,
                CannonadeStrip,
                FlammableWildfire,
                AlexipharmicJohnny,
                DischargeProteroglyph,
                InfiltrateKindling,
                BilboRhasophore,
                ChamberOutvenom,
                GunmanSlash,
                AblazeRayGun,
                ContagionMalihini,
                FangNavyBlue,
                ChocolateSombre,
                EnvenomationSheathe,
                AflameReign,
                AglyphTang,
                AlexitericMachineGun,
                ForteTheriac,
                FlagofTruceNaked,
                HydraRough,
                BaldricOphi,
                HangerMapepire,
                BlankSpittingSnake,
                CounteroffensiveShutterbug,
                GlaiveRuby,
                EelTenderfoot,
                CoffeeSalamander,
                CastleShadow,
                AnguineMaroon,
                GopherLubber,
                FrontfangedScalp,
                FrontXiphoid,
                BurntRinghals,
                FireTruckRegal,
                ArchenemySidearm,
                CarryLandlubber,
                BlacksnakeToledo,
                ExcaliburPyrolatry,
                CounterintelligenceKinglet,
                IceMiss,
                BearerPitch,
                BackswordSerpent,
                HornedViperMusket,
                FoxholePummel,
                DunRamrod,
                ClipNeophyte,
                InternshipPilot,
                FoxSnakeMocha,
                BungarotoxinSnakeskin,
                DoubleTrail,
                FalchionPoker,
                BbGunScute,
                HognosedViper,
                ThompsonSubmachineGun,
                FoemanRegicide,
                AdversaryStoke,
                EnsiformOpisthoglyph,
                FoxReptile,
                BottleGreenVictory,
                GreenhornTwist,
                BaselardScimitar,
                CobraLunge,
                AubergineSurly,
                FirelessUnfledged,
                CurtanaRoyalty,
                FerSally,
                GarterSnakeLately,
                CalibrateJillaroo,
                CollaborateLance,
                ArrowrootOphidian,
                HamadryadTarantula,
                AdderMisfire,
                IrisTsuba,
                AirgunStonefish,
                HepaticMustard,
                CombatPrefire,
                HolsterSwordsmanship,
                EscolarSpittingCobra,
                FiretrapMelano,
                CheckVinous,
                BeachheadLeaden,
                ComputerPhobiaNightAdder,
                BothropsMusketry,
                AntagonistLodgment,
                CorposantWhinyard,
                BlackoutMurk,
                ChassisPrivateer,
                DeadlySheath,
                FightSight,
                FirehousePuny,
                BlindSnakeUnsheathe,
                DeMachine,
                FoilRecoil,
                EnvenomateMatachin,
                CannonryStoker,
                CarpetSnakeSaber,
                DubMudSnake,
                ChelaOverkill,
                FireplugNoviceship,
                CanVirus,
                BuckwheaterVenin,
                AceSwordless,
                AllongePartisan,
                CampfireNewChum,
                CrotoxinMulberry,
                DerisionStygian,
                DarklingTyro,
                GrassSnakeRekindle,
                AntagonizePitchy,
                EmplacementOpisthoglypha,
                GunshotSomber,
                BrandSequester,
                ConflagrationPlat,
                GunnerPitchdark,
                FlareSlate,
                AcinacesVictor,
                InkyStickUp,
                FriendSponson,
                AnguiformLethal,
                AttackSovereign,
                GloomyRookie,
                AckSwordCane,
                ConsumeLower,
                ApitherapyUmber,
                BurningKindle,
                FlagTrigger,
                InvasionMatador,
                AntigunMilkSnake,
                ConstrictorWeapon,
                GloomSpike,
                EyedPython,
                IncendiarySlug,
                CrownKingsnake,
                BlackDuckMine,
                FenceVenom,
                FireNovitiate,
                FrogYoungling,
                IngleMachinePistol,
                BlunderbussTeal,
                CopperheadStratagem,
                CubSerpentiform,
                DragonRingedSnake,
                AmbuscadePop,
                HaftStout,
                FangedOilfish,
                FreshmanSlither,
                InnovativeSilencer,
                AugerShot,
                CollaborationNewbie,
                GladiolusIsToast,
                DingyTuck,
                ArchariosSharpshooter,
                DarkQuadrate,
                DungeonRam,
                BlazeLight,
                AutomaticSwordfish,
                EmpyrosisSad,
                IgnitePlum,
                Firebomb,
                RattlesnakeRoot,
                BackViper,
                FlintlockSabotage,
                AspVenomous,
                GriffinShooter,
                BlackenMagazine,
                BeltShell,
                GunpointMate,
                CastUp,
                ClockXiphophyllous,
                FiredrakeRefire,
                BoreSnakebite,
                CarbylamineNeurotropic,
                ChapeMalice,
                HoldUpRedbackSpider,
                AntiveninTraverse,
                DeepSword,
                GunstockZombie,
                BoaConstrictorRifling,
                ColubrineMilk,
                EnkindleReload,
                FirepowerQuarter,
                ForaySmother,
                ChargeKing,
                ClaymoreLow,
                ColubridRex,
                ImmolateMarksman,
                HellfirePopgun,
                HostileMadtom,
                BlackamoorSable,
                FlakWaster,
                CoverVenomosalivary,
                AccoladeTrain,
                BackfireLine,
                ColtRetreat,
                HolocaustShah,
                EnvenomOvercast,
                InterceptorPyromancy,
                CutlassSwordsman,
                CollaboratorSwordKnot,
                ClaretPicket,
                CatchStarter,
                FireballSabre,
                GrapeSurrender,
                AnacondaTommyGun,
                CheckmateThundercloud,
                HereMatchlock,
                AbatisPilotSnake,
                BarrelSting,
                CombustibleNight,
                CommandoLock,
                BeestingTrench,
                AfireSlough,
                CoralSnakePyro,
                BloodPhosphodiesterase,
                HiltMurrey,
                CharcoalPrisonerofWar,
                GunmetalSwelter,
                CaliginousPuce,
                BrownSloe,
                FiredFishQuench,
                CaperLynx,
                TastyCalyx,
                SiameseLavender,
                BeauChichi,
                DogPanther,
                BlossomJelly,
                SharpPapergirl,
                MoppetTear,
                BlowHandsome,
                SisterSmirk,
                FelineSweetTooth,
                SealSneak,
                TinyFetis,
                LassBloom,
                BoxDinky,
                BriocheRam,
                SweetKitty,
                GrimalkinDelicacy,
                LionMuscatel,
                ExtrafloralUnicorn,
                NewsgirlLitter,
                CatsBalm,
                DiscriminateBlancmange,
                TroopBobcat,
                PunctiliousQuirky,
                GuideyUnpretty,
                ScurryHuge,
                SlatternLoving,
                OnlyGirlChild,
                SwellSomali,
                LatinaCyme,
                ScrumptiousKettleofFish,
                PearlPosy,
                AlyssumMilkmaid,
                ChrysanthemumPeachy,
                CalicoBun,
                BunnyCatPudgy,
                CandiedFragrant,
                MadamTomboy,
                GynoeciumFeat,
                PreciseAnthesis,
                SaccharineLamb,
                CoquettePleasant,
                LilacSweetly,
                EmbarrassedMeow,
                FloweringMissy,
                CuttyClamber,
                PrettilyThalamus,
                EncounterPollination,
                PatrolBonbon,
                PortFem,
                BudgereePerianth,
                PsycheStaminate,
                BatMitzvahQuatrefoil,
                HoneyedSugar,
            },
    };
}
}
