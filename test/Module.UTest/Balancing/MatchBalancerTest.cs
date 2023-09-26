using Crpg.Module.Api.Models.Clans;
using Crpg.Module.Api.Models.Users;
using Crpg.Module.Balancing;
using Crpg.Module.Helpers;
using NUnit.Framework;

namespace Crpg.Module.UTest.Balancing;

public class MatchBalancerTest
{
    private static WeightedCrpgUser CreateWeightedUser(int id, string name, float weight, int? clanId = null)
        => new(
            new CrpgUser
            {
                Id = id,
                Name = name,
                ClanMembership = clanId == null ? null : new CrpgClanMember { ClanId = clanId.Value },
            }, weight);

    private static readonly WeightedCrpgUser Aragorn = CreateWeightedUser(0, nameof(Aragorn), -1456, 1);
    private static readonly WeightedCrpgUser Arwen = CreateWeightedUser(1, nameof(Arwen), -2783, 1);
    private static readonly WeightedCrpgUser Frodon = CreateWeightedUser(2, nameof(Frodon), 16235, 1);
    private static readonly WeightedCrpgUser Sam = CreateWeightedUser(3, nameof(Sam), 15545, 1);
    private static readonly WeightedCrpgUser Sangoku = CreateWeightedUser(4, nameof(Sangoku), 2000, 2);
    private static readonly WeightedCrpgUser Krilin = CreateWeightedUser(5, nameof(Krilin), 1000, 2);
    private static readonly WeightedCrpgUser RolandDeschain = CreateWeightedUser(6, nameof(RolandDeschain), 2800, 3);
    private static readonly WeightedCrpgUser HarryPotter = CreateWeightedUser(7, nameof(HarryPotter), 2000, 4);
    private static readonly WeightedCrpgUser Magneto = CreateWeightedUser(8, nameof(Magneto), 2700, 6);
    private static readonly WeightedCrpgUser ProfCharles = CreateWeightedUser(9, nameof(ProfCharles), 2800, 5);
    private static readonly WeightedCrpgUser UsainBolt = CreateWeightedUser(10, nameof(UsainBolt), 1200);
    private static readonly WeightedCrpgUser Agent007 = CreateWeightedUser(11, nameof(Agent007), 1300);
    private static readonly WeightedCrpgUser SpongeBob = CreateWeightedUser(12, nameof(SpongeBob), 800);
    private static readonly WeightedCrpgUser Patrick = CreateWeightedUser(13, nameof(Patrick), 500);
    private static readonly WeightedCrpgUser Madonna = CreateWeightedUser(14, nameof(Madonna), 1100);
    private static readonly WeightedCrpgUser LaraCroft = CreateWeightedUser(15, nameof(LaraCroft), 3500);
    private static readonly WeightedCrpgUser JeanneDArc = CreateWeightedUser(16, nameof(JeanneDArc), 2400);
    private static readonly WeightedCrpgUser Merlin = CreateWeightedUser(17, nameof(Merlin), 2700);
    private static readonly WeightedCrpgUser Bob = CreateWeightedUser(18, nameof(Bob), 1100);
    private static readonly WeightedCrpgUser Thomas = CreateWeightedUser(19, nameof(Thomas), 2400);
    private static readonly WeightedCrpgUser RonWeasley = CreateWeightedUser(20, nameof(RonWeasley), 600, 4);
    private static readonly WeightedCrpgUser Jean01 = CreateWeightedUser(21, nameof(Jean01), 3000, 7);
    private static readonly WeightedCrpgUser Jean02 = CreateWeightedUser(22, nameof(Jean02), 2500, 7);
    private static readonly WeightedCrpgUser Jean03 = CreateWeightedUser(23, nameof(Jean03), 2100, 7);
    private static readonly WeightedCrpgUser Jean04 = CreateWeightedUser(24, nameof(Jean04), 1200, 7);
    private static readonly WeightedCrpgUser Jean05 = CreateWeightedUser(25, nameof(Jean05), 800, 7);
    private static readonly WeightedCrpgUser Glutentag01 = CreateWeightedUser(26, nameof(Glutentag01), 900, 8);
    private static readonly WeightedCrpgUser Glutentag02 = CreateWeightedUser(27, nameof(Glutentag02), 200, 8);
    private static readonly WeightedCrpgUser Glutentag03 = CreateWeightedUser(28, nameof(Glutentag03), 2200, 8);
    private static readonly WeightedCrpgUser Glutentag04 = CreateWeightedUser(29, nameof(Glutentag04), 400, 8);
    private static readonly WeightedCrpgUser Glutentag05 = CreateWeightedUser(30, nameof(Glutentag05), 800, 8);
    private static readonly WeightedCrpgUser Vlexance01 = CreateWeightedUser(31, nameof(Vlexance01), 2600, 9);
    private static readonly WeightedCrpgUser Vlexance02 = CreateWeightedUser(32, nameof(Vlexance02), 2300, 9);
    private static readonly WeightedCrpgUser Vlexance03 = CreateWeightedUser(33, nameof(Vlexance03), 1300, 9);
    private static readonly WeightedCrpgUser Vlexance04 = CreateWeightedUser(34, nameof(Vlexance04), 1100, 9);
    private static readonly WeightedCrpgUser Vlexance05 = CreateWeightedUser(35, nameof(Vlexance05), 300, 9);
    private static readonly WeightedCrpgUser Hudax01 = CreateWeightedUser(36, nameof(Hudax01), 1100, 10);
    private static readonly WeightedCrpgUser Hudax02 = CreateWeightedUser(37, nameof(Hudax02), 2900, 10);
    private static readonly WeightedCrpgUser Hudax03 = CreateWeightedUser(38, nameof(Hudax03), 1700, 10);
    private static readonly WeightedCrpgUser Hudax04 = CreateWeightedUser(39, nameof(Hudax04), 1500, 10);
    private static readonly WeightedCrpgUser Hudax05 = CreateWeightedUser(40, nameof(Hudax05), 2200, 10);
    private static readonly WeightedCrpgUser Hudax06 = CreateWeightedUser(5036, nameof(Hudax06), 1900, 10);
    private static readonly WeightedCrpgUser Hudax07 = CreateWeightedUser(5037, nameof(Hudax07), 8000, 10);
    private static readonly WeightedCrpgUser Hudax08 = CreateWeightedUser(5038, nameof(Hudax08), 1300, 10);
    private static readonly WeightedCrpgUser Hudax09 = CreateWeightedUser(5039, nameof(Hudax09), 1400, 10);
    private static readonly WeightedCrpgUser Hudax10 = CreateWeightedUser(5040, nameof(Hudax10), 700, 10);
    private static readonly WeightedCrpgUser GerryShepherd = CreateWeightedUser(41, nameof(GerryShepherd), 2000, 11);
    private static readonly WeightedCrpgUser BullyDog = CreateWeightedUser(42, nameof(BullyDog), 1600, 11);
    private static readonly WeightedCrpgUser LabbyRetriever = CreateWeightedUser(43, nameof(LabbyRetriever), 1500, 11);
    private static readonly WeightedCrpgUser GoldyRetriever = CreateWeightedUser(44, nameof(GoldyRetriever), 2000, 12);
    private static readonly WeightedCrpgUser SibbyHusky = CreateWeightedUser(45, nameof(SibbyHusky), 1000, 12);
    private static readonly WeightedCrpgUser Poodlums = CreateWeightedUser(46, nameof(Poodlums), 2800, 13);
    private static readonly WeightedCrpgUser BordyCollie = CreateWeightedUser(47, nameof(BordyCollie), 2000, 14);
    private static readonly WeightedCrpgUser Rottyweiler = CreateWeightedUser(48, nameof(Rottyweiler), 2700, 15);
    private static readonly WeightedCrpgUser Daschyhund = CreateWeightedUser(49, nameof(Daschyhund), 2800, 16);
    private static readonly WeightedCrpgUser GreatieDane = CreateWeightedUser(50, nameof(GreatieDane), 1200);
    private static readonly WeightedCrpgUser YorkyTerrier = CreateWeightedUser(51, nameof(YorkyTerrier), 1300);
    private static readonly WeightedCrpgUser CockySpaniel = CreateWeightedUser(52, nameof(CockySpaniel), 800);
    private static readonly WeightedCrpgUser Pomyranian = CreateWeightedUser(53, nameof(Pomyranian), 500);
    private static readonly WeightedCrpgUser Bullymastiff = CreateWeightedUser(54, nameof(Bullymastiff), 1100);
    private static readonly WeightedCrpgUser JackyRussell = CreateWeightedUser(55, nameof(JackyRussell), 3500);
    private static readonly WeightedCrpgUser Akitayinu = CreateWeightedUser(56, nameof(Akitayinu), 2400);
    private static readonly WeightedCrpgUser Maltiepoo = CreateWeightedUser(57, nameof(Maltiepoo), 2700);
    private static readonly WeightedCrpgUser Doberymann = CreateWeightedUser(58, nameof(Doberymann), 1100);
    private static readonly WeightedCrpgUser Sheeiitzu = CreateWeightedUser(59, nameof(Sheeiitzu), 2400);
    private static readonly WeightedCrpgUser BassetyHound = CreateWeightedUser(60, nameof(BassetyHound), 600, 14);
    private static readonly WeightedCrpgUser GopherSnakeWeb = CreateWeightedUser(1000, nameof(GopherSnakeWeb), 819, 58);
    private static readonly WeightedCrpgUser AmbushSword = CreateWeightedUser(1001, nameof(AmbushSword), 2019, 50);
    private static readonly WeightedCrpgUser FencingPacMan = CreateWeightedUser(1002, nameof(FencingPacMan), 738, 53);
    private static readonly WeightedCrpgUser EbonSalient = CreateWeightedUser(1003, nameof(EbonSalient), 1381, 52);
    private static readonly WeightedCrpgUser CannonSnaky = CreateWeightedUser(1004, nameof(CannonSnaky), 2140, 55);
    private static readonly WeightedCrpgUser DarklyWine = CreateWeightedUser(1005, nameof(DarklyWine), 2295, 52);
    private static readonly WeightedCrpgUser BonfireQuillon = CreateWeightedUser(1006, nameof(BonfireQuillon), 2304, 52);
    private static readonly WeightedCrpgUser BunnySlopeStationHouse = CreateWeightedUser(1007, nameof(BunnySlopeStationHouse), 2067, 50);
    private static readonly WeightedCrpgUser BridgeheadRattlesnake = CreateWeightedUser(1008, nameof(BridgeheadRattlesnake), 1809, 54);
    private static readonly WeightedCrpgUser InfernoSunless = CreateWeightedUser(1009, nameof(InfernoSunless), 1765, 56);
    private static readonly WeightedCrpgUser BarricadePrince = CreateWeightedUser(1010, nameof(BarricadePrince), 2150, 52);
    private static readonly WeightedCrpgUser FoulKingdom = CreateWeightedUser(1011, nameof(FoulKingdom), 1718, 51);
    private static readonly WeightedCrpgUser DarknessJoeBlake = CreateWeightedUser(1012, nameof(DarknessJoeBlake), 2499, 57);
    private static readonly WeightedCrpgUser ExtinguisherPommel = CreateWeightedUser(1013, nameof(ExtinguisherPommel), 1791, 60);
    private static readonly WeightedCrpgUser CaliberKingship = CreateWeightedUser(1014, nameof(CaliberKingship), 692, 50);
    private static readonly WeightedCrpgUser FirelightSalvo = CreateWeightedUser(1015, nameof(FirelightSalvo), 2226, 51);
    private static readonly WeightedCrpgUser GarnetMonarch = CreateWeightedUser(1016, nameof(GarnetMonarch), 1075, 51);
    private static readonly WeightedCrpgUser EdgedKatana = CreateWeightedUser(1017, nameof(EdgedKatana), 2335, 60);
    private static readonly WeightedCrpgUser AntichristKnife = CreateWeightedUser(1018, nameof(AntichristKnife), 2743, 50);
    private static readonly WeightedCrpgUser DarkenThrust = CreateWeightedUser(1019, nameof(DarkenThrust), 1084, 52);
    private static readonly WeightedCrpgUser AnaphylacticShockLowering = CreateWeightedUser(1020, nameof(AnaphylacticShockLowering), 1969, 55);
    private static readonly WeightedCrpgUser ApprenticeSpottedAdder = CreateWeightedUser(1021, nameof(ApprenticeSpottedAdder), 2189, 57);
    private static readonly WeightedCrpgUser DrawTreacle = CreateWeightedUser(1022, nameof(DrawTreacle), 2215, 59);
    private static readonly WeightedCrpgUser AglyphousObscure = CreateWeightedUser(1023, nameof(AglyphousObscure), 2381, 52);
    private static readonly WeightedCrpgUser BackfangedWalk = CreateWeightedUser(1024, nameof(BackfangedWalk), 1341, 54);
    private static readonly WeightedCrpgUser ArachnomorphaeScathe = CreateWeightedUser(1025, nameof(ArachnomorphaeScathe), 2160, 55);
    private static readonly WeightedCrpgUser DisenvenomShadowy = CreateWeightedUser(1026, nameof(DisenvenomShadowy), 2330, 54);
    private static readonly WeightedCrpgUser BroadswordKick = CreateWeightedUser(1027, nameof(BroadswordKick), 2978, 53);
    private static readonly WeightedCrpgUser DuskNovelist = CreateWeightedUser(1028, nameof(DuskNovelist), 729, 50);
    private static readonly WeightedCrpgUser PinkPanther = CreateWeightedUser(1029, nameof(PinkPanther), 2854, 56);
    private static readonly WeightedCrpgUser DirkSubfusc = CreateWeightedUser(1030, nameof(DirkSubfusc), 2423, 57);
    private static readonly WeightedCrpgUser FireServiceProbationer = CreateWeightedUser(1031, nameof(FireServiceProbationer), 1800, 60);
    private static readonly WeightedCrpgUser BetrayPrehensor = CreateWeightedUser(1032, nameof(BetrayPrehensor), 2739, 50);
    private static readonly WeightedCrpgUser FlaskTigerSnake = CreateWeightedUser(1033, nameof(FlaskTigerSnake), 1737, 52);
    private static readonly WeightedCrpgUser BeginnerPlatypus = CreateWeightedUser(1034, nameof(BeginnerPlatypus), 2472, 51);
    private static readonly WeightedCrpgUser BushmasterSteel = CreateWeightedUser(1035, nameof(BushmasterSteel), 1930, 58);
    private static readonly WeightedCrpgUser BreechIron = CreateWeightedUser(1036, nameof(BreechIron), 513, 50);
    private static readonly WeightedCrpgUser BarbecueLivid = CreateWeightedUser(1037, nameof(BarbecueLivid), 1065, 59);
    private static readonly WeightedCrpgUser InfantRinkhals = CreateWeightedUser(1038, nameof(InfantRinkhals), 1612, 51);
    private static readonly WeightedCrpgUser AtterStranger = CreateWeightedUser(1039, nameof(AtterStranger), 2987, 60);
    private static readonly WeightedCrpgUser BanditKrait = CreateWeightedUser(1040, nameof(BanditKrait), 2313, 51);
    private static readonly WeightedCrpgUser IntelligenceMatchless = CreateWeightedUser(1041, nameof(IntelligenceMatchless), 2064, 50);
    private static readonly WeightedCrpgUser GrillMuzzle = CreateWeightedUser(1042, nameof(GrillMuzzle), 555, 52);
    private static readonly WeightedCrpgUser BombinateTwo = CreateWeightedUser(1043, nameof(BombinateTwo), 2778, 58);
    private static readonly WeightedCrpgUser GunRapid = CreateWeightedUser(1044, nameof(GunRapid), 1269, 58);
    private static readonly WeightedCrpgUser FlameproofReprisal = CreateWeightedUser(1045, nameof(FlameproofReprisal), 631, 60);
    private static readonly WeightedCrpgUser FullerMoccasin = CreateWeightedUser(1046, nameof(FullerMoccasin), 2547, 51);
    private static readonly WeightedCrpgUser HarassSmokeScreen = CreateWeightedUser(1047, nameof(HarassSmokeScreen), 2266, 55);
    private static readonly WeightedCrpgUser CyanoSax = CreateWeightedUser(1048, nameof(CyanoSax), 1456, 50);
    private static readonly WeightedCrpgUser DarksomeSwivel = CreateWeightedUser(1049, nameof(DarksomeSwivel), 1458, 53);
    private static readonly WeightedCrpgUser CounterspyMamba = CreateWeightedUser(1050, nameof(CounterspyMamba), 1223, 59);
    private static readonly WeightedCrpgUser FirewardRingedWaterSnake = CreateWeightedUser(1051, nameof(FirewardRingedWaterSnake), 2477, 51);
    private static readonly WeightedCrpgUser CombustMurky = CreateWeightedUser(1052, nameof(CombustMurky), 2812, 53);
    private static readonly WeightedCrpgUser AlightRoyal = CreateWeightedUser(1053, nameof(AlightRoyal), 1850, 53);
    private static readonly WeightedCrpgUser HandgunStrafe = CreateWeightedUser(1054, nameof(HandgunStrafe), 1086, 52);
    private static readonly WeightedCrpgUser FraternizeTenebrous = CreateWeightedUser(1055, nameof(FraternizeTenebrous), 1936, 53);
    private static readonly WeightedCrpgUser CounterespionageReconnaissance = CreateWeightedUser(1056, nameof(CounterespionageReconnaissance), 1021, 58);
    private static readonly WeightedCrpgUser HissRabbit = CreateWeightedUser(1057, nameof(HissRabbit), 2537, 57);
    private static readonly WeightedCrpgUser HappyVirulent = CreateWeightedUser(1058, nameof(HappyVirulent), 2478, 60);
    private static readonly WeightedCrpgUser FieryRaspberry = CreateWeightedUser(1059, nameof(FieryRaspberry), 1385, 50);
    private static readonly WeightedCrpgUser DigeratiOpisthoglyphous = CreateWeightedUser(1060, nameof(DigeratiOpisthoglyphous), 2185, 57);
    private static readonly WeightedCrpgUser CongoEelRingSnake = CreateWeightedUser(1061, nameof(CongoEelRingSnake), 2382, 53);
    private static readonly WeightedCrpgUser CountermineMopUp = CreateWeightedUser(1062, nameof(CountermineMopUp), 2511, 55);
    private static readonly WeightedCrpgUser InvadeShoot = CreateWeightedUser(1063, nameof(InvadeShoot), 523, 54);
    private static readonly WeightedCrpgUser HouseSnakePrime = CreateWeightedUser(1064, nameof(HouseSnakePrime), 2579, 52);
    private static readonly WeightedCrpgUser BurnTaupe = CreateWeightedUser(1065, nameof(BurnTaupe), 988, 54);
    private static readonly WeightedCrpgUser CourtNeophytism = CreateWeightedUser(1066, nameof(CourtNeophytism), 2362, 51);
    private static readonly WeightedCrpgUser EaterSerpentine = CreateWeightedUser(1067, nameof(EaterSerpentine), 1872, 55);
    private static readonly WeightedCrpgUser FiresideLimber = CreateWeightedUser(1068, nameof(FiresideLimber), 2486, 59);
    private static readonly WeightedCrpgUser GunslingerMole = CreateWeightedUser(1069, nameof(GunslingerMole), 744, 59);
    private static readonly WeightedCrpgUser FlameVirulence = CreateWeightedUser(1070, nameof(FlameVirulence), 810, 54);
    private static readonly WeightedCrpgUser IgneousTail = CreateWeightedUser(1071, nameof(IgneousTail), 1142, 53);
    private static readonly WeightedCrpgUser GapWalnut = CreateWeightedUser(1072, nameof(GapWalnut), 1023, 51);
    private static readonly WeightedCrpgUser BombardSullen = CreateWeightedUser(1073, nameof(BombardSullen), 2013, 56);
    private static readonly WeightedCrpgUser DaggerShooting = CreateWeightedUser(1074, nameof(DaggerShooting), 639, 57);
    private static readonly WeightedCrpgUser CimmerianPistol = CreateWeightedUser(1075, nameof(CimmerianPistol), 1753, 59);
    private static readonly WeightedCrpgUser BiteNavy = CreateWeightedUser(1076, nameof(BiteNavy), 1845, 52);
    private static readonly WeightedCrpgUser GreenieMelittin = CreateWeightedUser(1077, nameof(GreenieMelittin), 702, 55);
    private static readonly WeightedCrpgUser BlackToxin = CreateWeightedUser(1078, nameof(BlackToxin), 2714, 57);
    private static readonly WeightedCrpgUser GirdWaterMoccasin = CreateWeightedUser(1079, nameof(GirdWaterMoccasin), 1876, 58);
    private static readonly WeightedCrpgUser AirGunKingly = CreateWeightedUser(1080, nameof(AirGunKingly), 1691, 57);
    private static readonly WeightedCrpgUser FireproofSwarthy = CreateWeightedUser(1081, nameof(FireproofSwarthy), 1043, 60);
    private static readonly WeightedCrpgUser GuardSepia = CreateWeightedUser(1082, nameof(GuardSepia), 2588, 60);
    private static readonly WeightedCrpgUser FairPuttotheSword = CreateWeightedUser(1083, nameof(FairPuttotheSword), 1486, 53);
    private static readonly WeightedCrpgUser AbecedarianWaterPistol = CreateWeightedUser(1084, nameof(AbecedarianWaterPistol), 2079, 55);
    private static readonly WeightedCrpgUser EmberSwordplay = CreateWeightedUser(1085, nameof(EmberSwordplay), 1639, 55);
    private static readonly WeightedCrpgUser DuskyScabbard = CreateWeightedUser(1086, nameof(DuskyScabbard), 2837, 55);
    private static readonly WeightedCrpgUser CadetShed = CreateWeightedUser(1087, nameof(CadetShed), 1522, 55);
    private static readonly WeightedCrpgUser BalefireWorm = CreateWeightedUser(1088, nameof(BalefireWorm), 2132, 59);
    private static readonly WeightedCrpgUser EngageSansevieria = CreateWeightedUser(1089, nameof(EngageSansevieria), 1001, 51);
    private static readonly WeightedCrpgUser BrownstoneQuisling = CreateWeightedUser(1090, nameof(BrownstoneQuisling), 2385, 56);
    private static readonly WeightedCrpgUser AlexitericalTaipan = CreateWeightedUser(1091, nameof(AlexitericalTaipan), 720, 58);
    private static readonly WeightedCrpgUser BladeShotgun = CreateWeightedUser(1092, nameof(BladeShotgun), 2797, 51);
    private static readonly WeightedCrpgUser AntiaircraftPunk = CreateWeightedUser(1093, nameof(AntiaircraftPunk), 2236, 55);
    private static readonly WeightedCrpgUser GunfireListeningPost = CreateWeightedUser(1094, nameof(GunfireListeningPost), 2646, 56);
    private static readonly WeightedCrpgUser BuckFeverScowl = CreateWeightedUser(1095, nameof(BuckFeverScowl), 2252, 56);
    private static readonly WeightedCrpgUser ChaseProteroglypha = CreateWeightedUser(1096, nameof(ChaseProteroglypha), 1069, 56);
    private static readonly WeightedCrpgUser FoeYataghan = CreateWeightedUser(1097, nameof(FoeYataghan), 612, 56);
    private static readonly WeightedCrpgUser BrunetteWadding = CreateWeightedUser(1098, nameof(BrunetteWadding), 1019, 52);
    private static readonly WeightedCrpgUser BoomslangYounker = CreateWeightedUser(1099, nameof(BoomslangYounker), 1740, 55);
    private static readonly WeightedCrpgUser BoaScout = CreateWeightedUser(1100, nameof(BoaScout), 1069, 51);
    private static readonly WeightedCrpgUser AlphabetarianSerum = CreateWeightedUser(1101, nameof(AlphabetarianSerum), 837, 52);
    private static readonly WeightedCrpgUser EmpoisonSnake = CreateWeightedUser(1102, nameof(EmpoisonSnake), 1721, 57);
    private static readonly WeightedCrpgUser InflammablePuffAdder = CreateWeightedUser(1103, nameof(InflammablePuffAdder), 1292, 50);
    private static readonly WeightedCrpgUser BullfightTiro = CreateWeightedUser(1104, nameof(BullfightTiro), 579, 60);
    private static readonly WeightedCrpgUser FirearmKeelback = CreateWeightedUser(1105, nameof(FirearmKeelback), 2183, 56);
    private static readonly WeightedCrpgUser FiringPumpernickel = CreateWeightedUser(1106, nameof(FiringPumpernickel), 1124, 58);
    private static readonly WeightedCrpgUser InimicalVennation = CreateWeightedUser(1107, nameof(InimicalVennation), 2878, 53);
    private static readonly WeightedCrpgUser ConeShellPiece = CreateWeightedUser(1108, nameof(ConeShellPiece), 1220, 50);
    private static readonly WeightedCrpgUser InitiateMarsala = CreateWeightedUser(1109, nameof(InitiateMarsala), 2767, 59);
    private static readonly WeightedCrpgUser BulletRacer = CreateWeightedUser(1110, nameof(BulletRacer), 2957, 60);
    private static readonly WeightedCrpgUser EggplantRifle = CreateWeightedUser(1111, nameof(EggplantRifle), 930, 51);
    private static readonly WeightedCrpgUser EbonyQueen = CreateWeightedUser(1112, nameof(EbonyQueen), 1050, 52);
    private static readonly WeightedCrpgUser InflameMorglay = CreateWeightedUser(1113, nameof(InflameMorglay), 1846, 53);
    private static readonly WeightedCrpgUser ComeUnlimber = CreateWeightedUser(1114, nameof(ComeUnlimber), 1467, 54);
    private static readonly WeightedCrpgUser FighterRange = CreateWeightedUser(1115, nameof(FighterRange), 1061, 53);
    private static readonly WeightedCrpgUser CottonmouthOxblood = CreateWeightedUser(1116, nameof(CottonmouthOxblood), 2781, 55);
    private static readonly WeightedCrpgUser FifthColumnParry = CreateWeightedUser(1117, nameof(FifthColumnParry), 2384, 51);
    private static readonly WeightedCrpgUser CarbuncleParley = CreateWeightedUser(1118, nameof(CarbuncleParley), 1220, 56);
    private static readonly WeightedCrpgUser FoibleUnfriend = CreateWeightedUser(1119, nameof(FoibleUnfriend), 1287, 57);
    private static readonly WeightedCrpgUser DamascusSteelProfession = CreateWeightedUser(1120, nameof(DamascusSteelProfession), 1895, 57);
    private static readonly WeightedCrpgUser AntimissileSap = CreateWeightedUser(1121, nameof(AntimissileSap), 1022, 50);
    private static readonly WeightedCrpgUser FloretTityus = CreateWeightedUser(1122, nameof(FloretTityus), 1596, 54);
    private static readonly WeightedCrpgUser CoachwhipRapier = CreateWeightedUser(1123, nameof(CoachwhipRapier), 1102, 50);
    private static readonly WeightedCrpgUser BootySubmachineGun = CreateWeightedUser(1124, nameof(BootySubmachineGun), 2262, 52);
    private static readonly WeightedCrpgUser DamoclesProteroglyphous = CreateWeightedUser(1125, nameof(DamoclesProteroglyphous), 2610, 56);
    private static readonly WeightedCrpgUser CannonadeStrip = CreateWeightedUser(1126, nameof(CannonadeStrip), 1511, 50);
    private static readonly WeightedCrpgUser FlammableWildfire = CreateWeightedUser(1127, nameof(FlammableWildfire), 2633, 50);
    private static readonly WeightedCrpgUser AlexipharmicJohnny = CreateWeightedUser(1128, nameof(AlexipharmicJohnny), 2358, 59);
    private static readonly WeightedCrpgUser DischargeProteroglyph = CreateWeightedUser(1129, nameof(DischargeProteroglyph), 2145, 54);
    private static readonly WeightedCrpgUser InfiltrateKindling = CreateWeightedUser(1130, nameof(InfiltrateKindling), 1323, 54);
    private static readonly WeightedCrpgUser BilboRhasophore = CreateWeightedUser(1131, nameof(BilboRhasophore), 984, 60);
    private static readonly WeightedCrpgUser ChamberOutvenom = CreateWeightedUser(1132, nameof(ChamberOutvenom), 892, 56);
    private static readonly WeightedCrpgUser GunmanSlash = CreateWeightedUser(1133, nameof(GunmanSlash), 678, 53);
    private static readonly WeightedCrpgUser AblazeRayGun = CreateWeightedUser(1134, nameof(AblazeRayGun), 540, 60);
    private static readonly WeightedCrpgUser ContagionMalihini = CreateWeightedUser(1135, nameof(ContagionMalihini), 1520, 52);
    private static readonly WeightedCrpgUser FangNavyBlue = CreateWeightedUser(1136, nameof(FangNavyBlue), 833, 56);
    private static readonly WeightedCrpgUser ChocolateSombre = CreateWeightedUser(1137, nameof(ChocolateSombre), 2840, 52);
    private static readonly WeightedCrpgUser EnvenomationSheathe = CreateWeightedUser(1138, nameof(EnvenomationSheathe), 2312, 58);
    private static readonly WeightedCrpgUser AflameReign = CreateWeightedUser(1139, nameof(AflameReign), 2654, 60);
    private static readonly WeightedCrpgUser AglyphTang = CreateWeightedUser(1140, nameof(AglyphTang), 1677, 56);
    private static readonly WeightedCrpgUser AlexitericMachineGun = CreateWeightedUser(1141, nameof(AlexitericMachineGun), 826, 50);
    private static readonly WeightedCrpgUser ForteTheriac = CreateWeightedUser(1142, nameof(ForteTheriac), 706, 57);
    private static readonly WeightedCrpgUser FlagofTruceNaked = CreateWeightedUser(1143, nameof(FlagofTruceNaked), 1609, 50);
    private static readonly WeightedCrpgUser HydraRough = CreateWeightedUser(1144, nameof(HydraRough), 2991, 51);
    private static readonly WeightedCrpgUser BaldricOphi = CreateWeightedUser(1145, nameof(BaldricOphi), 609, 54);
    private static readonly WeightedCrpgUser HangerMapepire = CreateWeightedUser(1146, nameof(HangerMapepire), 1869, 51);
    private static readonly WeightedCrpgUser BlankSpittingSnake = CreateWeightedUser(1147, nameof(BlankSpittingSnake), 2391, 54);
    private static readonly WeightedCrpgUser CounteroffensiveShutterbug = CreateWeightedUser(1148, nameof(CounteroffensiveShutterbug), 637, 56);
    private static readonly WeightedCrpgUser GlaiveRuby = CreateWeightedUser(1149, nameof(GlaiveRuby), 1795, 50);
    private static readonly WeightedCrpgUser EelTenderfoot = CreateWeightedUser(1150, nameof(EelTenderfoot), 2384, 58);
    private static readonly WeightedCrpgUser CoffeeSalamander = CreateWeightedUser(1151, nameof(CoffeeSalamander), 1604, 55);
    private static readonly WeightedCrpgUser CastleShadow = CreateWeightedUser(1152, nameof(CastleShadow), 1230, 52);
    private static readonly WeightedCrpgUser AnguineMaroon = CreateWeightedUser(1153, nameof(AnguineMaroon), 2287, 54);
    private static readonly WeightedCrpgUser GopherLubber = CreateWeightedUser(1154, nameof(GopherLubber), 2166, 52);
    private static readonly WeightedCrpgUser FrontfangedScalp = CreateWeightedUser(1155, nameof(FrontfangedScalp), 1969, 53);
    private static readonly WeightedCrpgUser FrontXiphoid = CreateWeightedUser(1156, nameof(FrontXiphoid), 1973, 55);
    private static readonly WeightedCrpgUser BurntRinghals = CreateWeightedUser(1157, nameof(BurntRinghals), 1243, 59);
    private static readonly WeightedCrpgUser FireTruckRegal = CreateWeightedUser(1158, nameof(FireTruckRegal), 1518, 55);
    private static readonly WeightedCrpgUser ArchenemySidearm = CreateWeightedUser(1159, nameof(ArchenemySidearm), 599, 54);
    private static readonly WeightedCrpgUser CarryLandlubber = CreateWeightedUser(1160, nameof(CarryLandlubber), 2970, 58);
    private static readonly WeightedCrpgUser BlacksnakeToledo = CreateWeightedUser(1161, nameof(BlacksnakeToledo), 1690, 54);
    private static readonly WeightedCrpgUser ExcaliburPyrolatry = CreateWeightedUser(1162, nameof(ExcaliburPyrolatry), 1279, 58);
    private static readonly WeightedCrpgUser CounterintelligenceKinglet = CreateWeightedUser(1163, nameof(CounterintelligenceKinglet), 2365, 51);
    private static readonly WeightedCrpgUser IceMiss = CreateWeightedUser(1164, nameof(IceMiss), 1283, 50);
    private static readonly WeightedCrpgUser BearerPitch = CreateWeightedUser(1165, nameof(BearerPitch), 896, 53);
    private static readonly WeightedCrpgUser BackswordSerpent = CreateWeightedUser(1166, nameof(BackswordSerpent), 1537, 57);
    private static readonly WeightedCrpgUser HornedViperMusket = CreateWeightedUser(1167, nameof(HornedViperMusket), 2288, 55);
    private static readonly WeightedCrpgUser FoxholePummel = CreateWeightedUser(1168, nameof(FoxholePummel), 887, 60);
    private static readonly WeightedCrpgUser DunRamrod = CreateWeightedUser(1169, nameof(DunRamrod), 1296, 51);
    private static readonly WeightedCrpgUser ClipNeophyte = CreateWeightedUser(1170, nameof(ClipNeophyte), 1907, 60);
    private static readonly WeightedCrpgUser InternshipPilot = CreateWeightedUser(1171, nameof(InternshipPilot), 1423, 50);
    private static readonly WeightedCrpgUser FoxSnakeMocha = CreateWeightedUser(1172, nameof(FoxSnakeMocha), 1588, 51);
    private static readonly WeightedCrpgUser BungarotoxinSnakeskin = CreateWeightedUser(1173, nameof(BungarotoxinSnakeskin), 2260, 51);
    private static readonly WeightedCrpgUser FloatTrail = CreateWeightedUser(1174, nameof(FloatTrail), 1478, 58);
    private static readonly WeightedCrpgUser FalchionPoker = CreateWeightedUser(1175, nameof(FalchionPoker), 2138, 51);
    private static readonly WeightedCrpgUser BbGunScute = CreateWeightedUser(1176, nameof(BbGunScute), 2266, 54);
    private static readonly WeightedCrpgUser HognosedViper = CreateWeightedUser(1177, nameof(HognosedViper), 2242, 60);
    private static readonly WeightedCrpgUser ThompsonSubmachineGun = CreateWeightedUser(1178, nameof(ThompsonSubmachineGun), 1534, 52);
    private static readonly WeightedCrpgUser FoemanRegicide = CreateWeightedUser(1179, nameof(FoemanRegicide), 1104, 57);
    private static readonly WeightedCrpgUser AdversaryStoke = CreateWeightedUser(1180, nameof(AdversaryStoke), 2027, 60);
    private static readonly WeightedCrpgUser EnsiformOpisthoglyph = CreateWeightedUser(1181, nameof(EnsiformOpisthoglyph), 1273, 54);
    private static readonly WeightedCrpgUser FoxReptile = CreateWeightedUser(1182, nameof(FoxReptile), 574, 56);
    private static readonly WeightedCrpgUser BottleGreenVictory = CreateWeightedUser(1183, nameof(BottleGreenVictory), 1149, 51);
    private static readonly WeightedCrpgUser GreenhornTwist = CreateWeightedUser(1184, nameof(GreenhornTwist), 1278, 50);
    private static readonly WeightedCrpgUser BaselardScimitar = CreateWeightedUser(1185, nameof(BaselardScimitar), 2868, 56);
    private static readonly WeightedCrpgUser CobraLunge = CreateWeightedUser(1186, nameof(CobraLunge), 2748, 51);
    private static readonly WeightedCrpgUser AubergineSurly = CreateWeightedUser(1187, nameof(AubergineSurly), 1283, 57);
    private static readonly WeightedCrpgUser FirelessUnfledged = CreateWeightedUser(1188, nameof(FirelessUnfledged), 1141, 59);
    private static readonly WeightedCrpgUser CurtanaRoyalty = CreateWeightedUser(1189, nameof(CurtanaRoyalty), 2297, 51);
    private static readonly WeightedCrpgUser FerSally = CreateWeightedUser(1190, nameof(FerSally), 1408, 55);
    private static readonly WeightedCrpgUser GarterSnakeLately = CreateWeightedUser(1191, nameof(GarterSnakeLately), 816, 52);
    private static readonly WeightedCrpgUser CalibrateJillaroo = CreateWeightedUser(1192, nameof(CalibrateJillaroo), 1800, 51);
    private static readonly WeightedCrpgUser CollaborateLance = CreateWeightedUser(1193, nameof(CollaborateLance), 1634, 51);
    private static readonly WeightedCrpgUser ArrowrootOphidian = CreateWeightedUser(1194, nameof(ArrowrootOphidian), 2924, 57);
    private static readonly WeightedCrpgUser HamadryadTarantula = CreateWeightedUser(1195, nameof(HamadryadTarantula), 1455, 50);
    private static readonly WeightedCrpgUser AdderMisfire = CreateWeightedUser(1196, nameof(AdderMisfire), 2734, 55);
    private static readonly WeightedCrpgUser IrisTsuba = CreateWeightedUser(1197, nameof(IrisTsuba), 2552, 51);
    private static readonly WeightedCrpgUser AirgunStonefish = CreateWeightedUser(1198, nameof(AirgunStonefish), 2460, 53);
    private static readonly WeightedCrpgUser HepaticMustard = CreateWeightedUser(1199, nameof(HepaticMustard), 2104, 53);
    private static readonly WeightedCrpgUser CombatPrefire = CreateWeightedUser(1200, nameof(CombatPrefire), 1030);
    private static readonly WeightedCrpgUser HolsterSwordsmanship = CreateWeightedUser(1201, nameof(HolsterSwordsmanship), 1576);
    private static readonly WeightedCrpgUser EscolarSpittingCobra = CreateWeightedUser(1202, nameof(EscolarSpittingCobra), 2246);
    private static readonly WeightedCrpgUser FiretrapMelano = CreateWeightedUser(1203, nameof(FiretrapMelano), 2741);
    private static readonly WeightedCrpgUser CheckVinous = CreateWeightedUser(1204, nameof(CheckVinous), 752);
    private static readonly WeightedCrpgUser BeachheadLeaden = CreateWeightedUser(1205, nameof(BeachheadLeaden), 1594);
    private static readonly WeightedCrpgUser ComputerPhobiaNightAdder = CreateWeightedUser(1206, nameof(ComputerPhobiaNightAdder), 690);
    private static readonly WeightedCrpgUser BothropsMusketry = CreateWeightedUser(1207, nameof(BothropsMusketry), 2419);
    private static readonly WeightedCrpgUser AntagonistLodgment = CreateWeightedUser(1208, nameof(AntagonistLodgment), 1900);
    private static readonly WeightedCrpgUser CorposantWhinyard = CreateWeightedUser(1209, nameof(CorposantWhinyard), 1707);
    private static readonly WeightedCrpgUser BlackoutMurk = CreateWeightedUser(1210, nameof(BlackoutMurk), 2113);
    private static readonly WeightedCrpgUser ChassisPrivateer = CreateWeightedUser(1211, nameof(ChassisPrivateer), 2613);
    private static readonly WeightedCrpgUser DeadlySheath = CreateWeightedUser(1212, nameof(DeadlySheath), 2170);
    private static readonly WeightedCrpgUser FightSight = CreateWeightedUser(1213, nameof(FightSight), 1646);
    private static readonly WeightedCrpgUser FirehousePuny = CreateWeightedUser(1214, nameof(FirehousePuny), 1198);
    private static readonly WeightedCrpgUser BlindSnakeUnsheathe = CreateWeightedUser(1215, nameof(BlindSnakeUnsheathe), 2332);
    private static readonly WeightedCrpgUser DeMachine = CreateWeightedUser(1216, nameof(DeMachine), 913);
    private static readonly WeightedCrpgUser FoilRecoil = CreateWeightedUser(1217, nameof(FoilRecoil), 1480);
    private static readonly WeightedCrpgUser EnvenomateMatachin = CreateWeightedUser(1218, nameof(EnvenomateMatachin), 632);
    private static readonly WeightedCrpgUser CannonryStoker = CreateWeightedUser(1219, nameof(CannonryStoker), 1146);
    private static readonly WeightedCrpgUser CarpetSnakeSaber = CreateWeightedUser(1220, nameof(CarpetSnakeSaber), 1166);
    private static readonly WeightedCrpgUser DubMudSnake = CreateWeightedUser(1221, nameof(DubMudSnake), 2726);
    private static readonly WeightedCrpgUser ChelaOverkill = CreateWeightedUser(1222, nameof(ChelaOverkill), 2915);
    private static readonly WeightedCrpgUser FireplugNoviceship = CreateWeightedUser(1223, nameof(FireplugNoviceship), 702);
    private static readonly WeightedCrpgUser CanVirus = CreateWeightedUser(1224, nameof(CanVirus), 2865);
    private static readonly WeightedCrpgUser BuckwheaterVenin = CreateWeightedUser(1225, nameof(BuckwheaterVenin), 1908);
    private static readonly WeightedCrpgUser AceSwordless = CreateWeightedUser(1226, nameof(AceSwordless), 919);
    private static readonly WeightedCrpgUser AllongePartisan = CreateWeightedUser(1227, nameof(AllongePartisan), 2804);
    private static readonly WeightedCrpgUser CampfireNewChum = CreateWeightedUser(1228, nameof(CampfireNewChum), 826);
    private static readonly WeightedCrpgUser CrotoxinMulberry = CreateWeightedUser(1229, nameof(CrotoxinMulberry), 1273);
    private static readonly WeightedCrpgUser DerisionStygian = CreateWeightedUser(1230, nameof(DerisionStygian), 1008);
    private static readonly WeightedCrpgUser DarklingTyro = CreateWeightedUser(1231, nameof(DarklingTyro), 1130);
    private static readonly WeightedCrpgUser GrassSnakeRekindle = CreateWeightedUser(1232, nameof(GrassSnakeRekindle), 1275);
    private static readonly WeightedCrpgUser AntagonizePitchy = CreateWeightedUser(1233, nameof(AntagonizePitchy), 2149);
    private static readonly WeightedCrpgUser EmplacementOpisthoglypha = CreateWeightedUser(1234, nameof(EmplacementOpisthoglypha), 2782);
    private static readonly WeightedCrpgUser GunshotSomber = CreateWeightedUser(1235, nameof(GunshotSomber), 1052);
    private static readonly WeightedCrpgUser BrandSequester = CreateWeightedUser(1236, nameof(BrandSequester), 1556);
    private static readonly WeightedCrpgUser ConflagrationPlat = CreateWeightedUser(1237, nameof(ConflagrationPlat), 503);
    private static readonly WeightedCrpgUser GunnerPitchdark = CreateWeightedUser(1238, nameof(GunnerPitchdark), 1514);
    private static readonly WeightedCrpgUser FlareSlate = CreateWeightedUser(1239, nameof(FlareSlate), 2592);
    private static readonly WeightedCrpgUser AcinacesVictor = CreateWeightedUser(1240, nameof(AcinacesVictor), 1349);
    private static readonly WeightedCrpgUser InkyStickUp = CreateWeightedUser(1241, nameof(InkyStickUp), 2306);
    private static readonly WeightedCrpgUser FriendSponson = CreateWeightedUser(1242, nameof(FriendSponson), 790);
    private static readonly WeightedCrpgUser AnguiformLethal = CreateWeightedUser(1243, nameof(AnguiformLethal), 1280);
    private static readonly WeightedCrpgUser AttackSovereign = CreateWeightedUser(1244, nameof(AttackSovereign), 1514);
    private static readonly WeightedCrpgUser GloomyRookie = CreateWeightedUser(1245, nameof(GloomyRookie), 1233);
    private static readonly WeightedCrpgUser AckSwordCane = CreateWeightedUser(1246, nameof(AckSwordCane), 2727);
    private static readonly WeightedCrpgUser ConsumeLower = CreateWeightedUser(1247, nameof(ConsumeLower), 698);
    private static readonly WeightedCrpgUser ApitherapyUmber = CreateWeightedUser(1248, nameof(ApitherapyUmber), 864);
    private static readonly WeightedCrpgUser BurningKindle = CreateWeightedUser(1249, nameof(BurningKindle), 1741);
    private static readonly WeightedCrpgUser FlagTrigger = CreateWeightedUser(1250, nameof(FlagTrigger), 2633);
    private static readonly WeightedCrpgUser InvasionMatador = CreateWeightedUser(1251, nameof(InvasionMatador), 1582);
    private static readonly WeightedCrpgUser AntigunMilkSnake = CreateWeightedUser(1252, nameof(AntigunMilkSnake), 1843);
    private static readonly WeightedCrpgUser ConstrictorWeapon = CreateWeightedUser(1253, nameof(ConstrictorWeapon), 2141);
    private static readonly WeightedCrpgUser GloomSpike = CreateWeightedUser(1254, nameof(GloomSpike), 1837);
    private static readonly WeightedCrpgUser EyedPython = CreateWeightedUser(1255, nameof(EyedPython), 2393);
    private static readonly WeightedCrpgUser IncendiarySlug = CreateWeightedUser(1256, nameof(IncendiarySlug), 1070);
    private static readonly WeightedCrpgUser CrownKingsnake = CreateWeightedUser(1257, nameof(CrownKingsnake), 2725);
    private static readonly WeightedCrpgUser BlackDuckMine = CreateWeightedUser(1258, nameof(BlackDuckMine), 1435);
    private static readonly WeightedCrpgUser FenceVenom = CreateWeightedUser(1259, nameof(FenceVenom), 2088);
    private static readonly WeightedCrpgUser FireNovitiate = CreateWeightedUser(1260, nameof(FireNovitiate), 1142);
    private static readonly WeightedCrpgUser FrogYoungling = CreateWeightedUser(1261, nameof(FrogYoungling), 2885);
    private static readonly WeightedCrpgUser IngleMachinePistol = CreateWeightedUser(1262, nameof(IngleMachinePistol), 2553);
    private static readonly WeightedCrpgUser BlunderbussTeal = CreateWeightedUser(1263, nameof(BlunderbussTeal), 2716);
    private static readonly WeightedCrpgUser CopperheadStratagem = CreateWeightedUser(1264, nameof(CopperheadStratagem), 914);
    private static readonly WeightedCrpgUser CubSerpentiform = CreateWeightedUser(1265, nameof(CubSerpentiform), 1261);
    private static readonly WeightedCrpgUser DragonRingedSnake = CreateWeightedUser(1266, nameof(DragonRingedSnake), 2928);
    private static readonly WeightedCrpgUser AmbuscadePop = CreateWeightedUser(1267, nameof(AmbuscadePop), 2102);
    private static readonly WeightedCrpgUser HaftStout = CreateWeightedUser(1268, nameof(HaftStout), 1133);
    private static readonly WeightedCrpgUser FangedOilfish = CreateWeightedUser(1269, nameof(FangedOilfish), 2176);
    private static readonly WeightedCrpgUser FreshmanSlither = CreateWeightedUser(1270, nameof(FreshmanSlither), 2107);
    private static readonly WeightedCrpgUser InnovativeSilencer = CreateWeightedUser(1271, nameof(InnovativeSilencer), 898);
    private static readonly WeightedCrpgUser AugerShot = CreateWeightedUser(1272, nameof(AugerShot), 1088);
    private static readonly WeightedCrpgUser CollaborationNewbie = CreateWeightedUser(1273, nameof(CollaborationNewbie), 977);
    private static readonly WeightedCrpgUser GladiolusIsToast = CreateWeightedUser(1274, nameof(GladiolusIsToast), 2984);
    private static readonly WeightedCrpgUser DingyTuck = CreateWeightedUser(1275, nameof(DingyTuck), 2851);
    private static readonly WeightedCrpgUser ArchariosSharpshooter = CreateWeightedUser(1276, nameof(ArchariosSharpshooter), 2362);
    private static readonly WeightedCrpgUser DarkQuadrate = CreateWeightedUser(1277, nameof(DarkQuadrate), 1444);
    private static readonly WeightedCrpgUser DungeonRam = CreateWeightedUser(1278, nameof(DungeonRam), 678);
    private static readonly WeightedCrpgUser BlazeLight = CreateWeightedUser(1279, nameof(BlazeLight), 1449);
    private static readonly WeightedCrpgUser AutomaticSwordfish = CreateWeightedUser(1280, nameof(AutomaticSwordfish), 1252);
    private static readonly WeightedCrpgUser EmpyrosisSad = CreateWeightedUser(1281, nameof(EmpyrosisSad), 2620);
    private static readonly WeightedCrpgUser IgnitePlum = CreateWeightedUser(1282, nameof(IgnitePlum), 2814);
    private static readonly WeightedCrpgUser Firebomb = CreateWeightedUser(1283, nameof(Firebomb), 501);
    private static readonly WeightedCrpgUser RattlesnakeRoot = CreateWeightedUser(1284, nameof(RattlesnakeRoot), 2437);
    private static readonly WeightedCrpgUser BackViper = CreateWeightedUser(1285, nameof(BackViper), 2994);
    private static readonly WeightedCrpgUser FlintlockSabotage = CreateWeightedUser(1286, nameof(FlintlockSabotage), 2694);
    private static readonly WeightedCrpgUser AspVenomous = CreateWeightedUser(1287, nameof(AspVenomous), 1966);
    private static readonly WeightedCrpgUser GriffinShooter = CreateWeightedUser(1288, nameof(GriffinShooter), 2282);
    private static readonly WeightedCrpgUser BlackenMagazine = CreateWeightedUser(1289, nameof(BlackenMagazine), 2335);
    private static readonly WeightedCrpgUser BeltShell = CreateWeightedUser(1290, nameof(BeltShell), 819);
    private static readonly WeightedCrpgUser GunpointMate = CreateWeightedUser(1291, nameof(GunpointMate), 1938);
    private static readonly WeightedCrpgUser CastUp = CreateWeightedUser(1292, nameof(CastUp), 2953);
    private static readonly WeightedCrpgUser ClockXiphophyllous = CreateWeightedUser(1293, nameof(ClockXiphophyllous), 2363);
    private static readonly WeightedCrpgUser FiredrakeRefire = CreateWeightedUser(1294, nameof(FiredrakeRefire), 1244);
    private static readonly WeightedCrpgUser BoreSnakebite = CreateWeightedUser(1295, nameof(BoreSnakebite), 541);
    private static readonly WeightedCrpgUser CarbylamineNeurotropic = CreateWeightedUser(1296, nameof(CarbylamineNeurotropic), 2358);
    private static readonly WeightedCrpgUser ChapeMalice = CreateWeightedUser(1297, nameof(ChapeMalice), 2521);
    private static readonly WeightedCrpgUser HoldUpRedbackSpider = CreateWeightedUser(1298, nameof(HoldUpRedbackSpider), 1579);
    private static readonly WeightedCrpgUser AntiveninTraverse = CreateWeightedUser(1299, nameof(AntiveninTraverse), 2973);
    private static readonly WeightedCrpgUser DeepSword = CreateWeightedUser(1300, nameof(DeepSword), 2405);
    private static readonly WeightedCrpgUser GunstockZombie = CreateWeightedUser(1301, nameof(GunstockZombie), 2507);
    private static readonly WeightedCrpgUser BoaConstrictorRifling = CreateWeightedUser(1302, nameof(BoaConstrictorRifling), 768);
    private static readonly WeightedCrpgUser ColubrineMilk = CreateWeightedUser(1303, nameof(ColubrineMilk), 2523);
    private static readonly WeightedCrpgUser EnkindleReload = CreateWeightedUser(1304, nameof(EnkindleReload), 2265);
    private static readonly WeightedCrpgUser FirepowerQuarter = CreateWeightedUser(1305, nameof(FirepowerQuarter), 2271);
    private static readonly WeightedCrpgUser ForaySmother = CreateWeightedUser(1306, nameof(ForaySmother), 685);
    private static readonly WeightedCrpgUser ChargeKing = CreateWeightedUser(1307, nameof(ChargeKing), 1606);
    private static readonly WeightedCrpgUser ClaymoreLow = CreateWeightedUser(1308, nameof(ClaymoreLow), 2833);
    private static readonly WeightedCrpgUser ColubridRex = CreateWeightedUser(1309, nameof(ColubridRex), 2814);
    private static readonly WeightedCrpgUser ImmolateMarksman = CreateWeightedUser(1310, nameof(ImmolateMarksman), 1618);
    private static readonly WeightedCrpgUser HellfirePopgun = CreateWeightedUser(1311, nameof(HellfirePopgun), 1366);
    private static readonly WeightedCrpgUser HostileMadtom = CreateWeightedUser(1312, nameof(HostileMadtom), 1042);
    private static readonly WeightedCrpgUser BlackamoorSable = CreateWeightedUser(1313, nameof(BlackamoorSable), 1426);
    private static readonly WeightedCrpgUser FlakWaster = CreateWeightedUser(1314, nameof(FlakWaster), 2620);
    private static readonly WeightedCrpgUser CoverVenomosalivary = CreateWeightedUser(1315, nameof(CoverVenomosalivary), 1268);
    private static readonly WeightedCrpgUser AccoladeTrain = CreateWeightedUser(1316, nameof(AccoladeTrain), 2860);
    private static readonly WeightedCrpgUser BackfireLine = CreateWeightedUser(1317, nameof(BackfireLine), 2815);
    private static readonly WeightedCrpgUser ColtRetreat = CreateWeightedUser(1318, nameof(ColtRetreat), 1579);
    private static readonly WeightedCrpgUser HolocaustShah = CreateWeightedUser(1319, nameof(HolocaustShah), 2648);
    private static readonly WeightedCrpgUser EnvenomOvercast = CreateWeightedUser(1320, nameof(EnvenomOvercast), 2482);
    private static readonly WeightedCrpgUser InterceptorPyromancy = CreateWeightedUser(1321, nameof(InterceptorPyromancy), 1170);
    private static readonly WeightedCrpgUser CutlassSwordsman = CreateWeightedUser(1322, nameof(CutlassSwordsman), 2727);
    private static readonly WeightedCrpgUser CollaboratorSwordKnot = CreateWeightedUser(1323, nameof(CollaboratorSwordKnot), 1222);
    private static readonly WeightedCrpgUser ClaretPicket = CreateWeightedUser(1324, nameof(ClaretPicket), 1978);
    private static readonly WeightedCrpgUser CatchStarter = CreateWeightedUser(1325, nameof(CatchStarter), 2531);
    private static readonly WeightedCrpgUser FireballSabre = CreateWeightedUser(1326, nameof(FireballSabre), 1449);
    private static readonly WeightedCrpgUser GrapeSurrender = CreateWeightedUser(1327, nameof(GrapeSurrender), 2972);
    private static readonly WeightedCrpgUser AnacondaTommyGun = CreateWeightedUser(1328, nameof(AnacondaTommyGun), 2268);
    private static readonly WeightedCrpgUser CheckmateThundercloud = CreateWeightedUser(1329, nameof(CheckmateThundercloud), 570);
    private static readonly WeightedCrpgUser HereMatchlock = CreateWeightedUser(1330, nameof(HereMatchlock), 1099);
    private static readonly WeightedCrpgUser AbatisPilotSnake = CreateWeightedUser(1331, nameof(AbatisPilotSnake), 1161);
    private static readonly WeightedCrpgUser BarrelSting = CreateWeightedUser(1332, nameof(BarrelSting), 2809);
    private static readonly WeightedCrpgUser CombustibleNight = CreateWeightedUser(1333, nameof(CombustibleNight), 2443);
    private static readonly WeightedCrpgUser CommandoLock = CreateWeightedUser(1334, nameof(CommandoLock), 1266);
    private static readonly WeightedCrpgUser BeestingTrench = CreateWeightedUser(1335, nameof(BeestingTrench), 630);
    private static readonly WeightedCrpgUser AfireSlough = CreateWeightedUser(1336, nameof(AfireSlough), 1619);
    private static readonly WeightedCrpgUser CoralSnakePyro = CreateWeightedUser(1337, nameof(CoralSnakePyro), 615);
    private static readonly WeightedCrpgUser BloodPhosphodiesterase = CreateWeightedUser(1338, nameof(BloodPhosphodiesterase), 695);
    private static readonly WeightedCrpgUser HiltMurrey = CreateWeightedUser(1339, nameof(HiltMurrey), 1174);
    private static readonly WeightedCrpgUser CharcoalPrisonerofWar = CreateWeightedUser(1340, nameof(CharcoalPrisonerofWar), 1696);
    private static readonly WeightedCrpgUser GunmetalSwelter = CreateWeightedUser(1341, nameof(GunmetalSwelter), 2797);
    private static readonly WeightedCrpgUser CaliginousPuce = CreateWeightedUser(1342, nameof(CaliginousPuce), 825);
    private static readonly WeightedCrpgUser BrownSloe = CreateWeightedUser(1343, nameof(BrownSloe), 1748);
    private static readonly WeightedCrpgUser FiredFishQuench = CreateWeightedUser(1344, nameof(FiredFishQuench), 558);
    private static readonly WeightedCrpgUser CaperLynx = CreateWeightedUser(1345, nameof(CaperLynx), 770);
    private static readonly WeightedCrpgUser TastyCalyx = CreateWeightedUser(1346, nameof(TastyCalyx), 545);
    private static readonly WeightedCrpgUser SiameseLavender = CreateWeightedUser(1347, nameof(SiameseLavender), 1912);
    private static readonly WeightedCrpgUser BeauChichi = CreateWeightedUser(1348, nameof(BeauChichi), 2004);
    private static readonly WeightedCrpgUser DogPanther = CreateWeightedUser(1349, nameof(DogPanther), 2210);
    private static readonly WeightedCrpgUser BlossomJelly = CreateWeightedUser(1350, nameof(BlossomJelly), 2133);
    private static readonly WeightedCrpgUser SharpPapergirl = CreateWeightedUser(1351, nameof(SharpPapergirl), 2547);
    private static readonly WeightedCrpgUser MoppetTear = CreateWeightedUser(1352, nameof(MoppetTear), 1364);
    private static readonly WeightedCrpgUser BlowHandsome = CreateWeightedUser(1353, nameof(BlowHandsome), 1455);
    private static readonly WeightedCrpgUser SisterSmirk = CreateWeightedUser(1354, nameof(SisterSmirk), 2864);
    private static readonly WeightedCrpgUser FelineSweetTooth = CreateWeightedUser(1355, nameof(FelineSweetTooth), 742);
    private static readonly WeightedCrpgUser SealSneak = CreateWeightedUser(1356, nameof(SealSneak), 1227);
    private static readonly WeightedCrpgUser TinyFetis = CreateWeightedUser(1357, nameof(TinyFetis), 1402);
    private static readonly WeightedCrpgUser LassBloom = CreateWeightedUser(1358, nameof(LassBloom), 751);
    private static readonly WeightedCrpgUser BoxDinky = CreateWeightedUser(1359, nameof(BoxDinky), 1688);
    private static readonly WeightedCrpgUser BriocheRam = CreateWeightedUser(1360, nameof(BriocheRam), 2750);
    private static readonly WeightedCrpgUser SweetKitty = CreateWeightedUser(1361, nameof(SweetKitty), 2678);
    private static readonly WeightedCrpgUser GrimalkinDelicacy = CreateWeightedUser(1362, nameof(GrimalkinDelicacy), 1057);
    private static readonly WeightedCrpgUser LionMuscatel = CreateWeightedUser(1363, nameof(LionMuscatel), 1903);
    private static readonly WeightedCrpgUser ExtrafloralUnicorn = CreateWeightedUser(1364, nameof(ExtrafloralUnicorn), 2923);
    private static readonly WeightedCrpgUser NewsgirlLitter = CreateWeightedUser(1365, nameof(NewsgirlLitter), 2515);
    private static readonly WeightedCrpgUser CatsBalm = CreateWeightedUser(1366, nameof(CatsBalm), 1786);
    private static readonly WeightedCrpgUser DiscriminateBlancmange = CreateWeightedUser(1367, nameof(DiscriminateBlancmange), 2151);
    private static readonly WeightedCrpgUser TroopBobcat = CreateWeightedUser(1368, nameof(TroopBobcat), 502);
    private static readonly WeightedCrpgUser PunctiliousQuirky = CreateWeightedUser(1369, nameof(PunctiliousQuirky), 865);
    private static readonly WeightedCrpgUser GuideyUnpretty = CreateWeightedUser(1370, nameof(GuideyUnpretty), 1450);
    private static readonly WeightedCrpgUser ScurryHuge = CreateWeightedUser(1371, nameof(ScurryHuge), 754);
    private static readonly WeightedCrpgUser SlatternLoving = CreateWeightedUser(1372, nameof(SlatternLoving), 2238);
    private static readonly WeightedCrpgUser OnlyGirlChild = CreateWeightedUser(1373, nameof(OnlyGirlChild), 1249);
    private static readonly WeightedCrpgUser SwellSomali = CreateWeightedUser(1374, nameof(SwellSomali), 2813);
    private static readonly WeightedCrpgUser LatinaCyme = CreateWeightedUser(1375, nameof(LatinaCyme), 2221);
    private static readonly WeightedCrpgUser ScrumptiousKettleofFish = CreateWeightedUser(1376, nameof(ScrumptiousKettleofFish), 2061);
    private static readonly WeightedCrpgUser PearlPosy = CreateWeightedUser(1377, nameof(PearlPosy), 1745);
    private static readonly WeightedCrpgUser AlyssumMilkmaid = CreateWeightedUser(1378, nameof(AlyssumMilkmaid), 1251);
    private static readonly WeightedCrpgUser ChrysanthemumPeachy = CreateWeightedUser(1379, nameof(ChrysanthemumPeachy), 1083);
    private static readonly WeightedCrpgUser CalicoBun = CreateWeightedUser(1380, nameof(CalicoBun), 635);
    private static readonly WeightedCrpgUser BunnyCatPudgy = CreateWeightedUser(1381, nameof(BunnyCatPudgy), 2810);
    private static readonly WeightedCrpgUser CandiedFragrant = CreateWeightedUser(1382, nameof(CandiedFragrant), 1004);
    private static readonly WeightedCrpgUser MadamTomboy = CreateWeightedUser(1383, nameof(MadamTomboy), 975);
    private static readonly WeightedCrpgUser GynoeciumFeat = CreateWeightedUser(1384, nameof(GynoeciumFeat), 866);
    private static readonly WeightedCrpgUser PreciseAnthesis = CreateWeightedUser(1385, nameof(PreciseAnthesis), 2696);
    private static readonly WeightedCrpgUser SaccharineLamb = CreateWeightedUser(1386, nameof(SaccharineLamb), 2021);
    private static readonly WeightedCrpgUser CoquettePleasant = CreateWeightedUser(1387, nameof(CoquettePleasant), 1384);
    private static readonly WeightedCrpgUser LilacSweetly = CreateWeightedUser(1388, nameof(LilacSweetly), 782);
    private static readonly WeightedCrpgUser EmbarrassedMeow = CreateWeightedUser(1389, nameof(EmbarrassedMeow), 1489);
    private static readonly WeightedCrpgUser FloweringMissy = CreateWeightedUser(1390, nameof(FloweringMissy), 896);
    private static readonly WeightedCrpgUser CuttyClamber = CreateWeightedUser(1391, nameof(CuttyClamber), 2338);
    private static readonly WeightedCrpgUser PrettilyThalamus = CreateWeightedUser(1392, nameof(PrettilyThalamus), 1346);
    private static readonly WeightedCrpgUser EncounterPollination = CreateWeightedUser(1393, nameof(EncounterPollination), 576);
    private static readonly WeightedCrpgUser PatrolBonbon = CreateWeightedUser(1394, nameof(PatrolBonbon), 2468);
    private static readonly WeightedCrpgUser PortFem = CreateWeightedUser(1395, nameof(PortFem), 1659);
    private static readonly WeightedCrpgUser BudgereePerianth = CreateWeightedUser(1396, nameof(BudgereePerianth), 2764);
    private static readonly WeightedCrpgUser PsycheStaminate = CreateWeightedUser(1397, nameof(PsycheStaminate), 1035);
    private static readonly WeightedCrpgUser HoneyedSugar = CreateWeightedUser(1399, nameof(HoneyedSugar), 2216);
    private static readonly WeightedCrpgUser Gandalf = CreateWeightedUser(0, nameof(Gandalf), 3565, 1);
    private static readonly WeightedCrpgUser Saroumane = CreateWeightedUser(1, nameof(Saroumane), 4752, 1);

