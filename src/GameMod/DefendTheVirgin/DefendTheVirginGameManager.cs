using Crpg.GameMod.Api;
using Crpg.GameMod.Api.Models.Characters;
using Crpg.GameMod.Api.Models.Items;
using Crpg.GameMod.Api.Models.Users;
using Crpg.GameMod.Common;
using Newtonsoft.Json;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.ObjectSystem;
using TaleWorlds.PlatformService;
using Platform = Crpg.GameMod.Api.Models.Users.Platform;

namespace Crpg.GameMod.DefendTheVirgin;

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
        }, missionController => new MissionBehavior[]
        {
            new MissionOptionsComponent(),
            new MissionCombatantsLogic(),
            waveController,
            crpgUserAccessor,
            waveSpawnLogic,
            crpgLogic,
            crpgExperienceTable,
            new AgentHumanAILogic(),
            new MissionHardBorderPlacer(),
            new MissionBoundaryPlacer(),
            new MissionBoundaryCrossingHandler(),
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
                Game.CreateGame(new DefendTheVirginGame(_crpgConstants), this).DoLoading();
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
        var login = await PlatformServices.Instance.CreateLobbyClientLoginProvider();
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
            AddEquipment(equipment, index, equippedItem.Item.TemplateMbId);
        }

        return CrpgCharacterObject.New(new TextObject(crpgCharacter.Name), skills, equipment, constants);
    }

    private void AddEquipment(Equipment equipments, EquipmentIndex idx, string? itemId)
    {
        var itemObject = MBObjectManager.Instance.GetObject<ItemObject>(itemId);
        var equipmentElement = new EquipmentElement(itemObject);
        equipments.AddEquipmentToSlotWithoutAgent(idx, equipmentElement);
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
}
