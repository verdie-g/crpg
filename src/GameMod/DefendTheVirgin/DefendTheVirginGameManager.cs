using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Crpg.GameMod.Api;
using Crpg.GameMod.Api.Models.Characters;
using Crpg.GameMod.Api.Models.Items;
using Crpg.GameMod.Api.Models.Users;
using Crpg.GameMod.Common;
using Crpg.GameMod.Helpers;
using Crpg.Sdk;
using Crpg.Sdk.Abstractions;
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
        private static readonly IRandom Rng = new ThreadSafeRandom();

        private readonly ICrpgClient _crpgClient = new CrpgHttpClient();

        private Task<CrpgUser>? _getUserTask;
        private Task<IList<CrpgItem>>? _getItemsTask;
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
                    _getItemsTask = GetCrpgItems();
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
                    nextStep = _getUserTask!.IsCompleted && _getItemsTask!.IsCompleted
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

            if (_getItemsTask!.IsFaulted)
            {
                MBDebug.Print(_getItemsTask.Exception!.ToString());
                return;
            }

            LoadItems(_getItemsTask.Result, MBObjectManager.Instance);

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
            // The real id seems to be Id2 for Steam and GOG, not sure about Epic
            string platformUserId = login.GetPlayerId().Id2.ToString();
            string userName = login.GetUserName();

            var res = await _crpgClient.GetUser(platform, platformUserId, userName);
            return res.Data!;
        }

        private async Task<IList<CrpgItem>> GetCrpgItems()
        {
            var res = await _crpgClient.GetItems();
            return res.Data!;
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

        /// <summary>
        /// For each cRPG item get its template from the <see cref="MBObjectManager"/> and create a new object by
        /// overriding all fields with the ones of the cRPG item.
        /// </summary>
        private void LoadItems(IList<CrpgItem> crpgItems, MBObjectManager objectManager)
        {
            foreach (var crpgItem in crpgItems)
            {
                var mbItem = objectManager.GetObject<ItemObject>(crpgItem.TemplateMbId);
                if (mbItem == null)
                {
                    // TODO: log warning.
                    continue;
                }

                // No way to create an item programmatically so use reflection.
                mbItem = ReflectionHelper.DeepClone(mbItem);
                ReflectionHelper.SetProperty(mbItem, nameof(ItemObject.StringId), "crpg_" + crpgItem.Id);
                ReflectionHelper.SetProperty(mbItem, nameof(ItemObject.Name), new TextObject(crpgItem.Name));
                ReflectionHelper.SetProperty(mbItem, nameof(ItemObject.Value), crpgItem.Value);
                ReflectionHelper.SetProperty(mbItem, nameof(ItemObject.Weight), crpgItem.Weight);

                if (crpgItem.Armor != null)
                {
                    if (mbItem.ArmorComponent == null)
                    {
                        // TODO: log warning
                    }
                    else
                    {
                        ReflectionHelper.SetProperty(mbItem.ArmorComponent, nameof(ArmorComponent.HeadArmor), crpgItem.Armor.HeadArmor);
                        ReflectionHelper.SetProperty(mbItem.ArmorComponent, nameof(ArmorComponent.BodyArmor), crpgItem.Armor.BodyArmor);
                        ReflectionHelper.SetProperty(mbItem.ArmorComponent, nameof(ArmorComponent.ArmArmor), crpgItem.Armor.ArmArmor);
                        ReflectionHelper.SetProperty(mbItem.ArmorComponent, nameof(ArmorComponent.LegArmor), crpgItem.Armor.LegArmor);
                    }
                }

                if (crpgItem.Mount != null)
                {
                    if (mbItem.HorseComponent == null)
                    {
                        // TODO: log warning
                    }
                    else
                    {
                        ReflectionHelper.SetProperty(mbItem.HorseComponent, nameof(HorseComponent.BodyLength), crpgItem.Mount.BodyLength);
                        ReflectionHelper.SetProperty(mbItem.HorseComponent, nameof(HorseComponent.ChargeDamage), crpgItem.Mount.ChargeDamage);
                        ReflectionHelper.SetProperty(mbItem.HorseComponent, nameof(HorseComponent.Maneuver), crpgItem.Mount.Maneuver);
                        ReflectionHelper.SetProperty(mbItem.HorseComponent, nameof(HorseComponent.Speed), crpgItem.Mount.Speed);
                        ReflectionHelper.SetProperty(mbItem.HorseComponent.Monster, nameof(Monster.HitPoints), crpgItem.Mount.HitPoints);
                    }
                }

                if (crpgItem.Weapons != null)
                {
                    if (mbItem.Weapons == null || crpgItem.Weapons.Count != mbItem.Weapons.Count)
                    {
                        // TODO: log warning
                    }
                    else
                    {
                        for (int i = 0; i < crpgItem.Weapons.Count; i += 1)
                        {
                            ReflectionHelper.SetProperty(mbItem.Weapons[i], nameof(WeaponComponentData.Accuracy), crpgItem.Weapons[i].Accuracy);
                            ReflectionHelper.SetProperty(mbItem.Weapons[i], nameof(WeaponComponentData.MissileSpeed), crpgItem.Weapons[i].MissileSpeed);
                            ReflectionHelper.SetProperty(mbItem.Weapons[i], nameof(WeaponComponentData.MaxDataValue), (short)crpgItem.Weapons[i].StackAmount);
                            ReflectionHelper.SetProperty(mbItem.Weapons[i], nameof(WeaponComponentData.WeaponBalance), crpgItem.Weapons[i].Balance);
                            // If we want to override length here we should also override things like CenterOfMass according to WeaponComponentData.Deserialize.
                            ReflectionHelper.SetProperty(mbItem.Weapons[i], nameof(WeaponComponentData.BodyArmor), crpgItem.Weapons[i].BodyArmor);
                            ReflectionHelper.SetProperty(mbItem.Weapons[i], nameof(WeaponComponentData.ThrustDamage), crpgItem.Weapons[i].ThrustDamage);
                            ReflectionHelper.SetProperty(mbItem.Weapons[i], nameof(WeaponComponentData.ThrustSpeed), crpgItem.Weapons[i].ThrustSpeed);
                            // According to WeaponComponentData.Deserialize Handling = ThrustSpeed.
                            ReflectionHelper.SetProperty(mbItem.Weapons[i], nameof(WeaponComponentData.Handling), crpgItem.Weapons[i].ThrustSpeed);
                            ReflectionHelper.SetProperty(mbItem.Weapons[i], nameof(WeaponComponentData.SwingDamage), crpgItem.Weapons[i].SwingDamage);
                            ReflectionHelper.SetProperty(mbItem.Weapons[i], nameof(WeaponComponentData.SwingSpeed), crpgItem.Weapons[i].SwingSpeed);

                            // Damage factors are computed from swing and thrust damage so this method needs to be called.
                            ReflectionHelper.InvokeMethod(mbItem.Weapons[i], "SetDamageFactors", new object[] { crpgItem.Weight });
                        }
                    }
                }

                MBObjectManager.Instance.RegisterObject(mbItem);
            }
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
            skills.SetPropertyValue(CrpgSkills.MountedArchery, crpgCharacter.Statistics.Skills.MountedArchery);

            skills.SetPropertyValue(DefaultSkills.OneHanded, crpgCharacter.Statistics.WeaponProficiencies.OneHanded);
            skills.SetPropertyValue(DefaultSkills.TwoHanded, crpgCharacter.Statistics.WeaponProficiencies.TwoHanded);
            skills.SetPropertyValue(DefaultSkills.Polearm, crpgCharacter.Statistics.WeaponProficiencies.Polearm);
            skills.SetPropertyValue(DefaultSkills.Bow, crpgCharacter.Statistics.WeaponProficiencies.Bow);
            skills.SetPropertyValue(DefaultSkills.Crossbow, crpgCharacter.Statistics.WeaponProficiencies.Crossbow);
            skills.SetPropertyValue(DefaultSkills.Throwing, crpgCharacter.Statistics.WeaponProficiencies.Throwing);

            var equipment = new Equipment();
            foreach (var equippedItem in crpgCharacter.EquippedItems)
            {
                var index = ItemSlotToIndex[equippedItem.Slot];
                AddEquipment(equipment, index, "crpg_" + equippedItem.Item.Id, skills);
            }

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
                    var swingDamage = (int)ReflectionHelper.GetProperty(weapon, nameof(WeaponComponentData.SwingDamage));
                    ReflectionHelper.SetProperty(weapon, nameof(WeaponComponentData.SwingDamage), swingDamage + (int)(swingDamage * factor));

                    var thrustDamage = (int)ReflectionHelper.GetProperty(weapon, nameof(WeaponComponentData.ThrustDamage));
                    ReflectionHelper.SetProperty(weapon, nameof(WeaponComponentData.ThrustDamage), thrustDamage + (int)(thrustDamage * factor));

                    // It seems like damage is used by missiles and melee use damage factor. Damage factors need to be recomputed.
                    ReflectionHelper.InvokeMethod(weapon, "SetDamageFactors", new object[] { itemObject.Weight });
                }
            }
        }

        private static readonly Dictionary<CrpgItemSlot, EquipmentIndex> ItemSlotToIndex = new Dictionary<CrpgItemSlot, EquipmentIndex>
        {
            [CrpgItemSlot.Head] = EquipmentIndex.Head,
            [CrpgItemSlot.Shoulder] = EquipmentIndex.Cape,
            [CrpgItemSlot.Body] = EquipmentIndex.Body,
            [CrpgItemSlot.Hand] = EquipmentIndex.Gloves,
            [CrpgItemSlot.Leg] = EquipmentIndex.Leg,
            [CrpgItemSlot.MountHarness] = EquipmentIndex.HorseHarness,
            [CrpgItemSlot.Mount] = EquipmentIndex.Horse,
            [CrpgItemSlot.Weapon0] = EquipmentIndex.Weapon0,
            [CrpgItemSlot.Weapon1] = EquipmentIndex.Weapon1,
            [CrpgItemSlot.Weapon2] = EquipmentIndex.Weapon2,
            [CrpgItemSlot.Weapon3] = EquipmentIndex.Weapon3,
        };

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