    private static readonly GameMatch Game1 = new()
    {
        TeamA = new List<WeightedCrpgUser>(),
        TeamB = new List<WeightedCrpgUser>(),
        Waiting = new List<WeightedCrpgUser>
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

    private static readonly GameMatch Game2 = new()
    {
        TeamA = new List<WeightedCrpgUser>
        {
            Arwen,
            Frodon,
            Sam,
            Sangoku,
        },
        TeamB = new List<WeightedCrpgUser>
        {
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
        },
        Waiting = new List<WeightedCrpgUser>
        {
            Merlin,
            Bob,
            Thomas,
        },
    };

    private static readonly GameMatch Game3 = new()
    {
        TeamA = new List<WeightedCrpgUser>(),
        TeamB = new List<WeightedCrpgUser>(),
        Waiting = new List<WeightedCrpgUser>
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
        },
    };

    private static readonly GameMatch GameWithVeryStrongClanGroup = new()
    {
        TeamA = new List<WeightedCrpgUser>(),
        TeamB = new List<WeightedCrpgUser>(),
        Waiting = new List<WeightedCrpgUser>
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
        TeamA = new List<WeightedCrpgUser>(),
        TeamB = new List<WeightedCrpgUser>(),
        Waiting = new List<WeightedCrpgUser>(),
    };

