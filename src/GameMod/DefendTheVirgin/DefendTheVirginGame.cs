using System.Collections.Generic;
using Crpg.GameMod.Common;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace Crpg.GameMod.DefendTheVirgin
{
    public class DefendTheVirginGame : GameType
    {
        public override void OnDestroy()
        {
        }

        public override void OnStateChanged(GameState oldState)
        {
        }

        protected override void OnInitialize()
        {
            Game currentGame = CurrentGame;
            currentGame.FirstInitialize(false);

            InitializeGameTexts(currentGame.GameTextManager);
            IGameStarter gameStarter = new BasicGameStarter();
            InitializeGameModels(gameStarter);
            GameManager.OnGameStart(CurrentGame, gameStarter);
            MBObjectManager objectManager = currentGame.ObjectManager;
            currentGame.SecondInitialize(gameStarter.Models);

            currentGame.CreateGameManager();
            GameManager.BeginGameStart(CurrentGame);
            CurrentGame.ThirdInitialize();

            currentGame.CreateObjects();
            currentGame.InitializeDefaultGameObjects();
            CrpgSkills.Initialize(currentGame);
            currentGame.LoadBasicFiles(false);
            LoadCustomGameXmls();
            objectManager.ClearEmptyObjects();
            currentGame.SetDefaultEquipments(new Dictionary<string, Equipment>());
            currentGame.CreateLists();
            objectManager.ClearEmptyObjects();
            GameManager.OnCampaignStart(CurrentGame, null);
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
            basicGameStarter.AddModel(new CustomBattleAgentStatCalculateModel());
            basicGameStarter.AddModel(new CustomBattleApplyWeatherEffectsModel());
            basicGameStarter.AddModel(new MultiplayerAgentApplyDamageModel());
            basicGameStarter.AddModel(new DefaultRidingModel());
            basicGameStarter.AddModel(new DefaultStrikeMagnitudeModel());
            basicGameStarter.AddModel(new CrpgSkillList());
            basicGameStarter.AddModel(new CustomBattleMoraleModel());
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
}
