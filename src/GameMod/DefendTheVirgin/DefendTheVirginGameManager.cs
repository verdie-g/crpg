using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Crpg.GameMod.Api;
using Crpg.GameMod.Api.Models;
using Crpg.GameMod.Api.Models.Characters;
using Crpg.GameMod.Api.Models.Users;
using Crpg.GameMod.Common;
using Crpg.GameMod.Helpers;
using Newtonsoft.Json;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TaleWorlds.PlatformService;
using Module = TaleWorlds.MountAndBlade.Module;
using Platform = Crpg.GameMod.Api.Models.Users.Platform;

namespace Crpg.GameMod.DefendTheVirgin
{
    public class DefendTheVirginGameManager : MBGameManager
    {
        private static readonly Random Rng = new Random();

        private readonly ICrpgClient _crpgClient = new CrpgHttpClient();

        private Task<CrpgUser>? _getUserTask;
        private WaveGroup[][]? _waves;

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

            if (_getUserTask!.IsFaulted)
            {
                MBDebug.Print(_getUserTask.Exception!.ToString());
                return;
            }

            string scene = GetRandomVillageScene();
            AtmosphereInfo atmosphereInfoForMission = GetRandomAtmosphere();

            InformationManager.DisplayMessage(new InformationMessage($"Map is {scene}."));
            InformationManager.DisplayMessage(new InformationMessage("Visit c-rpg.eu to upgrade your character."));

            var waveController = new WaveController(_waves!.Length);
            var waveSpawnLogic = new WaveSpawnLogic(waveController, _waves!, CreateCharacter(_getUserTask.Result.Character));
            var crpgLogic = new CrpgLogic(waveController, _crpgClient, _waves!, _getUserTask.Result);

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
                waveSpawnLogic,
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
            var platform = (Platform)Enum.Parse(typeof(Platform), PlatformServices.ProviderName, true);
            var login = PlatformServices.Instance.CreateLobbyClientLoginProvider();
            login.Initialize(null, PlatformServices.Instance.GetInitParams()); // PreferredUserName is not used

            await _crpgClient.Initialize();
            var res = await _crpgClient.Update(new CrpgGameUpdateRequest
            {
                GameUserUpdates = new[]
                {
                    new CrpgGameUserUpdate
                    {
                        Platform = platform,
                        // The real id seems to be Id2 for Steam and GOG, not sure about Epic
                        PlatformUserId = login.GetPlayerId().Id2.ToString(),
                        CharacterName = login.GetUserName(),
                    },
                },
            });

            return res.Data!.Users[0];
        }

        private static WaveGroup[][] LoadWaves()
        {
            string path = BasePath.Name + "Modules/cRPG/ModuleData/waves.json";
            var waves = JsonConvert.DeserializeObject<WaveGroup[][]>(File.ReadAllText(path));
            foreach (var wave in waves)
            {
                foreach (var group in wave)
                {
                    // In case count was not set
                    group.Count = Math.Max(group.Count, 1);
                }
            }

            return waves;
        }