    private static readonly GameMatch OneManGame = new()
    {
        TeamA = new List<WeightedCrpgUser>(),
        TeamB = new List<WeightedCrpgUser>(),
        Waiting = new List<WeightedCrpgUser>
        {
            Aragorn,
        },
    };

    private static readonly GameMatch TwoManGame = new()
    {
        TeamA = new List<WeightedCrpgUser>(),
        TeamB = new List<WeightedCrpgUser>(),
        Waiting = new List<WeightedCrpgUser>
        {
            Aragorn,
            Arwen,
        },
    };

    private static readonly GameMatch ThreeManGame = new()
    {
        TeamA = new List<WeightedCrpgUser>(),
        TeamB = new List<WeightedCrpgUser>(),
        Waiting = new List<WeightedCrpgUser>
        {
            Aragorn,
            Arwen,
            Madonna,
        },
    };

    private static readonly GameMatch EmptyTeamGame = new()
    {
        TeamA = new List<WeightedCrpgUser>(),
        TeamB = new List<WeightedCrpgUser>
        {
            Aragorn,
            Arwen,
            Madonna,
            Frodon,
        },
        Waiting = new List<WeightedCrpgUser>(),
    };

    private static readonly GameMatch GameWithWithOneGroup = new()
    {
        TeamA = new List<WeightedCrpgUser>(),
        TeamB = new List<WeightedCrpgUser>(),
        Waiting = new List<WeightedCrpgUser>
        {
            Saroumane,
            Arwen,
            Aragorn,
            Gandalf,
        },
    };
    [Test]
    public void KkMakeTeamOfSimilarSizesShouldNotBeThatBad()
    {
        var matchBalancer = new MatchBalancer();
        GameMatch balancedGame =
            matchBalancer.KkMakeTeamOfSimilarSizesWithoutSplittingClanGroups(GameWithVeryStrongClanGroup);
        float teamASize = balancedGame.TeamA.Count;
        float teamBSize = balancedGame.TeamB.Count;
        float sizeRatio = teamASize / teamBSize;
        float teamAMeanRating = WeightHelpers.ComputeTeamWeight(balancedGame.TeamA, 1);
        float teamBMeanRating = WeightHelpers.ComputeTeamWeight(balancedGame.TeamB, 1);
        float meanRatingRatio = teamAMeanRating / teamBMeanRating;
        MatchBalancingHelpers.DumpTeams(balancedGame);
        Assert.That(sizeRatio, Is.EqualTo(0.7f).Within(0.3f));
    }

