using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Api.Models.Clans;
using Crpg.Module.Api.Models.Users;
using Crpg.Module.Balancing;
using Crpg.Module.Helpers;
using NUnit.Framework;

namespace Crpg.Module.UTest.Balancing;

public class MatchBalancingSystemTest
 {
    private static readonly CrpgClan LOtr = new() { Id = 1, Name = "LOTR" };
    private static readonly CrpgClan DBz = new() { Id = 2, Name = "DBZ" };
    private static readonly CrpgClan Gilead = new() { Id = 3, Name = "Gilead" };
    private static readonly CrpgClan Poudlard = new() { Id = 4, Name = "Poudlard" };
    private static readonly CrpgClan XMen = new() { Id = 5, Name = "X-MEN" };
    private static readonly CrpgClan XMenVillain = new() { Id = 6, Name = "X-MEN Villains" };
    private static readonly CrpgClan JeanJean = new() { Id = 7, Name = "JeanJean" };
    private static readonly CrpgClan Glut = new() { Id = 8, Name = "Glut" };
    private static readonly CrpgClan Vlex = new() { Id = 9, Name = "Vlex" };
    private static readonly CrpgClan Hudahut = new() { Id = 10, Name = "Hudahut" };
    private static readonly CrpgClan BlackBaronesses = new() { Id = 11, Name = "Black Baronesses" };
    private static readonly CrpgClan NavyKnights = new() { Id = 12, Name = "Navy Knights" };
    private static readonly CrpgClan PurplePeasants = new() { Id = 13, Name = "Purple Peasants" };
    private static readonly CrpgClan RedRitters = new() { Id = 14, Name = "Red Ritters" };
    private static readonly CrpgClan LemonLevies = new() { Id = 15, Name = "Lemon Levies" };
    private static readonly CrpgClan ScarletShieldmaidens = new() { Id = 16, Name = "Scarlet Shieldmaidens" };
    private static readonly CrpgClan Dumpsters = new() { Id = 50, Name = "DUMPSTERS" };
    private static readonly CrpgClan Trashcans = new() { Id = 51, Name = "TRASHCANS" };
    private static readonly CrpgClan Scrubs = new() { Id = 52, Name = "SCRUBS" };
    private static readonly CrpgClan Garbage = new() { Id = 53, Name = "GARBAGE" };
    private static readonly CrpgClan Basura = new() { Id = 54, Name = "BASURA" };
    private static readonly CrpgClan Waste = new() { Id = 55, Name = "WASTE" };
    private static readonly CrpgClan Bads = new() { Id = 56, Name = "BADS" };
    private static readonly CrpgClan Poors = new() { Id = 57, Name = "POORS" };
    private static readonly CrpgClan Peasantry = new() { Id = 58, Name = "PEASANTRY" };
    private static readonly CrpgClan Serfs = new() { Id = 59, Name = "SERFS" };
    private static readonly CrpgClan Vagabonds = new() { Id = 60, Name = "VAGABONDS" };

    private static readonly CrpgUser Aragorn = new() { Character = new CrpgCharacter { Name = "Aragorn", Id = 0, Rating = new CrpgCharacterRating { Value = -1000 } }, ClanMembership = new CrpgClanMember { ClanId = 1 } };
    private static readonly CrpgUser Arwen = new() { Character = new CrpgCharacter { Name = "Arwen",  Id = 1, Rating = new CrpgCharacterRating { Value = -2000 } }, ClanMembership = new CrpgClanMember { ClanId = 1 } };
    private static readonly CrpgUser Frodon = new() { Character = new CrpgCharacter { Name = "Frodon", Id = 2, Rating = new CrpgCharacterRating { Value = 1600 } }, ClanMembership = new CrpgClanMember { ClanId = 1 } };
    private static readonly CrpgUser Sam = new() { Character = new CrpgCharacter { Name = "Sam", Id = 3, Rating = new CrpgCharacterRating { Value = 15000 } }, ClanMembership = new CrpgClanMember { ClanId = 1 } };
    private static readonly CrpgUser Sangoku = new() { Character = new CrpgCharacter { Name = "Sangoku", Id = 4, Rating = new CrpgCharacterRating { Value = 2000 } }, ClanMembership = new CrpgClanMember { ClanId = 2 } };
    private static readonly CrpgUser Krilin = new() { Character = new CrpgCharacter { Name = "Krilin", Id = 5, Rating = new CrpgCharacterRating { Value = 1000 } }, ClanMembership = new CrpgClanMember { ClanId = 2 } };
    private static readonly CrpgUser RolandDeschain = new() { Character = new CrpgCharacter { Name = "Roland Deschain", Id = 6, Rating = new CrpgCharacterRating { Value = 2800 } }, ClanMembership = new CrpgClanMember { ClanId = 3 } };
    private static readonly CrpgUser HarryPotter = new() { Character = new CrpgCharacter { Name = "Harry Potter", Id = 7, Rating = new CrpgCharacterRating { Value = 2000 } }, ClanMembership = new CrpgClanMember { ClanId = 4 } };
    private static readonly CrpgUser Magneto = new() { Character = new CrpgCharacter { Name = "Magneto", Id = 8, Rating = new CrpgCharacterRating { Value = 2700 } }, ClanMembership = new CrpgClanMember { ClanId = 6 } };
    private static readonly CrpgUser ProfCharles = new() { Character = new CrpgCharacter { Name = "Professor Charles", Id = 9, Rating = new CrpgCharacterRating { Value = 2800 } }, ClanMembership = new CrpgClanMember { ClanId = 5 } };
    private static readonly CrpgUser UsainBolt = new() { Character = new CrpgCharacter { Name = "Usain Bolt", Id = 10, Rating = new CrpgCharacterRating { Value = 1200 } } };
    private static readonly CrpgUser Agent007 = new() { Character = new CrpgCharacter { Name = "Agent 007", Id = 11, Rating = new CrpgCharacterRating { Value = 1300 } } };
    private static readonly CrpgUser SpongeBob = new() { Character = new CrpgCharacter { Name = "SpongeBob", Id = 12, Rating = new CrpgCharacterRating { Value = 800 } } };
    private static readonly CrpgUser Patrick = new() { Character = new CrpgCharacter { Name = "Patrick", Id = 13, Rating = new CrpgCharacterRating { Value = 500 } } };
    private static readonly CrpgUser Madonna = new() { Character = new CrpgCharacter { Name = "Madonna", Id = 14, Rating = new CrpgCharacterRating { Value = 1100 } } };
    private static readonly CrpgUser LaraCroft = new() { Character = new CrpgCharacter { Name = "Lara Croft", Id = 15, Rating = new CrpgCharacterRating { Value = 3500 } } };
    private static readonly CrpgUser JeanneDArc = new() { Character = new CrpgCharacter { Name = "Jeanne D'ARC", Id = 16, Rating = new CrpgCharacterRating { Value = 2400 } } };
    private static readonly CrpgUser Merlin = new() { Character = new CrpgCharacter { Name = "Merlin", Id = 17, Rating = new CrpgCharacterRating { Value = 2700 } } };
    private static readonly CrpgUser Bob = new() { Character = new CrpgCharacter { Name = "Bob", Id = 18, Rating = new CrpgCharacterRating { Value = 1100 } } };
    private static readonly CrpgUser Thomas = new() { Character = new CrpgCharacter { Name = "Thomas", Id = 19, Rating = new CrpgCharacterRating { Value = 2400 } } };
    private static readonly CrpgUser RonWeasley = new() { Character = new CrpgCharacter { Name = "Ron Weasley", Id = 20, Rating = new CrpgCharacterRating { Value = 600 } }, ClanMembership = new CrpgClanMember { ClanId = 4 } };
    private static readonly CrpgUser Jean01 = new() { Character = new CrpgCharacter { Name = "Jean_01", Id = 21, Rating = new CrpgCharacterRating { Value = 3000 } }, ClanMembership = new CrpgClanMember { ClanId = 7 } };
    private static readonly CrpgUser Jean02 = new() { Character = new CrpgCharacter { Name = "Jean_02", Id = 22, Rating = new CrpgCharacterRating { Value = 2500 } }, ClanMembership = new CrpgClanMember { ClanId = 7 } };
    private static readonly CrpgUser Jean03 = new() { Character = new CrpgCharacter { Name = "Jean_03", Id = 23, Rating = new CrpgCharacterRating { Value = 2100 } }, ClanMembership = new CrpgClanMember { ClanId = 7 } };
    private static readonly CrpgUser Jean04 = new() { Character = new CrpgCharacter { Name = "Jean_04", Id = 24, Rating = new CrpgCharacterRating { Value = 1200 } }, ClanMembership = new CrpgClanMember { ClanId = 7 } };
    private static readonly CrpgUser Jean05 = new() { Character = new CrpgCharacter { Name = "Jean_05", Id = 25, Rating = new CrpgCharacterRating { Value = 800 } }, ClanMembership = new CrpgClanMember { ClanId = 7 } };
    private static readonly CrpgUser Glutentag01 = new() { Character = new CrpgCharacter { Name = "Glutentag_01", Id = 26, Rating = new CrpgCharacterRating { Value = 900 } }, ClanMembership = new CrpgClanMember { ClanId = 8 } };
    private static readonly CrpgUser Glutentag02 = new() { Character = new CrpgCharacter { Name = "Glutentag_02", Id = 27, Rating = new CrpgCharacterRating { Value = 200 } }, ClanMembership = new CrpgClanMember { ClanId = 8 } };
    private static readonly CrpgUser Glutentag03 = new() { Character = new CrpgCharacter { Name = "Glutentag_03", Id = 28, Rating = new CrpgCharacterRating { Value = 2200 } }, ClanMembership = new CrpgClanMember { ClanId = 8 } };
    private static readonly CrpgUser Glutentag04 = new() { Character = new CrpgCharacter { Name = "Glutentag_04", Id = 29, Rating = new CrpgCharacterRating { Value = 400 } }, ClanMembership = new CrpgClanMember { ClanId = 8 } };
    private static readonly CrpgUser Glutentag05 = new() { Character = new CrpgCharacter { Name = "Glutentag_05", Id = 30, Rating = new CrpgCharacterRating { Value = 800 } }, ClanMembership = new CrpgClanMember { ClanId = 8 } };
    private static readonly CrpgUser Vlexance01 = new() { Character = new CrpgCharacter { Name = "Vlexance_01", Id = 31, Rating = new CrpgCharacterRating { Value = 2600 } }, ClanMembership = new CrpgClanMember { ClanId = 9 } };
    private static readonly CrpgUser Vlexance02 = new() { Character = new CrpgCharacter { Name = "Vlexance_02", Id = 32, Rating = new CrpgCharacterRating { Value = 2300 } }, ClanMembership = new CrpgClanMember { ClanId = 9 } };
    private static readonly CrpgUser Vlexance03 = new() { Character = new CrpgCharacter { Name = "Vlexance_03", Id = 33, Rating = new CrpgCharacterRating { Value = 1300 } }, ClanMembership = new CrpgClanMember { ClanId = 9 } };
    private static readonly CrpgUser Vlexance04 = new() { Character = new CrpgCharacter { Name = "Vlexance_04", Id = 34, Rating = new CrpgCharacterRating { Value = 1100 } }, ClanMembership = new CrpgClanMember { ClanId = 9 } };
    private static readonly CrpgUser Vlexance05 = new() { Character = new CrpgCharacter { Name = "Vlexance_05", Id = 35, Rating = new CrpgCharacterRating { Value = 300 } }, ClanMembership = new CrpgClanMember { ClanId = 9 } };
    private static readonly CrpgUser Hudax01 = new() { Character = new CrpgCharacter { Name = "Hudax_01", Id = 36, Rating = new CrpgCharacterRating { Value = 1100 } }, ClanMembership = new CrpgClanMember { ClanId = 10 } };
    private static readonly CrpgUser Hudax02 = new() { Character = new CrpgCharacter { Name = "Hudax_02", Id = 37, Rating = new CrpgCharacterRating { Value = 2900 } }, ClanMembership = new CrpgClanMember { ClanId = 10 } };
    private static readonly CrpgUser Hudax03 = new() { Character = new CrpgCharacter { Name = "Hudax_03", Id = 38, Rating = new CrpgCharacterRating { Value = 1700 } }, ClanMembership = new CrpgClanMember { ClanId = 10 } };
    private static readonly CrpgUser Hudax04 = new() { Character = new CrpgCharacter { Name = "Hudax_04", Id = 39, Rating = new CrpgCharacterRating { Value = 1500 } }, ClanMembership = new CrpgClanMember { ClanId = 10 } };
    private static readonly CrpgUser Hudax05 = new() { Character = new CrpgCharacter { Name = "Hudax_05", Id = 40, Rating = new CrpgCharacterRating { Value = 2200 } }, ClanMembership = new CrpgClanMember { ClanId = 10 } };
    private static readonly CrpgUser Hudax06 = new() { Character = new CrpgCharacter { Name = string.Empty, Id = 5036, Rating = new CrpgCharacterRating { Value = 1900 } }, ClanMembership = new CrpgClanMember { ClanId = 10 } };
    private static readonly CrpgUser Hudax07 = new() { Character = new CrpgCharacter { Name = string.Empty, Id = 5037, Rating = new CrpgCharacterRating { Value = 8000 } }, ClanMembership = new CrpgClanMember { ClanId = 10 } };
    private static readonly CrpgUser Hudax08 = new() { Character = new CrpgCharacter { Name = string.Empty, Id = 5038, Rating = new CrpgCharacterRating { Value = 1300 } }, ClanMembership = new CrpgClanMember { ClanId = 10 } };
    private static readonly CrpgUser Hudax09 = new() { Character = new CrpgCharacter { Name = string.Empty, Id = 5039, Rating = new CrpgCharacterRating { Value = 1400 } }, ClanMembership = new CrpgClanMember { ClanId = 10 } };
    private static readonly CrpgUser Hudax10 = new() { Character = new CrpgCharacter { Name = string.Empty, Id = 5040, Rating = new CrpgCharacterRating { Value = 700 } }, ClanMembership = new CrpgClanMember { ClanId = 10 } };
    private static readonly CrpgUser GerryShepherd = new() { Character = new CrpgCharacter { Name = "GerryShepherd", Id = 41, Rating = new CrpgCharacterRating { Value = 2000 } }, ClanMembership = new CrpgClanMember { ClanId = 11 } };
    private static readonly CrpgUser BullyDog = new() { Character = new CrpgCharacter { Name = "BullyDog", Id = 42, Rating = new CrpgCharacterRating { Value = 1600 } }, ClanMembership = new CrpgClanMember { ClanId = 11 } };
    private static readonly CrpgUser LabbyRetriever = new() { Character = new CrpgCharacter { Name = "LabbyRetriever", Id = 43, Rating = new CrpgCharacterRating { Value = 1500 } }, ClanMembership = new CrpgClanMember { ClanId = 11 } };
    private static readonly CrpgUser GoldyRetriever = new() { Character = new CrpgCharacter { Name = "GoldyRetriever", Id = 44, Rating = new CrpgCharacterRating { Value = 2000 } }, ClanMembership = new CrpgClanMember { ClanId = 12 } };
    private static readonly CrpgUser SibbyHusky = new() { Character = new CrpgCharacter { Name = "SibbyHusky", Id = 45, Rating = new CrpgCharacterRating { Value = 1000 } }, ClanMembership = new CrpgClanMember { ClanId = 12 } };
    private static readonly CrpgUser Poodlums = new() { Character = new CrpgCharacter { Name = "Poodlums", Id = 46, Rating = new CrpgCharacterRating { Value = 2800 } }, ClanMembership = new CrpgClanMember { ClanId = 13 } };
    private static readonly CrpgUser BordyCollie = new() { Character = new CrpgCharacter { Name = "BordyCollie", Id = 47, Rating = new CrpgCharacterRating { Value = 2000 } }, ClanMembership = new CrpgClanMember { ClanId = 14 } };
    private static readonly CrpgUser Rottyweiler = new() { Character = new CrpgCharacter { Name = "Rottyweiler", Id = 48, Rating = new CrpgCharacterRating { Value = 2700 } }, ClanMembership = new CrpgClanMember { ClanId = 15 } };
    private static readonly CrpgUser Daschyhund = new() { Character = new CrpgCharacter { Name = "Daschyhund", Id = 49, Rating = new CrpgCharacterRating { Value = 2800 } }, ClanMembership = new CrpgClanMember { ClanId = 16 } };
    private static readonly CrpgUser GreatieDane = new() { Character = new CrpgCharacter { Name = "GreatieDane", Id = 50, Rating = new CrpgCharacterRating { Value = 1200 } } };
    private static readonly CrpgUser YorkyTerrier = new() { Character = new CrpgCharacter { Name = "YorkyTerrier", Id = 51, Rating = new CrpgCharacterRating { Value = 1300 } } };
    private static readonly CrpgUser CockySpaniel = new() { Character = new CrpgCharacter { Name = "CockySpaniel", Id = 52, Rating = new CrpgCharacterRating { Value = 800 } } };
    private static readonly CrpgUser Pomyranian = new() { Character = new CrpgCharacter { Name = "Pomyranian", Id = 53, Rating = new CrpgCharacterRating { Value = 500 } } };
    private static readonly CrpgUser Bullymastiff = new() { Character = new CrpgCharacter { Name = "Bullymastiff", Id = 54, Rating = new CrpgCharacterRating { Value = 1100 } } };
    private static readonly CrpgUser JackyRussell = new() { Character = new CrpgCharacter { Name = "JackyRussell", Id = 55, Rating = new CrpgCharacterRating { Value = 3500 } } };
    private static readonly CrpgUser Akitayinu = new() { Character = new CrpgCharacter { Name = "Akitayinu", Id = 56, Rating = new CrpgCharacterRating { Value = 2400 } } };
    private static readonly CrpgUser Maltiepoo = new() { Character = new CrpgCharacter { Name = "Maltiepoo", Id = 57, Rating = new CrpgCharacterRating { Value = 2700 } } };
    private static readonly CrpgUser Doberymann = new() { Character = new CrpgCharacter { Name = "Doberymann", Id = 58, Rating = new CrpgCharacterRating { Value = 1100 } } };
    private static readonly CrpgUser Sheeiitzu = new() { Character = new CrpgCharacter { Name = "Sheeiitzu", Id = 59, Rating = new CrpgCharacterRating { Value = 2400 } } };
    private static readonly CrpgUser BassetyHound = new() { Character = new CrpgCharacter { Name = "BassetyHound", Id = 60, Rating = new CrpgCharacterRating { Value = 600 } }, ClanMembership = new CrpgClanMember { ClanId = 14 } };
    private static readonly CrpgUser GopherSnakeWeb = new() { Character = new CrpgCharacter { Name = "GopherSnakeWeb", Id = 1000, Rating = new CrpgCharacterRating { Value = 819 } }, ClanMembership = new CrpgClanMember { ClanId = 58 } };
    private static readonly CrpgUser AmbushSword = new() { Character = new CrpgCharacter { Name = "AmbushSword", Id = 1001, Rating = new CrpgCharacterRating { Value = 2019 } }, ClanMembership = new CrpgClanMember { ClanId = 50 } };
    private static readonly CrpgUser FencingPacMan = new() { Character = new CrpgCharacter { Name = "FencingPacMan", Id = 1002, Rating = new CrpgCharacterRating { Value = 738 } }, ClanMembership = new CrpgClanMember { ClanId = 53 } };
    private static readonly CrpgUser EbonSalient = new() { Character = new CrpgCharacter { Name = "EbonSalient", Id = 1003, Rating = new CrpgCharacterRating { Value = 1381 } }, ClanMembership = new CrpgClanMember { ClanId = 52 } };
    private static readonly CrpgUser CannonSnaky = new() { Character = new CrpgCharacter { Name = "CannonSnaky", Id = 1004, Rating = new CrpgCharacterRating { Value = 2140 } }, ClanMembership = new CrpgClanMember { ClanId = 55 } };
    private static readonly CrpgUser DarklyWine = new() { Character = new CrpgCharacter { Name = "DarklyWine", Id = 1005, Rating = new CrpgCharacterRating { Value = 2295 } }, ClanMembership = new CrpgClanMember { ClanId = 52 } };
    private static readonly CrpgUser BonfireQuillon = new() { Character = new CrpgCharacter { Name = "BonfireQuillon", Id = 1006, Rating = new CrpgCharacterRating { Value = 2304 } }, ClanMembership = new CrpgClanMember { ClanId = 52 } };
    private static readonly CrpgUser BunnySlopeStationHouse = new() { Character = new CrpgCharacter { Name = "BunnySlopeStationHouse", Id = 1007, Rating = new CrpgCharacterRating { Value = 2067 } }, ClanMembership = new CrpgClanMember { ClanId = 50 } };
    private static readonly CrpgUser BridgeheadRattlesnake = new() { Character = new CrpgCharacter { Name = "BridgeheadRattlesnake", Id = 1008, Rating = new CrpgCharacterRating { Value = 1809 } }, ClanMembership = new CrpgClanMember { ClanId = 54 } };
    private static readonly CrpgUser InfernoSunless = new() { Character = new CrpgCharacter { Name = "InfernoSunless", Id = 1009, Rating = new CrpgCharacterRating { Value = 1765 } }, ClanMembership = new CrpgClanMember { ClanId = 56 } };
    private static readonly CrpgUser BarricadePrince = new() { Character = new CrpgCharacter { Name = "BarricadePrince", Id = 1010, Rating = new CrpgCharacterRating { Value = 2150 } }, ClanMembership = new CrpgClanMember { ClanId = 52 } };
    private static readonly CrpgUser FoulKingdom = new() { Character = new CrpgCharacter { Name = "FoulKingdom", Id = 1011, Rating = new CrpgCharacterRating { Value = 1718 } }, ClanMembership = new CrpgClanMember { ClanId = 51 } };
    private static readonly CrpgUser DarknessJoeBlake = new() { Character = new CrpgCharacter { Name = "DarknessJoeBlake", Id = 1012, Rating = new CrpgCharacterRating { Value = 2499 } }, ClanMembership = new CrpgClanMember { ClanId = 57 } };
    private static readonly CrpgUser ExtinguisherPommel = new() { Character = new CrpgCharacter { Name = "ExtinguisherPommel", Id = 1013, Rating = new CrpgCharacterRating { Value = 1791 } }, ClanMembership = new CrpgClanMember { ClanId = 60 } };
    private static readonly CrpgUser CaliberKingship = new() { Character = new CrpgCharacter { Name = "CaliberKingship", Id = 1014, Rating = new CrpgCharacterRating { Value = 692 } }, ClanMembership = new CrpgClanMember { ClanId = 50 } };
    private static readonly CrpgUser FirelightSalvo = new() { Character = new CrpgCharacter { Name = "FirelightSalvo", Id = 1015, Rating = new CrpgCharacterRating { Value = 2226 } }, ClanMembership = new CrpgClanMember { ClanId = 51 } };
    private static readonly CrpgUser GarnetMonarch = new() { Character = new CrpgCharacter { Name = "GarnetMonarch", Id = 1016, Rating = new CrpgCharacterRating { Value = 1075 } }, ClanMembership = new CrpgClanMember { ClanId = 51 } };
    private static readonly CrpgUser EdgedKatana = new() { Character = new CrpgCharacter { Name = "EdgedKatana", Id = 1017, Rating = new CrpgCharacterRating { Value = 2335 } }, ClanMembership = new CrpgClanMember { ClanId = 60 } };
    private static readonly CrpgUser AntichristKnife = new() { Character = new CrpgCharacter { Name = "AntichristKnife", Id = 1018, Rating = new CrpgCharacterRating { Value = 2743 } }, ClanMembership = new CrpgClanMember { ClanId = 50 } };
    private static readonly CrpgUser DarkenThrust = new() { Character = new CrpgCharacter { Name = "DarkenThrust", Id = 1019, Rating = new CrpgCharacterRating { Value = 1084 } }, ClanMembership = new CrpgClanMember { ClanId = 52 } };
    private static readonly CrpgUser AnaphylacticShockLowering = new() { Character = new CrpgCharacter { Name = "AnaphylacticShockLowering", Id = 1020, Rating = new CrpgCharacterRating { Value = 1969 } }, ClanMembership = new CrpgClanMember { ClanId = 55 } };
    private static readonly CrpgUser ApprenticeSpottedAdder = new() { Character = new CrpgCharacter { Name = "ApprenticeSpottedAdder", Id = 1021, Rating = new CrpgCharacterRating { Value = 2189 } }, ClanMembership = new CrpgClanMember { ClanId = 57 } };
    private static readonly CrpgUser DrawTreacle = new() { Character = new CrpgCharacter { Name = "DrawTreacle", Id = 1022, Rating = new CrpgCharacterRating { Value = 2215 } }, ClanMembership = new CrpgClanMember { ClanId = 59 } };
    private static readonly CrpgUser AglyphousObscure = new() { Character = new CrpgCharacter { Name = "AglyphousObscure", Id = 1023, Rating = new CrpgCharacterRating { Value = 2381 } }, ClanMembership = new CrpgClanMember { ClanId = 52 } };
    private static readonly CrpgUser BackfangedWalk = new() { Character = new CrpgCharacter { Name = "BackfangedWalk", Id = 1024, Rating = new CrpgCharacterRating { Value = 1341 } }, ClanMembership = new CrpgClanMember { ClanId = 54 } };
    private static readonly CrpgUser ArachnomorphaeScathe = new() { Character = new CrpgCharacter { Name = "ArachnomorphaeScathe", Id = 1025, Rating = new CrpgCharacterRating { Value = 2160 } }, ClanMembership = new CrpgClanMember { ClanId = 55 } };
    private static readonly CrpgUser DisenvenomShadowy = new() { Character = new CrpgCharacter { Name = "DisenvenomShadowy", Id = 1026, Rating = new CrpgCharacterRating { Value = 2330 } }, ClanMembership = new CrpgClanMember { ClanId = 54 } };
    private static readonly CrpgUser BroadswordKick = new() { Character = new CrpgCharacter { Name = "BroadswordKick", Id = 1027, Rating = new CrpgCharacterRating { Value = 2978 } }, ClanMembership = new CrpgClanMember { ClanId = 53 } };
    private static readonly CrpgUser DuskNovelist = new() { Character = new CrpgCharacter { Name = "DuskNovelist", Id = 1028, Rating = new CrpgCharacterRating { Value = 729 } }, ClanMembership = new CrpgClanMember { ClanId = 50 } };
    private static readonly CrpgUser PinkPanther = new() { Character = new CrpgCharacter { Name = "PinkPanther", Id = 1029, Rating = new CrpgCharacterRating { Value = 2854 } }, ClanMembership = new CrpgClanMember { ClanId = 56 } };
    private static readonly CrpgUser DirkSubfusc = new() { Character = new CrpgCharacter { Name = "DirkSubfusc", Id = 1030, Rating = new CrpgCharacterRating { Value = 2423 } }, ClanMembership = new CrpgClanMember { ClanId = 57 } };
    private static readonly CrpgUser FireServiceProbationer = new() { Character = new CrpgCharacter { Name = "FireServiceProbationer", Id = 1031, Rating = new CrpgCharacterRating { Value = 1800 } }, ClanMembership = new CrpgClanMember { ClanId = 60 } };
    private static readonly CrpgUser BetrayPrehensor = new() { Character = new CrpgCharacter { Name = "BetrayPrehensor", Id = 1032, Rating = new CrpgCharacterRating { Value = 2739 } }, ClanMembership = new CrpgClanMember { ClanId = 50 } };
    private static readonly CrpgUser FlaskTigerSnake = new() { Character = new CrpgCharacter { Name = "FlaskTigerSnake", Id = 1033, Rating = new CrpgCharacterRating { Value = 1737 } }, ClanMembership = new CrpgClanMember { ClanId = 52 } };
    private static readonly CrpgUser BeginnerPlatypus = new() { Character = new CrpgCharacter { Name = "BeginnerPlatypus", Id = 1034, Rating = new CrpgCharacterRating { Value = 2472 } }, ClanMembership = new CrpgClanMember { ClanId = 51 } };
    private static readonly CrpgUser BushmasterSteel = new() { Character = new CrpgCharacter { Name = "BushmasterSteel", Id = 1035, Rating = new CrpgCharacterRating { Value = 1930 } }, ClanMembership = new CrpgClanMember { ClanId = 58 } };
    private static readonly CrpgUser BreechIron = new() { Character = new CrpgCharacter { Name = "BreechIron", Id = 1036, Rating = new CrpgCharacterRating { Value = 513 } }, ClanMembership = new CrpgClanMember { ClanId = 50 } };
    private static readonly CrpgUser BarbecueLivid = new() { Character = new CrpgCharacter { Name = "BarbecueLivid", Id = 1037, Rating = new CrpgCharacterRating { Value = 1065 } }, ClanMembership = new CrpgClanMember { ClanId = 59 } };
    private static readonly CrpgUser InfantRinkhals = new() { Character = new CrpgCharacter { Name = "InfantRinkhals", Id = 1038, Rating = new CrpgCharacterRating { Value = 1612 } }, ClanMembership = new CrpgClanMember { ClanId = 51 } };
    private static readonly CrpgUser AtterStranger = new() { Character = new CrpgCharacter { Name = "AtterStranger", Id = 1039, Rating = new CrpgCharacterRating { Value = 2987 } }, ClanMembership = new CrpgClanMember { ClanId = 60 } };
    private static readonly CrpgUser BanditKrait = new() { Character = new CrpgCharacter { Name = "BanditKrait", Id = 1040, Rating = new CrpgCharacterRating { Value = 2313 } }, ClanMembership = new CrpgClanMember { ClanId = 51 } };
    private static readonly CrpgUser IntelligenceMatchless = new() { Character = new CrpgCharacter { Name = "IntelligenceMatchless", Id = 1041, Rating = new CrpgCharacterRating { Value = 2064 } }, ClanMembership = new CrpgClanMember { ClanId = 50 } };
    private static readonly CrpgUser GrillMuzzle = new() { Character = new CrpgCharacter { Name = "GrillMuzzle", Id = 1042, Rating = new CrpgCharacterRating { Value = 555 } }, ClanMembership = new CrpgClanMember { ClanId = 52 } };
    private static readonly CrpgUser BombinateTwo = new() { Character = new CrpgCharacter { Name = "BombinateTwo", Id = 1043, Rating = new CrpgCharacterRating { Value = 2778 } }, ClanMembership = new CrpgClanMember { ClanId = 58 } };
    private static readonly CrpgUser GunRapid = new() { Character = new CrpgCharacter { Name = "GunRapid", Id = 1044, Rating = new CrpgCharacterRating { Value = 1269 } }, ClanMembership = new CrpgClanMember { ClanId = 58 } };
    private static readonly CrpgUser FlameproofReprisal = new() { Character = new CrpgCharacter { Name = "FlameproofReprisal", Id = 1045, Rating = new CrpgCharacterRating { Value = 631 } }, ClanMembership = new CrpgClanMember { ClanId = 60 } };
    private static readonly CrpgUser FullerMoccasin = new() { Character = new CrpgCharacter { Name = "FullerMoccasin", Id = 1046, Rating = new CrpgCharacterRating { Value = 2547 } }, ClanMembership = new CrpgClanMember { ClanId = 51 } };
    private static readonly CrpgUser HarassSmokeScreen = new() { Character = new CrpgCharacter { Name = "HarassSmokeScreen", Id = 1047, Rating = new CrpgCharacterRating { Value = 2266 } }, ClanMembership = new CrpgClanMember { ClanId = 55 } };
    private static readonly CrpgUser CyanoSax = new() { Character = new CrpgCharacter { Name = "CyanoSax", Id = 1048, Rating = new CrpgCharacterRating { Value = 1456 } }, ClanMembership = new CrpgClanMember { ClanId = 50 } };
    private static readonly CrpgUser DarksomeSwivel = new() { Character = new CrpgCharacter { Name = "DarksomeSwivel", Id = 1049, Rating = new CrpgCharacterRating { Value = 1458 } }, ClanMembership = new CrpgClanMember { ClanId = 53 } };
    private static readonly CrpgUser CounterspyMamba = new() { Character = new CrpgCharacter { Name = "CounterspyMamba", Id = 1050, Rating = new CrpgCharacterRating { Value = 1223 } }, ClanMembership = new CrpgClanMember { ClanId = 59 } };
    private static readonly CrpgUser FirewardRingedWaterSnake = new() { Character = new CrpgCharacter { Name = "FirewardRingedWaterSnake", Id = 1051, Rating = new CrpgCharacterRating { Value = 2477 } }, ClanMembership = new CrpgClanMember { ClanId = 51 } };
    private static readonly CrpgUser CombustMurky = new() { Character = new CrpgCharacter { Name = "CombustMurky", Id = 1052, Rating = new CrpgCharacterRating { Value = 2812 } }, ClanMembership = new CrpgClanMember { ClanId = 53 } };
    private static readonly CrpgUser AlightRoyal = new() { Character = new CrpgCharacter { Name = "AlightRoyal", Id = 1053, Rating = new CrpgCharacterRating { Value = 1850 } }, ClanMembership = new CrpgClanMember { ClanId = 53 } };
    private static readonly CrpgUser HandgunStrafe = new() { Character = new CrpgCharacter { Name = "HandgunStrafe", Id = 1054, Rating = new CrpgCharacterRating { Value = 1086 } }, ClanMembership = new CrpgClanMember { ClanId = 52 } };
    private static readonly CrpgUser FraternizeTenebrous = new() { Character = new CrpgCharacter { Name = "FraternizeTenebrous", Id = 1055, Rating = new CrpgCharacterRating { Value = 1936 } }, ClanMembership = new CrpgClanMember { ClanId = 53 } };
    private static readonly CrpgUser CounterespionageReconnaissance = new() { Character = new CrpgCharacter { Name = "CounterespionageReconnaissance", Id = 1056, Rating = new CrpgCharacterRating { Value = 1021 } }, ClanMembership = new CrpgClanMember { ClanId = 58 } };
    private static readonly CrpgUser HissRabbit = new() { Character = new CrpgCharacter { Name = "HissRabbit", Id = 1057, Rating = new CrpgCharacterRating { Value = 2537 } }, ClanMembership = new CrpgClanMember { ClanId = 57 } };
    private static readonly CrpgUser HappyVirulent = new() { Character = new CrpgCharacter { Name = "HappyVirulent", Id = 1058, Rating = new CrpgCharacterRating { Value = 2478 } }, ClanMembership = new CrpgClanMember { ClanId = 60 } };
    private static readonly CrpgUser FieryRaspberry = new() { Character = new CrpgCharacter { Name = "FieryRaspberry", Id = 1059, Rating = new CrpgCharacterRating { Value = 1385 } }, ClanMembership = new CrpgClanMember { ClanId = 50 } };
    private static readonly CrpgUser DigeratiOpisthoglyphous = new() { Character = new CrpgCharacter { Name = "DigeratiOpisthoglyphous", Id = 1060, Rating = new CrpgCharacterRating { Value = 2185 } }, ClanMembership = new CrpgClanMember { ClanId = 57 } };
    private static readonly CrpgUser CongoEelRingSnake = new() { Character = new CrpgCharacter { Name = "CongoEelRingSnake", Id = 1061, Rating = new CrpgCharacterRating { Value = 2382 } }, ClanMembership = new CrpgClanMember { ClanId = 53 } };
    private static readonly CrpgUser CountermineMopUp = new() { Character = new CrpgCharacter { Name = "CountermineMopUp", Id = 1062, Rating = new CrpgCharacterRating { Value = 2511 } }, ClanMembership = new CrpgClanMember { ClanId = 55 } };
    private static readonly CrpgUser InvadeShoot = new() { Character = new CrpgCharacter { Name = "InvadeShoot", Id = 1063, Rating = new CrpgCharacterRating { Value = 523 } }, ClanMembership = new CrpgClanMember { ClanId = 54 } };
    private static readonly CrpgUser HouseSnakePrime = new() { Character = new CrpgCharacter { Name = "HouseSnakePrime", Id = 1064, Rating = new CrpgCharacterRating { Value = 2579 } }, ClanMembership = new CrpgClanMember { ClanId = 52 } };
    private static readonly CrpgUser BurnTaupe = new() { Character = new CrpgCharacter { Name = "BurnTaupe", Id = 1065, Rating = new CrpgCharacterRating { Value = 988 } }, ClanMembership = new CrpgClanMember { ClanId = 54 } };
    private static readonly CrpgUser CourtNeophytism = new() { Character = new CrpgCharacter { Name = "CourtNeophytism", Id = 1066, Rating = new CrpgCharacterRating { Value = 2362 } }, ClanMembership = new CrpgClanMember { ClanId = 51 } };
    private static readonly CrpgUser EaterSerpentine = new() { Character = new CrpgCharacter { Name = "EaterSerpentine", Id = 1067, Rating = new CrpgCharacterRating { Value = 1872 } }, ClanMembership = new CrpgClanMember { ClanId = 55 } };
    private static readonly CrpgUser FiresideLimber = new() { Character = new CrpgCharacter { Name = "FiresideLimber", Id = 1068, Rating = new CrpgCharacterRating { Value = 2486 } }, ClanMembership = new CrpgClanMember { ClanId = 59 } };
    private static readonly CrpgUser GunslingerMole = new() { Character = new CrpgCharacter { Name = "GunslingerMole", Id = 1069, Rating = new CrpgCharacterRating { Value = 744 } }, ClanMembership = new CrpgClanMember { ClanId = 59 } };
    private static readonly CrpgUser FlameVirulence = new() { Character = new CrpgCharacter { Name = "FlameVirulence", Id = 1070, Rating = new CrpgCharacterRating { Value = 810 } }, ClanMembership = new CrpgClanMember { ClanId = 54 } };
    private static readonly CrpgUser IgneousTail = new() { Character = new CrpgCharacter { Name = "IgneousTail", Id = 1071, Rating = new CrpgCharacterRating { Value = 1142 } }, ClanMembership = new CrpgClanMember { ClanId = 53 } };
    private static readonly CrpgUser GapWalnut = new() { Character = new CrpgCharacter { Name = "GapWalnut", Id = 1072, Rating = new CrpgCharacterRating { Value = 1023 } }, ClanMembership = new CrpgClanMember { ClanId = 51 } };
    private static readonly CrpgUser BombardSullen = new() { Character = new CrpgCharacter { Name = "BombardSullen", Id = 1073, Rating = new CrpgCharacterRating { Value = 2013 } }, ClanMembership = new CrpgClanMember { ClanId = 56 } };
    private static readonly CrpgUser DaggerShooting = new() { Character = new CrpgCharacter { Name = "DaggerShooting", Id = 1074, Rating = new CrpgCharacterRating { Value = 639 } }, ClanMembership = new CrpgClanMember { ClanId = 57 } };
    private static readonly CrpgUser CimmerianPistol = new() { Character = new CrpgCharacter { Name = "CimmerianPistol", Id = 1075, Rating = new CrpgCharacterRating { Value = 1753 } }, ClanMembership = new CrpgClanMember { ClanId = 59 } };
    private static readonly CrpgUser BiteNavy = new() { Character = new CrpgCharacter { Name = "BiteNavy", Id = 1076, Rating = new CrpgCharacterRating { Value = 1845 } }, ClanMembership = new CrpgClanMember { ClanId = 52 } };
    private static readonly CrpgUser GreenieMelittin = new() { Character = new CrpgCharacter { Name = "GreenieMelittin", Id = 1077, Rating = new CrpgCharacterRating { Value = 702 } }, ClanMembership = new CrpgClanMember { ClanId = 55 } };
    private static readonly CrpgUser BlackToxin = new() { Character = new CrpgCharacter { Name = "BlackToxin", Id = 1078, Rating = new CrpgCharacterRating { Value = 2714 } }, ClanMembership = new CrpgClanMember { ClanId = 57 } };
    private static readonly CrpgUser GirdWaterMoccasin = new() { Character = new CrpgCharacter { Name = "GirdWaterMoccasin", Id = 1079, Rating = new CrpgCharacterRating { Value = 1876 } }, ClanMembership = new CrpgClanMember { ClanId = 58 } };
    private static readonly CrpgUser AirGunKingly = new() { Character = new CrpgCharacter { Name = "AirGunKingly", Id = 1080, Rating = new CrpgCharacterRating { Value = 1691 } }, ClanMembership = new CrpgClanMember { ClanId = 57 } };
    private static readonly CrpgUser FireproofSwarthy = new() { Character = new CrpgCharacter { Name = "FireproofSwarthy", Id = 1081, Rating = new CrpgCharacterRating { Value = 1043 } }, ClanMembership = new CrpgClanMember { ClanId = 60 } };
    private static readonly CrpgUser GuardSepia = new() { Character = new CrpgCharacter { Name = "GuardSepia", Id = 1082, Rating = new CrpgCharacterRating { Value = 2588 } }, ClanMembership = new CrpgClanMember { ClanId = 60 } };
    private static readonly CrpgUser FairPuttotheSword = new() { Character = new CrpgCharacter { Name = "FairPuttotheSword", Id = 1083, Rating = new CrpgCharacterRating { Value = 1486 } }, ClanMembership = new CrpgClanMember { ClanId = 53 } };
    private static readonly CrpgUser AbecedarianWaterPistol = new() { Character = new CrpgCharacter { Name = "AbecedarianWaterPistol", Id = 1084, Rating = new CrpgCharacterRating { Value = 2079 } }, ClanMembership = new CrpgClanMember { ClanId = 55 } };
    private static readonly CrpgUser EmberSwordplay = new() { Character = new CrpgCharacter { Name = "EmberSwordplay", Id = 1085, Rating = new CrpgCharacterRating { Value = 1639 } }, ClanMembership = new CrpgClanMember { ClanId = 55 } };
    private static readonly CrpgUser DuskyScabbard = new() { Character = new CrpgCharacter { Name = "DuskyScabbard", Id = 1086, Rating = new CrpgCharacterRating { Value = 2837 } }, ClanMembership = new CrpgClanMember { ClanId = 55 } };
    private static readonly CrpgUser CadetShed = new() { Character = new CrpgCharacter { Name = "CadetShed", Id = 1087, Rating = new CrpgCharacterRating { Value = 1522 } }, ClanMembership = new CrpgClanMember { ClanId = 55 } };
    private static readonly CrpgUser BalefireWorm = new() { Character = new CrpgCharacter { Name = "BalefireWorm", Id = 1088, Rating = new CrpgCharacterRating { Value = 2132 } }, ClanMembership = new CrpgClanMember { ClanId = 59 } };
    private static readonly CrpgUser EngageSansevieria = new() { Character = new CrpgCharacter { Name = "EngageSansevieria", Id = 1089, Rating = new CrpgCharacterRating { Value = 1001 } }, ClanMembership = new CrpgClanMember { ClanId = 51 } };
    private static readonly CrpgUser BrownstoneQuisling = new() { Character = new CrpgCharacter { Name = "BrownstoneQuisling", Id = 1090, Rating = new CrpgCharacterRating { Value = 2385 } }, ClanMembership = new CrpgClanMember { ClanId = 56 } };
    private static readonly CrpgUser AlexitericalTaipan = new() { Character = new CrpgCharacter { Name = "AlexitericalTaipan", Id = 1091, Rating = new CrpgCharacterRating { Value = 720 } }, ClanMembership = new CrpgClanMember { ClanId = 58 } };
    private static readonly CrpgUser BladeShotgun = new() { Character = new CrpgCharacter { Name = "BladeShotgun", Id = 1092, Rating = new CrpgCharacterRating { Value = 2797 } }, ClanMembership = new CrpgClanMember { ClanId = 51 } };
    private static readonly CrpgUser AntiaircraftPunk = new() { Character = new CrpgCharacter { Name = "AntiaircraftPunk", Id = 1093, Rating = new CrpgCharacterRating { Value = 2236 } }, ClanMembership = new CrpgClanMember { ClanId = 55 } };
    private static readonly CrpgUser GunfireListeningPost = new() { Character = new CrpgCharacter { Name = "GunfireListeningPost", Id = 1094, Rating = new CrpgCharacterRating { Value = 2646 } }, ClanMembership = new CrpgClanMember { ClanId = 56 } };
    private static readonly CrpgUser BuckFeverScowl = new() { Character = new CrpgCharacter { Name = "BuckFeverScowl", Id = 1095, Rating = new CrpgCharacterRating { Value = 2252 } }, ClanMembership = new CrpgClanMember { ClanId = 56 } };
    private static readonly CrpgUser ChaseProteroglypha = new() { Character = new CrpgCharacter { Name = "ChaseProteroglypha", Id = 1096, Rating = new CrpgCharacterRating { Value = 1069 } }, ClanMembership = new CrpgClanMember { ClanId = 56 } };
    private static readonly CrpgUser FoeYataghan = new() { Character = new CrpgCharacter { Name = "FoeYataghan", Id = 1097, Rating = new CrpgCharacterRating { Value = 612 } }, ClanMembership = new CrpgClanMember { ClanId = 56 } };
    private static readonly CrpgUser BrunetteWadding = new() { Character = new CrpgCharacter { Name = "BrunetteWadding", Id = 1098, Rating = new CrpgCharacterRating { Value = 1019 } }, ClanMembership = new CrpgClanMember { ClanId = 52 } };
    private static readonly CrpgUser BoomslangYounker = new() { Character = new CrpgCharacter { Name = "BoomslangYounker", Id = 1099, Rating = new CrpgCharacterRating { Value = 1740 } }, ClanMembership = new CrpgClanMember { ClanId = 55 } };
    private static readonly CrpgUser BoaScout = new() { Character = new CrpgCharacter { Name = "BoaScout", Id = 1100, Rating = new CrpgCharacterRating { Value = 1069 } }, ClanMembership = new CrpgClanMember { ClanId = 51 } };
    private static readonly CrpgUser AlphabetarianSerum = new() { Character = new CrpgCharacter { Name = "AlphabetarianSerum", Id = 1101, Rating = new CrpgCharacterRating { Value = 837 } }, ClanMembership = new CrpgClanMember { ClanId = 52 } };
    private static readonly CrpgUser EmpoisonSnake = new() { Character = new CrpgCharacter { Name = "EmpoisonSnake", Id = 1102, Rating = new CrpgCharacterRating { Value = 1721 } }, ClanMembership = new CrpgClanMember { ClanId = 57 } };
    private static readonly CrpgUser InflammablePuffAdder = new() { Character = new CrpgCharacter { Name = "InflammablePuffAdder", Id = 1103, Rating = new CrpgCharacterRating { Value = 1292 } }, ClanMembership = new CrpgClanMember { ClanId = 50 } };
    private static readonly CrpgUser BullfightTiro = new() { Character = new CrpgCharacter { Name = "BullfightTiro", Id = 1104, Rating = new CrpgCharacterRating { Value = 579 } }, ClanMembership = new CrpgClanMember { ClanId = 60 } };
    private static readonly CrpgUser FirearmKeelback = new() { Character = new CrpgCharacter { Name = "FirearmKeelback", Id = 1105, Rating = new CrpgCharacterRating { Value = 2183 } }, ClanMembership = new CrpgClanMember { ClanId = 56 } };
    private static readonly CrpgUser FiringPumpernickel = new() { Character = new CrpgCharacter { Name = "FiringPumpernickel", Id = 1106, Rating = new CrpgCharacterRating { Value = 1124 } }, ClanMembership = new CrpgClanMember { ClanId = 58 } };
    private static readonly CrpgUser InimicalVennation = new() { Character = new CrpgCharacter { Name = "InimicalVennation", Id = 1107, Rating = new CrpgCharacterRating { Value = 2878 } }, ClanMembership = new CrpgClanMember { ClanId = 53 } };
    private static readonly CrpgUser ConeShellPiece = new() { Character = new CrpgCharacter { Name = "ConeShellPiece", Id = 1108, Rating = new CrpgCharacterRating { Value = 1220 } }, ClanMembership = new CrpgClanMember { ClanId = 50 } };
    private static readonly CrpgUser InitiateMarsala = new() { Character = new CrpgCharacter { Name = "InitiateMarsala", Id = 1109, Rating = new CrpgCharacterRating { Value = 2767 } }, ClanMembership = new CrpgClanMember { ClanId = 59 } };
    private static readonly CrpgUser BulletRacer = new() { Character = new CrpgCharacter { Name = "BulletRacer", Id = 1110, Rating = new CrpgCharacterRating { Value = 2957 } }, ClanMembership = new CrpgClanMember { ClanId = 60 } };
    private static readonly CrpgUser EggplantRifle = new() { Character = new CrpgCharacter { Name = "EggplantRifle", Id = 1111, Rating = new CrpgCharacterRating { Value = 930 } }, ClanMembership = new CrpgClanMember { ClanId = 51 } };
    private static readonly CrpgUser EbonyQueen = new() { Character = new CrpgCharacter { Name = "EbonyQueen", Id = 1112, Rating = new CrpgCharacterRating { Value = 1050 } }, ClanMembership = new CrpgClanMember { ClanId = 52 } };
    private static readonly CrpgUser InflameMorglay = new() { Character = new CrpgCharacter { Name = "InflameMorglay", Id = 1113, Rating = new CrpgCharacterRating { Value = 1846 } }, ClanMembership = new CrpgClanMember { ClanId = 53 } };
    private static readonly CrpgUser ComeUnlimber = new() { Character = new CrpgCharacter { Name = "ComeUnlimber", Id = 1114, Rating = new CrpgCharacterRating { Value = 1467 } }, ClanMembership = new CrpgClanMember { ClanId = 54 } };
    private static readonly CrpgUser FighterRange = new() { Character = new CrpgCharacter { Name = "FighterRange", Id = 1115, Rating = new CrpgCharacterRating { Value = 1061 } }, ClanMembership = new CrpgClanMember { ClanId = 53 } };
    private static readonly CrpgUser CottonmouthOxblood = new() { Character = new CrpgCharacter { Name = "CottonmouthOxblood", Id = 1116, Rating = new CrpgCharacterRating { Value = 2781 } }, ClanMembership = new CrpgClanMember { ClanId = 55 } };
    private static readonly CrpgUser FifthColumnParry = new() { Character = new CrpgCharacter { Name = "FifthColumnParry", Id = 1117, Rating = new CrpgCharacterRating { Value = 2384 } }, ClanMembership = new CrpgClanMember { ClanId = 51 } };
    private static readonly CrpgUser CarbuncleParley = new() { Character = new CrpgCharacter { Name = "CarbuncleParley", Id = 1118, Rating = new CrpgCharacterRating { Value = 1220 } }, ClanMembership = new CrpgClanMember { ClanId = 56 } };
    private static readonly CrpgUser FoibleUnfriend = new() { Character = new CrpgCharacter { Name = "FoibleUnfriend", Id = 1119, Rating = new CrpgCharacterRating { Value = 1287 } }, ClanMembership = new CrpgClanMember { ClanId = 57 } };
    private static readonly CrpgUser DamascusSteelProfession = new() { Character = new CrpgCharacter { Name = "DamascusSteelProfession", Id = 1120, Rating = new CrpgCharacterRating { Value = 1895 } }, ClanMembership = new CrpgClanMember { ClanId = 57 } };
    private static readonly CrpgUser AntimissileSap = new() { Character = new CrpgCharacter { Name = "AntimissileSap", Id = 1121, Rating = new CrpgCharacterRating { Value = 1022 } }, ClanMembership = new CrpgClanMember { ClanId = 50 } };
    private static readonly CrpgUser FloretTityus = new() { Character = new CrpgCharacter { Name = "FloretTityus", Id = 1122, Rating = new CrpgCharacterRating { Value = 1596 } }, ClanMembership = new CrpgClanMember { ClanId = 54 } };
    private static readonly CrpgUser CoachwhipRapier = new() { Character = new CrpgCharacter { Name = "CoachwhipRapier", Id = 1123, Rating = new CrpgCharacterRating { Value = 1102 } }, ClanMembership = new CrpgClanMember { ClanId = 50 } };
    private static readonly CrpgUser BootySubmachineGun = new() { Character = new CrpgCharacter { Name = "BootySubmachineGun", Id = 1124, Rating = new CrpgCharacterRating { Value = 2262 } }, ClanMembership = new CrpgClanMember { ClanId = 52 } };
    private static readonly CrpgUser DamoclesProteroglyphous = new() { Character = new CrpgCharacter { Name = "DamoclesProteroglyphous", Id = 1125, Rating = new CrpgCharacterRating { Value = 2610 } }, ClanMembership = new CrpgClanMember { ClanId = 56 } };
    private static readonly CrpgUser CannonadeStrip = new() { Character = new CrpgCharacter { Name = "CannonadeStrip", Id = 1126, Rating = new CrpgCharacterRating { Value = 1511 } }, ClanMembership = new CrpgClanMember { ClanId = 50 } };
    private static readonly CrpgUser FlammableWildfire = new() { Character = new CrpgCharacter { Name = "FlammableWildfire", Id = 1127, Rating = new CrpgCharacterRating { Value = 2633 } }, ClanMembership = new CrpgClanMember { ClanId = 50 } };
    private static readonly CrpgUser AlexipharmicJohnny = new() { Character = new CrpgCharacter { Name = "AlexipharmicJohnny", Id = 1128, Rating = new CrpgCharacterRating { Value = 2358 } }, ClanMembership = new CrpgClanMember { ClanId = 59 } };
    private static readonly CrpgUser DischargeProteroglyph = new() { Character = new CrpgCharacter { Name = "DischargeProteroglyph", Id = 1129, Rating = new CrpgCharacterRating { Value = 2145 } }, ClanMembership = new CrpgClanMember { ClanId = 54 } };
    private static readonly CrpgUser InfiltrateKindling = new() { Character = new CrpgCharacter { Name = "InfiltrateKindling", Id = 1130, Rating = new CrpgCharacterRating { Value = 1323 } }, ClanMembership = new CrpgClanMember { ClanId = 54 } };
    private static readonly CrpgUser BilboRhasophore = new() { Character = new CrpgCharacter { Name = "BilboRhasophore", Id = 1131, Rating = new CrpgCharacterRating { Value = 984 } }, ClanMembership = new CrpgClanMember { ClanId = 60 } };
    private static readonly CrpgUser ChamberOutvenom = new() { Character = new CrpgCharacter { Name = "ChamberOutvenom", Id = 1132, Rating = new CrpgCharacterRating { Value = 892 } }, ClanMembership = new CrpgClanMember { ClanId = 56 } };
    private static readonly CrpgUser GunmanSlash = new() { Character = new CrpgCharacter { Name = "GunmanSlash", Id = 1133, Rating = new CrpgCharacterRating { Value = 678 } }, ClanMembership = new CrpgClanMember { ClanId = 53 } };
    private static readonly CrpgUser AblazeRayGun = new() { Character = new CrpgCharacter { Name = "AblazeRayGun", Id = 1134, Rating = new CrpgCharacterRating { Value = 540 } }, ClanMembership = new CrpgClanMember { ClanId = 60 } };
    private static readonly CrpgUser ContagionMalihini = new() { Character = new CrpgCharacter { Name = "ContagionMalihini", Id = 1135, Rating = new CrpgCharacterRating { Value = 1520 } }, ClanMembership = new CrpgClanMember { ClanId = 52 } };
    private static readonly CrpgUser FangNavyBlue = new() { Character = new CrpgCharacter { Name = "FangNavyBlue", Id = 1136, Rating = new CrpgCharacterRating { Value = 833 } }, ClanMembership = new CrpgClanMember { ClanId = 56 } };
    private static readonly CrpgUser ChocolateSombre = new() { Character = new CrpgCharacter { Name = "ChocolateSombre", Id = 1137, Rating = new CrpgCharacterRating { Value = 2840 } }, ClanMembership = new CrpgClanMember { ClanId = 52 } };
    private static readonly CrpgUser EnvenomationSheathe = new() { Character = new CrpgCharacter { Name = "EnvenomationSheathe", Id = 1138, Rating = new CrpgCharacterRating { Value = 2312 } }, ClanMembership = new CrpgClanMember { ClanId = 58 } };
    private static readonly CrpgUser AflameReign = new() { Character = new CrpgCharacter { Name = "AflameReign", Id = 1139, Rating = new CrpgCharacterRating { Value = 2654 } }, ClanMembership = new CrpgClanMember { ClanId = 60 } };
    private static readonly CrpgUser AglyphTang = new() { Character = new CrpgCharacter { Name = "AglyphTang", Id = 1140, Rating = new CrpgCharacterRating { Value = 1677 } }, ClanMembership = new CrpgClanMember { ClanId = 56 } };
    private static readonly CrpgUser AlexitericMachineGun = new() { Character = new CrpgCharacter { Name = "AlexitericMachineGun", Id = 1141, Rating = new CrpgCharacterRating { Value = 826 } }, ClanMembership = new CrpgClanMember { ClanId = 50 } };
    private static readonly CrpgUser ForteTheriac = new() { Character = new CrpgCharacter { Name = "ForteTheriac", Id = 1142, Rating = new CrpgCharacterRating { Value = 706 } }, ClanMembership = new CrpgClanMember { ClanId = 57 } };
    private static readonly CrpgUser FlagofTruceNaked = new() { Character = new CrpgCharacter { Name = "FlagofTruceNaked", Id = 1143, Rating = new CrpgCharacterRating { Value = 1609 } }, ClanMembership = new CrpgClanMember { ClanId = 50 } };
    private static readonly CrpgUser HydraRough = new() { Character = new CrpgCharacter { Name = "HydraRough", Id = 1144, Rating = new CrpgCharacterRating { Value = 2991 } }, ClanMembership = new CrpgClanMember { ClanId = 51 } };
    private static readonly CrpgUser BaldricOphi = new() { Character = new CrpgCharacter { Name = "BaldricOphi", Id = 1145, Rating = new CrpgCharacterRating { Value = 609 } }, ClanMembership = new CrpgClanMember { ClanId = 54 } };
    private static readonly CrpgUser HangerMapepire = new() { Character = new CrpgCharacter { Name = "HangerMapepire", Id = 1146, Rating = new CrpgCharacterRating { Value = 1869 } }, ClanMembership = new CrpgClanMember { ClanId = 51 } };
    private static readonly CrpgUser BlankSpittingSnake = new() { Character = new CrpgCharacter { Name = "BlankSpittingSnake", Id = 1147, Rating = new CrpgCharacterRating { Value = 2391 } }, ClanMembership = new CrpgClanMember { ClanId = 54 } };
    private static readonly CrpgUser CounteroffensiveShutterbug = new() { Character = new CrpgCharacter { Name = "CounteroffensiveShutterbug", Id = 1148, Rating = new CrpgCharacterRating { Value = 637 } }, ClanMembership = new CrpgClanMember { ClanId = 56 } };
    private static readonly CrpgUser GlaiveRuby = new() { Character = new CrpgCharacter { Name = "GlaiveRuby", Id = 1149, Rating = new CrpgCharacterRating { Value = 1795 } }, ClanMembership = new CrpgClanMember { ClanId = 50 } };
    private static readonly CrpgUser EelTenderfoot = new() { Character = new CrpgCharacter { Name = "EelTenderfoot", Id = 1150, Rating = new CrpgCharacterRating { Value = 2384 } }, ClanMembership = new CrpgClanMember { ClanId = 58 } };
    private static readonly CrpgUser CoffeeSalamander = new() { Character = new CrpgCharacter { Name = "CoffeeSalamander", Id = 1151, Rating = new CrpgCharacterRating { Value = 1604 } }, ClanMembership = new CrpgClanMember { ClanId = 55 } };
    private static readonly CrpgUser CastleShadow = new() { Character = new CrpgCharacter { Name = "CastleShadow", Id = 1152, Rating = new CrpgCharacterRating { Value = 1230 } }, ClanMembership = new CrpgClanMember { ClanId = 52 } };
    private static readonly CrpgUser AnguineMaroon = new() { Character = new CrpgCharacter { Name = "AnguineMaroon", Id = 1153, Rating = new CrpgCharacterRating { Value = 2287 } }, ClanMembership = new CrpgClanMember { ClanId = 54 } };
    private static readonly CrpgUser GopherLubber = new() { Character = new CrpgCharacter { Name = "GopherLubber", Id = 1154, Rating = new CrpgCharacterRating { Value = 2166 } }, ClanMembership = new CrpgClanMember { ClanId = 52 } };
    private static readonly CrpgUser FrontfangedScalp = new() { Character = new CrpgCharacter { Name = "FrontfangedScalp", Id = 1155, Rating = new CrpgCharacterRating { Value = 1969 } }, ClanMembership = new CrpgClanMember { ClanId = 53 } };
    private static readonly CrpgUser FrontXiphoid = new() { Character = new CrpgCharacter { Name = "FrontXiphoid", Id = 1156, Rating = new CrpgCharacterRating { Value = 1973 } }, ClanMembership = new CrpgClanMember { ClanId = 55 } };
    private static readonly CrpgUser BurntRinghals = new() { Character = new CrpgCharacter { Name = "BurntRinghals", Id = 1157, Rating = new CrpgCharacterRating { Value = 1243 } }, ClanMembership = new CrpgClanMember { ClanId = 59 } };
    private static readonly CrpgUser FireTruckRegal = new() { Character = new CrpgCharacter { Name = "FireTruckRegal", Id = 1158, Rating = new CrpgCharacterRating { Value = 1518 } }, ClanMembership = new CrpgClanMember { ClanId = 55 } };
    private static readonly CrpgUser ArchenemySidearm = new() { Character = new CrpgCharacter { Name = "ArchenemySidearm", Id = 1159, Rating = new CrpgCharacterRating { Value = 599 } }, ClanMembership = new CrpgClanMember { ClanId = 54 } };
    private static readonly CrpgUser CarryLandlubber = new() { Character = new CrpgCharacter { Name = "CarryLandlubber", Id = 1160, Rating = new CrpgCharacterRating { Value = 2970 } }, ClanMembership = new CrpgClanMember { ClanId = 58 } };
    private static readonly CrpgUser BlacksnakeToledo = new() { Character = new CrpgCharacter { Name = "BlacksnakeToledo", Id = 1161, Rating = new CrpgCharacterRating { Value = 1690 } }, ClanMembership = new CrpgClanMember { ClanId = 54 } };
    private static readonly CrpgUser ExcaliburPyrolatry = new() { Character = new CrpgCharacter { Name = "ExcaliburPyrolatry", Id = 1162, Rating = new CrpgCharacterRating { Value = 1279 } }, ClanMembership = new CrpgClanMember { ClanId = 58 } };
    private static readonly CrpgUser CounterintelligenceKinglet = new() { Character = new CrpgCharacter { Name = "CounterintelligenceKinglet", Id = 1163, Rating = new CrpgCharacterRating { Value = 2365 } }, ClanMembership = new CrpgClanMember { ClanId = 51 } };
    private static readonly CrpgUser IceMiss = new() { Character = new CrpgCharacter { Name = "IceMiss", Id = 1164, Rating = new CrpgCharacterRating { Value = 1283 } }, ClanMembership = new CrpgClanMember { ClanId = 50 } };
    private static readonly CrpgUser BearerPitch = new() { Character = new CrpgCharacter { Name = "BearerPitch", Id = 1165, Rating = new CrpgCharacterRating { Value = 896 } }, ClanMembership = new CrpgClanMember { ClanId = 53 } };
    private static readonly CrpgUser BackswordSerpent = new() { Character = new CrpgCharacter { Name = "BackswordSerpent", Id = 1166, Rating = new CrpgCharacterRating { Value = 1537 } }, ClanMembership = new CrpgClanMember { ClanId = 57 } };
    private static readonly CrpgUser HornedViperMusket = new() { Character = new CrpgCharacter { Name = "HornedViperMusket", Id = 1167, Rating = new CrpgCharacterRating { Value = 2288 } }, ClanMembership = new CrpgClanMember { ClanId = 55 } };
    private static readonly CrpgUser FoxholePummel = new() { Character = new CrpgCharacter { Name = "FoxholePummel", Id = 1168, Rating = new CrpgCharacterRating { Value = 887 } }, ClanMembership = new CrpgClanMember { ClanId = 60 } };
    private static readonly CrpgUser DunRamrod = new() { Character = new CrpgCharacter { Name = "DunRamrod", Id = 1169, Rating = new CrpgCharacterRating { Value = 1296 } }, ClanMembership = new CrpgClanMember { ClanId = 51 } };
    private static readonly CrpgUser ClipNeophyte = new() { Character = new CrpgCharacter { Name = "ClipNeophyte", Id = 1170, Rating = new CrpgCharacterRating { Value = 1907 } }, ClanMembership = new CrpgClanMember { ClanId = 60 } };
    private static readonly CrpgUser InternshipPilot = new() { Character = new CrpgCharacter { Name = "InternshipPilot", Id = 1171, Rating = new CrpgCharacterRating { Value = 1423 } }, ClanMembership = new CrpgClanMember { ClanId = 50 } };
    private static readonly CrpgUser FoxSnakeMocha = new() { Character = new CrpgCharacter { Name = "FoxSnakeMocha", Id = 1172, Rating = new CrpgCharacterRating { Value = 1588 } }, ClanMembership = new CrpgClanMember { ClanId = 51 } };
    private static readonly CrpgUser BungarotoxinSnakeskin = new() { Character = new CrpgCharacter { Name = "BungarotoxinSnakeskin", Id = 1173, Rating = new CrpgCharacterRating { Value = 2260 } }, ClanMembership = new CrpgClanMember { ClanId = 51 } };
    private static readonly CrpgUser FloatTrail = new() { Character = new CrpgCharacter { Name = "FloatTrail", Id = 1174, Rating = new CrpgCharacterRating { Value = 1478 } }, ClanMembership = new CrpgClanMember { ClanId = 58 } };
    private static readonly CrpgUser FalchionPoker = new() { Character = new CrpgCharacter { Name = "FalchionPoker", Id = 1175, Rating = new CrpgCharacterRating { Value = 2138 } }, ClanMembership = new CrpgClanMember { ClanId = 51 } };
    private static readonly CrpgUser BbGunScute = new() { Character = new CrpgCharacter { Name = "BbGunScute", Id = 1176, Rating = new CrpgCharacterRating { Value = 2266 } }, ClanMembership = new CrpgClanMember { ClanId = 54 } };
    private static readonly CrpgUser HognosedViper = new() { Character = new CrpgCharacter { Name = "HognosedViper", Id = 1177, Rating = new CrpgCharacterRating { Value = 2242 } }, ClanMembership = new CrpgClanMember { ClanId = 60 } };
    private static readonly CrpgUser ThompsonSubmachineGun = new() { Character = new CrpgCharacter { Name = "ThompsonSubmachineGun", Id = 1178, Rating = new CrpgCharacterRating { Value = 1534 } }, ClanMembership = new CrpgClanMember { ClanId = 52 } };
    private static readonly CrpgUser FoemanRegicide = new() { Character = new CrpgCharacter { Name = "FoemanRegicide", Id = 1179, Rating = new CrpgCharacterRating { Value = 1104 } }, ClanMembership = new CrpgClanMember { ClanId = 57 } };
    private static readonly CrpgUser AdversaryStoke = new() { Character = new CrpgCharacter { Name = "AdversaryStoke", Id = 1180, Rating = new CrpgCharacterRating { Value = 2027 } }, ClanMembership = new CrpgClanMember { ClanId = 60 } };
    private static readonly CrpgUser EnsiformOpisthoglyph = new() { Character = new CrpgCharacter { Name = "EnsiformOpisthoglyph", Id = 1181, Rating = new CrpgCharacterRating { Value = 1273 } }, ClanMembership = new CrpgClanMember { ClanId = 54 } };
    private static readonly CrpgUser FoxReptile = new() { Character = new CrpgCharacter { Name = "FoxReptile", Id = 1182, Rating = new CrpgCharacterRating { Value = 574 } }, ClanMembership = new CrpgClanMember { ClanId = 56 } };
    private static readonly CrpgUser BottleGreenVictory = new() { Character = new CrpgCharacter { Name = "BottleGreenVictory", Id = 1183, Rating = new CrpgCharacterRating { Value = 1149 } }, ClanMembership = new CrpgClanMember { ClanId = 51 } };
    private static readonly CrpgUser GreenhornTwist = new() { Character = new CrpgCharacter { Name = "GreenhornTwist", Id = 1184, Rating = new CrpgCharacterRating { Value = 1278 } }, ClanMembership = new CrpgClanMember { ClanId = 50 } };
    private static readonly CrpgUser BaselardScimitar = new() { Character = new CrpgCharacter { Name = "BaselardScimitar", Id = 1185, Rating = new CrpgCharacterRating { Value = 2868 } }, ClanMembership = new CrpgClanMember { ClanId = 56 } };
    private static readonly CrpgUser CobraLunge = new() { Character = new CrpgCharacter { Name = "CobraLunge", Id = 1186, Rating = new CrpgCharacterRating { Value = 2748 } }, ClanMembership = new CrpgClanMember { ClanId = 51 } };
    private static readonly CrpgUser AubergineSurly = new() { Character = new CrpgCharacter { Name = "AubergineSurly", Id = 1187, Rating = new CrpgCharacterRating { Value = 1283 } }, ClanMembership = new CrpgClanMember { ClanId = 57 } };
    private static readonly CrpgUser FirelessUnfledged = new() { Character = new CrpgCharacter { Name = "FirelessUnfledged", Id = 1188, Rating = new CrpgCharacterRating { Value = 1141 } }, ClanMembership = new CrpgClanMember { ClanId = 59 } };
    private static readonly CrpgUser CurtanaRoyalty = new() { Character = new CrpgCharacter { Name = "CurtanaRoyalty", Id = 1189, Rating = new CrpgCharacterRating { Value = 2297 } }, ClanMembership = new CrpgClanMember { ClanId = 51 } };
    private static readonly CrpgUser FerSally = new() { Character = new CrpgCharacter { Name = "FerSally", Id = 1190, Rating = new CrpgCharacterRating { Value = 1408 } }, ClanMembership = new CrpgClanMember { ClanId = 55 } };
    private static readonly CrpgUser GarterSnakeLately = new() { Character = new CrpgCharacter { Name = "GarterSnakeLately", Id = 1191, Rating = new CrpgCharacterRating { Value = 816 } }, ClanMembership = new CrpgClanMember { ClanId = 52 } };
    private static readonly CrpgUser CalibrateJillaroo = new() { Character = new CrpgCharacter { Name = "CalibrateJillaroo", Id = 1192, Rating = new CrpgCharacterRating { Value = 1800 } }, ClanMembership = new CrpgClanMember { ClanId = 51 } };
    private static readonly CrpgUser CollaborateLance = new() { Character = new CrpgCharacter { Name = "CollaborateLance", Id = 1193, Rating = new CrpgCharacterRating { Value = 1634 } }, ClanMembership = new CrpgClanMember { ClanId = 51 } };
    private static readonly CrpgUser ArrowrootOphidian = new() { Character = new CrpgCharacter { Name = "ArrowrootOphidian", Id = 1194, Rating = new CrpgCharacterRating { Value = 2924 } }, ClanMembership = new CrpgClanMember { ClanId = 57 } };
    private static readonly CrpgUser HamadryadTarantula = new() { Character = new CrpgCharacter { Name = "HamadryadTarantula", Id = 1195, Rating = new CrpgCharacterRating { Value = 1455 } }, ClanMembership = new CrpgClanMember { ClanId = 50 } };
    private static readonly CrpgUser AdderMisfire = new() { Character = new CrpgCharacter { Name = "AdderMisfire", Id = 1196, Rating = new CrpgCharacterRating { Value = 2734 } }, ClanMembership = new CrpgClanMember { ClanId = 55 } };
    private static readonly CrpgUser IrisTsuba = new() { Character = new CrpgCharacter { Name = "IrisTsuba", Id = 1197, Rating = new CrpgCharacterRating { Value = 2552 } }, ClanMembership = new CrpgClanMember { ClanId = 51 } };
    private static readonly CrpgUser AirgunStonefish = new() { Character = new CrpgCharacter { Name = "AirgunStonefish", Id = 1198, Rating = new CrpgCharacterRating { Value = 2460 } }, ClanMembership = new CrpgClanMember { ClanId = 53 } };
    private static readonly CrpgUser HepaticMustard = new() { Character = new CrpgCharacter { Name = "HepaticMustard", Id = 1199, Rating = new CrpgCharacterRating { Value = 2104 } }, ClanMembership = new CrpgClanMember { ClanId = 53 } };
    private static readonly CrpgUser CombatPrefire = new() { Character = new CrpgCharacter { Name = "CombatPrefire", Id = 1200, Rating = new CrpgCharacterRating { Value = 1030 } } };
    private static readonly CrpgUser HolsterSwordsmanship = new() { Character = new CrpgCharacter { Name = "HolsterSwordsmanship", Id = 1201, Rating = new CrpgCharacterRating { Value = 1576 } } };
    private static readonly CrpgUser EscolarSpittingCobra = new() { Character = new CrpgCharacter { Name = "EscolarSpittingCobra", Id = 1202, Rating = new CrpgCharacterRating { Value = 2246 } } };
    private static readonly CrpgUser FiretrapMelano = new() { Character = new CrpgCharacter { Name = "FiretrapMelano", Id = 1203, Rating = new CrpgCharacterRating { Value = 2741 } } };
    private static readonly CrpgUser CheckVinous = new() { Character = new CrpgCharacter { Name = "CheckVinous", Id = 1204, Rating = new CrpgCharacterRating { Value = 752 } } };
    private static readonly CrpgUser BeachheadLeaden = new() { Character = new CrpgCharacter { Name = "BeachheadLeaden", Id = 1205, Rating = new CrpgCharacterRating { Value = 1594 } } };
    private static readonly CrpgUser ComputerPhobiaNightAdder = new() { Character = new CrpgCharacter { Name = "ComputerPhobiaNightAdder", Id = 1206, Rating = new CrpgCharacterRating { Value = 690 } } };
    private static readonly CrpgUser BothropsMusketry = new() { Character = new CrpgCharacter { Name = "BothropsMusketry", Id = 1207, Rating = new CrpgCharacterRating { Value = 2419 } } };
    private static readonly CrpgUser AntagonistLodgment = new() { Character = new CrpgCharacter { Name = "AntagonistLodgment", Id = 1208, Rating = new CrpgCharacterRating { Value = 1900 } } };
    private static readonly CrpgUser CorposantWhinyard = new() { Character = new CrpgCharacter { Name = "CorposantWhinyard", Id = 1209, Rating = new CrpgCharacterRating { Value = 1707 } } };
    private static readonly CrpgUser BlackoutMurk = new() { Character = new CrpgCharacter { Name = "BlackoutMurk", Id = 1210, Rating = new CrpgCharacterRating { Value = 2113 } } };
    private static readonly CrpgUser ChassisPrivateer = new() { Character = new CrpgCharacter { Name = "ChassisPrivateer", Id = 1211, Rating = new CrpgCharacterRating { Value = 2613 } } };
    private static readonly CrpgUser DeadlySheath = new() { Character = new CrpgCharacter { Name = "DeadlySheath", Id = 1212, Rating = new CrpgCharacterRating { Value = 2170 } } };
    private static readonly CrpgUser FightSight = new() { Character = new CrpgCharacter { Name = "FightSight", Id = 1213, Rating = new CrpgCharacterRating { Value = 1646 } } };
    private static readonly CrpgUser FirehousePuny = new() { Character = new CrpgCharacter { Name = "FirehousePuny", Id = 1214, Rating = new CrpgCharacterRating { Value = 1198 } } };
    private static readonly CrpgUser BlindSnakeUnsheathe = new() { Character = new CrpgCharacter { Name = "BlindSnakeUnsheathe", Id = 1215, Rating = new CrpgCharacterRating { Value = 2332 } } };
    private static readonly CrpgUser DeMachine = new() { Character = new CrpgCharacter { Name = "DeMachine", Id = 1216, Rating = new CrpgCharacterRating { Value = 913 } } };
    private static readonly CrpgUser FoilRecoil = new() { Character = new CrpgCharacter { Name = "FoilRecoil", Id = 1217, Rating = new CrpgCharacterRating { Value = 1480 } } };
    private static readonly CrpgUser EnvenomateMatachin = new() { Character = new CrpgCharacter { Name = "EnvenomateMatachin", Id = 1218, Rating = new CrpgCharacterRating { Value = 632 } } };
    private static readonly CrpgUser CannonryStoker = new() { Character = new CrpgCharacter { Name = "CannonryStoker", Id = 1219, Rating = new CrpgCharacterRating { Value = 1146 } } };
    private static readonly CrpgUser CarpetSnakeSaber = new() { Character = new CrpgCharacter { Name = "CarpetSnakeSaber", Id = 1220, Rating = new CrpgCharacterRating { Value = 1166 } } };
    private static readonly CrpgUser DubMudSnake = new() { Character = new CrpgCharacter { Name = "DubMudSnake", Id = 1221, Rating = new CrpgCharacterRating { Value = 2726 } } };
    private static readonly CrpgUser ChelaOverkill = new() { Character = new CrpgCharacter { Name = "ChelaOverkill", Id = 1222, Rating = new CrpgCharacterRating { Value = 2915 } } };
    private static readonly CrpgUser FireplugNoviceship = new() { Character = new CrpgCharacter { Name = "FireplugNoviceship", Id = 1223, Rating = new CrpgCharacterRating { Value = 702 } } };
    private static readonly CrpgUser CanVirus = new() { Character = new CrpgCharacter { Name = "CanVirus", Id = 1224, Rating = new CrpgCharacterRating { Value = 2865 } } };
    private static readonly CrpgUser BuckwheaterVenin = new() { Character = new CrpgCharacter { Name = "BuckwheaterVenin", Id = 1225, Rating = new CrpgCharacterRating { Value = 1908 } } };
    private static readonly CrpgUser AceSwordless = new() { Character = new CrpgCharacter { Name = "AceSwordless", Id = 1226, Rating = new CrpgCharacterRating { Value = 919 } } };
    private static readonly CrpgUser AllongePartisan = new() { Character = new CrpgCharacter { Name = "AllongePartisan", Id = 1227, Rating = new CrpgCharacterRating { Value = 2804 } } };
    private static readonly CrpgUser CampfireNewChum = new() { Character = new CrpgCharacter { Name = "CampfireNewChum", Id = 1228, Rating = new CrpgCharacterRating { Value = 826 } } };
    private static readonly CrpgUser CrotoxinMulberry = new() { Character = new CrpgCharacter { Name = "CrotoxinMulberry", Id = 1229, Rating = new CrpgCharacterRating { Value = 1273 } } };
    private static readonly CrpgUser DerisionStygian = new() { Character = new CrpgCharacter { Name = "DerisionStygian", Id = 1230, Rating = new CrpgCharacterRating { Value = 1008 } } };
    private static readonly CrpgUser DarklingTyro = new() { Character = new CrpgCharacter { Name = "DarklingTyro", Id = 1231, Rating = new CrpgCharacterRating { Value = 1130 } } };
    private static readonly CrpgUser GrassSnakeRekindle = new() { Character = new CrpgCharacter { Name = "GrassSnakeRekindle", Id = 1232, Rating = new CrpgCharacterRating { Value = 1275 } } };
    private static readonly CrpgUser AntagonizePitchy = new() { Character = new CrpgCharacter { Name = "AntagonizePitchy", Id = 1233, Rating = new CrpgCharacterRating { Value = 2149 } } };
    private static readonly CrpgUser EmplacementOpisthoglypha = new() { Character = new CrpgCharacter { Name = "EmplacementOpisthoglypha", Id = 1234, Rating = new CrpgCharacterRating { Value = 2782 } } };
    private static readonly CrpgUser GunshotSomber = new() { Character = new CrpgCharacter { Name = "GunshotSomber", Id = 1235, Rating = new CrpgCharacterRating { Value = 1052 } } };
    private static readonly CrpgUser BrandSequester = new() { Character = new CrpgCharacter { Name = "BrandSequester", Id = 1236, Rating = new CrpgCharacterRating { Value = 1556 } } };
    private static readonly CrpgUser ConflagrationPlat = new() { Character = new CrpgCharacter { Name = "ConflagrationPlat", Id = 1237, Rating = new CrpgCharacterRating { Value = 503 } } };
    private static readonly CrpgUser GunnerPitchdark = new() { Character = new CrpgCharacter { Name = "GunnerPitchdark", Id = 1238, Rating = new CrpgCharacterRating { Value = 1514 } } };
    private static readonly CrpgUser FlareSlate = new() { Character = new CrpgCharacter { Name = "FlareSlate", Id = 1239, Rating = new CrpgCharacterRating { Value = 2592 } } };
    private static readonly CrpgUser AcinacesVictor = new() { Character = new CrpgCharacter { Name = "AcinacesVictor", Id = 1240, Rating = new CrpgCharacterRating { Value = 1349 } } };
    private static readonly CrpgUser InkyStickUp = new() { Character = new CrpgCharacter { Name = "InkyStickUp", Id = 1241, Rating = new CrpgCharacterRating { Value = 2306 } } };
    private static readonly CrpgUser FriendSponson = new() { Character = new CrpgCharacter { Name = "FriendSponson", Id = 1242, Rating = new CrpgCharacterRating { Value = 790 } } };
    private static readonly CrpgUser AnguiformLethal = new() { Character = new CrpgCharacter { Name = "AnguiformLethal", Id = 1243, Rating = new CrpgCharacterRating { Value = 1280 } } };
    private static readonly CrpgUser AttackSovereign = new() { Character = new CrpgCharacter { Name = "AttackSovereign", Id = 1244, Rating = new CrpgCharacterRating { Value = 1514 } } };
    private static readonly CrpgUser GloomyRookie = new() { Character = new CrpgCharacter { Name = "GloomyRookie", Id = 1245, Rating = new CrpgCharacterRating { Value = 1233 } } };
    private static readonly CrpgUser AckSwordCane = new() { Character = new CrpgCharacter { Name = "AckSwordCane", Id = 1246, Rating = new CrpgCharacterRating { Value = 2727 } } };
    private static readonly CrpgUser ConsumeLower = new() { Character = new CrpgCharacter { Name = "ConsumeLower", Id = 1247, Rating = new CrpgCharacterRating { Value = 698 } } };
    private static readonly CrpgUser ApitherapyUmber = new() { Character = new CrpgCharacter { Name = "ApitherapyUmber", Id = 1248, Rating = new CrpgCharacterRating { Value = 864 } } };
    private static readonly CrpgUser BurningKindle = new() { Character = new CrpgCharacter { Name = "BurningKindle", Id = 1249, Rating = new CrpgCharacterRating { Value = 1741 } } };
    private static readonly CrpgUser FlagTrigger = new() { Character = new CrpgCharacter { Name = "FlagTrigger", Id = 1250, Rating = new CrpgCharacterRating { Value = 2633 } } };
    private static readonly CrpgUser InvasionMatador = new() { Character = new CrpgCharacter { Name = "InvasionMatador", Id = 1251, Rating = new CrpgCharacterRating { Value = 1582 } } };
    private static readonly CrpgUser AntigunMilkSnake = new() { Character = new CrpgCharacter { Name = "AntigunMilkSnake", Id = 1252, Rating = new CrpgCharacterRating { Value = 1843 } } };
    private static readonly CrpgUser ConstrictorWeapon = new() { Character = new CrpgCharacter { Name = "ConstrictorWeapon", Id = 1253, Rating = new CrpgCharacterRating { Value = 2141 } } };
    private static readonly CrpgUser GloomSpike = new() { Character = new CrpgCharacter { Name = "GloomSpike", Id = 1254, Rating = new CrpgCharacterRating { Value = 1837 } } };
    private static readonly CrpgUser EyedPython = new() { Character = new CrpgCharacter { Name = "EyedPython", Id = 1255, Rating = new CrpgCharacterRating { Value = 2393 } } };
    private static readonly CrpgUser IncendiarySlug = new() { Character = new CrpgCharacter { Name = "IncendiarySlug", Id = 1256, Rating = new CrpgCharacterRating { Value = 1070 } } };
    private static readonly CrpgUser CrownKingsnake = new() { Character = new CrpgCharacter { Name = "CrownKingsnake", Id = 1257, Rating = new CrpgCharacterRating { Value = 2725 } } };
    private static readonly CrpgUser BlackDuckMine = new() { Character = new CrpgCharacter { Name = "BlackDuckMine", Id = 1258, Rating = new CrpgCharacterRating { Value = 1435 } } };
    private static readonly CrpgUser FenceVenom = new() { Character = new CrpgCharacter { Name = "FenceVenom", Id = 1259, Rating = new CrpgCharacterRating { Value = 2088 } } };
    private static readonly CrpgUser FireNovitiate = new() { Character = new CrpgCharacter { Name = "FireNovitiate", Id = 1260, Rating = new CrpgCharacterRating { Value = 1142 } } };
    private static readonly CrpgUser FrogYoungling = new() { Character = new CrpgCharacter { Name = "FrogYoungling", Id = 1261, Rating = new CrpgCharacterRating { Value = 2885 } } };
    private static readonly CrpgUser IngleMachinePistol = new() { Character = new CrpgCharacter { Name = "IngleMachinePistol", Id = 1262, Rating = new CrpgCharacterRating { Value = 2553 } } };
    private static readonly CrpgUser BlunderbussTeal = new() { Character = new CrpgCharacter { Name = "BlunderbussTeal", Id = 1263, Rating = new CrpgCharacterRating { Value = 2716 } } };
    private static readonly CrpgUser CopperheadStratagem = new() { Character = new CrpgCharacter { Name = "CopperheadStratagem", Id = 1264, Rating = new CrpgCharacterRating { Value = 914 } } };
    private static readonly CrpgUser CubSerpentiform = new() { Character = new CrpgCharacter { Name = "CubSerpentiform", Id = 1265, Rating = new CrpgCharacterRating { Value = 1261 } } };
    private static readonly CrpgUser DragonRingedSnake = new() { Character = new CrpgCharacter { Name = "DragonRingedSnake", Id = 1266, Rating = new CrpgCharacterRating { Value = 2928 } } };
    private static readonly CrpgUser AmbuscadePop = new() { Character = new CrpgCharacter { Name = "AmbuscadePop", Id = 1267, Rating = new CrpgCharacterRating { Value = 2102 } } };
    private static readonly CrpgUser HaftStout = new() { Character = new CrpgCharacter { Name = "HaftStout", Id = 1268, Rating = new CrpgCharacterRating { Value = 1133 } } };
    private static readonly CrpgUser FangedOilfish = new() { Character = new CrpgCharacter { Name = "FangedOilfish", Id = 1269, Rating = new CrpgCharacterRating { Value = 2176 } } };
    private static readonly CrpgUser FreshmanSlither = new() { Character = new CrpgCharacter { Name = "FreshmanSlither", Id = 1270, Rating = new CrpgCharacterRating { Value = 2107 } } };
    private static readonly CrpgUser InnovativeSilencer = new() { Character = new CrpgCharacter { Name = "InnovativeSilencer", Id = 1271, Rating = new CrpgCharacterRating { Value = 898 } } };
    private static readonly CrpgUser AugerShot = new() { Character = new CrpgCharacter { Name = "AugerShot", Id = 1272, Rating = new CrpgCharacterRating { Value = 1088 } } };
    private static readonly CrpgUser CollaborationNewbie = new() { Character = new CrpgCharacter { Name = "CollaborationNewbie", Id = 1273, Rating = new CrpgCharacterRating { Value = 977 } } };
    private static readonly CrpgUser GladiolusIsToast = new() { Character = new CrpgCharacter { Name = "GladiolusIsToast", Id = 1274, Rating = new CrpgCharacterRating { Value = 2984 } } };
    private static readonly CrpgUser DingyTuck = new() { Character = new CrpgCharacter { Name = "DingyTuck", Id = 1275, Rating = new CrpgCharacterRating { Value = 2851 } } };
    private static readonly CrpgUser ArchariosSharpshooter = new() { Character = new CrpgCharacter { Name = "ArchariosSharpshooter", Id = 1276, Rating = new CrpgCharacterRating { Value = 2362 } } };
    private static readonly CrpgUser DarkQuadrate = new() { Character = new CrpgCharacter { Name = "DarkQuadrate", Id = 1277, Rating = new CrpgCharacterRating { Value = 1444 } } };
    private static readonly CrpgUser DungeonRam = new() { Character = new CrpgCharacter { Name = "DungeonRam", Id = 1278, Rating = new CrpgCharacterRating { Value = 678 } } };
    private static readonly CrpgUser BlazeLight = new() { Character = new CrpgCharacter { Name = "BlazeLight", Id = 1279, Rating = new CrpgCharacterRating { Value = 1449 } } };
    private static readonly CrpgUser AutomaticSwordfish = new() { Character = new CrpgCharacter { Name = "AutomaticSwordfish", Id = 1280, Rating = new CrpgCharacterRating { Value = 1252 } } };
    private static readonly CrpgUser EmpyrosisSad = new() { Character = new CrpgCharacter { Name = "EmpyrosisSad", Id = 1281, Rating = new CrpgCharacterRating { Value = 2620 } } };
    private static readonly CrpgUser IgnitePlum = new() { Character = new CrpgCharacter { Name = "IgnitePlum", Id = 1282, Rating = new CrpgCharacterRating { Value = 2814 } } };
    private static readonly CrpgUser Firebomb = new() { Character = new CrpgCharacter { Name = "Firebomb", Id = 1283, Rating = new CrpgCharacterRating { Value = 501 } } };
    private static readonly CrpgUser RattlesnakeRoot = new() { Character = new CrpgCharacter { Name = "RattlesnakeRoot", Id = 1284, Rating = new CrpgCharacterRating { Value = 2437 } } };
    private static readonly CrpgUser BackViper = new() { Character = new CrpgCharacter { Name = "BackViper", Id = 1285, Rating = new CrpgCharacterRating { Value = 2994 } } };
    private static readonly CrpgUser FlintlockSabotage = new() { Character = new CrpgCharacter { Name = "FlintlockSabotage", Id = 1286, Rating = new CrpgCharacterRating { Value = 2694 } } };
    private static readonly CrpgUser AspVenomous = new() { Character = new CrpgCharacter { Name = "AspVenomous", Id = 1287, Rating = new CrpgCharacterRating { Value = 1966 } } };
    private static readonly CrpgUser GriffinShooter = new() { Character = new CrpgCharacter { Name = "GriffinShooter", Id = 1288, Rating = new CrpgCharacterRating { Value = 2282 } } };
    private static readonly CrpgUser BlackenMagazine = new() { Character = new CrpgCharacter { Name = "BlackenMagazine", Id = 1289, Rating = new CrpgCharacterRating { Value = 2335 } } };
    private static readonly CrpgUser BeltShell = new() { Character = new CrpgCharacter { Name = "BeltShell", Id = 1290, Rating = new CrpgCharacterRating { Value = 819 } } };
    private static readonly CrpgUser GunpointMate = new() { Character = new CrpgCharacter { Name = "GunpointMate", Id = 1291, Rating = new CrpgCharacterRating { Value = 1938 } } };
    private static readonly CrpgUser CastUp = new() { Character = new CrpgCharacter { Name = "CastUp", Id = 1292, Rating = new CrpgCharacterRating { Value = 2953 } } };
    private static readonly CrpgUser ClockXiphophyllous = new() { Character = new CrpgCharacter { Name = "ClockXiphophyllous", Id = 1293, Rating = new CrpgCharacterRating { Value = 2363 } } };
    private static readonly CrpgUser FiredrakeRefire = new() { Character = new CrpgCharacter { Name = "FiredrakeRefire", Id = 1294, Rating = new CrpgCharacterRating { Value = 1244 } } };
    private static readonly CrpgUser BoreSnakebite = new() { Character = new CrpgCharacter { Name = "BoreSnakebite", Id = 1295, Rating = new CrpgCharacterRating { Value = 541 } } };
    private static readonly CrpgUser CarbylamineNeurotropic = new() { Character = new CrpgCharacter { Name = "CarbylamineNeurotropic", Id = 1296, Rating = new CrpgCharacterRating { Value = 2358 } } };
    private static readonly CrpgUser ChapeMalice = new() { Character = new CrpgCharacter { Name = "ChapeMalice", Id = 1297, Rating = new CrpgCharacterRating { Value = 2521 } } };
    private static readonly CrpgUser HoldUpRedbackSpider = new() { Character = new CrpgCharacter { Name = "HoldUpRedbackSpider", Id = 1298, Rating = new CrpgCharacterRating { Value = 1579 } } };
    private static readonly CrpgUser AntiveninTraverse = new() { Character = new CrpgCharacter { Name = "AntiveninTraverse", Id = 1299, Rating = new CrpgCharacterRating { Value = 2973 } } };
    private static readonly CrpgUser DeepSword = new() { Character = new CrpgCharacter { Name = "DeepSword", Id = 1300, Rating = new CrpgCharacterRating { Value = 2405 } } };
    private static readonly CrpgUser GunstockZombie = new() { Character = new CrpgCharacter { Name = "GunstockZombie", Id = 1301, Rating = new CrpgCharacterRating { Value = 2507 } } };
    private static readonly CrpgUser BoaConstrictorRifling = new() { Character = new CrpgCharacter { Name = "BoaConstrictorRifling", Id = 1302, Rating = new CrpgCharacterRating { Value = 768 } } };
    private static readonly CrpgUser ColubrineMilk = new() { Character = new CrpgCharacter { Name = "ColubrineMilk", Id = 1303, Rating = new CrpgCharacterRating { Value = 2523 } } };
    private static readonly CrpgUser EnkindleReload = new() { Character = new CrpgCharacter { Name = "EnkindleReload", Id = 1304, Rating = new CrpgCharacterRating { Value = 2265 } } };
    private static readonly CrpgUser FirepowerQuarter = new() { Character = new CrpgCharacter { Name = "FirepowerQuarter", Id = 1305, Rating = new CrpgCharacterRating { Value = 2271 } } };
    private static readonly CrpgUser ForaySmother = new() { Character = new CrpgCharacter { Name = "ForaySmother", Id = 1306, Rating = new CrpgCharacterRating { Value = 685 } } };
    private static readonly CrpgUser ChargeKing = new() { Character = new CrpgCharacter { Name = "ChargeKing", Id = 1307, Rating = new CrpgCharacterRating { Value = 1606 } } };
    private static readonly CrpgUser ClaymoreLow = new() { Character = new CrpgCharacter { Name = "ClaymoreLow", Id = 1308, Rating = new CrpgCharacterRating { Value = 2833 } } };
    private static readonly CrpgUser ColubridRex = new() { Character = new CrpgCharacter { Name = "ColubridRex", Id = 1309, Rating = new CrpgCharacterRating { Value = 2814 } } };
    private static readonly CrpgUser ImmolateMarksman = new() { Character = new CrpgCharacter { Name = "ImmolateMarksman", Id = 1310, Rating = new CrpgCharacterRating { Value = 1618 } } };
    private static readonly CrpgUser HellfirePopgun = new() { Character = new CrpgCharacter { Name = "HellfirePopgun", Id = 1311, Rating = new CrpgCharacterRating { Value = 1366 } } };
    private static readonly CrpgUser HostileMadtom = new() { Character = new CrpgCharacter { Name = "HostileMadtom", Id = 1312, Rating = new CrpgCharacterRating { Value = 1042 } } };
    private static readonly CrpgUser BlackamoorSable = new() { Character = new CrpgCharacter { Name = "BlackamoorSable", Id = 1313, Rating = new CrpgCharacterRating { Value = 1426 } } };
    private static readonly CrpgUser FlakWaster = new() { Character = new CrpgCharacter { Name = "FlakWaster", Id = 1314, Rating = new CrpgCharacterRating { Value = 2620 } } };
    private static readonly CrpgUser CoverVenomosalivary = new() { Character = new CrpgCharacter { Name = "CoverVenomosalivary", Id = 1315, Rating = new CrpgCharacterRating { Value = 1268 } } };
    private static readonly CrpgUser AccoladeTrain = new() { Character = new CrpgCharacter { Name = "AccoladeTrain", Id = 1316, Rating = new CrpgCharacterRating { Value = 2860 } } };
    private static readonly CrpgUser BackfireLine = new() { Character = new CrpgCharacter { Name = "BackfireLine", Id = 1317, Rating = new CrpgCharacterRating { Value = 2815 } } };
    private static readonly CrpgUser ColtRetreat = new() { Character = new CrpgCharacter { Name = "ColtRetreat", Id = 1318, Rating = new CrpgCharacterRating { Value = 1579 } } };
    private static readonly CrpgUser HolocaustShah = new() { Character = new CrpgCharacter { Name = "HolocaustShah", Id = 1319, Rating = new CrpgCharacterRating { Value = 2648 } } };
    private static readonly CrpgUser EnvenomOvercast = new() { Character = new CrpgCharacter { Name = "EnvenomOvercast", Id = 1320, Rating = new CrpgCharacterRating { Value = 2482 } } };
    private static readonly CrpgUser InterceptorPyromancy = new() { Character = new CrpgCharacter { Name = "InterceptorPyromancy", Id = 1321, Rating = new CrpgCharacterRating { Value = 1170 } } };
    private static readonly CrpgUser CutlassSwordsman = new() { Character = new CrpgCharacter { Name = "CutlassSwordsman", Id = 1322, Rating = new CrpgCharacterRating { Value = 2727 } } };
    private static readonly CrpgUser CollaboratorSwordKnot = new() { Character = new CrpgCharacter { Name = "CollaboratorSwordKnot", Id = 1323, Rating = new CrpgCharacterRating { Value = 1222 } } };
    private static readonly CrpgUser ClaretPicket = new() { Character = new CrpgCharacter { Name = "ClaretPicket", Id = 1324, Rating = new CrpgCharacterRating { Value = 1978 } } };
    private static readonly CrpgUser CatchStarter = new() { Character = new CrpgCharacter { Name = "CatchStarter", Id = 1325, Rating = new CrpgCharacterRating { Value = 2531 } } };
    private static readonly CrpgUser FireballSabre = new() { Character = new CrpgCharacter { Name = "FireballSabre", Id = 1326, Rating = new CrpgCharacterRating { Value = 1449 } } };
    private static readonly CrpgUser GrapeSurrender = new() { Character = new CrpgCharacter { Name = "GrapeSurrender", Id = 1327, Rating = new CrpgCharacterRating { Value = 2972 } } };
    private static readonly CrpgUser AnacondaTommyGun = new() { Character = new CrpgCharacter { Name = "AnacondaTommyGun", Id = 1328, Rating = new CrpgCharacterRating { Value = 2268 } } };
    private static readonly CrpgUser CheckmateThundercloud = new() { Character = new CrpgCharacter { Name = "CheckmateThundercloud", Id = 1329, Rating = new CrpgCharacterRating { Value = 570 } } };
    private static readonly CrpgUser HereMatchlock = new() { Character = new CrpgCharacter { Name = "HereMatchlock", Id = 1330, Rating = new CrpgCharacterRating { Value = 1099 } } };
    private static readonly CrpgUser AbatisPilotSnake = new() { Character = new CrpgCharacter { Name = "AbatisPilotSnake", Id = 1331, Rating = new CrpgCharacterRating { Value = 1161 } } };
    private static readonly CrpgUser BarrelSting = new() { Character = new CrpgCharacter { Name = "BarrelSting", Id = 1332, Rating = new CrpgCharacterRating { Value = 2809 } } };
    private static readonly CrpgUser CombustibleNight = new() { Character = new CrpgCharacter { Name = "CombustibleNight", Id = 1333, Rating = new CrpgCharacterRating { Value = 2443 } } };
    private static readonly CrpgUser CommandoLock = new() { Character = new CrpgCharacter { Name = "CommandoLock", Id = 1334, Rating = new CrpgCharacterRating { Value = 1266 } } };
    private static readonly CrpgUser BeestingTrench = new() { Character = new CrpgCharacter { Name = "BeestingTrench", Id = 1335, Rating = new CrpgCharacterRating { Value = 630 } } };
    private static readonly CrpgUser AfireSlough = new() { Character = new CrpgCharacter { Name = "AfireSlough", Id = 1336, Rating = new CrpgCharacterRating { Value = 1619 } } };
    private static readonly CrpgUser CoralSnakePyro = new() { Character = new CrpgCharacter { Name = "CoralSnakePyro", Id = 1337, Rating = new CrpgCharacterRating { Value = 615 } } };
    private static readonly CrpgUser BloodPhosphodiesterase = new() { Character = new CrpgCharacter { Name = "BloodPhosphodiesterase", Id = 1338, Rating = new CrpgCharacterRating { Value = 695 } } };
    private static readonly CrpgUser HiltMurrey = new() { Character = new CrpgCharacter { Name = "HiltMurrey", Id = 1339, Rating = new CrpgCharacterRating { Value = 1174 } } };
    private static readonly CrpgUser CharcoalPrisonerofWar = new() { Character = new CrpgCharacter { Name = "CharcoalPrisonerofWar", Id = 1340, Rating = new CrpgCharacterRating { Value = 1696 } } };
    private static readonly CrpgUser GunmetalSwelter = new() { Character = new CrpgCharacter { Name = "GunmetalSwelter", Id = 1341, Rating = new CrpgCharacterRating { Value = 2797 } } };
    private static readonly CrpgUser CaliginousPuce = new() { Character = new CrpgCharacter { Name = "CaliginousPuce", Id = 1342, Rating = new CrpgCharacterRating { Value = 825 } } };
    private static readonly CrpgUser BrownSloe = new() { Character = new CrpgCharacter { Name = "BrownSloe", Id = 1343, Rating = new CrpgCharacterRating { Value = 1748 } } };
    private static readonly CrpgUser FiredFishQuench = new() { Character = new CrpgCharacter { Name = "FiredFishQuench", Id = 1344, Rating = new CrpgCharacterRating { Value = 558 } } };
    private static readonly CrpgUser CaperLynx = new() { Character = new CrpgCharacter { Name = "CaperLynx", Id = 1345, Rating = new CrpgCharacterRating { Value = 770 } } };
    private static readonly CrpgUser TastyCalyx = new() { Character = new CrpgCharacter { Name = "TastyCalyx", Id = 1346, Rating = new CrpgCharacterRating { Value = 545 } } };
    private static readonly CrpgUser SiameseLavender = new() { Character = new CrpgCharacter { Name = "SiameseLavender", Id = 1347, Rating = new CrpgCharacterRating { Value = 1912 } } };
    private static readonly CrpgUser BeauChichi = new() { Character = new CrpgCharacter { Name = "BeauChichi", Id = 1348, Rating = new CrpgCharacterRating { Value = 2004 } } };
    private static readonly CrpgUser DogPanther = new() { Character = new CrpgCharacter { Name = "DogPanther", Id = 1349, Rating = new CrpgCharacterRating { Value = 2210 } } };
    private static readonly CrpgUser BlossomJelly = new() { Character = new CrpgCharacter { Name = "BlossomJelly", Id = 1350, Rating = new CrpgCharacterRating { Value = 2133 } } };
    private static readonly CrpgUser SharpPapergirl = new() { Character = new CrpgCharacter { Name = "SharpPapergirl", Id = 1351, Rating = new CrpgCharacterRating { Value = 2547 } } };
    private static readonly CrpgUser MoppetTear = new() { Character = new CrpgCharacter { Name = "MoppetTear", Id = 1352, Rating = new CrpgCharacterRating { Value = 1364 } } };
    private static readonly CrpgUser BlowHandsome = new() { Character = new CrpgCharacter { Name = "BlowHandsome", Id = 1353, Rating = new CrpgCharacterRating { Value = 1455 } } };
    private static readonly CrpgUser SisterSmirk = new() { Character = new CrpgCharacter { Name = "SisterSmirk", Id = 1354, Rating = new CrpgCharacterRating { Value = 2864 } } };
    private static readonly CrpgUser FelineSweetTooth = new() { Character = new CrpgCharacter { Name = "FelineSweetTooth", Id = 1355, Rating = new CrpgCharacterRating { Value = 742 } } };
    private static readonly CrpgUser SealSneak = new() { Character = new CrpgCharacter { Name = "SealSneak", Id = 1356, Rating = new CrpgCharacterRating { Value = 1227 } } };
    private static readonly CrpgUser TinyFetis = new() { Character = new CrpgCharacter { Name = "TinyFetis", Id = 1357, Rating = new CrpgCharacterRating { Value = 1402 } } };
    private static readonly CrpgUser LassBloom = new() { Character = new CrpgCharacter { Name = "LassBloom", Id = 1358, Rating = new CrpgCharacterRating { Value = 751 } } };
    private static readonly CrpgUser BoxDinky = new() { Character = new CrpgCharacter { Name = "BoxDinky", Id = 1359, Rating = new CrpgCharacterRating { Value = 1688 } } };
    private static readonly CrpgUser BriocheRam = new() { Character = new CrpgCharacter { Name = "BriocheRam", Id = 1360, Rating = new CrpgCharacterRating { Value = 2750 } } };
    private static readonly CrpgUser SweetKitty = new() { Character = new CrpgCharacter { Name = "SweetKitty", Id = 1361, Rating = new CrpgCharacterRating { Value = 2678 } } };
    private static readonly CrpgUser GrimalkinDelicacy = new() { Character = new CrpgCharacter { Name = "GrimalkinDelicacy", Id = 1362, Rating = new CrpgCharacterRating { Value = 1057 } } };
    private static readonly CrpgUser LionMuscatel = new() { Character = new CrpgCharacter { Name = "LionMuscatel", Id = 1363, Rating = new CrpgCharacterRating { Value = 1903 } } };
    private static readonly CrpgUser ExtrafloralUnicorn = new() { Character = new CrpgCharacter { Name = "ExtrafloralUnicorn", Id = 1364, Rating = new CrpgCharacterRating { Value = 2923 } } };
    private static readonly CrpgUser NewsgirlLitter = new() { Character = new CrpgCharacter { Name = "NewsgirlLitter", Id = 1365, Rating = new CrpgCharacterRating { Value = 2515 } } };
    private static readonly CrpgUser CatsBalm = new() { Character = new CrpgCharacter { Name = "CatsBalm", Id = 1366, Rating = new CrpgCharacterRating { Value = 1786 } } };
    private static readonly CrpgUser DiscriminateBlancmange = new() { Character = new CrpgCharacter { Name = "DiscriminateBlancmange", Id = 1367, Rating = new CrpgCharacterRating { Value = 2151 } } };
    private static readonly CrpgUser TroopBobcat = new() { Character = new CrpgCharacter { Name = "TroopBobcat", Id = 1368, Rating = new CrpgCharacterRating { Value = 502 } } };
    private static readonly CrpgUser PunctiliousQuirky = new() { Character = new CrpgCharacter { Name = "PunctiliousQuirky", Id = 1369, Rating = new CrpgCharacterRating { Value = 865 } } };
    private static readonly CrpgUser GuideyUnpretty = new() { Character = new CrpgCharacter { Name = "GuideyUnpretty", Id = 1370, Rating = new CrpgCharacterRating { Value = 1450 } } };
    private static readonly CrpgUser ScurryHuge = new() { Character = new CrpgCharacter { Name = "ScurryHuge", Id = 1371, Rating = new CrpgCharacterRating { Value = 754 } } };
    private static readonly CrpgUser SlatternLoving = new() { Character = new CrpgCharacter { Name = "SlatternLoving", Id = 1372, Rating = new CrpgCharacterRating { Value = 2238 } } };
    private static readonly CrpgUser OnlyGirlChild = new() { Character = new CrpgCharacter { Name = "OnlyGirlChild", Id = 1373, Rating = new CrpgCharacterRating { Value = 1249 } } };
    private static readonly CrpgUser SwellSomali = new() { Character = new CrpgCharacter { Name = "SwellSomali", Id = 1374, Rating = new CrpgCharacterRating { Value = 2813 } } };
    private static readonly CrpgUser LatinaCyme = new() { Character = new CrpgCharacter { Name = "LatinaCyme", Id = 1375, Rating = new CrpgCharacterRating { Value = 2221 } } };
    private static readonly CrpgUser ScrumptiousKettleofFish = new() { Character = new CrpgCharacter { Name = "ScrumptiousKettleofFish", Id = 1376, Rating = new CrpgCharacterRating { Value = 2061 } } };
    private static readonly CrpgUser PearlPosy = new() { Character = new CrpgCharacter { Name = "PearlPosy", Id = 1377, Rating = new CrpgCharacterRating { Value = 1745 } } };
    private static readonly CrpgUser AlyssumMilkmaid = new() { Character = new CrpgCharacter { Name = "AlyssumMilkmaid", Id = 1378, Rating = new CrpgCharacterRating { Value = 1251 } } };
    private static readonly CrpgUser ChrysanthemumPeachy = new() { Character = new CrpgCharacter { Name = "ChrysanthemumPeachy", Id = 1379, Rating = new CrpgCharacterRating { Value = 1083 } } };
    private static readonly CrpgUser CalicoBun = new() { Character = new CrpgCharacter { Name = "CalicoBun", Id = 1380, Rating = new CrpgCharacterRating { Value = 635 } } };
    private static readonly CrpgUser BunnyCatPudgy = new() { Character = new CrpgCharacter { Name = "BunnyCatPudgy", Id = 1381, Rating = new CrpgCharacterRating { Value = 2810 } } };
    private static readonly CrpgUser CandiedFragrant = new() { Character = new CrpgCharacter { Name = "CandiedFragrant", Id = 1382, Rating = new CrpgCharacterRating { Value = 1004 } } };
    private static readonly CrpgUser MadamTomboy = new() { Character = new CrpgCharacter { Name = "MadamTomboy", Id = 1383, Rating = new CrpgCharacterRating { Value = 975 } } };
    private static readonly CrpgUser GynoeciumFeat = new() { Character = new CrpgCharacter { Name = "GynoeciumFeat", Id = 1384, Rating = new CrpgCharacterRating { Value = 866 } } };
    private static readonly CrpgUser PreciseAnthesis = new() { Character = new CrpgCharacter { Name = "PreciseAnthesis", Id = 1385, Rating = new CrpgCharacterRating { Value = 2696 } } };
    private static readonly CrpgUser SaccharineLamb = new() { Character = new CrpgCharacter { Name = "SaccharineLamb", Id = 1386, Rating = new CrpgCharacterRating { Value = 2021 } } };
    private static readonly CrpgUser CoquettePleasant = new() { Character = new CrpgCharacter { Name = "CoquettePleasant", Id = 1387, Rating = new CrpgCharacterRating { Value = 1384 } } };
    private static readonly CrpgUser LilacSweetly = new() { Character = new CrpgCharacter { Name = "LilacSweetly", Id = 1388, Rating = new CrpgCharacterRating { Value = 782 } } };
    private static readonly CrpgUser EmbarrassedMeow = new() { Character = new CrpgCharacter { Name = "EmbarrassedMeow", Id = 1389, Rating = new CrpgCharacterRating { Value = 1489 } } };
    private static readonly CrpgUser FloweringMissy = new() { Character = new CrpgCharacter { Name = "FloweringMissy", Id = 1390, Rating = new CrpgCharacterRating { Value = 896 } } };
    private static readonly CrpgUser CuttyClamber = new() { Character = new CrpgCharacter { Name = "CuttyClamber", Id = 1391, Rating = new CrpgCharacterRating { Value = 2338 } } };
    private static readonly CrpgUser PrettilyThalamus = new() { Character = new CrpgCharacter { Name = "PrettilyThalamus", Id = 1392, Rating = new CrpgCharacterRating { Value = 1346 } } };
    private static readonly CrpgUser EncounterPollination = new() { Character = new CrpgCharacter { Name = "EncounterPollination", Id = 1393, Rating = new CrpgCharacterRating { Value = 576 } } };
    private static readonly CrpgUser PatrolBonbon = new() { Character = new CrpgCharacter { Name = "PatrolBonbon", Id = 1394, Rating = new CrpgCharacterRating { Value = 2468 } } };
    private static readonly CrpgUser PortFem = new() { Character = new CrpgCharacter { Name = "PortFem", Id = 1395, Rating = new CrpgCharacterRating { Value = 1659 } } };
    private static readonly CrpgUser BudgereePerianth = new() { Character = new CrpgCharacter { Name = "BudgereePerianth", Id = 1396, Rating = new CrpgCharacterRating { Value = 2764 } } };
    private static readonly CrpgUser PsycheStaminate = new() { Character = new CrpgCharacter { Name = "PsycheStaminate", Id = 1397, Rating = new CrpgCharacterRating { Value = 1035 } } };
    private static readonly CrpgUser HoneyedSugar = new() { Character = new CrpgCharacter { Name = "HoneyedSugar", Id = 1399, Rating = new CrpgCharacterRating { Value = 2216 } } };

    private static readonly GameMatch Game1 = new()
    {
        TeamA = new List<CrpgUser>(),
        TeamB = new List<CrpgUser>(),
        Waiting = new List<CrpgUser>
        {
            Arwen,
            Frodon,
            Sam,
            Sangoku,
            Krilin,
            RolandDeschain,
            HarryPotter,
            Magneto,
            ProfCharles,
            UsainBolt,
            Agent007,
            SpongeBob,
            Patrick,
            Madonna,
            LaraCroft,
            JeanneDArc,
            Merlin,
            Bob,
            Thomas,
            RonWeasley,
            Jean01,
            Jean02,
            Jean03,
            Jean04,
            Jean05,
            Glutentag01,
            Glutentag02,
            Glutentag03,
            Glutentag04,
            Glutentag05,
            Vlexance01,
            Vlexance02,
            Vlexance03,
            Vlexance04,
            Vlexance05,
            Hudax01,
            Hudax02,
            Hudax03,
            Hudax04,
            Hudax05,
            Hudax06,
            Hudax07,
            Hudax08,
            Hudax09,
            Hudax10,
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
            FloatTrail,
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
            HoneyedSugar,
        },
    };

    private static readonly GameMatch GameWithVeryStrongClanGroup = new()
    {
        TeamA = new List<CrpgUser>(),
        TeamB = new List<CrpgUser>(),
        Waiting = new List<CrpgUser>
        {
            Aragorn,
            Arwen,
            Frodon,
            Sam,
            Sangoku,
            Krilin,
            RolandDeschain,
            HarryPotter,
            Magneto,
            ProfCharles,
            UsainBolt,
            Agent007,
            SpongeBob,
            Patrick,
            Madonna,
            LaraCroft,
        },
    };

    private static readonly GameMatch NoManGame = new()
    {
        TeamA = new List<CrpgUser>(),
        TeamB = new List<CrpgUser>(),
        Waiting = new List<CrpgUser>(),
    };

    private static readonly GameMatch OneManGame = new()
    {
        TeamA = new List<CrpgUser>(),
        TeamB = new List<CrpgUser>(),
        Waiting = new List<CrpgUser>
        {
            Aragorn,
        },
    };

    private static readonly GameMatch TwoManGame = new()
    {
        TeamA = new List<CrpgUser>(),
        TeamB = new List<CrpgUser>(),
        Waiting = new List<CrpgUser>
        {
            Aragorn,
            Arwen,
        },
    };

    private static readonly GameMatch ThreeManGame = new()
    {
        TeamA = new List<CrpgUser>(),
        TeamB = new List<CrpgUser>(),
        Waiting = new List<CrpgUser>
        {
            Aragorn,
            Arwen,
            Madonna,
        },
    };
    private static readonly GameMatch EmptyTeamGame = new()
    {
        TeamA = new List<CrpgUser>(),
        TeamB = new List<CrpgUser>
        {
            Aragorn,
            Arwen,
            Madonna,
            Frodon,
        },
        Waiting = new List<CrpgUser>(),
    };
    [Test]
    public void KkMakeTeamOfSimilarSizesShouldNotBeThatBad()
    {
        var matchBalancer = new MatchBalancingSystem();
        GameMatch balancedGame = matchBalancer.KkMakeTeamOfSimilarSizesWithoutSplittingClanGroups(GameWithVeryStrongClanGroup);
        float teamASize = balancedGame.TeamA.Count;
        float teamBSize = balancedGame.TeamB.Count;
        float sizeRatio = teamASize / teamBSize;
        float teamAMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(balancedGame.TeamA, 1);
        float teamBMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(balancedGame.TeamB, 1);
        float meanRatingRatio = teamAMeanRating / teamBMeanRating;
        MatchBalancingHelpers.DumpTeams(balancedGame);
        Assert.AreEqual(sizeRatio, 0.7f, 0.3f);
    }

    [Test]
    public void BannerBalancing()
    {
        float unbalancedTeamAMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(Game1.TeamA, 1);
        float unbalancedTeamBMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(Game1.TeamB, 1);
        float unbalancedMeanRatingRatio = unbalancedTeamAMeanRating / unbalancedTeamBMeanRating;
        Console.WriteLine($"unbalanced rating ratio = {unbalancedMeanRatingRatio}");

        GameMatch balancedGame = PureBannerBalancing(Game1);
        float teamASize = balancedGame.TeamA.Count;
        float teamBSize = balancedGame.TeamB.Count;
        float sizeRatio = teamASize / teamBSize;
        Console.WriteLine($"balanced size ratio = {sizeRatio}");
        float teamARating = RatingHelpers.ComputeTeamRatingPowerSum(balancedGame.TeamA, 1);
        float teamBRating = RatingHelpers.ComputeTeamRatingPowerSum(balancedGame.TeamB, 1);
        float ratingRatio = teamARating / teamBRating;
        Console.WriteLine($"teamASize = {teamASize} teamBSize = {teamBSize}");
        Console.WriteLine($"teamARating = new CrpgCharacterRating {{ Value = {teamARating} teamBRating = new CrpgCharacterRating {{ Value = {teamBRating} }} }}");
        Assert.AreEqual(ratingRatio, 1, 0.2);
    }

    [Test]
    public void BannerBalancingWithEdgeCaseWarmup()
    {
        var matchBalancer = new MatchBalancingSystem();

        float unbalancedTeamAMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(Game1.TeamA, 1);
        float unbalancedTeamBMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(Game1.TeamB, 1);
        float unbalancedMeanRatingRatio = unbalancedTeamAMeanRating / unbalancedTeamBMeanRating;
        GameMatch balancedGame = matchBalancer.BannerBalancingWithEdgeCases(Game1, firstBalance: true);
        float teamASize = balancedGame.TeamA.Count;
        float teamBSize = balancedGame.TeamB.Count;
        float sizeRatio = teamASize / teamBSize;
        float teamARating = RatingHelpers.ComputeTeamRatingPowerSum(balancedGame.TeamA, 1);
        float teamBRating = RatingHelpers.ComputeTeamRatingPowerSum(balancedGame.TeamB, 1);
        float ratingRatio = teamARating / teamBRating;
        Assert.AreEqual(ratingRatio, 1, 0.2);
    }

    [Test]
    public void BannerBalancingWithEdgeCaseNotWarmup()
    {
        var matchBalancer = new MatchBalancingSystem();
        GameMatch balancedGame = matchBalancer.NaiveCaptainBalancing(Game1);
        balancedGame = matchBalancer.BannerBalancingWithEdgeCases(balancedGame, firstBalance: false);

        float teamARating = RatingHelpers.ComputeTeamRatingPowerSum(balancedGame.TeamA, 1);
        float teamBRating = RatingHelpers.ComputeTeamRatingPowerSum(balancedGame.TeamB, 1);
        float ratingRatio = teamARating / teamBRating;
        Assert.AreEqual(ratingRatio, 1, 0.2);
    }

    [Test]
    public void BannerBalancingWithEdgeCaseNotWarmupShouldWorkWithOneTeamEmpty()
    {
        var matchBalancer = new MatchBalancingSystem();
        var balancedGame = matchBalancer.BannerBalancingWithEdgeCases(EmptyTeamGame, firstBalance: false);

        float teamARating = RatingHelpers.ComputeTeamRatingPowerSum(balancedGame.TeamA, 1);
        float teamBRating = RatingHelpers.ComputeTeamRatingPowerSum(balancedGame.TeamB, 1);
        float ratingRatio = teamARating / teamBRating;
        MatchBalancingHelpers.DumpTeams(balancedGame);
    }

    [Test]
    public void BannerBalancingWithEdgeCaseShouldWorkWithOneStrongClanGroup()
    {
        var matchBalancer = new MatchBalancingSystem();
        MatchBalancingHelpers.DumpTeams(GameWithVeryStrongClanGroup);
        float unbalancedTeamAMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(GameWithVeryStrongClanGroup.TeamA, 1);
        float unbalancedTeamBMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(GameWithVeryStrongClanGroup.TeamB, 1);
        float unbalancedMeanRatingRatio = unbalancedTeamAMeanRating / unbalancedTeamBMeanRating;
        GameMatch balancedGame = matchBalancer.BannerBalancingWithEdgeCases(GameWithVeryStrongClanGroup);
        float teamASize = balancedGame.TeamA.Count;
        float teamBSize = balancedGame.TeamB.Count;
        float sizeRatio = teamASize / teamBSize;
        float teamARating = RatingHelpers.ComputeTeamRatingPowerSum(balancedGame.TeamA, 1);
        float teamBRating = RatingHelpers.ComputeTeamRatingPowerSum(balancedGame.TeamB, 1);
        float ratingRatio = teamARating / teamBRating;
        MatchBalancingHelpers.DumpTeams(balancedGame);
        Assert.AreEqual(ratingRatio, 1, 0.2);
    }

    [Test]
    public void BannerBalancingWithEdgeCaseShouldNotLoseOrAddCharacters()
    {
        var matchBalancer = new MatchBalancingSystem();

        float unbalancedTeamAMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(Game1.TeamA, 1);
        float unbalancedTeamBMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(Game1.TeamB, 1);
        float unbalancedMeanRatingRatio = unbalancedTeamAMeanRating / unbalancedTeamBMeanRating;
        GameMatch balancedGame = matchBalancer.BannerBalancingWithEdgeCases(Game1);
        List<CrpgUser> allUsersFromBalancedGame = new();
        List<CrpgUser> allUsersFromUnbalancedGame = new();
        allUsersFromBalancedGame.AddRange(balancedGame.TeamA);
        allUsersFromBalancedGame.AddRange(balancedGame.TeamB);
        allUsersFromBalancedGame.AddRange(balancedGame.Waiting);
        allUsersFromUnbalancedGame.AddRange(Game1.TeamA);
        allUsersFromUnbalancedGame.AddRange(Game1.TeamB);
        allUsersFromUnbalancedGame.AddRange(Game1.Waiting);
        CollectionAssert.AreEqual(allUsersFromUnbalancedGame.OrderBy(u => u.Character.Id), allUsersFromBalancedGame.OrderBy(u => u.Character.Id));
    }

    [Test]
    public void BannerBalancingShouldNotSeperateCrpgClanMember()
    {
        var matchBalancer = new MatchBalancingSystem();

        float unbalancedTeamAMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(Game1.TeamA, 1);
        float unbalancedTeamBMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(Game1.TeamB, 1);
        float unbalancedMeanRatingRatio = unbalancedTeamAMeanRating / unbalancedTeamBMeanRating;
        Console.WriteLine($"unbalanced rating ratio = {unbalancedMeanRatingRatio}");

        GameMatch balancedGame = PureBannerBalancing(Game1);
        foreach (CrpgUser u in Game1.TeamA)
        {
            if (u.ClanMembership == null)
            {
                continue;
            }

            foreach (CrpgUser u2 in Game1.TeamB)
            {
                if (u2.ClanMembership != null)
                {
                    Assert.AreNotEqual(u.ClanMembership.ClanId, u2.ClanMembership.ClanId);
                }
            }
        }
    }

    [Test]
    public void BannerBalancingWithEdgeCaseShouldWorkWith0Persons()
    {
        var matchBalancer = new MatchBalancingSystem();

        float unbalancedTeamAMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(NoManGame.TeamA, 1);
        float unbalancedTeamBMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(NoManGame.TeamB, 1);
        float unbalancedMeanRatingRatio = unbalancedTeamAMeanRating / unbalancedTeamBMeanRating;
        GameMatch balancedGame = matchBalancer.BannerBalancingWithEdgeCases(NoManGame);
    }

    [Test]
    public void BannerBalancingWithEdgeCaseShouldWorkWith1Persons()
    {
        var matchBalancer = new MatchBalancingSystem();

        float unbalancedTeamAMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(OneManGame.TeamA, 1);
        float unbalancedTeamBMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(OneManGame.TeamB, 1);
        float unbalancedMeanRatingRatio = unbalancedTeamAMeanRating / unbalancedTeamBMeanRating;
        GameMatch balancedGame = matchBalancer.BannerBalancingWithEdgeCases(OneManGame);
    }

    [Test]
    public void BannerBalancingWithEdgeCaseShouldWorkWith2Persons()
    {
        var matchBalancer = new MatchBalancingSystem();

        float unbalancedTeamAMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(TwoManGame.TeamA, 1);
        float unbalancedTeamBMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(TwoManGame.TeamB, 1);
        float unbalancedMeanRatingRatio = unbalancedTeamAMeanRating / unbalancedTeamBMeanRating;
        GameMatch balancedGame = matchBalancer.BannerBalancingWithEdgeCases(TwoManGame);
    }

    [Test]
    public void BannerBalancingWithEdgeCaseShouldWorkWith3Persons()
    {
        var matchBalancer = new MatchBalancingSystem();

        float unbalancedTeamAMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(ThreeManGame.TeamA, 1);
        float unbalancedTeamBMeanRating = RatingHelpers.ComputeTeamRatingPowerSum(ThreeManGame.TeamB, 1);
        float unbalancedMeanRatingRatio = unbalancedTeamAMeanRating / unbalancedTeamBMeanRating;
        GameMatch balancedGame = matchBalancer.BannerBalancingWithEdgeCases(ThreeManGame);
    }

    [Test]
    public void PowerMeanShouldWork()
    {
        List<float> floats = new()
        {
            0, 0, 5, 5, 10, 10,
        };
        Console.WriteLine(MathHelper.PowerMean(floats, 1f));
        Assert.AreEqual(MathHelper.PowerMean(floats, 1f), 5, 0.01);
    }

    private GameMatch PureBannerBalancing(GameMatch gameMatch)
    {
        var matchBalancer = new MatchBalancingSystem();
        GameMatch unbalancedBannerGameMatch = matchBalancer.KkMakeTeamOfSimilarSizesWithoutSplittingClanGroups(gameMatch);
        unbalancedBannerGameMatch = matchBalancer.BalanceTeamOfSimilarSizes(unbalancedBannerGameMatch, true, 0.025f);
        return unbalancedBannerGameMatch;
    }
}
