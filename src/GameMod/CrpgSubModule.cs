using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Crpg.GameMod.DataExport;
using Crpg.GameMod.DefendTheVirgin;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.TwoDimension;
using Module = TaleWorlds.MountAndBlade.Module;
using Path = System.IO.Path;

namespace Crpg.GameMod
{
    internal class CrpgSubModule : MBSubModuleBase
    {
        /// <summary>
        /// Called immediately after loading the selected game mode (submodule) has completed.
        /// </summary>
        public override void BeginGameStart(Game game)
        {
            base.BeginGameStart(game);
        }

        /// <summary>
        /// Called once any game mode is started.
        /// </summary>
        public override void OnCampaignStart(Game game, object starterObject)
        {
            base.OnCampaignStart(game, starterObject);
        }

        /// <summary>
        /// Called once the initialisation for a game mode has finished.
        /// </summary>
        public override void OnGameInitializationFinished(Game game)
        {
            base.OnGameInitializationFinished(game);
        }

        /// <summary>
        /// Called seemingly as loading is ending, not entirely sure of this one.
        /// </summary>
        public override bool DoLoading(Game game)
        {
            return base.DoLoading(game);
        }

        /// <summary>
        /// Called when starting a new save in the campaign mode specifically.
        /// </summary>
        public override void OnNewGameCreated(Game game, object initializerObject)
        {
            base.OnNewGameCreated(game, initializerObject);
        }

        /// <summary>
        /// Called once a mission is started and behaviours are to be initialized.
        /// </summary>
        public override void OnMissionBehaviourInitialize(Mission mission)
        {
            base.OnMissionBehaviourInitialize(mission);
        }

        public override void OnMultiplayerGameStart(Game game, object starterObject)
        {
            base.OnMultiplayerGameStart(game, starterObject);
        }

        /// <summary>
        /// Called on exiting out of a mission/campaign.
        /// </summary>
        public override void OnGameEnd(Game game)
        {
            base.OnGameEnd(game);
        }

        /// <summary>
        /// Called during the first loading screen of the game, always the first override to be called, this is where
        /// you should be doing the bulk of your initial setup.
        /// </summary>
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            LoadSpriteSheets();

            Module.CurrentModule.AddInitialStateOption(new InitialStateOption("DefendTheVirgin",
                new TextObject("{=4gpGhbeJ}Defend The Virgin"),
                4567,
                () => MBGameManager.StartNewGame(new DefendTheVirginGameManager()),
                () => (false, new TextObject())));

#if false
            Module.CurrentModule.AddInitialStateOption(new InitialStateOption("ExportData",
                new TextObject("Export Data"), 4578, ExportData, () => false));
#endif

            // Uncomment to start watching UI changes.
            // UIResourceManager.UIResourceDepot.StartWatchingChangesInDepot();

            /*
            Module.CurrentModule.AddMultiplayerGameMode(new CrpgBattleGameMode());

            // Game mode needs scenes, else selecting it in UI crashes
            // TODO: move to an xml file
            Module.CurrentModule.GetMultiplayerGameTypes()
                .First(gti => gti.GameType == CrpgBattleGameMode.GameModeName)
                .Scenes.Add("mp_skirmish_spawn_test");
            */
        }

        /// <summary>
        /// Called just before the main menu first appears, helpful if your mod depends on other things being set up
        /// during the initial load.
        /// </summary>
        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();
        }

        /// <summary>
        /// Called immediately upon loading after selecting a game mode (submodule) from the main menu.
        /// </summary>
        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);
        }

        /// <summary>
        /// This is called once every frame, you should avoid expensive operations being called directly here and
        /// instead do as little work as possible for performance reasons.
        /// </summary>
        /// <param name="delta">The time in milliseconds the last frame took to complete.</param>
        protected override void OnApplicationTick(float delta)
        {
            base.OnApplicationTick(delta);
            // Uncomment to hot reload UI after changes.
            // UIResourceManager.UIResourceDepot.CheckForChanges();
        }

        /// <summary>
        /// Called when exiting Bannerlord entirely.
        /// </summary>
        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();
        }

        /// <summary>
        /// This method loads the sprites from the Assets folder. Doing this manually is surprising but it is done like
        /// that in TaleWorlds.MountAndBlade.GauntletUI.GauntletUISubModule.OnSubModuleLoad.
        /// </summary>
        private static void LoadSpriteSheets()
        {
            foreach (string filename in Directory.GetFiles(BasePath.Name + "Modules/cRPG/GUI", "*SpriteData.xml", SearchOption.AllDirectories))
            {
                var spriteDataDoc = XDocument.Load(filename);
                foreach (XElement spriteCategoryNode in spriteDataDoc.Root!.Descendants("SpriteCategory"))
                {
                    string spriteCategoryName = spriteCategoryNode.Element("Name")!.Value;
                    SpriteCategory spriteCategory = UIResourceManager.SpriteData.SpriteCategories[spriteCategoryName];
                    spriteCategory.Load(UIResourceManager.ResourceContext, UIResourceManager.UIResourceDepot);
                }
            }
        }

        private static void ExportData()
        {
            const string outputPath = "../../CrpgData";
            var exporters = new IDataExporter[]
            {
                new ItemExporter(),
                new SettlementExporter(),
            };

            Directory.CreateDirectory(outputPath);
            InformationManager.DisplayMessage(new InformationMessage($"Exporting data to {Path.GetFullPath(outputPath)}."));
            Task.WhenAll(exporters.Select(e => e.Export(outputPath))).ContinueWith(t =>
            {
                InformationManager.DisplayMessage(t.IsFaulted
                    ? new InformationMessage(t.Exception!.Message)
                    : new InformationMessage("Done."));
            });
        }
    }
}