    [Test]
    public void BannerBalancing()
    {
        float unbalancedTeamAMeanRating = WeightHelpers.ComputeTeamWeight(Game1.TeamA, 1);
        float unbalancedTeamBMeanRating = WeightHelpers.ComputeTeamWeight(Game1.TeamB, 1);
        float unbalancedMeanRatingRatio = unbalancedTeamAMeanRating / unbalancedTeamBMeanRating;
        Console.WriteLine($"unbalanced rating ratio = {unbalancedMeanRatingRatio}");

        GameMatch balancedGame = PureBannerBalancing(Game1);
        float teamASize = balancedGame.TeamA.Count;
        float teamBSize = balancedGame.TeamB.Count;
        float sizeRatio = teamASize / teamBSize;
        Console.WriteLine($"balanced size ratio = {sizeRatio}");
        float teamARating = WeightHelpers.ComputeTeamWeight(balancedGame.TeamA, 1);
        float teamBRating = WeightHelpers.ComputeTeamWeight(balancedGame.TeamB, 1);
        float ratingRatio = teamARating / teamBRating;
        Console.WriteLine($"teamASize = {teamASize} teamBSize = {teamBSize}");
        Console.WriteLine($"teamARating = new CrpgCharacterRating {{ Value = {teamARating} teamBRating = new CrpgCharacterRating {{ Value = {teamBRating} }} }}");
        Assert.That(ratingRatio, Is.EqualTo(1).Within(0.2));
    }

