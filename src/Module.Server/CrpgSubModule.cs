using System.Xml.Linq;
using Crpg.Module.Battle;
using Crpg.Module.Common;
using Crpg.Module.Common.Models;
using Newtonsoft.Json;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

#if CRPG_CLIENT
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.TwoDimension;
#endif

#if CRPG_EXPORT
using Crpg.Module.DataExport;
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
        AddCrpgXmls();
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
        basicGameStarter.AddModel(new CrpgAgentApplyDamageModel(_constants));
    }

    private void AddCrpgXmls()
    {
        // Add the singleplayer items xml files to the resources so that they are loaded as the same time as the multiplayer ones.
        XmlResource.XmlInformationList.AddRange(new MbObjectXmlInformation[]
        {
#if CRPG_CLIENT
            new("Items", "items", "SandBoxCore", new List<string>()),
            new("CraftingPieces", "crafting_pieces", "Native", new List<string>()),
#else
            new("Items", "items", "cRPG", new List<string>()),
            new("CraftingPieces", "crafting_pieces", "cRPG", new List<string>()),
#endif
        });
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
        const string outputPath = "../../CrpgData";
        IDataExporter[] exporters =
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
#endif
}