        private static string GetRandomVillageScene()
        {
            string[] scenes =
            {
                "aserai_village_e", // 127m
                // "aserai_village_i", // 200m
                // "aserai_village_j", // 257m
                // "aserai_village_l", // 296m
                // "battania_village_h", // 252m
                "battania_village_i", // 120m
                "battania_village_j", // 120m
                // "battania_village_k", // 218m
                // "battania_village_l", // 340m
                // "empire_village_002", // 170m
                "empire_village_003", // 152m
                // "empire_village_004", // 300m
                "empire_village_007", // 150m
                // "empire_village_008", // CRASH WITH NO EXCEPTION
                // "empire_village_p", // CRASH WITH NO EXCEPTION
                // "empire_village_x", // CRASH WITH NO EXCEPTION
                "khuzait_village_a", // 169m
                // "khuzait_village_i", // 229m
                // "khuzait_village_j", // 214m
                // "khuzait_village_k", // 250m
                "khuzait_village_l", // 103m
                "sturgia_village_e", // 153m
                // "sturgia_village_f", // 160m
                "sturgia_village_g", // 100m
                // "sturgia_village_h", // 279m
                // "sturgia_village_j", // 291m
                // "sturgia_village_l", // 120m
                // "vlandia_village_g", // 270m
                // "vlandia_village_i", // 292m
                // "vlandia_village_k", // 300m
                // "vlandia_village_l", // 280m
                // "vlandia_village_m", // 342m
                // "vlandia_village_n", // 278m
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
                "TOD_12_00_SemiCloudy",
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

        private BasicCharacterObject CreateCharacter(CrpgCharacter crpgCharacter)
        {
            var skills = new CharacterSkills();
            skills.SetPropertyValue(CrpgSkills.Strength, crpgCharacter.Statistics.Attributes.Strength);
            skills.SetPropertyValue(CrpgSkills.Agility, crpgCharacter.Statistics.Attributes.Agility);

            skills.SetPropertyValue(CrpgSkills.IronFlesh, crpgCharacter.Statistics.Skills.IronFlesh);
            skills.SetPropertyValue(CrpgSkills.PowerStrike, crpgCharacter.Statistics.Skills.PowerStrike);
            skills.SetPropertyValue(CrpgSkills.PowerDraw, crpgCharacter.Statistics.Skills.PowerDraw);
            skills.SetPropertyValue(CrpgSkills.PowerThrow, crpgCharacter.Statistics.Skills.PowerThrow);
            skills.SetPropertyValue(DefaultSkills.Athletics, crpgCharacter.Statistics.Skills.Athletics * 20 + 2 * crpgCharacter.Statistics.Attributes.Agility);
            skills.SetPropertyValue(DefaultSkills.Riding, crpgCharacter.Statistics.Skills.Riding * 20);
            skills.SetPropertyValue(CrpgSkills.WeaponMaster, crpgCharacter.Statistics.Skills.WeaponMaster);
            skills.SetPropertyValue(CrpgSkills.HorseArchery, crpgCharacter.Statistics.Skills.HorseArchery);

            skills.SetPropertyValue(DefaultSkills.OneHanded, crpgCharacter.Statistics.WeaponProficiencies.OneHanded);
            skills.SetPropertyValue(DefaultSkills.TwoHanded, crpgCharacter.Statistics.WeaponProficiencies.TwoHanded);
            skills.SetPropertyValue(DefaultSkills.Polearm, crpgCharacter.Statistics.WeaponProficiencies.Polearm);
            skills.SetPropertyValue(DefaultSkills.Bow, crpgCharacter.Statistics.WeaponProficiencies.Bow);
            skills.SetPropertyValue(DefaultSkills.Crossbow, crpgCharacter.Statistics.WeaponProficiencies.Crossbow);
            skills.SetPropertyValue(DefaultSkills.Throwing, crpgCharacter.Statistics.WeaponProficiencies.Throwing);

            var equipment = new Equipment();
            AddEquipment(equipment, EquipmentIndex.Head, crpgCharacter.Items.HeadItem?.MbId, skills);
            AddEquipment(equipment, EquipmentIndex.Cape, crpgCharacter.Items.ShoulderItem?.MbId, skills);
            AddEquipment(equipment, EquipmentIndex.Body, crpgCharacter.Items.BodyItem?.MbId, skills);
            AddEquipment(equipment, EquipmentIndex.Gloves, crpgCharacter.Items.HandItem?.MbId, skills);
            AddEquipment(equipment, EquipmentIndex.Leg, crpgCharacter.Items.LegItem?.MbId, skills);
            AddEquipment(equipment, EquipmentIndex.HorseHarness, crpgCharacter.Items.MountHarnessItem?.MbId, skills);
            AddEquipment(equipment, EquipmentIndex.Horse, crpgCharacter.Items.MountItem?.MbId, skills);
            AddEquipment(equipment, EquipmentIndex.Weapon0, crpgCharacter.Items.Weapon1Item?.MbId, skills);
            AddEquipment(equipment, EquipmentIndex.Weapon1, crpgCharacter.Items.Weapon2Item?.MbId, skills);
            AddEquipment(equipment, EquipmentIndex.Weapon2, crpgCharacter.Items.Weapon3Item?.MbId, skills);
            AddEquipment(equipment, EquipmentIndex.Weapon3, crpgCharacter.Items.Weapon4Item?.MbId, skills);

            var mbCharacter = new CrpgCharacterObject(new TextObject(crpgCharacter.Name), skills);
            SetCharacterBodyProperties(mbCharacter, crpgCharacter.BodyProperties, crpgCharacter.Gender);
            mbCharacter.InitializeEquipmentsOnLoad(new List<Equipment> { equipment });
            return mbCharacter;
        }

        private void SetCharacterBodyProperties(CrpgCharacterObject mbCharacter, string bodyPropertiesKey,
            CrpgCharacterGender gender)
        {
            var dynamicBodyProperties = new DynamicBodyProperties { Age = 20, Build = 1.0f, Weight = 0.5f }; // Age chosen arbitrarily.
            var staticBodyProperties = StaticBodyPropertiesFromString(bodyPropertiesKey);
            var bodyProperties = new BodyProperties(dynamicBodyProperties, staticBodyProperties);
            bodyProperties = bodyProperties.ClampForMultiplayer(); // Clamp height and set Build & Weight.
            mbCharacter.UpdatePlayerCharacterBodyProperties(bodyProperties, gender == CrpgCharacterGender.Female);
            ReflectionHelper.SetField(mbCharacter, "_dynamicBodyPropertiesMin", mbCharacter.GetBodyPropertiesMax().DynamicProperties);
        }

        private StaticBodyProperties StaticBodyPropertiesFromString(string bodyPropertiesKey)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml($"<BodyProperties key=\"{bodyPropertiesKey}\" />");
            if (!StaticBodyProperties.FromXmlNode(doc.DocumentElement, out var staticBodyProperties))
            {
                // TODO: log warning.
            }

            return staticBodyProperties;
        }

        private static void AddEquipment(Equipment equipments, EquipmentIndex idx, string? itemId, CharacterSkills skills)
        {
            var itemObject = GetModifiedItem(itemId, skills);
            var equipmentElement = new EquipmentElement(itemObject);
            equipments.AddEquipmentToSlotWithoutAgent(idx, equipmentElement);
        }

        // TODO: use ItemModifier for looming
        private static ItemObject? GetModifiedItem(string? itemId, CharacterSkills skills)
        {
            if (itemId == null)
            {
                return null;
            }

            var itemObject = MBObjectManager.Instance.GetObject<ItemObject>(itemId);
            if (itemObject == null) // Defensive check. Object should always exist.
            {
                // TODO: log warning.
                return null;
            }

            itemObject = ReflectionHelper.DeepClone(itemObject);

            if (itemObject.Weapons != null)
            {
                ModifyDamage(itemObject, skills.GetPropertyValue(CrpgSkills.PowerStrike) * 0.08f, WeaponClassesAffectedByPowerStrike);
                ModifyDamage(itemObject, skills.GetPropertyValue(CrpgSkills.PowerDraw) * 0.14f, WeaponClassesAffectedByPowerDraw);
                ModifyDamage(itemObject, skills.GetPropertyValue(CrpgSkills.PowerThrow) * 0.10f, WeaponClassesAffectedByPowerThrow);
            }

            if (itemObject.ItemType == ItemObject.ItemTypeEnum.Shield)
            {
                var shield = itemObject.WeaponComponent.PrimaryWeapon;

                float durabilityFactor = skills.GetPropertyValue(CrpgSkills.Shield) * 0.16f;
                var durability = (short)ReflectionHelper.GetProperty(shield, nameof(WeaponComponentData.MaxDataValue));
                ReflectionHelper.SetProperty(shield, nameof(WeaponComponentData.MaxDataValue), (short)(durability + durability * durabilityFactor));

                float speedFactor = skills.GetPropertyValue(CrpgSkills.Shield) * 0.03f;
                var speed = (int)ReflectionHelper.GetProperty(shield, nameof(WeaponComponentData.ThrustSpeed));
                ReflectionHelper.SetProperty(shield, nameof(WeaponComponentData.ThrustSpeed), (int)(speed + speed * speedFactor));
            }

            return itemObject;
        }

        private static void ModifyDamage(ItemObject itemObject, float factor, HashSet<WeaponClass> affectedClasses)
        {
            foreach (var weapon in itemObject.Weapons)
            {
                if (affectedClasses.Contains(weapon.WeaponClass))
                {
                    // It seems like damage is used by missiles and melee use damage factor. Scale both in any case.

                    var swingDamage = (int)ReflectionHelper.GetProperty(weapon, nameof(WeaponComponentData.SwingDamage));
                    ReflectionHelper.SetProperty(weapon, nameof(WeaponComponentData.SwingDamage), swingDamage + (int)(swingDamage * factor));

                    var thrustDamage = (int)ReflectionHelper.GetProperty(weapon, nameof(WeaponComponentData.ThrustDamage));
                    ReflectionHelper.SetProperty(weapon, nameof(WeaponComponentData.ThrustDamage), thrustDamage + (int)(thrustDamage * factor));

                    var swingDamageFactor = (float)ReflectionHelper.GetProperty(weapon, nameof(WeaponComponentData.SwingDamageFactor));
                    ReflectionHelper.SetProperty(weapon, nameof(WeaponComponentData.SwingDamageFactor), swingDamageFactor + swingDamageFactor * factor);

                    var thrustDamageFactor = (float)ReflectionHelper.GetProperty(weapon, nameof(WeaponComponentData.ThrustDamageFactor));
                    ReflectionHelper.SetProperty(weapon, nameof(WeaponComponentData.ThrustDamageFactor), thrustDamageFactor + thrustDamageFactor * factor);
                }
            }
        }

        private static readonly HashSet<WeaponClass> WeaponClassesAffectedByPowerStrike = new HashSet<WeaponClass>
        {
             WeaponClass.Dagger,
             WeaponClass.OneHandedSword,
             WeaponClass.TwoHandedSword,
             WeaponClass.OneHandedAxe,
             WeaponClass.TwoHandedAxe,
             WeaponClass.Mace,
             WeaponClass.Pick,
             WeaponClass.TwoHandedMace,
             WeaponClass.OneHandedPolearm,
             WeaponClass.TwoHandedPolearm,
             WeaponClass.LowGripPolearm,
        };

        private static readonly HashSet<WeaponClass> WeaponClassesAffectedByPowerDraw = new HashSet<WeaponClass>
        {
             WeaponClass.Bow,
        };

        private static readonly HashSet<WeaponClass> WeaponClassesAffectedByPowerThrow = new HashSet<WeaponClass>
        {
             WeaponClass.Stone,
             WeaponClass.Boulder,
             WeaponClass.ThrowingAxe,
             WeaponClass.ThrowingKnife,
             WeaponClass.Javelin,
        };
    }
}