    [Test]
    public void BannerBalancingWithEdgeCaseWarmup()
    {
        var matchBalancer = new MatchBalancer();

        float unbalancedTeamAMeanRating = WeightHelpers.ComputeTeamWeight(Game1.TeamA, 1);
        float unbalancedTeamBMeanRating = WeightHelpers.ComputeTeamWeight(Game1.TeamB, 1);
        float unbalancedMeanRatingRatio = unbalancedTeamAMeanRating / unbalancedTeamBMeanRating;
        GameMatch balancedGame = matchBalancer.BannerBalancingWithEdgeCases(Game1, firstBalance: true);
        float teamASize = balancedGame.TeamA.Count;
        float teamBSize = balancedGame.TeamB.Count;
        float sizeRatio = teamASize / teamBSize;
        float teamARating = WeightHelpers.ComputeTeamWeight(balancedGame.TeamA, 1);
        float teamBRating = WeightHelpers.ComputeTeamWeight(balancedGame.TeamB, 1);
        float ratingRatio = teamARating / teamBRating;
        Assert.That(ratingRatio, Is.EqualTo(1).Within(0.2));
    }

    [Test]
    public void BannerBalancingWithEdgeCaseNotWarmup()
    {
        var matchBalancer = new MatchBalancer();
        GameMatch balancedGame = matchBalancer.NaiveCaptainBalancing(Game1);
        balancedGame = matchBalancer.BannerBalancingWithEdgeCases(balancedGame, firstBalance: false);

        float teamARating = WeightHelpers.ComputeTeamWeight(balancedGame.TeamA, 1);
        float teamBRating = WeightHelpers.ComputeTeamWeight(balancedGame.TeamB, 1);
        float ratingRatio = teamARating / teamBRating;
        Assert.That(ratingRatio, Is.EqualTo(1).Within(0.2));
    }

