using Crpg.Module.Battle;
using Crpg.Module.Common;
using Crpg.Module.Common.Models;
using Crpg.Module.Duel;
using Crpg.Module.Siege;
using Newtonsoft.Json;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;

#if CRPG_SERVER
using Crpg.Module.HarmonyPatches;
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

        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        _constants = LoadCrpgConstants();
        TaleWorlds.MountAndBlade.Module.CurrentModule.AddMultiplayerGameMode(new CrpgBattleGameMode(_constants, isSkirmish: true));
        TaleWorlds.MountAndBlade.Module.CurrentModule.AddMultiplayerGameMode(new CrpgBattleGameMode(_constants, isSkirmish: false));
        TaleWorlds.MountAndBlade.Module.CurrentModule.AddMultiplayerGameMode(new CrpgSiegeGameMode(_constants));
        TaleWorlds.MountAndBlade.Module.CurrentModule.AddMultiplayerGameMode(new CrpgDuelGameMode(_constants));

#if CRPG_SERVER
        CrpgServerConfiguration.Init();
        BannerlordPatches.Apply();
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
        return JsonConvert.DeserializeObject<CrpgConstants>(File.ReadAllText(path))!;
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
    {
        Debug.Print(args.ExceptionObject.ToString(), color: Debug.DebugColor.Red);
    }

    private void InitializeGameModels(IGameStarter basicGameStarter)
    {
        basicGameStarter.AddModel(new CrpgAgentStatCalculateModel(_constants));
        basicGameStarter.AddModel(new CrpgItemValueModel());
        basicGameStarter.AddModel(new CrpgAgentApplyDamageModel(_constants));
        basicGameStarter.AddModel(new CrpgStrikeMagnitudeModel(_constants));
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
