using Crpg.GameMod.Common;
using TaleWorlds.Core;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace Crpg.GameMod.DefendTheVirgin;

internal class DefendTheVirginGame : GameType
{
    private readonly CrpgConstants _crpgConstants;

    public DefendTheVirginGame(CrpgConstants crpgConstants)
    {
        _crpgConstants = crpgConstants;
    }

    public override void OnDestroy()
    {
    }

    public override void OnStateChanged(GameState oldState)
    {
    }

    protected override void OnInitialize()
    {
        GameTextManager gameTextManager = CurrentGame.GameTextManager;
        InitializeGameTexts(gameTextManager);
        IGameStarter gameStarter = new BasicGameStarter();
        InitializeGameModels(gameStarter);
        GameManager.InitializeGameStarter(CurrentGame, gameStarter);
        GameManager.OnGameStart(CurrentGame, gameStarter);
        MBObjectManager objectManager = CurrentGame.ObjectManager;
        CurrentGame.SetBasicModels(gameStarter.Models);
        CurrentGame.CreateGameManager();
        GameManager.BeginGameStart(CurrentGame);
        CurrentGame.SetRandomGenerators();
        CurrentGame.InitializeDefaultGameObjects();
        CrpgSkills.Initialize(CurrentGame);
        CurrentGame.LoadBasicFiles();
        LoadCustomGameXmls();
        objectManager.UnregisterNonReadyObjects();
        CurrentGame.SetDefaultEquipments(new Dictionary<string, Equipment>());
        objectManager.UnregisterNonReadyObjects();
        GameManager.OnNewCampaignStart(CurrentGame, null);
        GameManager.OnAfterCampaignStart(CurrentGame);
        GameManager.OnGameInitializationFinished(CurrentGame);
    }

    protected override void BeforeRegisterTypes(MBObjectManager objectManager)
    {
    }

    protected override void OnRegisterTypes(MBObjectManager objectManager)
    {
        objectManager.RegisterType<BasicCharacterObject>("NPCCharacter", "NPCCharacters", 43U);
        objectManager.RegisterType<BasicCultureObject>("Culture", "SPCultures", 17U);
    }

    protected override void DoLoadingForGameType(
        GameTypeLoadingStates gameTypeLoadingState,
        out GameTypeLoadingStates nextState)
    {
        nextState = GameTypeLoadingStates.None;
        switch (gameTypeLoadingState)
        {
            case GameTypeLoadingStates.InitializeFirstStep:
                CurrentGame.Initialize();
                nextState = GameTypeLoadingStates.WaitSecondStep;
                break;
            case GameTypeLoadingStates.WaitSecondStep:
                nextState = GameTypeLoadingStates.LoadVisualsThirdState;
                break;
            case GameTypeLoadingStates.LoadVisualsThirdState:
                nextState = GameTypeLoadingStates.PostInitializeFourthState;
                break;
        }
    }

    private void InitializeGameModels(IGameStarter basicGameStarter)
    {
        basicGameStarter.AddModel(new MultiplayerAgentDecideKilledOrUnconsciousModel());
        basicGameStarter.AddModel(new CrpgAgentStatCalculateModel(_crpgConstants));
        basicGameStarter.AddModel(new CustomBattleApplyWeatherEffectsModel());
        basicGameStarter.AddModel(new CrpgAgentApplyDamageModel(_crpgConstants));
        basicGameStarter.AddModel(new DefaultRidingModel());
        basicGameStarter.AddModel(new DefaultStrikeMagnitudeModel());
        basicGameStarter.AddModel(new CustomBattleMoraleModel());
        basicGameStarter.AddModel(new DefaultDamageParticleModel());
    }

    private void InitializeGameTexts(GameTextManager gameTextManager)
    {
        gameTextManager.LoadGameTexts(ModuleHelper.GetModuleFullPath("Native") + "ModuleData/multiplayer_strings.xml");
        gameTextManager.LoadGameTexts(ModuleHelper.GetModuleFullPath("Native") + "ModuleData/global_strings.xml");
        gameTextManager.LoadGameTexts(ModuleHelper.GetModuleFullPath("Native") + "ModuleData/module_strings.xml");
        gameTextManager.LoadGameTexts(ModuleHelper.GetModuleFullPath("Native") + "ModuleData/native_strings.xml");
    }

    private void LoadCustomGameXmls()
    {
        ObjectManager.LoadXML("Items");
        ObjectManager.LoadXML("EquipmentRosters");
        ObjectManager.LoadXML("NPCCharacters");
        ObjectManager.LoadXML("SPCultures");
    }
}