    [Test]
    public void BannerBalancingWithEdgeCaseNotWarmupShouldWorkWithOneTeamEmpty()
    {
        var matchBalancer = new MatchBalancer();
        var balancedGame = matchBalancer.BannerBalancingWithEdgeCases(EmptyTeamGame, firstBalance: false);

        float teamARating = WeightHelpers.ComputeTeamWeight(balancedGame.TeamA, 1);
        float teamBRating = WeightHelpers.ComputeTeamWeight(balancedGame.TeamB, 1);
        float ratingRatio = teamARating / teamBRating;
        MatchBalancingHelpers.DumpTeams(balancedGame);
    }

    [Test]
    public void BannerBalancingWithEdgeCaseShouldWorkWithOneStrongClanGroup()
    {
        var matchBalancer = new MatchBalancer();
        MatchBalancingHelpers.DumpTeams(GameWithVeryStrongClanGroup);
        float unbalancedTeamAMeanRating = WeightHelpers.ComputeTeamWeight(GameWithVeryStrongClanGroup.TeamA, 1);
        float unbalancedTeamBMeanRating = WeightHelpers.ComputeTeamWeight(GameWithVeryStrongClanGroup.TeamB, 1);
        float unbalancedMeanRatingRatio = unbalancedTeamAMeanRating / unbalancedTeamBMeanRating;
        GameMatch balancedGame = matchBalancer.BannerBalancingWithEdgeCases(GameWithVeryStrongClanGroup);
        float teamASize = balancedGame.TeamA.Count;
        float teamBSize = balancedGame.TeamB.Count;
        float sizeRatio = teamASize / teamBSize;
        float teamARating = WeightHelpers.ComputeTeamWeight(balancedGame.TeamA, 1);
        float teamBRating = WeightHelpers.ComputeTeamWeight(balancedGame.TeamB, 1);
        float ratingRatio = teamARating / teamBRating;
        MatchBalancingHelpers.DumpTeams(balancedGame);
        Assert.That(ratingRatio, Is.EqualTo(1).Within(0.2));
    }
    [Test]
    public void BannerBalancingWithEdgeCaseShouldWorkWithOneClanGroup()
    {
        var matchBalancer = new MatchBalancer();
        MatchBalancingHelpers.DumpTeams(GameWithWithOneGroup);
        float unbalancedTeamAMeanRating = WeightHelpers.ComputeTeamWeight(GameWithWithOneGroup.TeamA, 1);
        float unbalancedTeamBMeanRating = WeightHelpers.ComputeTeamWeight(GameWithWithOneGroup.TeamB, 1);
        float unbalancedMeanRatingRatio = unbalancedTeamAMeanRating / unbalancedTeamBMeanRating;
        GameMatch balancedGame = matchBalancer.BannerBalancingWithEdgeCases(GameWithWithOneGroup);
        float teamASize = balancedGame.TeamA.Count;
        float teamBSize = balancedGame.TeamB.Count;
        float sizeRatio = teamASize / teamBSize;
        float teamARating = WeightHelpers.ComputeTeamWeight(balancedGame.TeamA, 1);
        float teamBRating = WeightHelpers.ComputeTeamWeight(balancedGame.TeamB, 1);
        float ratingRatio = teamARating / teamBRating;
        MatchBalancingHelpers.DumpTeams(balancedGame);
        Assert.That(ratingRatio, Is.EqualTo(1).Within(0.2));
    }
    [Test]
    public void BannerBalancingWithEdgeCaseShouldNotLoseOrAddCharacters()
    {
        var matchBalancer = new MatchBalancer();

        float unbalancedTeamAMeanRating = WeightHelpers.ComputeTeamWeight(Game1.TeamA, 1);
        float unbalancedTeamBMeanRating = WeightHelpers.ComputeTeamWeight(Game1.TeamB, 1);
        float unbalancedMeanRatingRatio = unbalancedTeamAMeanRating / unbalancedTeamBMeanRating;
        GameMatch balancedGame = matchBalancer.BannerBalancingWithEdgeCases(Game1);
        List<WeightedCrpgUser> allUsersFromBalancedGame = new();
        List<WeightedCrpgUser> allUsersFromUnbalancedGame = new();
        allUsersFromBalancedGame.AddRange(balancedGame.TeamA);
        allUsersFromBalancedGame.AddRange(balancedGame.TeamB);
        allUsersFromBalancedGame.AddRange(balancedGame.Waiting);
        allUsersFromUnbalancedGame.AddRange(Game1.TeamA);
        allUsersFromUnbalancedGame.AddRange(Game1.TeamB);
        allUsersFromUnbalancedGame.AddRange(Game1.Waiting);
        Assert.That(allUsersFromUnbalancedGame, Is.EquivalentTo(allUsersFromBalancedGame));
    }

