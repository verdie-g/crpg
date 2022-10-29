using System.Xml.Linq;
using Crpg.Module.Battle;
using Crpg.Module.Common;
using Crpg.Module.Common.Models;
using Newtonsoft.Json;
using TaleWorlds.Core;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;

#if CRPG_CLIENT
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.TwoDimension;
#endif

#if CRPG_EXPORT
using System.Runtime.CompilerServices;
using Crpg.Module.DataExport;
using TaleWorlds.Library;
using TaleWorlds.Localization;
#endif

namespace Crpg.Module;

internal class CrpgSubModule : MBSubModuleBase
{
    private CrpgConstants _constants = default!;

    protected override void OnSubModuleLoad()
    {
        base.OnSubModuleLoad();

        _constants = LoadCrpgConstants();
        LoadSpriteSheets();
        TaleWorlds.MountAndBlade.Module.CurrentModule.AddMultiplayerGameMode(new CrpgBattleGameMode(_constants));

#if CRPG_SERVER
        // Disable culture vote during intermission because there is no concept of cultures in cRPG.
        MultiplayerIntermissionVotingManager.Instance.IsCultureVoteEnabled = false;
#endif

#if CRPG_EXPORT
        TaleWorlds.MountAndBlade.Module.CurrentModule.AddInitialStateOption(new InitialStateOption("ExportData",
            new TextObject("Export Data"), 4578, ExportData, () => (false, null)));
#endif

        // Uncomment to start watching UI changes.
        // UIResourceManager.UIResourceDepot.StartWatchingChangesInDepot();
    }

    protected override void InitializeGameStarter(Game game, IGameStarter starterObject)
    {
        base.InitializeGameStarter(game, starterObject);
        InitializeGameModels(starterObject);
        CrpgSkills.Initialize(game);
        CrpgBannerEffects.Initialize(game);
        ManagedParameters.Instance.Initialize(ModuleHelper.GetXmlPath("Crpg", "managed_core_parameters"));
#if CRPG_CLIENT
        game.GameTextManager.LoadGameTexts();
#endif
    }

    protected override void OnApplicationTick(float delta)
    {
        base.OnApplicationTick(delta);
        // Uncomment to hot reload UI after changes.
        // UIResourceManager.UIResourceDepot.CheckForChanges();
    }

    private CrpgConstants LoadCrpgConstants()
    {
        string path = ModuleHelper.GetModuleFullPath("cRPG") + "ModuleData/constants.json";
        return JsonConvert.DeserializeObject<CrpgConstants>(File.ReadAllText(path));
    }

    private void InitializeGameModels(IGameStarter basicGameStarter)
    {
        basicGameStarter.AddModel(new CrpgAgentStatCalculateModel(_constants));
        basicGameStarter.AddModel(new CrpgItemValueModel());
        basicGameStarter.AddModel(new CrpgAgentApplyDamageModel(_constants));
        basicGameStarter.AddModel(new CrpgStrikeMagnitudeModel(_constants));
    }

    /// <summary>
    /// This method loads the sprites from the Assets folder. Doing this manually is surprising but it is done like
    /// that in TaleWorlds.MountAndBlade.GauntletUI.GauntletUISubModule.OnSubModuleLoad.
    /// </summary>
    private void LoadSpriteSheets()
    {
#if CRPG_CLIENT
        string guiPath = Path.Combine(ModuleHelper.GetModuleFullPath("cRPG"), "GUI");
        foreach (string filename in Directory.GetFiles(guiPath, "*SpriteData.xml", SearchOption.AllDirectories))
        {
            var spriteDataDoc = XDocument.Load(filename);
            foreach (XElement spriteCategoryNode in spriteDataDoc.Root!.Descendants("SpriteCategory"))
            {
                string spriteCategoryName = spriteCategoryNode.Element("Name")!.Value;
                SpriteCategory spriteCategory = UIResourceManager.SpriteData.SpriteCategories[spriteCategoryName];
                spriteCategory.Load(UIResourceManager.ResourceContext, UIResourceManager.UIResourceDepot);
            }
        }
#endif
    }

#if CRPG_EXPORT
    private void ExportData()
    {
        IDataExporter[] exporters =
        {
            new ItemExporter(),
            // new SettlementExporter(),
        };

        InformationManager.DisplayMessage(new InformationMessage("Exporting data."));
        string gitRepoPath = FindGitRepositoryRootPath();
        Task.WhenAll(exporters.Select(e => e.Export(gitRepoPath))).ContinueWith(t =>
        {
            InformationManager.DisplayMessage(t.IsFaulted
                ? new InformationMessage(t.Exception!.Message)
                : new InformationMessage("Done."));
        });
    }

    private string FindGitRepositoryRootPath([CallerFilePath] string currentFilePath = default!)
    {
        var dir = Directory.GetParent(currentFilePath);
        while (dir != null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, ".git")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not find cRPG git repository");
    }
#endif
}
