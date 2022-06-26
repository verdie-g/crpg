using System.Xml.Linq;
using Crpg.Module.Battle;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

#if CRPG_CLIENT
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.TwoDimension;
#endif

#if CRPG_EXPORT
using Crpg.Module.DataExport;
#endif

namespace Crpg.Module;

internal class CrpgSubModule : MBSubModuleBase
{
    protected override void OnSubModuleLoad()
    {
        base.OnSubModuleLoad();

        TaleWorlds.MountAndBlade.Module.CurrentModule.AddMultiplayerGameMode(new CrpgBattleGameMode());

#if CRPG_EXPORT
        Module.CurrentModule.AddInitialStateOption(new InitialStateOption("ExportData",
            new TextObject("Export Data"), 4578, ExportData, () => (false, null)));
#endif

        // Uncomment to start watching UI changes.
        // UIResourceManager.UIResourceDepot.StartWatchingChangesInDepot();
    }

    protected override void OnApplicationTick(float delta)
    {
        base.OnApplicationTick(delta);
        // Uncomment to hot reload UI after changes.
        // UIResourceManager.UIResourceDepot.CheckForChanges();
    }

    /// <summary>
    /// This method loads the sprites from the Assets folder. Doing this manually is surprising but it is done like
    /// that in TaleWorlds.MountAndBlade.GauntletUI.GauntletUISubModule.OnSubModuleLoad.
    /// </summary>
    private static void LoadSpriteSheets()
    {
#if CRPG_CLIENT
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
#endif
    }

#if CRPG_EXPORT
    private static void ExportData()
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