    [Test]
    public void BannerBalancingShouldNotSeperateCrpgClanMember()
    {
        var matchBalancer = new MatchBalancer();

        float unbalancedTeamAMeanRating = WeightHelpers.ComputeTeamWeight(Game1.TeamA, 1);
        float unbalancedTeamBMeanRating = WeightHelpers.ComputeTeamWeight(Game1.TeamB, 1);
        float unbalancedMeanRatingRatio = unbalancedTeamAMeanRating / unbalancedTeamBMeanRating;
        Console.WriteLine($"unbalanced rating ratio = {unbalancedMeanRatingRatio}");

        GameMatch balancedGame = PureBannerBalancing(Game1);
        foreach (WeightedCrpgUser u in Game1.TeamA)
        {
            if (u.ClanId == null)
            {
                continue;
            }

            foreach (WeightedCrpgUser u2 in Game1.TeamB)
            {
                if (u2.ClanId != null)
                {
                    Assert.That(u.ClanId, Is.Not.EqualTo(u2.ClanId));
                }
            }
        }
    }

    [Test]
    public void BannerBalancingWithEdgeCaseShouldWorkWith0Persons()
    {
        var matchBalancer = new MatchBalancer();

        float unbalancedTeamAMeanRating = WeightHelpers.ComputeTeamWeight(NoManGame.TeamA, 1);
        float unbalancedTeamBMeanRating = WeightHelpers.ComputeTeamWeight(NoManGame.TeamB, 1);
        float unbalancedMeanRatingRatio = unbalancedTeamAMeanRating / unbalancedTeamBMeanRating;
        GameMatch balancedGame = matchBalancer.BannerBalancingWithEdgeCases(NoManGame);
    }

