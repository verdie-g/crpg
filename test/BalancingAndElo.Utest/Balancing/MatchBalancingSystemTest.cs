﻿using System.Collections.Generic;
using Crpg.Module.Balancing;
using NUnit.Framework;
using Crpg.Module.Api.Models.Users;
using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Api.Models.Clans;

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
            Console.WriteLine("teamARating = new CrpgCharacterRating { Value = " + teamARating + " teamBRating = new CrpgCharacterRating { Value = " + teamBRating);
            Assert.AreEqual(RatingRatio, 1, 0.2);
        }


        [Test]
        public void BannerBalancingWithEdgeCase()
        {
            var matchBalancer = new MatchBalancingSystem();

            float unbalancedTeamAMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(game1.TeamA, 1);
            float unbalancedTeamBMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(game1.TeamB, 1);
            double unbalancedMeanRatingRatio = (double)unbalancedTeamAMeanRating / (double)unbalancedTeamBMeanRating;
            GameMatch balancedGame = matchBalancer.BannerBalancingWithEdgeCases(game1);
            float teamASize = balancedGame.TeamA.Count;
            float teamBSize = balancedGame.TeamB.Count;
            double sizeRatio = (double)teamASize / (double)teamBSize;
            float teamARating = RatingHelpers.ComputeTeamRatingPowerSum(balancedGame.TeamA, 1);
            float teamBRating = RatingHelpers.ComputeTeamRatingPowerSum(balancedGame.TeamB, 1);
            double RatingRatio = (double)teamARating / (double)teamBRating;
            Assert.AreEqual(RatingRatio, 1, 0.2);
        }

        [Test]
        public void BannerBalancingShouldNotSeperateCrpgClanMember()
        {
            var matchBalancer = new MatchBalancingSystem();

            float unbalancedTeamAMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(game1.TeamA, 1);
            float unbalancedTeamBMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(game1.TeamB, 1);
            double unbalancedMeanRatingRatio = (double)unbalancedTeamAMeanRating / (double)unbalancedTeamBMeanRating;
            Console.WriteLine("unbalanced rating ratio = " + unbalancedMeanRatingRatio);

            GameMatch balancedGame = matchBalancer.PureBannerBalancing(game1);
            foreach (CrpgUser u in game1.TeamA)
            {
                if(u.ClanMembership != null)
                {
                    foreach (CrpgUser u2 in game1.TeamB)
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


        private static CrpgUser arwen = new() { Character = new CrpgCharacter{ Name = "Arwen",  Id = 1, Rating = new CrpgCharacterRating { Value = 2000 }}, ClanMembership = new CrpgClanMember {ClanId = 1} };
        private static CrpgUser frodon = new() { Character = new CrpgCharacter { Name = "Frodon", Id = 2, Rating = new CrpgCharacterRating { Value = 1600 }}, ClanMembership = new CrpgClanMember { ClanId = 1} };
        private static CrpgUser sam = new() { Character = new CrpgCharacter{ Name = "Sam", Id = 3, Rating = new CrpgCharacterRating { Value = 1500 }}, ClanMembership = new CrpgClanMember { ClanId = 1} };
        private static CrpgUser sangoku = new() { Character = new CrpgCharacter{ Name = "Sangoku", Id = 4, Rating = new CrpgCharacterRating { Value = 2000 }}, ClanMembership = new CrpgClanMember { ClanId = 2} };
        private static CrpgUser krilin = new() { Character = new CrpgCharacter{ Name = "Krilin", Id = 5, Rating = new CrpgCharacterRating { Value = 1000 }}, ClanMembership = new CrpgClanMember { ClanId = 2} };
        private static CrpgUser rolandDeschain = new() { Character = new CrpgCharacter{ Name = "Roland Deschain", Id = 6, Rating = new CrpgCharacterRating { Value = 2800 }}, ClanMembership = new CrpgClanMember { ClanId = 3} };
        private static CrpgUser harryPotter = new() { Character = new CrpgCharacter{ Name = "Harry Potter", Id = 7, Rating = new CrpgCharacterRating { Value = 2000 }}, ClanMembership = new CrpgClanMember { ClanId = 4} };
        private static CrpgUser magneto = new() { Character = new CrpgCharacter{ Name = "Magneto", Id = 8, Rating = new CrpgCharacterRating { Value = 2700 }}, ClanMembership = new CrpgClanMember { ClanId = 6} };
        private static CrpgUser profCharles = new() { Character = new CrpgCharacter{ Name = "Professor Charles", Id = 9, Rating = new CrpgCharacterRating { Value = 2800 }}, ClanMembership = new CrpgClanMember { ClanId = 5} };
        private static CrpgUser usainBolt = new() { Character = new CrpgCharacter { Name = "Usain Bolt", Id = 10, Rating = new CrpgCharacterRating { Value = 1200 } } };
        private static CrpgUser agent007 = new() { Character = new CrpgCharacter{ Name = "Agent 007", Id = 11, Rating = new CrpgCharacterRating { Value = 1300 }}};
        private static CrpgUser spongeBob = new() { Character = new CrpgCharacter{ Name = "SpongeBob", Id = 12, Rating = new CrpgCharacterRating { Value = 800 }}};
        private static CrpgUser patrick = new() { Character = new CrpgCharacter{ Name = "Patrick", Id = 13, Rating = new CrpgCharacterRating { Value = 500 }}};
        private static CrpgUser madonna = new() { Character = new CrpgCharacter{ Name = "Madonna", Id = 14, Rating = new CrpgCharacterRating { Value = 1100 }}};
        private static CrpgUser laraCroft = new() { Character = new CrpgCharacter{ Name = "Lara Croft", Id = 15, Rating = new CrpgCharacterRating { Value = 3500 }}};
        private static CrpgUser jeanneDArc = new() { Character = new CrpgCharacter{ Name = "Jeanne D'ARC", Id = 16, Rating = new CrpgCharacterRating { Value = 2400 }}};
        private static CrpgUser merlin = new() { Character = new CrpgCharacter{ Name = "Merlin", Id = 17, Rating = new CrpgCharacterRating { Value = 2700 }}};
        private static CrpgUser bob = new() { Character = new CrpgCharacter{ Name = "Bob", Id = 18, Rating = new CrpgCharacterRating { Value = 1100 }}};
        private static CrpgUser thomas = new() { Character = new CrpgCharacter{ Name = "Thomas", Id = 19, Rating = new CrpgCharacterRating { Value = 2400 }}};
        private static CrpgUser ronWeasley = new() { Character = new CrpgCharacter{ Name = "Ron Weasley", Id = 20, Rating = new CrpgCharacterRating { Value = 600 }}, ClanMembership = new CrpgClanMember { ClanId = 4} };
        private static CrpgUser Jean_01 = new() { Character = new CrpgCharacter{ Name = "Jean_01", Id = 21, Rating = new CrpgCharacterRating { Value = 3000 }}, ClanMembership = new CrpgClanMember { ClanId = 7} };
        private static CrpgUser Jean_02 = new() { Character = new CrpgCharacter{ Name = "Jean_02", Id = 22, Rating = new CrpgCharacterRating { Value = 2500 }}, ClanMembership = new CrpgClanMember { ClanId = 7} };
        private static CrpgUser Jean_03 = new() { Character = new CrpgCharacter{ Name = "Jean_03", Id = 23, Rating = new CrpgCharacterRating { Value = 2100 }}, ClanMembership = new CrpgClanMember { ClanId = 7} };
        private static CrpgUser Jean_04 = new() { Character = new CrpgCharacter{ Name = "Jean_04", Id = 24, Rating = new CrpgCharacterRating { Value = 1200 }}, ClanMembership = new CrpgClanMember { ClanId = 7} };
        private static CrpgUser Jean_05 = new() { Character = new CrpgCharacter{ Name = "Jean_05", Id = 25, Rating = new CrpgCharacterRating { Value = 800 }}, ClanMembership = new CrpgClanMember { ClanId = 7} };
        private static CrpgUser Glutentag_01 = new() { Character = new CrpgCharacter{ Name = "Glutentag_01", Id = 26, Rating = new CrpgCharacterRating { Value = 900 }}, ClanMembership = new CrpgClanMember { ClanId = 8} };
        private static CrpgUser Glutentag_02 = new() { Character = new CrpgCharacter{ Name = "Glutentag_02", Id = 27, Rating = new CrpgCharacterRating { Value = 200 }}, ClanMembership = new CrpgClanMember { ClanId = 8} };
        private static CrpgUser Glutentag_03 = new() { Character = new CrpgCharacter { Name = "Glutentag_03", Id = 28, Rating = new CrpgCharacterRating { Value = 2200 } }, ClanMembership = new CrpgClanMember { ClanId = 8 } };
        private static CrpgUser Glutentag_04 = new() { Character = new CrpgCharacter{ Name = "Glutentag_04", Id = 29, Rating = new CrpgCharacterRating { Value = 400 }}, ClanMembership = new CrpgClanMember { ClanId = 8} };
        private static CrpgUser Glutentag_05 = new() { Character = new CrpgCharacter{ Name = "Glutentag_05", Id = 30, Rating = new CrpgCharacterRating { Value = 800 }}, ClanMembership = new CrpgClanMember { ClanId = 8} };
        private static CrpgUser Vlexance_01 = new() { Character = new CrpgCharacter{ Name = "Vlexance_01", Id = 31, Rating = new CrpgCharacterRating { Value = 2600 }}, ClanMembership = new CrpgClanMember { ClanId = 9} };
        private static CrpgUser Vlexance_02 = new() { Character = new CrpgCharacter{ Name = "Vlexance_02", Id = 32, Rating = new CrpgCharacterRating { Value = 2300 }}, ClanMembership = new CrpgClanMember { ClanId = 9} };
        private static CrpgUser Vlexance_03 = new() { Character = new CrpgCharacter{ Name = "Vlexance_03", Id = 33, Rating = new CrpgCharacterRating { Value = 1300 }}, ClanMembership = new CrpgClanMember { ClanId = 9} };
        private static CrpgUser Vlexance_04 = new() { Character = new CrpgCharacter{ Name = "Vlexance_04", Id = 34, Rating = new CrpgCharacterRating { Value = 1100 }}, ClanMembership = new CrpgClanMember { ClanId = 9} };
        private static CrpgUser Vlexance_05 = new() { Character = new CrpgCharacter{ Name = "Vlexance_05", Id = 35, Rating = new CrpgCharacterRating { Value = 300 }}, ClanMembership = new CrpgClanMember { ClanId = 9} };
        private static CrpgUser Hudax_01 = new() { Character = new CrpgCharacter{ Name = "Hudax_01", Id = 36, Rating = new CrpgCharacterRating { Value = 1100 }}, ClanMembership = new CrpgClanMember { ClanId = 10} };
        private static CrpgUser Hudax_02 = new() { Character = new CrpgCharacter{ Name = "Hudax_02", Id = 37, Rating = new CrpgCharacterRating { Value = 2900 }}, ClanMembership = new CrpgClanMember { ClanId = 10} };
        private static CrpgUser Hudax_03 = new() { Character = new CrpgCharacter{ Name = "Hudax_03", Id = 38, Rating = new CrpgCharacterRating { Value = 1700 }}, ClanMembership = new CrpgClanMember { ClanId = 10} };
        private static CrpgUser Hudax_04 = new() { Character = new CrpgCharacter{ Name = "Hudax_04", Id = 39, Rating = new CrpgCharacterRating { Value = 1500 }}, ClanMembership = new CrpgClanMember { ClanId = 10} };
        private static CrpgUser Hudax_05 = new() { Character = new CrpgCharacter{ Name = "Hudax_05", Id = 40, Rating = new CrpgCharacterRating { Value = 2200 }}, ClanMembership = new CrpgClanMember { ClanId = 10} };
        private static CrpgUser Hudax_06 = new() { Character = new CrpgCharacter{ Name = "", Id = 36, Rating = new CrpgCharacterRating { Value = 1900 }}, ClanMembership = new CrpgClanMember { ClanId = 10} };
        private static CrpgUser Hudax_07 = new() { Character = new CrpgCharacter{ Name = "", Id = 37, Rating = new CrpgCharacterRating { Value = 8000 }}, ClanMembership = new CrpgClanMember { ClanId = 10} };
        private static CrpgUser Hudax_08 = new() { Character = new CrpgCharacter{ Name = "", Id = 38, Rating = new CrpgCharacterRating { Value = 1300 }}, ClanMembership = new CrpgClanMember { ClanId = 10} };
        private static CrpgUser Hudax_09 = new() { Character = new CrpgCharacter{ Name = "", Id = 39, Rating = new CrpgCharacterRating { Value = 1400 }}, ClanMembership = new CrpgClanMember { ClanId = 10} };
        private static CrpgUser Hudax_10 = new() { Character = new CrpgCharacter{ Name = "", Id = 40, Rating = new CrpgCharacterRating { Value = 700 }}, ClanMembership = new CrpgClanMember { ClanId = 10} };
        private static CrpgUser GerryShepherd = new() { Character = new CrpgCharacter{ Name = "GerryShepherd", Id = 41, Rating = new CrpgCharacterRating { Value = 2000 }}, ClanMembership = new CrpgClanMember { ClanId = 11} };
        private static CrpgUser BullyDog = new() { Character = new CrpgCharacter{ Name = "BullyDog", Id = 42, Rating = new CrpgCharacterRating { Value = 1600 }}, ClanMembership = new CrpgClanMember { ClanId = 11} };
        private static CrpgUser LabbyRetriever = new() { Character = new CrpgCharacter{ Name = "LabbyRetriever", Id = 43, Rating = new CrpgCharacterRating { Value = 1500 }}, ClanMembership = new CrpgClanMember { ClanId = 11} };
        private static CrpgUser GoldyRetriever = new() { Character = new CrpgCharacter{ Name = "GoldyRetriever", Id = 44, Rating = new CrpgCharacterRating { Value = 2000 }}, ClanMembership = new CrpgClanMember { ClanId = 12} };
        private static CrpgUser SibbyHusky = new() { Character = new CrpgCharacter{ Name = "SibbyHusky", Id = 45, Rating = new CrpgCharacterRating { Value = 1000 }}, ClanMembership = new CrpgClanMember { ClanId = 12} };
        private static CrpgUser Poodlums = new() { Character = new CrpgCharacter{ Name = "Poodlums", Id = 46, Rating = new CrpgCharacterRating { Value = 2800 }}, ClanMembership = new CrpgClanMember { ClanId = 13} };
        private static CrpgUser BordyCollie = new() { Character = new CrpgCharacter{ Name = "BordyCollie", Id = 47, Rating = new CrpgCharacterRating { Value = 2000 }}, ClanMembership = new CrpgClanMember { ClanId = 14} };
        private static CrpgUser Rottyweiler = new() { Character = new CrpgCharacter{ Name = "Rottyweiler", Id = 48, Rating = new CrpgCharacterRating { Value = 2700 }}, ClanMembership = new CrpgClanMember { ClanId = 15} };
        private static CrpgUser Daschyhund = new() { Character = new CrpgCharacter{ Name = "Daschyhund", Id = 49, Rating = new CrpgCharacterRating { Value = 2800 }}, ClanMembership = new CrpgClanMember { ClanId = 16} };
        private static CrpgUser GreatieDane = new() { Character = new CrpgCharacter{ Name = "GreatieDane", Id = 50, Rating = new CrpgCharacterRating { Value = 1200 }}};
        private static CrpgUser YorkyTerrier = new() { Character = new CrpgCharacter{ Name = "YorkyTerrier", Id = 51, Rating = new CrpgCharacterRating { Value = 1300 }}};
        private static CrpgUser CockySpaniel = new() { Character = new CrpgCharacter{ Name = "CockySpaniel", Id = 52, Rating = new CrpgCharacterRating { Value = 800 }}};
        private static CrpgUser Pomyranian = new() { Character = new CrpgCharacter{ Name = "Pomyranian", Id = 53, Rating = new CrpgCharacterRating { Value = 500 }}};
        private static CrpgUser Bullymastiff = new() { Character = new CrpgCharacter{ Name = "Bullymastiff", Id = 54, Rating = new CrpgCharacterRating { Value = 1100 }}};
        private static CrpgUser JackyRussell = new() { Character = new CrpgCharacter{ Name = "JackyRussell", Id = 55, Rating = new CrpgCharacterRating { Value = 3500 }}};
        private static CrpgUser Akitayinu = new() { Character = new CrpgCharacter{ Name = "Akitayinu", Id = 56, Rating = new CrpgCharacterRating { Value = 2400 }}};
        private static CrpgUser Maltiepoo = new() { Character = new CrpgCharacter{ Name = "Maltiepoo", Id = 57, Rating = new CrpgCharacterRating { Value = 2700 }}};
        private static CrpgUser Doberymann = new() { Character = new CrpgCharacter{ Name = "Doberymann", Id = 58, Rating = new CrpgCharacterRating { Value = 1100 }}};
        private static CrpgUser Sheeiitzu = new() { Character = new CrpgCharacter{ Name = "Sheeiitzu", Id = 59, Rating = new CrpgCharacterRating { Value = 2400 }}};
        private static CrpgUser BassetyHound = new() { Character = new CrpgCharacter{ Name = "BassetyHound", Id = 60, Rating = new CrpgCharacterRating { Value = 600 }}, ClanMembership = new CrpgClanMember { ClanId = 14} };
        private static CrpgUser GopherSnakeWeb = new() { Character = new CrpgCharacter{ Name = "GopherSnakeWeb", Id = 1000, Rating = new CrpgCharacterRating { Value = 819 }}, ClanMembership = new CrpgClanMember { ClanId = 58} };
        private static CrpgUser AmbushSword = new() { Character = new CrpgCharacter{ Name = "AmbushSword", Id = 1001, Rating = new CrpgCharacterRating { Value = 2019 }}, ClanMembership = new CrpgClanMember { ClanId = 50} };
        private static CrpgUser FencingPacMan = new() { Character = new CrpgCharacter{ Name = "FencingPacMan", Id = 1002, Rating = new CrpgCharacterRating { Value = 738 }}, ClanMembership = new CrpgClanMember { ClanId = 53} };
        private static CrpgUser EbonSalient = new() { Character = new CrpgCharacter{ Name = "EbonSalient", Id = 1003, Rating = new CrpgCharacterRating { Value = 1381 }}, ClanMembership = new CrpgClanMember { ClanId = 52} };
        private static CrpgUser CannonSnaky = new() { Character = new CrpgCharacter{ Name = "CannonSnaky", Id = 1004, Rating = new CrpgCharacterRating { Value = 2140 }}, ClanMembership = new CrpgClanMember { ClanId = 55} };
        private static CrpgUser DarklyWine = new() { Character = new CrpgCharacter{ Name = "DarklyWine", Id = 1005, Rating = new CrpgCharacterRating { Value = 2295 }}, ClanMembership = new CrpgClanMember { ClanId = 52} };
        private static CrpgUser BonfireQuillon = new() { Character = new CrpgCharacter{ Name = "BonfireQuillon", Id = 1006, Rating = new CrpgCharacterRating { Value = 2304 }}, ClanMembership = new CrpgClanMember { ClanId = 52} };
        private static CrpgUser BunnySlopeStationHouse = new() { Character = new CrpgCharacter{ Name = "BunnySlopeStationHouse", Id = 1007, Rating = new CrpgCharacterRating { Value = 2067 }}, ClanMembership = new CrpgClanMember { ClanId = 50} };
        private static CrpgUser BridgeheadRattlesnake = new() { Character = new CrpgCharacter{ Name = "BridgeheadRattlesnake", Id = 1008, Rating = new CrpgCharacterRating { Value = 1809 }}, ClanMembership = new CrpgClanMember { ClanId = 54} };
        private static CrpgUser InfernoSunless = new() { Character = new CrpgCharacter{ Name = "InfernoSunless", Id = 1009, Rating = new CrpgCharacterRating { Value = 1765 }}, ClanMembership = new CrpgClanMember { ClanId = 56} };
        private static CrpgUser BarricadePrince = new() { Character = new CrpgCharacter{ Name = "BarricadePrince", Id = 1010, Rating = new CrpgCharacterRating { Value = 2150 }}, ClanMembership = new CrpgClanMember { ClanId = 52} };
        private static CrpgUser FoulKingdom = new() { Character = new CrpgCharacter{ Name = "FoulKingdom", Id = 1011, Rating = new CrpgCharacterRating { Value = 1718 }}, ClanMembership = new CrpgClanMember { ClanId = 51} };
        private static CrpgUser DarknessJoeBlake = new() { Character = new CrpgCharacter{ Name = "DarknessJoeBlake", Id = 1012, Rating = new CrpgCharacterRating { Value = 2499 }}, ClanMembership = new CrpgClanMember { ClanId = 57} };
        private static CrpgUser ExtinguisherPommel = new() { Character = new CrpgCharacter{ Name = "ExtinguisherPommel", Id = 1013, Rating = new CrpgCharacterRating { Value = 1791 }}, ClanMembership = new CrpgClanMember { ClanId = 60} };
        private static CrpgUser CaliberKingship = new() { Character = new CrpgCharacter{ Name = "CaliberKingship", Id = 1014, Rating = new CrpgCharacterRating { Value = 692 }}, ClanMembership = new CrpgClanMember { ClanId = 50} };
        private static CrpgUser FirelightSalvo = new() { Character = new CrpgCharacter{ Name = "FirelightSalvo", Id = 1015, Rating = new CrpgCharacterRating { Value = 2226 }}, ClanMembership = new CrpgClanMember { ClanId = 51} };
        private static CrpgUser GarnetMonarch = new() { Character = new CrpgCharacter{ Name = "GarnetMonarch", Id = 1016, Rating = new CrpgCharacterRating { Value = 1075 }}, ClanMembership = new CrpgClanMember { ClanId = 51} };
        private static CrpgUser EdgedKatana = new() { Character = new CrpgCharacter{ Name = "EdgedKatana", Id = 1017, Rating = new CrpgCharacterRating { Value = 2335 }}, ClanMembership = new CrpgClanMember { ClanId = 60} };
        private static CrpgUser AntichristKnife = new() { Character = new CrpgCharacter{ Name = "AntichristKnife", Id = 1018, Rating = new CrpgCharacterRating { Value = 2743 }}, ClanMembership = new CrpgClanMember { ClanId = 50} };
        private static CrpgUser DarkenThrust = new() { Character = new CrpgCharacter{ Name = "DarkenThrust", Id = 1019, Rating = new CrpgCharacterRating { Value = 1084 }}, ClanMembership = new CrpgClanMember { ClanId = 52} };
        private static CrpgUser AnaphylacticShockLowering = new() { Character = new CrpgCharacter{ Name = "AnaphylacticShockLowering", Id = 1020, Rating = new CrpgCharacterRating { Value = 1969 }}, ClanMembership = new CrpgClanMember { ClanId = 55} };
        private static CrpgUser ApprenticeSpottedAdder = new() { Character = new CrpgCharacter{ Name = "ApprenticeSpottedAdder", Id = 1021, Rating = new CrpgCharacterRating { Value = 2189 }}, ClanMembership = new CrpgClanMember { ClanId = 57} };
        private static CrpgUser DrawTreacle = new() { Character = new CrpgCharacter{ Name = "DrawTreacle", Id = 1022, Rating = new CrpgCharacterRating { Value = 2215 }}, ClanMembership = new CrpgClanMember { ClanId = 59} };
        private static CrpgUser AglyphousObscure = new() { Character = new CrpgCharacter{ Name = "AglyphousObscure", Id = 1023, Rating = new CrpgCharacterRating { Value = 2381 }}, ClanMembership = new CrpgClanMember { ClanId = 52} };
        private static CrpgUser BackfangedWalk = new() { Character = new CrpgCharacter{ Name = "BackfangedWalk", Id = 1024, Rating = new CrpgCharacterRating { Value = 1341 }}, ClanMembership = new CrpgClanMember { ClanId = 54} };
        private static CrpgUser ArachnomorphaeScathe = new() { Character = new CrpgCharacter{ Name = "ArachnomorphaeScathe", Id = 1025, Rating = new CrpgCharacterRating { Value = 2160 }}, ClanMembership = new CrpgClanMember { ClanId = 55} };
        private static CrpgUser DisenvenomShadowy = new() { Character = new CrpgCharacter{ Name = "DisenvenomShadowy", Id = 1026, Rating = new CrpgCharacterRating { Value = 2330 }}, ClanMembership = new CrpgClanMember { ClanId = 54} };
        private static CrpgUser BroadswordKick = new() { Character = new CrpgCharacter{ Name = "BroadswordKick", Id = 1027, Rating = new CrpgCharacterRating { Value = 2978 }}, ClanMembership = new CrpgClanMember { ClanId = 53} };
        private static CrpgUser DuskNovelist = new() { Character = new CrpgCharacter{ Name = "DuskNovelist", Id = 1028, Rating = new CrpgCharacterRating { Value = 729 }}, ClanMembership = new CrpgClanMember { ClanId = 50} };
        private static CrpgUser PinkPanther = new() { Character = new CrpgCharacter{ Name = "PinkPanther", Id = 1029, Rating = new CrpgCharacterRating { Value = 2854 }}, ClanMembership = new CrpgClanMember { ClanId = 56} };
        private static CrpgUser DirkSubfusc = new() { Character = new CrpgCharacter{ Name = "DirkSubfusc", Id = 1030, Rating = new CrpgCharacterRating { Value = 2423 }}, ClanMembership = new CrpgClanMember { ClanId = 57} };
        private static CrpgUser FireServiceProbationer = new() { Character = new CrpgCharacter{ Name = "FireServiceProbationer", Id = 1031, Rating = new CrpgCharacterRating { Value = 1800 }}, ClanMembership = new CrpgClanMember { ClanId = 60} };
        private static CrpgUser BetrayPrehensor = new() { Character = new CrpgCharacter{ Name = "BetrayPrehensor", Id = 1032, Rating = new CrpgCharacterRating { Value = 2739 }}, ClanMembership = new CrpgClanMember { ClanId = 50} };
        private static CrpgUser FlaskTigerSnake = new() { Character = new CrpgCharacter{ Name = "FlaskTigerSnake", Id = 1033, Rating = new CrpgCharacterRating { Value = 1737 }}, ClanMembership = new CrpgClanMember { ClanId = 52} };
        private static CrpgUser BeginnerPlatypus = new() { Character = new CrpgCharacter{ Name = "BeginnerPlatypus", Id = 1034, Rating = new CrpgCharacterRating { Value = 2472 }}, ClanMembership = new CrpgClanMember { ClanId = 51} };
        private static CrpgUser BushmasterSteel = new() { Character = new CrpgCharacter{ Name = "BushmasterSteel", Id = 1035, Rating = new CrpgCharacterRating { Value = 1930 }}, ClanMembership = new CrpgClanMember { ClanId = 58} };
        private static CrpgUser BreechIron = new() { Character = new CrpgCharacter{ Name = "BreechIron", Id = 1036, Rating = new CrpgCharacterRating { Value = 513 }}, ClanMembership = new CrpgClanMember { ClanId = 50} };
        private static CrpgUser BarbecueLivid = new() { Character = new CrpgCharacter{ Name = "BarbecueLivid", Id = 1037, Rating = new CrpgCharacterRating { Value = 1065 }}, ClanMembership = new CrpgClanMember { ClanId = 59} };
        private static CrpgUser InfantRinkhals = new() { Character = new CrpgCharacter{ Name = "InfantRinkhals", Id = 1038, Rating = new CrpgCharacterRating { Value = 1612 }}, ClanMembership = new CrpgClanMember { ClanId = 51} };
        private static CrpgUser AtterStranger = new() { Character = new CrpgCharacter{ Name = "AtterStranger", Id = 1039, Rating = new CrpgCharacterRating { Value = 2987 }}, ClanMembership = new CrpgClanMember { ClanId = 60} };
        private static CrpgUser BanditKrait = new() { Character = new CrpgCharacter{ Name = "BanditKrait", Id = 1040, Rating = new CrpgCharacterRating { Value = 2313 }}, ClanMembership = new CrpgClanMember { ClanId = 51} };
        private static CrpgUser IntelligenceMatchless = new() { Character = new CrpgCharacter{ Name = "IntelligenceMatchless", Id = 1041, Rating = new CrpgCharacterRating { Value = 2064 }}, ClanMembership = new CrpgClanMember { ClanId = 50} };
        private static CrpgUser GrillMuzzle = new() { Character = new CrpgCharacter{ Name = "GrillMuzzle", Id = 1042, Rating = new CrpgCharacterRating { Value = 555 }}, ClanMembership = new CrpgClanMember { ClanId = 52} };
        private static CrpgUser BombinateTwo = new() { Character = new CrpgCharacter{ Name = "BombinateTwo", Id = 1043, Rating = new CrpgCharacterRating { Value = 2778 }}, ClanMembership = new CrpgClanMember { ClanId = 58} };
        private static CrpgUser GunRapid = new() { Character = new CrpgCharacter{ Name = "GunRapid", Id = 1044, Rating = new CrpgCharacterRating { Value = 1269 }}, ClanMembership = new CrpgClanMember { ClanId = 58} };
        private static CrpgUser FlameproofReprisal = new() { Character = new CrpgCharacter{ Name = "FlameproofReprisal", Id = 1045, Rating = new CrpgCharacterRating { Value = 631 }}, ClanMembership = new CrpgClanMember { ClanId = 60} };
        private static CrpgUser FullerMoccasin = new() { Character = new CrpgCharacter{ Name = "FullerMoccasin", Id = 1046, Rating = new CrpgCharacterRating { Value = 2547 }}, ClanMembership = new CrpgClanMember { ClanId = 51} };
        private static CrpgUser HarassSmokeScreen = new() { Character = new CrpgCharacter{ Name = "HarassSmokeScreen", Id = 1047, Rating = new CrpgCharacterRating { Value = 2266 }}, ClanMembership = new CrpgClanMember { ClanId = 55} };
        private static CrpgUser CyanoSax = new() { Character = new CrpgCharacter{ Name = "CyanoSax", Id = 1048, Rating = new CrpgCharacterRating { Value = 1456 }}, ClanMembership = new CrpgClanMember { ClanId = 50} };
        private static CrpgUser DarksomeSwivel = new() { Character = new CrpgCharacter{ Name = "DarksomeSwivel", Id = 1049, Rating = new CrpgCharacterRating { Value = 1458 }}, ClanMembership = new CrpgClanMember { ClanId = 53} };
        private static CrpgUser CounterspyMamba = new() { Character = new CrpgCharacter{ Name = "CounterspyMamba", Id = 1050, Rating = new CrpgCharacterRating { Value = 1223 }}, ClanMembership = new CrpgClanMember { ClanId = 59} };
        private static CrpgUser FirewardRingedWaterSnake = new() { Character = new CrpgCharacter{ Name = "FirewardRingedWaterSnake", Id = 1051, Rating = new CrpgCharacterRating { Value = 2477 }}, ClanMembership = new CrpgClanMember { ClanId = 51} };
        private static CrpgUser CombustMurky = new() { Character = new CrpgCharacter{ Name = "CombustMurky", Id = 1052, Rating = new CrpgCharacterRating { Value = 2812 }}, ClanMembership = new CrpgClanMember { ClanId = 53} };
        private static CrpgUser AlightRoyal = new() { Character = new CrpgCharacter{ Name = "AlightRoyal", Id = 1053, Rating = new CrpgCharacterRating { Value = 1850 }}, ClanMembership = new CrpgClanMember { ClanId = 53} };
        private static CrpgUser HandgunStrafe = new() { Character = new CrpgCharacter{ Name = "HandgunStrafe", Id = 1054, Rating = new CrpgCharacterRating { Value = 1086 }}, ClanMembership = new CrpgClanMember { ClanId = 52} };
        private static CrpgUser FraternizeTenebrous = new() { Character = new CrpgCharacter{ Name = "FraternizeTenebrous", Id = 1055, Rating = new CrpgCharacterRating { Value = 1936 }}, ClanMembership = new CrpgClanMember { ClanId = 53} };
        private static CrpgUser CounterespionageReconnaissance = new() { Character = new CrpgCharacter{ Name = "CounterespionageReconnaissance", Id = 1056, Rating = new CrpgCharacterRating { Value = 1021 }}, ClanMembership = new CrpgClanMember { ClanId = 58} };
        private static CrpgUser HissRabbit = new() { Character = new CrpgCharacter{ Name = "HissRabbit", Id = 1057, Rating = new CrpgCharacterRating { Value = 2537 }}, ClanMembership = new CrpgClanMember { ClanId = 57} };
        private static CrpgUser HappyVirulent = new() { Character = new CrpgCharacter{ Name = "HappyVirulent", Id = 1058, Rating = new CrpgCharacterRating { Value = 2478 }}, ClanMembership = new CrpgClanMember { ClanId = 60} };
        private static CrpgUser FieryRaspberry = new() { Character = new CrpgCharacter{ Name = "FieryRaspberry", Id = 1059, Rating = new CrpgCharacterRating { Value = 1385 }}, ClanMembership = new CrpgClanMember { ClanId = 50} };
        private static CrpgUser DigeratiOpisthoglyphous = new() { Character = new CrpgCharacter{ Name = "DigeratiOpisthoglyphous", Id = 1060, Rating = new CrpgCharacterRating { Value = 2185 }}, ClanMembership = new CrpgClanMember { ClanId = 57} };
        private static CrpgUser CongoEelRingSnake = new() { Character = new CrpgCharacter{ Name = "CongoEelRingSnake", Id = 1061, Rating = new CrpgCharacterRating { Value = 2382 }}, ClanMembership = new CrpgClanMember { ClanId = 53} };
        private static CrpgUser CountermineMopUp = new() { Character = new CrpgCharacter{ Name = "CountermineMopUp", Id = 1062, Rating = new CrpgCharacterRating { Value = 2511 }}, ClanMembership = new CrpgClanMember { ClanId = 55} };
        private static CrpgUser InvadeShoot = new() { Character = new CrpgCharacter{ Name = "InvadeShoot", Id = 1063, Rating = new CrpgCharacterRating { Value = 523 }}, ClanMembership = new CrpgClanMember { ClanId = 54} };
        private static CrpgUser HouseSnakePrime = new() { Character = new CrpgCharacter{ Name = "HouseSnakePrime", Id = 1064, Rating = new CrpgCharacterRating { Value = 2579 }}, ClanMembership = new CrpgClanMember { ClanId = 52} };
        private static CrpgUser BurnTaupe = new() { Character = new CrpgCharacter{ Name = "BurnTaupe", Id = 1065, Rating = new CrpgCharacterRating { Value = 988 }}, ClanMembership = new CrpgClanMember { ClanId = 54} };
        private static CrpgUser CourtNeophytism = new() { Character = new CrpgCharacter{ Name = "CourtNeophytism", Id = 1066, Rating = new CrpgCharacterRating { Value = 2362 }}, ClanMembership = new CrpgClanMember { ClanId = 51} };
        private static CrpgUser EaterSerpentine = new() { Character = new CrpgCharacter{ Name = "EaterSerpentine", Id = 1067, Rating = new CrpgCharacterRating { Value = 1872 }}, ClanMembership = new CrpgClanMember { ClanId = 55} };
        private static CrpgUser FiresideLimber = new() { Character = new CrpgCharacter{ Name = "FiresideLimber", Id = 1068, Rating = new CrpgCharacterRating { Value = 2486 }}, ClanMembership = new CrpgClanMember { ClanId = 59} };
        private static CrpgUser GunslingerMole = new() { Character = new CrpgCharacter{ Name = "GunslingerMole", Id = 1069, Rating = new CrpgCharacterRating { Value = 744 }}, ClanMembership = new CrpgClanMember { ClanId = 59} };
        private static CrpgUser FlameVirulence = new() { Character = new CrpgCharacter{ Name = "FlameVirulence", Id = 1070, Rating = new CrpgCharacterRating { Value = 810 }}, ClanMembership = new CrpgClanMember { ClanId = 54} };
        private static CrpgUser IgneousTail = new() { Character = new CrpgCharacter{ Name = "IgneousTail", Id = 1071, Rating = new CrpgCharacterRating { Value = 1142 }}, ClanMembership = new CrpgClanMember { ClanId = 53} };
        private static CrpgUser GapWalnut = new() { Character = new CrpgCharacter{ Name = "GapWalnut", Id = 1072, Rating = new CrpgCharacterRating { Value = 1023 }}, ClanMembership = new CrpgClanMember { ClanId = 51} };
        private static CrpgUser BombardSullen = new() { Character = new CrpgCharacter{ Name = "BombardSullen", Id = 1073, Rating = new CrpgCharacterRating { Value = 2013 }}, ClanMembership = new CrpgClanMember { ClanId = 56} };
        private static CrpgUser DaggerShooting = new() { Character = new CrpgCharacter{ Name = "DaggerShooting", Id = 1074, Rating = new CrpgCharacterRating { Value = 639 }}, ClanMembership = new CrpgClanMember { ClanId = 57} };
        private static CrpgUser CimmerianPistol = new() { Character = new CrpgCharacter{ Name = "CimmerianPistol", Id = 1075, Rating = new CrpgCharacterRating { Value = 1753 }}, ClanMembership = new CrpgClanMember { ClanId = 59} };
        private static CrpgUser BiteNavy = new() { Character = new CrpgCharacter{ Name = "BiteNavy", Id = 1076, Rating = new CrpgCharacterRating { Value = 1845 }}, ClanMembership = new CrpgClanMember { ClanId = 52} };
        private static CrpgUser GreenieMelittin = new() { Character = new CrpgCharacter{ Name = "GreenieMelittin", Id = 1077, Rating = new CrpgCharacterRating { Value = 702 }}, ClanMembership = new CrpgClanMember { ClanId = 55} };
        private static CrpgUser BlackToxin = new() { Character = new CrpgCharacter{ Name = "BlackToxin", Id = 1078, Rating = new CrpgCharacterRating { Value = 2714 }}, ClanMembership = new CrpgClanMember { ClanId = 57} };
        private static CrpgUser GirdWaterMoccasin = new() { Character = new CrpgCharacter{ Name = "GirdWaterMoccasin", Id = 1079, Rating = new CrpgCharacterRating { Value = 1876 }}, ClanMembership = new CrpgClanMember { ClanId = 58} };
        private static CrpgUser AirGunKingly = new() { Character = new CrpgCharacter{ Name = "AirGunKingly", Id = 1080, Rating = new CrpgCharacterRating { Value = 1691 }}, ClanMembership = new CrpgClanMember { ClanId = 57} };
        private static CrpgUser FireproofSwarthy = new() { Character = new CrpgCharacter{ Name = "FireproofSwarthy", Id = 1081, Rating = new CrpgCharacterRating { Value = 1043 }}, ClanMembership = new CrpgClanMember { ClanId = 60} };
        private static CrpgUser GuardSepia = new() { Character = new CrpgCharacter{ Name = "GuardSepia", Id = 1082, Rating = new CrpgCharacterRating { Value = 2588 }}, ClanMembership = new CrpgClanMember { ClanId = 60} };
        private static CrpgUser FairPuttotheSword = new() { Character = new CrpgCharacter{ Name = "FairPuttotheSword", Id = 1083, Rating = new CrpgCharacterRating { Value = 1486 }}, ClanMembership = new CrpgClanMember { ClanId = 53} };
        private static CrpgUser AbecedarianWaterPistol = new() { Character = new CrpgCharacter{ Name = "AbecedarianWaterPistol", Id = 1084, Rating = new CrpgCharacterRating { Value = 2079 }}, ClanMembership = new CrpgClanMember { ClanId = 55} };
        private static CrpgUser EmberSwordplay = new() { Character = new CrpgCharacter{ Name = "EmberSwordplay", Id = 1085, Rating = new CrpgCharacterRating { Value = 1639 }}, ClanMembership = new CrpgClanMember { ClanId = 55} };
        private static CrpgUser DuskyScabbard = new() { Character = new CrpgCharacter{ Name = "DuskyScabbard", Id = 1086, Rating = new CrpgCharacterRating { Value = 2837 }}, ClanMembership = new CrpgClanMember { ClanId = 55} };
        private static CrpgUser CadetShed = new() { Character = new CrpgCharacter{ Name = "CadetShed", Id = 1087, Rating = new CrpgCharacterRating { Value = 1522 }}, ClanMembership = new CrpgClanMember { ClanId = 55} };
        private static CrpgUser BalefireWorm = new() { Character = new CrpgCharacter{ Name = "BalefireWorm", Id = 1088, Rating = new CrpgCharacterRating { Value = 2132 }}, ClanMembership = new CrpgClanMember { ClanId = 59} };
        private static CrpgUser EngageSansevieria = new() { Character = new CrpgCharacter{ Name = "EngageSansevieria", Id = 1089, Rating = new CrpgCharacterRating { Value = 1001 }}, ClanMembership = new CrpgClanMember { ClanId = 51} };
        private static CrpgUser BrownstoneQuisling = new() { Character = new CrpgCharacter{ Name = "BrownstoneQuisling", Id = 1090, Rating = new CrpgCharacterRating { Value = 2385 }}, ClanMembership = new CrpgClanMember { ClanId = 56} };
        private static CrpgUser AlexitericalTaipan = new() { Character = new CrpgCharacter{ Name = "AlexitericalTaipan", Id = 1091, Rating = new CrpgCharacterRating { Value = 720 }}, ClanMembership = new CrpgClanMember { ClanId = 58} };
        private static CrpgUser BladeShotgun = new() { Character = new CrpgCharacter{ Name = "BladeShotgun", Id = 1092, Rating = new CrpgCharacterRating { Value = 2797 }}, ClanMembership = new CrpgClanMember { ClanId = 51} };
        private static CrpgUser AntiaircraftPunk = new() { Character = new CrpgCharacter{ Name = "AntiaircraftPunk", Id = 1093, Rating = new CrpgCharacterRating { Value = 2236 }}, ClanMembership = new CrpgClanMember { ClanId = 55} };
        private static CrpgUser GunfireListeningPost = new() { Character = new CrpgCharacter{ Name = "GunfireListeningPost", Id = 1094, Rating = new CrpgCharacterRating { Value = 2646 }}, ClanMembership = new CrpgClanMember { ClanId = 56} };
        private static CrpgUser BuckFeverScowl = new() { Character = new CrpgCharacter{ Name = "BuckFeverScowl", Id = 1095, Rating = new CrpgCharacterRating { Value = 2252 }}, ClanMembership = new CrpgClanMember { ClanId = 56} };
        private static CrpgUser ChaseProteroglypha = new() { Character = new CrpgCharacter{ Name = "ChaseProteroglypha", Id = 1096, Rating = new CrpgCharacterRating { Value = 1069 }}, ClanMembership = new CrpgClanMember { ClanId = 56} };
        private static CrpgUser FoeYataghan = new() { Character = new CrpgCharacter{ Name = "FoeYataghan", Id = 1097, Rating = new CrpgCharacterRating { Value = 612 }}, ClanMembership = new CrpgClanMember { ClanId = 56} };
        private static CrpgUser BrunetteWadding = new() { Character = new CrpgCharacter{ Name = "BrunetteWadding", Id = 1098, Rating = new CrpgCharacterRating { Value = 1019 }}, ClanMembership = new CrpgClanMember { ClanId = 52} };
        private static CrpgUser BoomslangYounker = new() { Character = new CrpgCharacter{ Name = "BoomslangYounker", Id = 1099, Rating = new CrpgCharacterRating { Value = 1740 }}, ClanMembership = new CrpgClanMember { ClanId = 55} };
        private static CrpgUser BoaScout = new() { Character = new CrpgCharacter{ Name = "BoaScout", Id = 1100, Rating = new CrpgCharacterRating { Value = 1069 }}, ClanMembership = new CrpgClanMember { ClanId = 51} };
        private static CrpgUser AlphabetarianSerum = new() { Character = new CrpgCharacter{ Name = "AlphabetarianSerum", Id = 1101, Rating = new CrpgCharacterRating { Value = 837 }}, ClanMembership = new CrpgClanMember { ClanId = 52} };
        private static CrpgUser EmpoisonSnake = new() { Character = new CrpgCharacter{ Name = "EmpoisonSnake", Id = 1102, Rating = new CrpgCharacterRating { Value = 1721 }}, ClanMembership = new CrpgClanMember { ClanId = 57} };
        private static CrpgUser InflammablePuffAdder = new() { Character = new CrpgCharacter{ Name = "InflammablePuffAdder", Id = 1103, Rating = new CrpgCharacterRating { Value = 1292 }}, ClanMembership = new CrpgClanMember { ClanId = 50} };
        private static CrpgUser BullfightTiro = new() { Character = new CrpgCharacter{ Name = "BullfightTiro", Id = 1104, Rating = new CrpgCharacterRating { Value = 579 }}, ClanMembership = new CrpgClanMember { ClanId = 60} };
        private static CrpgUser FirearmKeelback = new() { Character = new CrpgCharacter{ Name = "FirearmKeelback", Id = 1105, Rating = new CrpgCharacterRating { Value = 2183 }}, ClanMembership = new CrpgClanMember { ClanId = 56} };
        private static CrpgUser FiringPumpernickel = new() { Character = new CrpgCharacter{ Name = "FiringPumpernickel", Id = 1106, Rating = new CrpgCharacterRating { Value = 1124 }}, ClanMembership = new CrpgClanMember { ClanId = 58} };
        private static CrpgUser InimicalVennation = new() { Character = new CrpgCharacter{ Name = "InimicalVennation", Id = 1107, Rating = new CrpgCharacterRating { Value = 2878 }}, ClanMembership = new CrpgClanMember { ClanId = 53} };
        private static CrpgUser ConeShellPiece = new() { Character = new CrpgCharacter{ Name = "ConeShellPiece", Id = 1108, Rating = new CrpgCharacterRating { Value = 1220 }}, ClanMembership = new CrpgClanMember { ClanId = 50} };
        private static CrpgUser InitiateMarsala = new() { Character = new CrpgCharacter{ Name = "InitiateMarsala", Id = 1109, Rating = new CrpgCharacterRating { Value = 2767 }}, ClanMembership = new CrpgClanMember { ClanId = 59} };
        private static CrpgUser BulletRacer = new() { Character = new CrpgCharacter{ Name = "BulletRacer", Id = 1110, Rating = new CrpgCharacterRating { Value = 2957 }}, ClanMembership = new CrpgClanMember { ClanId = 60} };
        private static CrpgUser EggplantRifle = new() { Character = new CrpgCharacter{ Name = "EggplantRifle", Id = 1111, Rating = new CrpgCharacterRating { Value = 930 }}, ClanMembership = new CrpgClanMember { ClanId = 51} };
        private static CrpgUser EbonyQueen = new() { Character = new CrpgCharacter{ Name = "EbonyQueen", Id = 1112, Rating = new CrpgCharacterRating { Value = 1050 }}, ClanMembership = new CrpgClanMember { ClanId = 52} };
        private static CrpgUser InflameMorglay = new() { Character = new CrpgCharacter{ Name = "InflameMorglay", Id = 1113, Rating = new CrpgCharacterRating { Value = 1846 }}, ClanMembership = new CrpgClanMember { ClanId = 53} };
        private static CrpgUser ComeUnlimber = new() { Character = new CrpgCharacter{ Name = "ComeUnlimber", Id = 1114, Rating = new CrpgCharacterRating { Value = 1467 }}, ClanMembership = new CrpgClanMember { ClanId = 54} };
        private static CrpgUser FighterRange = new() { Character = new CrpgCharacter{ Name = "FighterRange", Id = 1115, Rating = new CrpgCharacterRating { Value = 1061 }}, ClanMembership = new CrpgClanMember { ClanId = 53} };
        private static CrpgUser CottonmouthOxblood = new() { Character = new CrpgCharacter{ Name = "CottonmouthOxblood", Id = 1116, Rating = new CrpgCharacterRating { Value = 2781 }}, ClanMembership = new CrpgClanMember { ClanId = 55} };
        private static CrpgUser FifthColumnParry = new() { Character = new CrpgCharacter{ Name = "FifthColumnParry", Id = 1117, Rating = new CrpgCharacterRating { Value = 2384 }}, ClanMembership = new CrpgClanMember { ClanId = 51} };
        private static CrpgUser CarbuncleParley = new() { Character = new CrpgCharacter{ Name = "CarbuncleParley", Id = 1118, Rating = new CrpgCharacterRating { Value = 1220 }}, ClanMembership = new CrpgClanMember { ClanId = 56} };
        private static CrpgUser FoibleUnfriend = new() { Character = new CrpgCharacter{ Name = "FoibleUnfriend", Id = 1119, Rating = new CrpgCharacterRating { Value = 1287 }}, ClanMembership = new CrpgClanMember { ClanId = 57} };
        private static CrpgUser DamascusSteelProfession = new() { Character = new CrpgCharacter{ Name = "DamascusSteelProfession", Id = 1120, Rating = new CrpgCharacterRating { Value = 1895 }}, ClanMembership = new CrpgClanMember { ClanId = 57} };
        private static CrpgUser AntimissileSap = new() { Character = new CrpgCharacter{ Name = "AntimissileSap", Id = 1121, Rating = new CrpgCharacterRating { Value = 1022 }}, ClanMembership = new CrpgClanMember { ClanId = 50} };
        private static CrpgUser FloretTityus = new() { Character = new CrpgCharacter{ Name = "FloretTityus", Id = 1122, Rating = new CrpgCharacterRating { Value = 1596 }}, ClanMembership = new CrpgClanMember { ClanId = 54} };
        private static CrpgUser CoachwhipRapier = new() { Character = new CrpgCharacter{ Name = "CoachwhipRapier", Id = 1123, Rating = new CrpgCharacterRating { Value = 1102 }}, ClanMembership = new CrpgClanMember { ClanId = 50} };
        private static CrpgUser BootySubmachineGun = new() { Character = new CrpgCharacter{ Name = "BootySubmachineGun", Id = 1124, Rating = new CrpgCharacterRating { Value = 2262 }}, ClanMembership = new CrpgClanMember { ClanId = 52} };
        private static CrpgUser DamoclesProteroglyphous = new() { Character = new CrpgCharacter{ Name = "DamoclesProteroglyphous", Id = 1125, Rating = new CrpgCharacterRating { Value = 2610 }}, ClanMembership = new CrpgClanMember { ClanId = 56} };
        private static CrpgUser CannonadeStrip = new() { Character = new CrpgCharacter{ Name = "CannonadeStrip", Id = 1126, Rating = new CrpgCharacterRating { Value = 1511 }}, ClanMembership = new CrpgClanMember { ClanId = 50} };
        private static CrpgUser FlammableWildfire = new() { Character = new CrpgCharacter{ Name = "FlammableWildfire", Id = 1127, Rating = new CrpgCharacterRating { Value = 2633 }}, ClanMembership = new CrpgClanMember { ClanId = 50} };
        private static CrpgUser AlexipharmicJohnny = new() { Character = new CrpgCharacter{ Name = "AlexipharmicJohnny", Id = 1128, Rating = new CrpgCharacterRating { Value = 2358 }}, ClanMembership = new CrpgClanMember { ClanId = 59} };
        private static CrpgUser DischargeProteroglyph = new() { Character = new CrpgCharacter{ Name = "DischargeProteroglyph", Id = 1129, Rating = new CrpgCharacterRating { Value = 2145 }}, ClanMembership = new CrpgClanMember { ClanId = 54} };
        private static CrpgUser InfiltrateKindling = new() { Character = new CrpgCharacter{ Name = "InfiltrateKindling", Id = 1130, Rating = new CrpgCharacterRating { Value = 1323 }}, ClanMembership = new CrpgClanMember { ClanId = 54} };
        private static CrpgUser BilboRhasophore = new() { Character = new CrpgCharacter{ Name = "BilboRhasophore", Id = 1131, Rating = new CrpgCharacterRating { Value = 984 }}, ClanMembership = new CrpgClanMember { ClanId = 60} };
        private static CrpgUser ChamberOutvenom = new() { Character = new CrpgCharacter{ Name = "ChamberOutvenom", Id = 1132, Rating = new CrpgCharacterRating { Value = 892 }}, ClanMembership = new CrpgClanMember { ClanId = 56} };
        private static CrpgUser GunmanSlash = new() { Character = new CrpgCharacter{ Name = "GunmanSlash", Id = 1133, Rating = new CrpgCharacterRating { Value = 678 }}, ClanMembership = new CrpgClanMember { ClanId = 53} };
        private static CrpgUser AblazeRayGun = new() { Character = new CrpgCharacter{ Name = "AblazeRayGun", Id = 1134, Rating = new CrpgCharacterRating { Value = 540 }}, ClanMembership = new CrpgClanMember { ClanId = 60} };
        private static CrpgUser ContagionMalihini = new() { Character = new CrpgCharacter{ Name = "ContagionMalihini", Id = 1135, Rating = new CrpgCharacterRating { Value = 1520 }}, ClanMembership = new CrpgClanMember { ClanId = 52} };
        private static CrpgUser FangNavyBlue = new() { Character = new CrpgCharacter{ Name = "FangNavyBlue", Id = 1136, Rating = new CrpgCharacterRating { Value = 833 }}, ClanMembership = new CrpgClanMember { ClanId = 56} };
        private static CrpgUser ChocolateSombre = new() { Character = new CrpgCharacter{ Name = "ChocolateSombre", Id = 1137, Rating = new CrpgCharacterRating { Value = 2840 }}, ClanMembership = new CrpgClanMember { ClanId = 52} };
        private static CrpgUser EnvenomationSheathe = new() { Character = new CrpgCharacter{ Name = "EnvenomationSheathe", Id = 1138, Rating = new CrpgCharacterRating { Value = 2312 }}, ClanMembership = new CrpgClanMember { ClanId = 58} };
        private static CrpgUser AflameReign = new() { Character = new CrpgCharacter{ Name = "AflameReign", Id = 1139, Rating = new CrpgCharacterRating { Value = 2654 }}, ClanMembership = new CrpgClanMember { ClanId = 60} };
        private static CrpgUser AglyphTang = new() { Character = new CrpgCharacter{ Name = "AglyphTang", Id = 1140, Rating = new CrpgCharacterRating { Value = 1677 }}, ClanMembership = new CrpgClanMember { ClanId = 56} };
        private static CrpgUser AlexitericMachineGun = new() { Character = new CrpgCharacter{ Name = "AlexitericMachineGun", Id = 1141, Rating = new CrpgCharacterRating { Value = 826 }}, ClanMembership = new CrpgClanMember { ClanId = 50} };
        private static CrpgUser ForteTheriac = new() { Character = new CrpgCharacter{ Name = "ForteTheriac", Id = 1142, Rating = new CrpgCharacterRating { Value = 706 }}, ClanMembership = new CrpgClanMember { ClanId = 57} };
        private static CrpgUser FlagofTruceNaked = new() { Character = new CrpgCharacter{ Name = "FlagofTruceNaked", Id = 1143, Rating = new CrpgCharacterRating { Value = 1609 }}, ClanMembership = new CrpgClanMember { ClanId = 50} };
        private static CrpgUser HydraRough = new() { Character = new CrpgCharacter{ Name = "HydraRough", Id = 1144, Rating = new CrpgCharacterRating { Value = 2991 }}, ClanMembership = new CrpgClanMember { ClanId = 51} };
        private static CrpgUser BaldricOphi = new() { Character = new CrpgCharacter{ Name = "BaldricOphi", Id = 1145, Rating = new CrpgCharacterRating { Value = 609 }}, ClanMembership = new CrpgClanMember { ClanId = 54} };
        private static CrpgUser HangerMapepire = new() { Character = new CrpgCharacter{ Name = "HangerMapepire", Id = 1146, Rating = new CrpgCharacterRating { Value = 1869 }}, ClanMembership = new CrpgClanMember { ClanId = 51} };
        private static CrpgUser BlankSpittingSnake = new() { Character = new CrpgCharacter{ Name = "BlankSpittingSnake", Id = 1147, Rating = new CrpgCharacterRating { Value = 2391 }}, ClanMembership = new CrpgClanMember { ClanId = 54} };
        private static CrpgUser CounteroffensiveShutterbug = new() { Character = new CrpgCharacter{ Name = "CounteroffensiveShutterbug", Id = 1148, Rating = new CrpgCharacterRating { Value = 637 }}, ClanMembership = new CrpgClanMember { ClanId = 56} };
        private static CrpgUser GlaiveRuby = new() { Character = new CrpgCharacter{ Name = "GlaiveRuby", Id = 1149, Rating = new CrpgCharacterRating { Value = 1795 }}, ClanMembership = new CrpgClanMember { ClanId = 50} };
        private static CrpgUser EelTenderfoot = new() { Character = new CrpgCharacter{ Name = "EelTenderfoot", Id = 1150, Rating = new CrpgCharacterRating { Value = 2384 }}, ClanMembership = new CrpgClanMember { ClanId = 58} };
        private static CrpgUser CoffeeSalamander = new() { Character = new CrpgCharacter{ Name = "CoffeeSalamander", Id = 1151, Rating = new CrpgCharacterRating { Value = 1604 }}, ClanMembership = new CrpgClanMember { ClanId = 55} };
        private static CrpgUser CastleShadow = new() { Character = new CrpgCharacter{ Name = "CastleShadow", Id = 1152, Rating = new CrpgCharacterRating { Value = 1230 }}, ClanMembership = new CrpgClanMember { ClanId = 52} };
        private static CrpgUser AnguineMaroon = new() { Character = new CrpgCharacter{ Name = "AnguineMaroon", Id = 1153, Rating = new CrpgCharacterRating { Value = 2287 }}, ClanMembership = new CrpgClanMember { ClanId = 54} };
        private static CrpgUser GopherLubber = new() { Character = new CrpgCharacter{ Name = "GopherLubber", Id = 1154, Rating = new CrpgCharacterRating { Value = 2166 }}, ClanMembership = new CrpgClanMember { ClanId = 52} };
        private static CrpgUser FrontfangedScalp = new() { Character = new CrpgCharacter{ Name = "FrontfangedScalp", Id = 1155, Rating = new CrpgCharacterRating { Value = 1969 }}, ClanMembership = new CrpgClanMember { ClanId = 53} };
        private static CrpgUser FrontXiphoid = new() { Character = new CrpgCharacter{ Name = "FrontXiphoid", Id = 1156, Rating = new CrpgCharacterRating { Value = 1973 }}, ClanMembership = new CrpgClanMember { ClanId = 55} };
        private static CrpgUser BurntRinghals = new() { Character = new CrpgCharacter{ Name = "BurntRinghals", Id = 1157, Rating = new CrpgCharacterRating { Value = 1243 }}, ClanMembership = new CrpgClanMember { ClanId = 59} };
        private static CrpgUser FireTruckRegal = new() { Character = new CrpgCharacter{ Name = "FireTruckRegal", Id = 1158, Rating = new CrpgCharacterRating { Value = 1518 }}, ClanMembership = new CrpgClanMember { ClanId = 55} };
        private static CrpgUser ArchenemySidearm = new() { Character = new CrpgCharacter{ Name = "ArchenemySidearm", Id = 1159, Rating = new CrpgCharacterRating { Value = 599 }}, ClanMembership = new CrpgClanMember { ClanId = 54} };
        private static CrpgUser CarryLandlubber = new() { Character = new CrpgCharacter{ Name = "CarryLandlubber", Id = 1160, Rating = new CrpgCharacterRating { Value = 2970 }}, ClanMembership = new CrpgClanMember { ClanId = 58} };
        private static CrpgUser BlacksnakeToledo = new() { Character = new CrpgCharacter{ Name = "BlacksnakeToledo", Id = 1161, Rating = new CrpgCharacterRating { Value = 1690 }}, ClanMembership = new CrpgClanMember { ClanId = 54} };
        private static CrpgUser ExcaliburPyrolatry = new() { Character = new CrpgCharacter{ Name = "ExcaliburPyrolatry", Id = 1162, Rating = new CrpgCharacterRating { Value = 1279 }}, ClanMembership = new CrpgClanMember { ClanId = 58} };
        private static CrpgUser CounterintelligenceKinglet = new() { Character = new CrpgCharacter{ Name = "CounterintelligenceKinglet", Id = 1163, Rating = new CrpgCharacterRating { Value = 2365 }}, ClanMembership = new CrpgClanMember { ClanId = 51} };
        private static CrpgUser IceMiss = new() { Character = new CrpgCharacter{ Name = "IceMiss", Id = 1164, Rating = new CrpgCharacterRating { Value = 1283 }}, ClanMembership = new CrpgClanMember { ClanId = 50} };
        private static CrpgUser BearerPitch = new() { Character = new CrpgCharacter{ Name = "BearerPitch", Id = 1165, Rating = new CrpgCharacterRating { Value = 896 }}, ClanMembership = new CrpgClanMember { ClanId = 53} };
        private static CrpgUser BackswordSerpent = new() { Character = new CrpgCharacter{ Name = "BackswordSerpent", Id = 1166, Rating = new CrpgCharacterRating { Value = 1537 }}, ClanMembership = new CrpgClanMember { ClanId = 57} };
        private static CrpgUser HornedViperMusket = new() { Character = new CrpgCharacter{ Name = "HornedViperMusket", Id = 1167, Rating = new CrpgCharacterRating { Value = 2288 }}, ClanMembership = new CrpgClanMember { ClanId = 55} };
        private static CrpgUser FoxholePummel = new() { Character = new CrpgCharacter{ Name = "FoxholePummel", Id = 1168, Rating = new CrpgCharacterRating { Value = 887 }}, ClanMembership = new CrpgClanMember { ClanId = 60} };
        private static CrpgUser DunRamrod = new() { Character = new CrpgCharacter{ Name = "DunRamrod", Id = 1169, Rating = new CrpgCharacterRating { Value = 1296 }}, ClanMembership = new CrpgClanMember { ClanId = 51} };
        private static CrpgUser ClipNeophyte = new() { Character = new CrpgCharacter{ Name = "ClipNeophyte", Id = 1170, Rating = new CrpgCharacterRating { Value = 1907 }}, ClanMembership = new CrpgClanMember { ClanId = 60} };
        private static CrpgUser InternshipPilot = new() { Character = new CrpgCharacter{ Name = "InternshipPilot", Id = 1171, Rating = new CrpgCharacterRating { Value = 1423 }}, ClanMembership = new CrpgClanMember { ClanId = 50} };
        private static CrpgUser FoxSnakeMocha = new() { Character = new CrpgCharacter{ Name = "FoxSnakeMocha", Id = 1172, Rating = new CrpgCharacterRating { Value = 1588 }}, ClanMembership = new CrpgClanMember { ClanId = 51} };
        private static CrpgUser BungarotoxinSnakeskin = new() { Character = new CrpgCharacter{ Name = "BungarotoxinSnakeskin", Id = 1173, Rating = new CrpgCharacterRating { Value = 2260 }}, ClanMembership = new CrpgClanMember { ClanId = 51} };
        private static CrpgUser DoubleTrail = new() { Character = new CrpgCharacter{ Name = "DoubleTrail", Id = 1174, Rating = new CrpgCharacterRating { Value = 1478 }}, ClanMembership = new CrpgClanMember { ClanId = 58} };
        private static CrpgUser FalchionPoker = new() { Character = new CrpgCharacter{ Name = "FalchionPoker", Id = 1175, Rating = new CrpgCharacterRating { Value = 2138 }}, ClanMembership = new CrpgClanMember { ClanId = 51} };
        private static CrpgUser BbGunScute = new() { Character = new CrpgCharacter{ Name = "BbGunScute", Id = 1176, Rating = new CrpgCharacterRating { Value = 2266 }}, ClanMembership = new CrpgClanMember { ClanId = 54} };
        private static CrpgUser HognosedViper = new() { Character = new CrpgCharacter{ Name = "HognosedViper", Id = 1177, Rating = new CrpgCharacterRating { Value = 2242 }}, ClanMembership = new CrpgClanMember { ClanId = 60} };
        private static CrpgUser ThompsonSubmachineGun = new() { Character = new CrpgCharacter{ Name = "ThompsonSubmachineGun", Id = 1178, Rating = new CrpgCharacterRating { Value = 1534 }}, ClanMembership = new CrpgClanMember { ClanId = 52} };
        private static CrpgUser FoemanRegicide = new() { Character = new CrpgCharacter{ Name = "FoemanRegicide", Id = 1179, Rating = new CrpgCharacterRating { Value = 1104 }}, ClanMembership = new CrpgClanMember { ClanId = 57} };
        private static CrpgUser AdversaryStoke = new() { Character = new CrpgCharacter{ Name = "AdversaryStoke", Id = 1180, Rating = new CrpgCharacterRating { Value = 2027 }}, ClanMembership = new CrpgClanMember { ClanId = 60} };
        private static CrpgUser EnsiformOpisthoglyph = new() { Character = new CrpgCharacter{ Name = "EnsiformOpisthoglyph", Id = 1181, Rating = new CrpgCharacterRating { Value = 1273 }}, ClanMembership = new CrpgClanMember { ClanId = 54} };
        private static CrpgUser FoxReptile = new() { Character = new CrpgCharacter{ Name = "FoxReptile", Id = 1182, Rating = new CrpgCharacterRating { Value = 574 }}, ClanMembership = new CrpgClanMember { ClanId = 56} };
        private static CrpgUser BottleGreenVictory = new() { Character = new CrpgCharacter{ Name = "BottleGreenVictory", Id = 1183, Rating = new CrpgCharacterRating { Value = 1149 }}, ClanMembership = new CrpgClanMember { ClanId = 51} };
        private static CrpgUser GreenhornTwist = new() { Character = new CrpgCharacter{ Name = "GreenhornTwist", Id = 1184, Rating = new CrpgCharacterRating { Value = 1278 }}, ClanMembership = new CrpgClanMember { ClanId = 50} };
        private static CrpgUser BaselardScimitar = new() { Character = new CrpgCharacter{ Name = "BaselardScimitar", Id = 1185, Rating = new CrpgCharacterRating { Value = 2868 }}, ClanMembership = new CrpgClanMember { ClanId = 56} };
        private static CrpgUser CobraLunge = new() { Character = new CrpgCharacter{ Name = "CobraLunge", Id = 1186, Rating = new CrpgCharacterRating { Value = 2748 }}, ClanMembership = new CrpgClanMember { ClanId = 51} };
        private static CrpgUser AubergineSurly = new() { Character = new CrpgCharacter{ Name = "AubergineSurly", Id = 1187, Rating = new CrpgCharacterRating { Value = 1283 }}, ClanMembership = new CrpgClanMember { ClanId = 57} };
        private static CrpgUser FirelessUnfledged = new() { Character = new CrpgCharacter{ Name = "FirelessUnfledged", Id = 1188, Rating = new CrpgCharacterRating { Value = 1141 }}, ClanMembership = new CrpgClanMember { ClanId = 59} };
        private static CrpgUser CurtanaRoyalty = new() { Character = new CrpgCharacter{ Name = "CurtanaRoyalty", Id = 1189, Rating = new CrpgCharacterRating { Value = 2297 }}, ClanMembership = new CrpgClanMember { ClanId = 51} };
        private static CrpgUser FerSally = new() { Character = new CrpgCharacter{ Name = "FerSally", Id = 1190, Rating = new CrpgCharacterRating { Value = 1408 }}, ClanMembership = new CrpgClanMember { ClanId = 55} };
        private static CrpgUser GarterSnakeLately = new() { Character = new CrpgCharacter{ Name = "GarterSnakeLately", Id = 1191, Rating = new CrpgCharacterRating { Value = 816 }}, ClanMembership = new CrpgClanMember { ClanId = 52} };
        private static CrpgUser CalibrateJillaroo = new() { Character = new CrpgCharacter{ Name = "CalibrateJillaroo", Id = 1192, Rating = new CrpgCharacterRating { Value = 1800 }}, ClanMembership = new CrpgClanMember { ClanId = 51} };
        private static CrpgUser CollaborateLance = new() { Character = new CrpgCharacter{ Name = "CollaborateLance", Id = 1193, Rating = new CrpgCharacterRating { Value = 1634 }}, ClanMembership = new CrpgClanMember { ClanId = 51} };
        private static CrpgUser ArrowrootOphidian = new() { Character = new CrpgCharacter{ Name = "ArrowrootOphidian", Id = 1194, Rating = new CrpgCharacterRating { Value = 2924 }}, ClanMembership = new CrpgClanMember { ClanId = 57} };
        private static CrpgUser HamadryadTarantula = new() { Character = new CrpgCharacter{ Name = "HamadryadTarantula", Id = 1195, Rating = new CrpgCharacterRating { Value = 1455 }}, ClanMembership = new CrpgClanMember { ClanId = 50} };
        private static CrpgUser AdderMisfire = new() { Character = new CrpgCharacter{ Name = "AdderMisfire", Id = 1196, Rating = new CrpgCharacterRating { Value = 2734 }}, ClanMembership = new CrpgClanMember { ClanId = 55} };
        private static CrpgUser IrisTsuba = new() { Character = new CrpgCharacter{ Name = "IrisTsuba", Id = 1197, Rating = new CrpgCharacterRating { Value = 2552 }}, ClanMembership = new CrpgClanMember { ClanId = 51} };
        private static CrpgUser AirgunStonefish = new() { Character = new CrpgCharacter{ Name = "AirgunStonefish", Id = 1198, Rating = new CrpgCharacterRating { Value = 2460 }}, ClanMembership = new CrpgClanMember { ClanId = 53} };
        private static CrpgUser HepaticMustard = new() { Character = new CrpgCharacter{ Name = "HepaticMustard", Id = 1199, Rating = new CrpgCharacterRating { Value = 2104 }}, ClanMembership = new CrpgClanMember { ClanId = 53} };
        private static CrpgUser CombatPrefire = new() { Character = new CrpgCharacter{ Name = "CombatPrefire", Id = 1200, Rating = new CrpgCharacterRating { Value = 1030 }}};
        private static CrpgUser HolsterSwordsmanship = new() { Character = new CrpgCharacter{ Name = "HolsterSwordsmanship", Id = 1201, Rating = new CrpgCharacterRating { Value = 1576 }}};
        private static CrpgUser EscolarSpittingCobra = new() { Character = new CrpgCharacter{ Name = "EscolarSpittingCobra", Id = 1202, Rating = new CrpgCharacterRating { Value = 2246 }}};
        private static CrpgUser FiretrapMelano = new() { Character = new CrpgCharacter{ Name = "FiretrapMelano", Id = 1203, Rating = new CrpgCharacterRating { Value = 2741 }}};
        private static CrpgUser CheckVinous = new() { Character = new CrpgCharacter{ Name = "CheckVinous", Id = 1204, Rating = new CrpgCharacterRating { Value = 752 }}};
        private static CrpgUser BeachheadLeaden = new() { Character = new CrpgCharacter{ Name = "BeachheadLeaden", Id = 1205, Rating = new CrpgCharacterRating { Value = 1594 }}};
        private static CrpgUser ComputerPhobiaNightAdder = new() { Character = new CrpgCharacter{ Name = "ComputerPhobiaNightAdder", Id = 1206, Rating = new CrpgCharacterRating { Value = 690 }}};
        private static CrpgUser BothropsMusketry = new() { Character = new CrpgCharacter{ Name = "BothropsMusketry", Id = 1207, Rating = new CrpgCharacterRating { Value = 2419 }}};
        private static CrpgUser AntagonistLodgment = new() { Character = new CrpgCharacter{ Name = "AntagonistLodgment", Id = 1208, Rating = new CrpgCharacterRating { Value = 1900 }}};
        private static CrpgUser CorposantWhinyard = new() { Character = new CrpgCharacter{ Name = "CorposantWhinyard", Id = 1209, Rating = new CrpgCharacterRating { Value = 1707 }}};
        private static CrpgUser BlackoutMurk = new() { Character = new CrpgCharacter{ Name = "BlackoutMurk", Id = 1210, Rating = new CrpgCharacterRating { Value = 2113 }}};
        private static CrpgUser ChassisPrivateer = new() { Character = new CrpgCharacter{ Name = "ChassisPrivateer", Id = 1211, Rating = new CrpgCharacterRating { Value = 2613 }}};
        private static CrpgUser DeadlySheath = new() { Character = new CrpgCharacter{ Name = "DeadlySheath", Id = 1212, Rating = new CrpgCharacterRating { Value = 2170 }}};
        private static CrpgUser FightSight = new() { Character = new CrpgCharacter{ Name = "FightSight", Id = 1213, Rating = new CrpgCharacterRating { Value = 1646 }}};
        private static CrpgUser FirehousePuny = new() { Character = new CrpgCharacter{ Name = "FirehousePuny", Id = 1214, Rating = new CrpgCharacterRating { Value = 1198 }}};
        private static CrpgUser BlindSnakeUnsheathe = new() { Character = new CrpgCharacter{ Name = "BlindSnakeUnsheathe", Id = 1215, Rating = new CrpgCharacterRating { Value = 2332 }}};
        private static CrpgUser DeMachine = new() { Character = new CrpgCharacter{ Name = "DeMachine", Id = 1216, Rating = new CrpgCharacterRating { Value = 913 }}};
        private static CrpgUser FoilRecoil = new() { Character = new CrpgCharacter{ Name = "FoilRecoil", Id = 1217, Rating = new CrpgCharacterRating { Value = 1480 }}};
        private static CrpgUser EnvenomateMatachin = new() { Character = new CrpgCharacter{ Name = "EnvenomateMatachin", Id = 1218, Rating = new CrpgCharacterRating { Value = 632 }}};
        private static CrpgUser CannonryStoker = new() { Character = new CrpgCharacter{ Name = "CannonryStoker", Id = 1219, Rating = new CrpgCharacterRating { Value = 1146 }}};
        private static CrpgUser CarpetSnakeSaber = new() { Character = new CrpgCharacter{ Name = "CarpetSnakeSaber", Id = 1220, Rating = new CrpgCharacterRating { Value = 1166 }}};
        private static CrpgUser DubMudSnake = new() { Character = new CrpgCharacter{ Name = "DubMudSnake", Id = 1221, Rating = new CrpgCharacterRating { Value = 2726 }}};
        private static CrpgUser ChelaOverkill = new() { Character = new CrpgCharacter{ Name = "ChelaOverkill", Id = 1222, Rating = new CrpgCharacterRating { Value = 2915 }}};
        private static CrpgUser FireplugNoviceship = new() { Character = new CrpgCharacter{ Name = "FireplugNoviceship", Id = 1223, Rating = new CrpgCharacterRating { Value = 702 }}};
        private static CrpgUser CanVirus = new() { Character = new CrpgCharacter{ Name = "CanVirus", Id = 1224, Rating = new CrpgCharacterRating { Value = 2865 }}};
        private static CrpgUser BuckwheaterVenin = new() { Character = new CrpgCharacter{ Name = "BuckwheaterVenin", Id = 1225, Rating = new CrpgCharacterRating { Value = 1908 }}};
        private static CrpgUser AceSwordless = new() { Character = new CrpgCharacter{ Name = "AceSwordless", Id = 1226, Rating = new CrpgCharacterRating { Value = 919 }}};
        private static CrpgUser AllongePartisan = new() { Character = new CrpgCharacter{ Name = "AllongePartisan", Id = 1227, Rating = new CrpgCharacterRating { Value = 2804 }}};
        private static CrpgUser CampfireNewChum = new() { Character = new CrpgCharacter{ Name = "CampfireNewChum", Id = 1228, Rating = new CrpgCharacterRating { Value = 826 }}};
        private static CrpgUser CrotoxinMulberry = new() { Character = new CrpgCharacter{ Name = "CrotoxinMulberry", Id = 1229, Rating = new CrpgCharacterRating { Value = 1273 }}};
        private static CrpgUser DerisionStygian = new() { Character = new CrpgCharacter{ Name = "DerisionStygian", Id = 1230, Rating = new CrpgCharacterRating { Value = 1008 }}};
        private static CrpgUser DarklingTyro = new() { Character = new CrpgCharacter{ Name = "DarklingTyro", Id = 1231, Rating = new CrpgCharacterRating { Value = 1130 }}};
        private static CrpgUser GrassSnakeRekindle = new() { Character = new CrpgCharacter{ Name = "GrassSnakeRekindle", Id = 1232, Rating = new CrpgCharacterRating { Value = 1275 }}};
        private static CrpgUser AntagonizePitchy = new() { Character = new CrpgCharacter{ Name = "AntagonizePitchy", Id = 1233, Rating = new CrpgCharacterRating { Value = 2149 }}};
        private static CrpgUser EmplacementOpisthoglypha = new() { Character = new CrpgCharacter{ Name = "EmplacementOpisthoglypha", Id = 1234, Rating = new CrpgCharacterRating { Value = 2782 }}};
        private static CrpgUser GunshotSomber = new() { Character = new CrpgCharacter{ Name = "GunshotSomber", Id = 1235, Rating = new CrpgCharacterRating { Value = 1052 }}};
        private static CrpgUser BrandSequester = new() { Character = new CrpgCharacter{ Name = "BrandSequester", Id = 1236, Rating = new CrpgCharacterRating { Value = 1556 }}};
        private static CrpgUser ConflagrationPlat = new() { Character = new CrpgCharacter{ Name = "ConflagrationPlat", Id = 1237, Rating = new CrpgCharacterRating { Value = 503 }}};
        private static CrpgUser GunnerPitchdark = new() { Character = new CrpgCharacter{ Name = "GunnerPitchdark", Id = 1238, Rating = new CrpgCharacterRating { Value = 1514 }}};
        private static CrpgUser FlareSlate = new() { Character = new CrpgCharacter{ Name = "FlareSlate", Id = 1239, Rating = new CrpgCharacterRating { Value = 2592 }}};
        private static CrpgUser AcinacesVictor = new() { Character = new CrpgCharacter{ Name = "AcinacesVictor", Id = 1240, Rating = new CrpgCharacterRating { Value = 1349 }}};
        private static CrpgUser InkyStickUp = new() { Character = new CrpgCharacter{ Name = "InkyStickUp", Id = 1241, Rating = new CrpgCharacterRating { Value = 2306 }}};
        private static CrpgUser FriendSponson = new() { Character = new CrpgCharacter{ Name = "FriendSponson", Id = 1242, Rating = new CrpgCharacterRating { Value = 790 }}};
        private static CrpgUser AnguiformLethal = new() { Character = new CrpgCharacter{ Name = "AnguiformLethal", Id = 1243, Rating = new CrpgCharacterRating { Value = 1280 }}};
        private static CrpgUser AttackSovereign = new() { Character = new CrpgCharacter{ Name = "AttackSovereign", Id = 1244, Rating = new CrpgCharacterRating { Value = 1514 }}};
        private static CrpgUser GloomyRookie = new() { Character = new CrpgCharacter{ Name = "GloomyRookie", Id = 1245, Rating = new CrpgCharacterRating { Value = 1233 }}};
        private static CrpgUser AckSwordCane = new() { Character = new CrpgCharacter{ Name = "AckSwordCane", Id = 1246, Rating = new CrpgCharacterRating { Value = 2727 }}};
        private static CrpgUser ConsumeLower = new() { Character = new CrpgCharacter{ Name = "ConsumeLower", Id = 1247, Rating = new CrpgCharacterRating { Value = 698 }}};
        private static CrpgUser ApitherapyUmber = new() { Character = new CrpgCharacter{ Name = "ApitherapyUmber", Id = 1248, Rating = new CrpgCharacterRating { Value = 864 }}};
        private static CrpgUser BurningKindle = new() { Character = new CrpgCharacter{ Name = "BurningKindle", Id = 1249, Rating = new CrpgCharacterRating { Value = 1741 }}};
        private static CrpgUser FlagTrigger = new() { Character = new CrpgCharacter{ Name = "FlagTrigger", Id = 1250, Rating = new CrpgCharacterRating { Value = 2633 }}};
        private static CrpgUser InvasionMatador = new() { Character = new CrpgCharacter{ Name = "InvasionMatador", Id = 1251, Rating = new CrpgCharacterRating { Value = 1582 }}};
        private static CrpgUser AntigunMilkSnake = new() { Character = new CrpgCharacter{ Name = "AntigunMilkSnake", Id = 1252, Rating = new CrpgCharacterRating { Value = 1843 }}};
        private static CrpgUser ConstrictorWeapon = new() { Character = new CrpgCharacter{ Name = "ConstrictorWeapon", Id = 1253, Rating = new CrpgCharacterRating { Value = 2141 }}};
        private static CrpgUser GloomSpike = new() { Character = new CrpgCharacter{ Name = "GloomSpike", Id = 1254, Rating = new CrpgCharacterRating { Value = 1837 }}};
        private static CrpgUser EyedPython = new() { Character = new CrpgCharacter{ Name = "EyedPython", Id = 1255, Rating = new CrpgCharacterRating { Value = 2393 }}};
        private static CrpgUser IncendiarySlug = new() { Character = new CrpgCharacter{ Name = "IncendiarySlug", Id = 1256, Rating = new CrpgCharacterRating { Value = 1070 }}};
        private static CrpgUser CrownKingsnake = new() { Character = new CrpgCharacter{ Name = "CrownKingsnake", Id = 1257, Rating = new CrpgCharacterRating { Value = 2725 }}};
        private static CrpgUser BlackDuckMine = new() { Character = new CrpgCharacter{ Name = "BlackDuckMine", Id = 1258, Rating = new CrpgCharacterRating { Value = 1435 }}};
        private static CrpgUser FenceVenom = new() { Character = new CrpgCharacter{ Name = "FenceVenom", Id = 1259, Rating = new CrpgCharacterRating { Value = 2088 }}};
        private static CrpgUser FireNovitiate = new() { Character = new CrpgCharacter{ Name = "FireNovitiate", Id = 1260, Rating = new CrpgCharacterRating { Value = 1142 }}};
        private static CrpgUser FrogYoungling = new() { Character = new CrpgCharacter{ Name = "FrogYoungling", Id = 1261, Rating = new CrpgCharacterRating { Value = 2885 }}};
        private static CrpgUser IngleMachinePistol = new() { Character = new CrpgCharacter{ Name = "IngleMachinePistol", Id = 1262, Rating = new CrpgCharacterRating { Value = 2553 }}};
        private static CrpgUser BlunderbussTeal = new() { Character = new CrpgCharacter{ Name = "BlunderbussTeal", Id = 1263, Rating = new CrpgCharacterRating { Value = 2716 }}};
        private static CrpgUser CopperheadStratagem = new() { Character = new CrpgCharacter{ Name = "CopperheadStratagem", Id = 1264, Rating = new CrpgCharacterRating { Value = 914 }}};
        private static CrpgUser CubSerpentiform = new() { Character = new CrpgCharacter{ Name = "CubSerpentiform", Id = 1265, Rating = new CrpgCharacterRating { Value = 1261 }}};
        private static CrpgUser DragonRingedSnake = new() { Character = new CrpgCharacter{ Name = "DragonRingedSnake", Id = 1266, Rating = new CrpgCharacterRating { Value = 2928 }}};
        private static CrpgUser AmbuscadePop = new() { Character = new CrpgCharacter{ Name = "AmbuscadePop", Id = 1267, Rating = new CrpgCharacterRating { Value = 2102 }}};
        private static CrpgUser HaftStout = new() { Character = new CrpgCharacter{ Name = "HaftStout", Id = 1268, Rating = new CrpgCharacterRating { Value = 1133 }}};
        private static CrpgUser FangedOilfish = new() { Character = new CrpgCharacter{ Name = "FangedOilfish", Id = 1269, Rating = new CrpgCharacterRating { Value = 2176 }}};
        private static CrpgUser FreshmanSlither = new() { Character = new CrpgCharacter{ Name = "FreshmanSlither", Id = 1270, Rating = new CrpgCharacterRating { Value = 2107 }}};
        private static CrpgUser InnovativeSilencer = new() { Character = new CrpgCharacter{ Name = "InnovativeSilencer", Id = 1271, Rating = new CrpgCharacterRating { Value = 898 }}};
        private static CrpgUser AugerShot = new() { Character = new CrpgCharacter{ Name = "AugerShot", Id = 1272, Rating = new CrpgCharacterRating { Value = 1088 }}};
        private static CrpgUser CollaborationNewbie = new() { Character = new CrpgCharacter{ Name = "CollaborationNewbie", Id = 1273, Rating = new CrpgCharacterRating { Value = 977 }}};
        private static CrpgUser GladiolusIsToast = new() { Character = new CrpgCharacter{ Name = "GladiolusIsToast", Id = 1274, Rating = new CrpgCharacterRating { Value = 2984 }}};
        private static CrpgUser DingyTuck = new() { Character = new CrpgCharacter{ Name = "DingyTuck", Id = 1275, Rating = new CrpgCharacterRating { Value = 2851 }}};
        private static CrpgUser ArchariosSharpshooter = new() { Character = new CrpgCharacter{ Name = "ArchariosSharpshooter", Id = 1276, Rating = new CrpgCharacterRating { Value = 2362 }}};
        private static CrpgUser DarkQuadrate = new() { Character = new CrpgCharacter{ Name = "DarkQuadrate", Id = 1277, Rating = new CrpgCharacterRating { Value = 1444 }}};
        private static CrpgUser DungeonRam = new() { Character = new CrpgCharacter{ Name = "DungeonRam", Id = 1278, Rating = new CrpgCharacterRating { Value = 678 }}};
        private static CrpgUser BlazeLight = new() { Character = new CrpgCharacter{ Name = "BlazeLight", Id = 1279, Rating = new CrpgCharacterRating { Value = 1449 }}};
        private static CrpgUser AutomaticSwordfish = new() { Character = new CrpgCharacter{ Name = "AutomaticSwordfish", Id = 1280, Rating = new CrpgCharacterRating { Value = 1252 }}};
        private static CrpgUser EmpyrosisSad = new() { Character = new CrpgCharacter{ Name = "EmpyrosisSad", Id = 1281, Rating = new CrpgCharacterRating { Value = 2620 }}};
        private static CrpgUser IgnitePlum = new() { Character = new CrpgCharacter{ Name = "IgnitePlum", Id = 1282, Rating = new CrpgCharacterRating { Value = 2814 }}};
        private static CrpgUser Firebomb = new() { Character = new CrpgCharacter{ Name = "Firebomb", Id = 1283, Rating = new CrpgCharacterRating { Value = 501 }}};
        private static CrpgUser RattlesnakeRoot = new() { Character = new CrpgCharacter{ Name = "RattlesnakeRoot", Id = 1284, Rating = new CrpgCharacterRating { Value = 2437 }}};
        private static CrpgUser BackViper = new() { Character = new CrpgCharacter{ Name = "BackViper", Id = 1285, Rating = new CrpgCharacterRating { Value = 2994 }}};
        private static CrpgUser FlintlockSabotage = new() { Character = new CrpgCharacter{ Name = "FlintlockSabotage", Id = 1286, Rating = new CrpgCharacterRating { Value = 2694 }}};
        private static CrpgUser AspVenomous = new() { Character = new CrpgCharacter{ Name = "AspVenomous", Id = 1287, Rating = new CrpgCharacterRating { Value = 1966 }}};
        private static CrpgUser GriffinShooter = new() { Character = new CrpgCharacter{ Name = "GriffinShooter", Id = 1288, Rating = new CrpgCharacterRating { Value = 2282 }}};
        private static CrpgUser BlackenMagazine = new() { Character = new CrpgCharacter{ Name = "BlackenMagazine", Id = 1289, Rating = new CrpgCharacterRating { Value = 2335 }}};
        private static CrpgUser BeltShell = new() { Character = new CrpgCharacter{ Name = "BeltShell", Id = 1290, Rating = new CrpgCharacterRating { Value = 819 }}};
        private static CrpgUser GunpointMate = new() { Character = new CrpgCharacter{ Name = "GunpointMate", Id = 1291, Rating = new CrpgCharacterRating { Value = 1938 }}};
        private static CrpgUser CastUp = new() { Character = new CrpgCharacter{ Name = "CastUp", Id = 1292, Rating = new CrpgCharacterRating { Value = 2953 }}};
        private static CrpgUser ClockXiphophyllous = new() { Character = new CrpgCharacter{ Name = "ClockXiphophyllous", Id = 1293, Rating = new CrpgCharacterRating { Value = 2363 }}};
        private static CrpgUser FiredrakeRefire = new() { Character = new CrpgCharacter{ Name = "FiredrakeRefire", Id = 1294, Rating = new CrpgCharacterRating { Value = 1244 }}};
        private static CrpgUser BoreSnakebite = new() { Character = new CrpgCharacter{ Name = "BoreSnakebite", Id = 1295, Rating = new CrpgCharacterRating { Value = 541 }}};
        private static CrpgUser CarbylamineNeurotropic = new() { Character = new CrpgCharacter{ Name = "CarbylamineNeurotropic", Id = 1296, Rating = new CrpgCharacterRating { Value = 2358 }}};
        private static CrpgUser ChapeMalice = new() { Character = new CrpgCharacter{ Name = "ChapeMalice", Id = 1297, Rating = new CrpgCharacterRating { Value = 2521 }}};
        private static CrpgUser HoldUpRedbackSpider = new() { Character = new CrpgCharacter{ Name = "HoldUpRedbackSpider", Id = 1298, Rating = new CrpgCharacterRating { Value = 1579 }}};
        private static CrpgUser AntiveninTraverse = new() { Character = new CrpgCharacter{ Name = "AntiveninTraverse", Id = 1299, Rating = new CrpgCharacterRating { Value = 2973 }}};
        private static CrpgUser DeepSword = new() { Character = new CrpgCharacter{ Name = "DeepSword", Id = 1300, Rating = new CrpgCharacterRating { Value = 2405 }}};
        private static CrpgUser GunstockZombie = new() { Character = new CrpgCharacter{ Name = "GunstockZombie", Id = 1301, Rating = new CrpgCharacterRating { Value = 2507 }}};
        private static CrpgUser BoaConstrictorRifling = new() { Character = new CrpgCharacter{ Name = "BoaConstrictorRifling", Id = 1302, Rating = new CrpgCharacterRating { Value = 768 }}};
        private static CrpgUser ColubrineMilk = new() { Character = new CrpgCharacter{ Name = "ColubrineMilk", Id = 1303, Rating = new CrpgCharacterRating { Value = 2523 }}};
        private static CrpgUser EnkindleReload = new() { Character = new CrpgCharacter{ Name = "EnkindleReload", Id = 1304, Rating = new CrpgCharacterRating { Value = 2265 }}};
        private static CrpgUser FirepowerQuarter = new() { Character = new CrpgCharacter{ Name = "FirepowerQuarter", Id = 1305, Rating = new CrpgCharacterRating { Value = 2271 }}};
        private static CrpgUser ForaySmother = new() { Character = new CrpgCharacter{ Name = "ForaySmother", Id = 1306, Rating = new CrpgCharacterRating { Value = 685 }}};
        private static CrpgUser ChargeKing = new() { Character = new CrpgCharacter{ Name = "ChargeKing", Id = 1307, Rating = new CrpgCharacterRating { Value = 1606 }}};
        private static CrpgUser ClaymoreLow = new() { Character = new CrpgCharacter{ Name = "ClaymoreLow", Id = 1308, Rating = new CrpgCharacterRating { Value = 2833 }}};
        private static CrpgUser ColubridRex = new() { Character = new CrpgCharacter{ Name = "ColubridRex", Id = 1309, Rating = new CrpgCharacterRating { Value = 2814 }}};
        private static CrpgUser ImmolateMarksman = new() { Character = new CrpgCharacter{ Name = "ImmolateMarksman", Id = 1310, Rating = new CrpgCharacterRating { Value = 1618 }}};
        private static CrpgUser HellfirePopgun = new() { Character = new CrpgCharacter{ Name = "HellfirePopgun", Id = 1311, Rating = new CrpgCharacterRating { Value = 1366 }}};
        private static CrpgUser HostileMadtom = new() { Character = new CrpgCharacter{ Name = "HostileMadtom", Id = 1312, Rating = new CrpgCharacterRating { Value = 1042 }}};
        private static CrpgUser BlackamoorSable = new() { Character = new CrpgCharacter{ Name = "BlackamoorSable", Id = 1313, Rating = new CrpgCharacterRating { Value = 1426 }}};
        private static CrpgUser FlakWaster = new() { Character = new CrpgCharacter{ Name = "FlakWaster", Id = 1314, Rating = new CrpgCharacterRating { Value = 2620 }}};
        private static CrpgUser CoverVenomosalivary = new() { Character = new CrpgCharacter{ Name = "CoverVenomosalivary", Id = 1315, Rating = new CrpgCharacterRating { Value = 1268 }}};
        private static CrpgUser AccoladeTrain = new() { Character = new CrpgCharacter{ Name = "AccoladeTrain", Id = 1316, Rating = new CrpgCharacterRating { Value = 2860 }}};
        private static CrpgUser BackfireLine = new() { Character = new CrpgCharacter{ Name = "BackfireLine", Id = 1317, Rating = new CrpgCharacterRating { Value = 2815 }}};
        private static CrpgUser ColtRetreat = new() { Character = new CrpgCharacter{ Name = "ColtRetreat", Id = 1318, Rating = new CrpgCharacterRating { Value = 1579 }}};
        private static CrpgUser HolocaustShah = new() { Character = new CrpgCharacter{ Name = "HolocaustShah", Id = 1319, Rating = new CrpgCharacterRating { Value = 2648 }}};
        private static CrpgUser EnvenomOvercast = new() { Character = new CrpgCharacter{ Name = "EnvenomOvercast", Id = 1320, Rating = new CrpgCharacterRating { Value = 2482 }}};
        private static CrpgUser InterceptorPyromancy = new() { Character = new CrpgCharacter{ Name = "InterceptorPyromancy", Id = 1321, Rating = new CrpgCharacterRating { Value = 1170 }}};
        private static CrpgUser CutlassSwordsman = new() { Character = new CrpgCharacter{ Name = "CutlassSwordsman", Id = 1322, Rating = new CrpgCharacterRating { Value = 2727 }}};
        private static CrpgUser CollaboratorSwordKnot = new() { Character = new CrpgCharacter{ Name = "CollaboratorSwordKnot", Id = 1323, Rating = new CrpgCharacterRating { Value = 1222 }}};
        private static CrpgUser ClaretPicket = new() { Character = new CrpgCharacter{ Name = "ClaretPicket", Id = 1324, Rating = new CrpgCharacterRating { Value = 1978 }}};
        private static CrpgUser CatchStarter = new() { Character = new CrpgCharacter{ Name = "CatchStarter", Id = 1325, Rating = new CrpgCharacterRating { Value = 2531 }}};
        private static CrpgUser FireballSabre = new() { Character = new CrpgCharacter{ Name = "FireballSabre", Id = 1326, Rating = new CrpgCharacterRating { Value = 1449 }}};
        private static CrpgUser GrapeSurrender = new() { Character = new CrpgCharacter{ Name = "GrapeSurrender", Id = 1327, Rating = new CrpgCharacterRating { Value = 2972 }}};
        private static CrpgUser AnacondaTommyGun = new() { Character = new CrpgCharacter{ Name = "AnacondaTommyGun", Id = 1328, Rating = new CrpgCharacterRating { Value = 2268 }}};
        private static CrpgUser CheckmateThundercloud = new() { Character = new CrpgCharacter{ Name = "CheckmateThundercloud", Id = 1329, Rating = new CrpgCharacterRating { Value = 570 }}};
        private static CrpgUser HereMatchlock = new() { Character = new CrpgCharacter{ Name = "HereMatchlock", Id = 1330, Rating = new CrpgCharacterRating { Value = 1099 }}};
        private static CrpgUser AbatisPilotSnake = new() { Character = new CrpgCharacter{ Name = "AbatisPilotSnake", Id = 1331, Rating = new CrpgCharacterRating { Value = 1161 }}};
        private static CrpgUser BarrelSting = new() { Character = new CrpgCharacter{ Name = "BarrelSting", Id = 1332, Rating = new CrpgCharacterRating { Value = 2809 }}};
        private static CrpgUser CombustibleNight = new() { Character = new CrpgCharacter{ Name = "CombustibleNight", Id = 1333, Rating = new CrpgCharacterRating { Value = 2443 }}};
        private static CrpgUser CommandoLock = new() { Character = new CrpgCharacter{ Name = "CommandoLock", Id = 1334, Rating = new CrpgCharacterRating { Value = 1266 }}};
        private static CrpgUser BeestingTrench = new() { Character = new CrpgCharacter{ Name = "BeestingTrench", Id = 1335, Rating = new CrpgCharacterRating { Value = 630 }}};
        private static CrpgUser AfireSlough = new() { Character = new CrpgCharacter{ Name = "AfireSlough", Id = 1336, Rating = new CrpgCharacterRating { Value = 1619 }}};
        private static CrpgUser CoralSnakePyro = new() { Character = new CrpgCharacter{ Name = "CoralSnakePyro", Id = 1337, Rating = new CrpgCharacterRating { Value = 615 }}};
        private static CrpgUser BloodPhosphodiesterase = new() { Character = new CrpgCharacter{ Name = "BloodPhosphodiesterase", Id = 1338, Rating = new CrpgCharacterRating { Value = 695 }}};
        private static CrpgUser HiltMurrey = new() { Character = new CrpgCharacter{ Name = "HiltMurrey", Id = 1339, Rating = new CrpgCharacterRating { Value = 1174 }}};
        private static CrpgUser CharcoalPrisonerofWar = new() { Character = new CrpgCharacter{ Name = "CharcoalPrisonerofWar", Id = 1340, Rating = new CrpgCharacterRating { Value = 1696 }}};
        private static CrpgUser GunmetalSwelter = new() { Character = new CrpgCharacter{ Name = "GunmetalSwelter", Id = 1341, Rating = new CrpgCharacterRating { Value = 2797 }}};
        private static CrpgUser CaliginousPuce = new() { Character = new CrpgCharacter{ Name = "CaliginousPuce", Id = 1342, Rating = new CrpgCharacterRating { Value = 825 }}};
        private static CrpgUser BrownSloe = new() { Character = new CrpgCharacter{ Name = "BrownSloe", Id = 1343, Rating = new CrpgCharacterRating { Value = 1748 }}};
        private static CrpgUser FiredFishQuench = new() { Character = new CrpgCharacter{ Name = "FiredFishQuench", Id = 1344, Rating = new CrpgCharacterRating { Value = 558 }}};
        private static CrpgUser CaperLynx = new() { Character = new CrpgCharacter{ Name = "CaperLynx", Id = 1345, Rating = new CrpgCharacterRating { Value = 770 }}};
        private static CrpgUser TastyCalyx = new() { Character = new CrpgCharacter{ Name = "TastyCalyx", Id = 1346, Rating = new CrpgCharacterRating { Value = 545 }}};
        private static CrpgUser SiameseLavender = new() { Character = new CrpgCharacter{ Name = "SiameseLavender", Id = 1347, Rating = new CrpgCharacterRating { Value = 1912 }}};
        private static CrpgUser BeauChichi = new() { Character = new CrpgCharacter{ Name = "BeauChichi", Id = 1348, Rating = new CrpgCharacterRating { Value = 2004 }}};
        private static CrpgUser DogPanther = new() { Character = new CrpgCharacter{ Name = "DogPanther", Id = 1349, Rating = new CrpgCharacterRating { Value = 2210 }}};
        private static CrpgUser BlossomJelly = new() { Character = new CrpgCharacter{ Name = "BlossomJelly", Id = 1350, Rating = new CrpgCharacterRating { Value = 2133 }}};
        private static CrpgUser SharpPapergirl = new() { Character = new CrpgCharacter{ Name = "SharpPapergirl", Id = 1351, Rating = new CrpgCharacterRating { Value = 2547 }}};
        private static CrpgUser MoppetTear = new() { Character = new CrpgCharacter{ Name = "MoppetTear", Id = 1352, Rating = new CrpgCharacterRating { Value = 1364 }}};
        private static CrpgUser BlowHandsome = new() { Character = new CrpgCharacter{ Name = "BlowHandsome", Id = 1353, Rating = new CrpgCharacterRating { Value = 1455 }}};
        private static CrpgUser SisterSmirk = new() { Character = new CrpgCharacter{ Name = "SisterSmirk", Id = 1354, Rating = new CrpgCharacterRating { Value = 2864 }}};
        private static CrpgUser FelineSweetTooth = new() { Character = new CrpgCharacter{ Name = "FelineSweetTooth", Id = 1355, Rating = new CrpgCharacterRating { Value = 742 }}};
        private static CrpgUser SealSneak = new() { Character = new CrpgCharacter{ Name = "SealSneak", Id = 1356, Rating = new CrpgCharacterRating { Value = 1227 }}};
        private static CrpgUser TinyFetis = new() { Character = new CrpgCharacter{ Name = "TinyFetis", Id = 1357, Rating = new CrpgCharacterRating { Value = 1402 }}};
        private static CrpgUser LassBloom = new() { Character = new CrpgCharacter{ Name = "LassBloom", Id = 1358, Rating = new CrpgCharacterRating { Value = 751 }}};
        private static CrpgUser BoxDinky = new() { Character = new CrpgCharacter{ Name = "BoxDinky", Id = 1359, Rating = new CrpgCharacterRating { Value = 1688 }}};
        private static CrpgUser BriocheRam = new() { Character = new CrpgCharacter{ Name = "BriocheRam", Id = 1360, Rating = new CrpgCharacterRating { Value = 2750 }}};
        private static CrpgUser SweetKitty = new() { Character = new CrpgCharacter{ Name = "SweetKitty", Id = 1361, Rating = new CrpgCharacterRating { Value = 2678 }}};
        private static CrpgUser GrimalkinDelicacy = new() { Character = new CrpgCharacter{ Name = "GrimalkinDelicacy", Id = 1362, Rating = new CrpgCharacterRating { Value = 1057 }}};
        private static CrpgUser LionMuscatel = new() { Character = new CrpgCharacter{ Name = "LionMuscatel", Id = 1363, Rating = new CrpgCharacterRating { Value = 1903 }}};
        private static CrpgUser ExtrafloralUnicorn = new() { Character = new CrpgCharacter{ Name = "ExtrafloralUnicorn", Id = 1364, Rating = new CrpgCharacterRating { Value = 2923 }}};
        private static CrpgUser NewsgirlLitter = new() { Character = new CrpgCharacter{ Name = "NewsgirlLitter", Id = 1365, Rating = new CrpgCharacterRating { Value = 2515 }}};
        private static CrpgUser CatsBalm = new() { Character = new CrpgCharacter{ Name = "CatsBalm", Id = 1366, Rating = new CrpgCharacterRating { Value = 1786 }}};
        private static CrpgUser DiscriminateBlancmange = new() { Character = new CrpgCharacter{ Name = "DiscriminateBlancmange", Id = 1367, Rating = new CrpgCharacterRating { Value = 2151 }}};
        private static CrpgUser TroopBobcat = new() { Character = new CrpgCharacter{ Name = "TroopBobcat", Id = 1368, Rating = new CrpgCharacterRating { Value = 502 }}};
        private static CrpgUser PunctiliousQuirky = new() { Character = new CrpgCharacter{ Name = "PunctiliousQuirky", Id = 1369, Rating = new CrpgCharacterRating { Value = 865 }}};
        private static CrpgUser GuideyUnpretty = new() { Character = new CrpgCharacter{ Name = "GuideyUnpretty", Id = 1370, Rating = new CrpgCharacterRating { Value = 1450 }}};
        private static CrpgUser ScurryHuge = new() { Character = new CrpgCharacter{ Name = "ScurryHuge", Id = 1371, Rating = new CrpgCharacterRating { Value = 754 }}};
        private static CrpgUser SlatternLoving = new() { Character = new CrpgCharacter{ Name = "SlatternLoving", Id = 1372, Rating = new CrpgCharacterRating { Value = 2238 }}};
        private static CrpgUser OnlyGirlChild = new() { Character = new CrpgCharacter{ Name = "OnlyGirlChild", Id = 1373, Rating = new CrpgCharacterRating { Value = 1249 }}};
        private static CrpgUser SwellSomali = new() { Character = new CrpgCharacter{ Name = "SwellSomali", Id = 1374, Rating = new CrpgCharacterRating { Value = 2813 }}};
        private static CrpgUser LatinaCyme = new() { Character = new CrpgCharacter{ Name = "LatinaCyme", Id = 1375, Rating = new CrpgCharacterRating { Value = 2221 }}};
        private static CrpgUser ScrumptiousKettleofFish = new() { Character = new CrpgCharacter{ Name = "ScrumptiousKettleofFish", Id = 1376, Rating = new CrpgCharacterRating { Value = 2061 }}};
        private static CrpgUser PearlPosy = new() { Character = new CrpgCharacter{ Name = "PearlPosy", Id = 1377, Rating = new CrpgCharacterRating { Value = 1745 }}};
        private static CrpgUser AlyssumMilkmaid = new() { Character = new CrpgCharacter{ Name = "AlyssumMilkmaid", Id = 1378, Rating = new CrpgCharacterRating { Value = 1251 }}};
        private static CrpgUser ChrysanthemumPeachy = new() { Character = new CrpgCharacter{ Name = "ChrysanthemumPeachy", Id = 1379, Rating = new CrpgCharacterRating { Value = 1083 }}};
        private static CrpgUser CalicoBun = new() { Character = new CrpgCharacter{ Name = "CalicoBun", Id = 1380, Rating = new CrpgCharacterRating { Value = 635 }}};
        private static CrpgUser BunnyCatPudgy = new() { Character = new CrpgCharacter{ Name = "BunnyCatPudgy", Id = 1381, Rating = new CrpgCharacterRating { Value = 2810 }}};
        private static CrpgUser CandiedFragrant = new() { Character = new CrpgCharacter{ Name = "CandiedFragrant", Id = 1382, Rating = new CrpgCharacterRating { Value = 1004 }}};
        private static CrpgUser MadamTomboy = new() { Character = new CrpgCharacter{ Name = "MadamTomboy", Id = 1383, Rating = new CrpgCharacterRating { Value = 975 }}};
        private static CrpgUser GynoeciumFeat = new() { Character = new CrpgCharacter{ Name = "GynoeciumFeat", Id = 1384, Rating = new CrpgCharacterRating { Value = 866 }}};
        private static CrpgUser PreciseAnthesis = new() { Character = new CrpgCharacter{ Name = "PreciseAnthesis", Id = 1385, Rating = new CrpgCharacterRating { Value = 2696 }}};
        private static CrpgUser SaccharineLamb = new() { Character = new CrpgCharacter{ Name = "SaccharineLamb", Id = 1386, Rating = new CrpgCharacterRating { Value = 2021 }}};
        private static CrpgUser CoquettePleasant = new() { Character = new CrpgCharacter{ Name = "CoquettePleasant", Id = 1387, Rating = new CrpgCharacterRating { Value = 1384 }}};
        private static CrpgUser LilacSweetly = new() { Character = new CrpgCharacter{ Name = "LilacSweetly", Id = 1388, Rating = new CrpgCharacterRating { Value = 782 }}};
        private static CrpgUser EmbarrassedMeow = new() { Character = new CrpgCharacter{ Name = "EmbarrassedMeow", Id = 1389, Rating = new CrpgCharacterRating { Value = 1489 }}};
        private static CrpgUser FloweringMissy = new() { Character = new CrpgCharacter{ Name = "FloweringMissy", Id = 1390, Rating = new CrpgCharacterRating { Value = 896 }}};
        private static CrpgUser CuttyClamber = new() { Character = new CrpgCharacter{ Name = "CuttyClamber", Id = 1391, Rating = new CrpgCharacterRating { Value = 2338 }}};
        private static CrpgUser PrettilyThalamus = new() { Character = new CrpgCharacter{ Name = "PrettilyThalamus", Id = 1392, Rating = new CrpgCharacterRating { Value = 1346 }}};
        private static CrpgUser EncounterPollination = new() { Character = new CrpgCharacter{ Name = "EncounterPollination", Id = 1393, Rating = new CrpgCharacterRating { Value = 576 }}};
        private static CrpgUser PatrolBonbon = new() { Character = new CrpgCharacter{ Name = "PatrolBonbon", Id = 1394, Rating = new CrpgCharacterRating { Value = 2468 }}};
        private static CrpgUser PortFem = new() { Character = new CrpgCharacter{ Name = "PortFem", Id = 1395, Rating = new CrpgCharacterRating { Value = 1659 }}};
        private static CrpgUser BudgereePerianth = new() { Character = new CrpgCharacter{ Name = "BudgereePerianth", Id = 1396, Rating = new CrpgCharacterRating { Value = 2764 }}};
        private static CrpgUser PsycheStaminate = new() { Character = new CrpgCharacter{ Name = "PsycheStaminate", Id = 1397, Rating = new CrpgCharacterRating { Value = 1035 }}};
        private static CrpgUser HoneyedSugar = new() { Character = new CrpgCharacter{ Name = "HoneyedSugar", Id = 1399, Rating = new CrpgCharacterRating { Value = 2216 }}};
    private GameMatch game1 = new()
    {
        TeamA = new List<CrpgUser> { },
        TeamB = new List<CrpgUser> { },
        Waiting = new List<CrpgUser>
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
                Vlexance_03,/*
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
                HoneyedSugar,*/
            },
    };
}
}
