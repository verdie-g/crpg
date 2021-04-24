﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Crpg.GameMod.Api;
using Crpg.GameMod.Api.Models.Characters;
using Crpg.GameMod.Api.Models.Items;
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
        private static readonly Random Rng = new();

        private readonly ICrpgClient _crpgClient = new CrpgHttpClient();

        private Task<CrpgUser>? _getUserTask;
        private Task<IList<CrpgItem>>? _getItemsTask;
        private CrpgConstants? _crpgConstants;
        private WaveGroup[][]? _waves;

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

            InformationManager.DisplayMessage(new InformationMessage("Visit c-rpg.eu to upgrade your character."));

            var waveController = new WaveController(_waves!.Length);
            var crpgUserAccessor = new CrpgUserAccessor(_getUserTask.Result);
            var character = CreateCharacter(crpgUserAccessor.User.Character, _crpgConstants!);
            var waveSpawnLogic = new WaveSpawnLogic(waveController, _waves!, character);
            var crpgLogic = new CrpgLogic(waveController, _crpgClient, _waves!, crpgUserAccessor);
            var crpgExperienceTable = new CrpgExperienceTable(_crpgConstants!);

            // First argument, missionName, is used to find missionViews. In ViewCreatorManager.CheckAssemblyScreens
            // it gets all methods with an attribute ViewMethod(missionName) in all classes with a ViewCreatorModule
            // attribute.
            MissionState.OpenNew("DefendTheVirgin", new MissionInitializerRecord("empire_village_007")
            {
                DoNotUseLoadingScreen = false,
                PlayingInCampaignMode = false,
                AtmosphereOnCampaign = GetRandomAtmosphere(),
                SceneLevels = string.Empty,
                TimeOfDay = 6f,
            }, missionController => new MissionBehaviour[]
            {
                new MissionCombatantsLogic(),
                waveController,
                crpgUserAccessor,
                waveSpawnLogic,
                crpgLogic,
                crpgExperienceTable,
                new AgentBattleAILogic(),
                new MissionHardBorderPlacer(),
                new MissionBoundaryPlacer(),
                new MissionBoundaryCrossingHandler(),
                new AgentFadeOutLogic(),
            });
        }

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
                    _crpgConstants = LoadCrpgConstants();
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

        private WaveGroup[][] LoadWaves()
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

        private CrpgConstants LoadCrpgConstants()
        {
            string path = BasePath.Name + "Modules/cRPG/ModuleData/constants.json";
            return JsonConvert.DeserializeObject<CrpgConstants>(File.ReadAllText(path));
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
                            ReflectionHelper.SetProperty(mbItem.Weapons[i], nameof(WeaponComponentData.Handling), crpgItem.Weapons[i].Handling);
                            ReflectionHelper.SetProperty(mbItem.Weapons[i], nameof(WeaponComponentData.BodyArmor), crpgItem.Weapons[i].BodyArmor);

                            float thrustDamageFactor = mbItem.Weapons[i].ThrustDamageFactor * crpgItem.Weapons[i].ThrustDamage / mbItem.Weapons[i].ThrustDamage;
                            ReflectionHelper.SetProperty(mbItem.Weapons[i], nameof(WeaponComponentData.ThrustDamage), crpgItem.Weapons[i].ThrustDamage);
                            ReflectionHelper.SetProperty(mbItem.Weapons[i], nameof(WeaponComponentData.ThrustDamageFactor), thrustDamageFactor);
                            ReflectionHelper.SetProperty(mbItem.Weapons[i], nameof(WeaponComponentData.ThrustSpeed), crpgItem.Weapons[i].ThrustSpeed);

                            float swingDamageFactor = mbItem.Weapons[i].SwingDamageFactor * crpgItem.Weapons[i].SwingDamage / mbItem.Weapons[i].SwingDamage;
                            ReflectionHelper.SetProperty(mbItem.Weapons[i], nameof(WeaponComponentData.SwingDamage), crpgItem.Weapons[i].SwingDamage);
                            ReflectionHelper.SetProperty(mbItem.Weapons[i], nameof(WeaponComponentData.SwingDamageFactor), swingDamageFactor);
                            ReflectionHelper.SetProperty(mbItem.Weapons[i], nameof(WeaponComponentData.SwingSpeed), crpgItem.Weapons[i].SwingSpeed);
                        }
                    }
                }

                MBObjectManager.Instance.RegisterObject(mbItem);
            }
        }

        private AtmosphereInfo GetRandomAtmosphere()
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

        private BasicCharacterObject CreateCharacter(CrpgCharacter crpgCharacter, CrpgConstants constants)
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
                AddEquipment(equipment, index, "crpg_" + equippedItem.Item.Id, skills, constants);
            }

            return CrpgCharacterObject.New(new TextObject(crpgCharacter.Name), skills, equipment, constants);
        }

        private void AddEquipment(Equipment equipments, EquipmentIndex idx, string? itemId,
            CharacterSkills skills, CrpgConstants constants)
        {
            var itemObject = GetModifiedItem(itemId, skills, constants);
            var equipmentElement = new EquipmentElement(itemObject);
            equipments.AddEquipmentToSlotWithoutAgent(idx, equipmentElement);
        }

        private ItemObject? GetModifiedItem(string? itemId, CharacterSkills skills, CrpgConstants constants)
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
                float powerStrikeFactor = MathHelper.ApplyPolynomialFunction(skills.GetPropertyValue(CrpgSkills.PowerStrike), constants.DamageFactorForPowerStrikeCoefs);
                ModifyDamage(itemObject, powerStrikeFactor, WeaponClassesAffectedByPowerStrike);
                float powerDrawFactor = MathHelper.ApplyPolynomialFunction(skills.GetPropertyValue(CrpgSkills.PowerDraw), constants.DamageFactorForPowerDrawCoefs);
                ModifyDamage(itemObject, powerDrawFactor, WeaponClassesAffectedByPowerDraw);
                float powerThrowFactor = MathHelper.ApplyPolynomialFunction(skills.GetPropertyValue(CrpgSkills.PowerThrow), constants.DamageFactorForPowerThrowCoefs);
                ModifyDamage(itemObject, powerThrowFactor, WeaponClassesAffectedByPowerThrow);
            }

            if (itemObject.ItemType == ItemObject.ItemTypeEnum.Shield)
            {
                var shield = itemObject.WeaponComponent.PrimaryWeapon;

                float durabilityFactor = MathHelper.ApplyPolynomialFunction(skills.GetPropertyValue(CrpgSkills.Shield), constants.DurabilityFactorForShieldCoefs);
                var durability = (short)ReflectionHelper.GetProperty(shield, nameof(WeaponComponentData.MaxDataValue));
                ReflectionHelper.SetProperty(shield, nameof(WeaponComponentData.MaxDataValue), (short)(durability * durabilityFactor));

                float speedFactor = MathHelper.ApplyPolynomialFunction(skills.GetPropertyValue(CrpgSkills.Shield), constants.SpeedFactorForShieldCoefs);
                var speed = (int)ReflectionHelper.GetProperty(shield, nameof(WeaponComponentData.ThrustSpeed));
                ReflectionHelper.SetProperty(shield, nameof(WeaponComponentData.ThrustSpeed), (int)(speed * speedFactor));
            }

            return itemObject;
        }

        private void ModifyDamage(ItemObject itemObject, float factor, HashSet<WeaponClass> affectedClasses)
        {
            foreach (var weapon in itemObject.Weapons)
            {
                if (affectedClasses.Contains(weapon.WeaponClass))
                {
                    // It seems like damage is used by missiles and melee use damage factor. Scale both in any case.
                    // According to TaleWorlds.Core.Crafting.CalculateSwingBaseDamage, damage is calculated from damage
                    // factor and other fields like weight or speed. Scaling both damage and damage factor they same way
                    // might not work as expected.
                    var swingDamage = (int)ReflectionHelper.GetProperty(weapon, nameof(WeaponComponentData.SwingDamage));
                    ReflectionHelper.SetProperty(weapon, nameof(WeaponComponentData.SwingDamage), (int)(swingDamage * factor));

                    var swingDamageFactor = (float)ReflectionHelper.GetProperty(weapon, nameof(WeaponComponentData.SwingDamageFactor));
                    ReflectionHelper.SetProperty(weapon, nameof(WeaponComponentData.SwingDamageFactor), swingDamageFactor * factor);

                    var thrustDamage = (int)ReflectionHelper.GetProperty(weapon, nameof(WeaponComponentData.ThrustDamage));
                    ReflectionHelper.SetProperty(weapon, nameof(WeaponComponentData.ThrustDamage), (int)(thrustDamage * factor));

                    var thrustDamageFactor = (float)ReflectionHelper.GetProperty(weapon, nameof(WeaponComponentData.ThrustDamageFactor));
                    ReflectionHelper.SetProperty(weapon, nameof(WeaponComponentData.ThrustDamageFactor), thrustDamageFactor * factor);
                }
            }
        }

        private static readonly Dictionary<CrpgItemSlot, EquipmentIndex> ItemSlotToIndex = new()
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

        private static readonly HashSet<WeaponClass> WeaponClassesAffectedByPowerStrike = new()
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

        private static readonly HashSet<WeaponClass> WeaponClassesAffectedByPowerDraw = new()
        {
             WeaponClass.Bow,
        };

        private static readonly HashSet<WeaponClass> WeaponClassesAffectedByPowerThrow = new()
        {
             WeaponClass.Stone,
             WeaponClass.Boulder,
             WeaponClass.ThrowingAxe,
             WeaponClass.ThrowingKnife,
             WeaponClass.Javelin,
        };
    }
}