    [Test]
    public void BannerBalancingWithEdgeCaseShouldWorkWith1Persons()
    {
        var matchBalancer = new MatchBalancer();

        float unbalancedTeamAMeanRating = WeightHelpers.ComputeTeamWeight(OneManGame.TeamA, 1);
        float unbalancedTeamBMeanRating = WeightHelpers.ComputeTeamWeight(OneManGame.TeamB, 1);
        float unbalancedMeanRatingRatio = unbalancedTeamAMeanRating / unbalancedTeamBMeanRating;
        GameMatch balancedGame = matchBalancer.BannerBalancingWithEdgeCases(OneManGame);
    }

    [Test]
    public void BannerBalancingWithEdgeCaseShouldWorkWith2Persons()
    {
        var matchBalancer = new MatchBalancer();

        float unbalancedTeamAMeanRating = WeightHelpers.ComputeTeamWeight(TwoManGame.TeamA, 1);
        float unbalancedTeamBMeanRating = WeightHelpers.ComputeTeamWeight(TwoManGame.TeamB, 1);
        float unbalancedMeanRatingRatio = unbalancedTeamAMeanRating / unbalancedTeamBMeanRating;
        GameMatch balancedGame = matchBalancer.BannerBalancingWithEdgeCases(TwoManGame);
    }

    [Test]
    public void BannerBalancingWithEdgeCaseShouldWorkWith3Persons()
    {
        var matchBalancer = new MatchBalancer();

        float unbalancedTeamAMeanRating = WeightHelpers.ComputeTeamWeight(ThreeManGame.TeamA, 1);
        float unbalancedTeamBMeanRating = WeightHelpers.ComputeTeamWeight(ThreeManGame.TeamB, 1);
        float unbalancedMeanRatingRatio = unbalancedTeamAMeanRating / unbalancedTeamBMeanRating;
        GameMatch balancedGame = matchBalancer.BannerBalancingWithEdgeCases(ThreeManGame);
    }

    [Test]
    public void PowerMeanShouldWork()
    {
        List<float> floats = new() { 0, 0, 5, 5, 10, 10 };
        Console.WriteLine(MathHelper.PowerMean(floats, 1f));
        Assert.That(MathHelper.PowerMean(floats, 1f), Is.EqualTo(5).Within(0.01));
    }

    [Test]
    public void ConvertingToClanGroupsThenToUserListShouldDoNothing()
    {
        List<WeightedCrpgUser> userList = new();
        userList.AddRange(Game3.Waiting);
        userList.AddRange(Game3.TeamA);
        userList.AddRange(Game3.TeamB);
        List<ClanGroup> clanGroups = MatchBalancingHelpers.SplitUsersIntoClanGroups(userList);
        List<WeightedCrpgUser> newUserList = MatchBalancingHelpers.JoinClanGroupsIntoUsers(clanGroups);
        Assert.That(userList, Is.EquivalentTo(newUserList));
    }

    [Test]
    public void RegroupClansShouldEmptyWaiting()
    {
        var game = MatchBalancingHelpers.RejoinClans(Game2);
        Assert.That(game.Waiting, Is.Empty);
    }

    [Test]
    public void RegroupClansShouldNotLoseOrAddCharacters()
    {
        GameMatch balancedGame = MatchBalancingHelpers.RejoinClans(Game2);
        List<WeightedCrpgUser> allUsersFromBalancedGame = new();
        List<WeightedCrpgUser> allUsersFromUnbalancedGame = new();
        allUsersFromBalancedGame.AddRange(balancedGame.TeamA);
        allUsersFromBalancedGame.AddRange(balancedGame.TeamB);
        allUsersFromBalancedGame.AddRange(balancedGame.Waiting);
        allUsersFromUnbalancedGame.AddRange(Game2.TeamA);
        allUsersFromUnbalancedGame.AddRange(Game2.TeamB);
        allUsersFromUnbalancedGame.AddRange(Game2.Waiting);
        Assert.That(allUsersFromUnbalancedGame, Is.EquivalentTo(allUsersFromBalancedGame));
    }

    [Test]
    public void RegroupClansShouldRegroupPlayerByClan()
    {
        GameMatch balancedGame = MatchBalancingHelpers.RejoinClans(Game2);
        List<WeightedCrpgUser> allUsersFromBalancedGame = new();
        List<WeightedCrpgUser> allUsersFromUnbalancedGame = new();
        allUsersFromBalancedGame.AddRange(balancedGame.TeamA);
        allUsersFromBalancedGame.AddRange(balancedGame.TeamB);
        allUsersFromBalancedGame.AddRange(balancedGame.Waiting);
        List<int> teamAClanId = balancedGame.TeamA.Where(u => u.ClanId != null).Select(u => u.ClanId!.Value).ToList();
        List<int> teamBClanId = balancedGame.TeamB.Where(u => u.ClanId != null).Select(u => u.ClanId!.Value).ToList();
        var intersection = teamAClanId.Intersect(teamBClanId);
        Assert.That(intersection, Is.Empty);
        MatchBalancingHelpers.DumpTeamsStatus(balancedGame);
    }

    [Test]
    public void RejoinClanDoesNotKeepClanMatesSeparated()
    {
        GameMatch game = new()
        {
            TeamA = new List<WeightedCrpgUser>
            {
                Hudax01, Hudax02, Sangoku, RolandDeschain,
            },
            TeamB = new List<WeightedCrpgUser>
            {
                Jean01,
                Jean02,
                Hudax03,
                ProfCharles,
                Jean03,
            },
            Waiting = new List<WeightedCrpgUser>
            {
                Jean04,
                Krilin,
                Hudax04,
                Daschyhund,
                GreatieDane,
                BassetyHound,
            },
        };

        var game2 = MatchBalancingHelpers.RejoinClans(game);
        Assert.That(game2.Waiting, Is.Empty);
        Assert.That(
            game2.TeamB.Where(u => u.ClanId != null).Select(u => u.ClanId!.Value)
                .Intersect(game2.TeamB.Where(u => u.ClanId != null).Select(u => u.ClanId!.Value))
                .Any(),
            Is.True);
    }

    public void RejoinClanDoesNotLosePlayers()
    {
        GameMatch game = new()
        {
            TeamA = new List<WeightedCrpgUser>
            {
                Hudax01, Hudax02, Sangoku, RolandDeschain,
            },
            TeamB = new List<WeightedCrpgUser>
            {
                Jean01,
                Jean02,
                Hudax03,
                ProfCharles,
                Jean03,
            },
            Waiting = new List<WeightedCrpgUser>
            {
                Jean04,
                Krilin,
                Hudax04,
                Daschyhund,
                GreatieDane,
                BassetyHound,
            },
        };

        var game2 = MatchBalancingHelpers.RejoinClans(game);
        Assert.That(game2.Waiting, Is.Empty);
        Assert.That(game2.TeamA.Count + game2.TeamB.Count + game2.Waiting.Count == 15);
    }

    private GameMatch PureBannerBalancing(GameMatch gameMatch)
    {
        var matchBalancer = new MatchBalancer();
        GameMatch unbalancedBannerGameMatch =
            matchBalancer.KkMakeTeamOfSimilarSizesWithoutSplittingClanGroups(gameMatch);
        unbalancedBannerGameMatch = matchBalancer.BalanceTeamOfSimilarSizes(unbalancedBannerGameMatch, true, 0.025f);
        return unbalancedBannerGameMatch;
    }
}
