using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Crpg.GameMod.Api;
using Crpg.GameMod.Api.Models;
using Steamworks;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.GameMod.DefendTheVirgin
{
    public class DefendTheVirginGameManager : MBGameManager
    {
        private static readonly Random Rng = new Random();

        private readonly ICrpgClient _crpgClient = new CrpgHttpClient("eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJuYW1laWQiOjAsInJvbGUiOiJnYW1lIiwibmJmIjoxNjAxMDQ4NzEyLCJleHAiOiIxNjAyMDAwMDAwIiwiaWF0IjoxNjAxMDQ4NzEyfQ.8GuUTu3Gs5-JH3_oHCuFWuNxF6ChjWK4P4vfs_2KFuE");

        private Task<CrpgUser>? _getUserTask;
        private IList<Wave>? _waves;

        protected override void DoLoadingForGameManager(
            GameManagerLoadingSteps gameManagerLoadingStep,
            out GameManagerLoadingSteps nextStep)
        {
            nextStep = GameManagerLoadingSteps.None;
            switch (gameManagerLoadingStep)
            {
                case GameManagerLoadingSteps.PreInitializeZerothStep:
                    LoadModuleData(false);
                    _getUserTask = GetUserAsync();
                    _waves = LoadWaves();
                    MBGlobals.InitializeReferences();
                    Game.CreateGame(new DefendTheVirginGame(), this).DoLoading();
                    nextStep = GameManagerLoadingSteps.FirstInitializeFirstStep;
                    break;
                case GameManagerLoadingSteps.FirstInitializeFirstStep:
                    bool flag = true;
                    foreach (MBSubModuleBase subModule in Module.CurrentModule.SubModules)
                    {
                        flag = flag && subModule.DoLoading(Game.Current);
                    }

                    nextStep = flag
                        ? GameManagerLoadingSteps.WaitSecondStep
                        : GameManagerLoadingSteps.FirstInitializeFirstStep;
                    break;
                case GameManagerLoadingSteps.WaitSecondStep:
                    StartNewGame();
                    nextStep = GameManagerLoadingSteps.SecondInitializeThirdState;
                    break;
                case GameManagerLoadingSteps.SecondInitializeThirdState:
                    nextStep = Game.Current.DoLoading()
                        ? GameManagerLoadingSteps.PostInitializeFourthState
                        : GameManagerLoadingSteps.SecondInitializeThirdState;
                    break;
                case GameManagerLoadingSteps.PostInitializeFourthState:
                    nextStep = _getUserTask!.IsCompleted
                        ? GameManagerLoadingSteps.FinishLoadingFifthStep
                        : GameManagerLoadingSteps.PostInitializeFourthState;
                    break;
                case GameManagerLoadingSteps.FinishLoadingFifthStep:
                    nextStep = GameManagerLoadingSteps.None;
                    break;
            }
        }

        public override void OnLoadFinished()
        {
            base.OnLoadFinished();

            string scene = GetRandomVillageScene();
            AtmosphereInfo atmosphereInfoForMission = GetRandomAtmosphere();

            InformationManager.DisplayMessage(new InformationMessage($"Map is {scene}."));
            InformationManager.DisplayMessage(new InformationMessage("Visit c-rpg.eu to upgrade your character."));

            var waveController = new WaveController(_waves!.Count);
            var waveSpawningBehavior = new WaveSpawnLogic(waveController, _waves!);
            var crpgLogic = new CrpgLogic(waveController, _crpgClient, _waves!, _getUserTask!.Result);

            MissionState.OpenNew("DefendTheVirgin", new MissionInitializerRecord(scene)
            {
                DoNotUseLoadingScreen = false,
                PlayingInCampaignMode = false,
                AtmosphereOnCampaign = atmosphereInfoForMission,
                SceneLevels = string.Empty,
                TimeOfDay = 6f,
            }, missionController => new MissionBehaviour[]
            {
                new MissionCombatantsLogic(),
                waveController,
                waveSpawningBehavior,
                crpgLogic,
                new AgentBattleAILogic(),
                new MissionHardBorderPlacer(),
                new MissionBoundaryPlacer(),
                new MissionBoundaryCrossingHandler(),
                new AgentFadeOutLogic(),
            });
        }

        private async Task<CrpgUser> GetUserAsync()
        {
            var steamId = SteamUser.GetSteamID();
            string name = SteamFriends.GetFriendPersonaName(steamId);

            var res = await _crpgClient.Update(new CrpgGameUpdateRequest
            {
                GameUserUpdates = new[]
                {
                    new CrpgGameUserUpdate { PlatformId = steamId.ToString(), CharacterName = name },
                },
            });

            return res.Users[0];
        }

        private static IList<Wave> LoadWaves()
        {
            var waves = new List<Wave>();

            string path = BasePath.Name + "Modules/cRPG/ModuleData/waves.xml";
            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            XmlNode wavesNode = doc.ChildNodes[1];
            foreach (XmlNode waveNode in wavesNode.ChildNodes)
            {
                var wave = new Wave { Groups = new List<WaveGroup>() };

                XmlNode unitsNode = waveNode.ChildNodes[0];
                foreach (XmlNode unitNode in unitsNode.ChildNodes)
                {
                    var waveGroup = new WaveGroup { Count = 1 };
                    foreach (XmlAttribute attribute in unitNode.Attributes)
                    {
                        switch (attribute.Name)
                        {
                            case "id":
                                waveGroup.CharacterId = attribute.Value;
                                break;
                            case "count":
                                waveGroup.Count = int.Parse(attribute.Value);
                                break;
                        }

                        wave.Groups.Add(waveGroup);
                    }
                }

                waves.Add(wave);
            }

            return waves;
        }

        private static string GetRandomVillageScene()
        {
            string[] scenes =
            {
                "aserai_village_e",
                // "aserai_village_i", // 200m
                "aserai_village_j",
                "aserai_village_l",
                "battania_village_h",
                "battania_village_i",
                "battania_village_j",
                "battania_village_k",
                "battania_village_l",
                "empire_village_002",
                // "empire_village_003", // 152m
                // "empire_village_004", // 300m
                "empire_village_007",
                "empire_village_008",
                "empire_village_p",
                "empire_village_x",
                "khuzait_village_a",
                "khuzait_village_i",
                // "khuzait_village_j", // 214m
                "khuzait_village_k",
                // "khuzait_village_l", // 103m
                "sturgia_village_e",
                "sturgia_village_f",
                "sturgia_village_g",
                "sturgia_village_h",
                "sturgia_village_j",
                // "sturgia_village_l", // 120m
                "vlandia_village_g",
                // "vlandia_village_i", // 292m
                "vlandia_village_k",
                "vlandia_village_l",
                // "vlandia_village_m", // 342m
                "vlandia_village_n",
            };

            return scenes[Rng.Next(0, scenes.Length)];
        }

        private static AtmosphereInfo GetRandomAtmosphere()
        {
            string[] atmospheres =
            {
                "TOD_01_00_SemiCloudy",
                "TOD_02_00_SemiCloudy",
                "TOD_03_00_SemiCloudy",
                "TOD_04_00_SemiCloudy",
                "TOD_05_00_SemiCloudy",
                "TOD_06_00_SemiCloudy",
                "TOD_07_00_SemiCloudy",
                "TOD_08_00_SemiCloudy",
                "TOD_09_00_SemiCloudy",
                "TOD_10_00_SemiCloudy",
                "TOD_11_00_SemiCloudy",
                "TOD_12_00_SemiCloudy"
            };
            string atmosphere = atmospheres[Rng.Next(0, atmospheres.Length)];

            string[] seasons =
            {
                "spring",
                "summer",
                "fall",
                "winter",
            };
            int seasonId = Rng.Next(0, seasons.Length);

            return new AtmosphereInfo
            {
                AtmosphereName = atmosphere,
                TimeInfo = new TimeInformation { Season = seasonId },
            };
        }
    }
}
