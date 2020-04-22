using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using Newtonsoft.Json;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using Module = TaleWorlds.MountAndBlade.Module;
using Path = System.IO.Path;
using Steamworks;
using TaleWorlds.Library;
using Crpg.GameMod.Api;
using Crpg.GameMod.Api.Requests;
using Crpg.GameMod.Api.Responses;
using System.Threading.Tasks;

namespace Crpg.GameMod
{
    public class CrpgSubModule : MBSubModuleBase
    {
        //[DllImport("Rgl.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "?toggle_imgui_console_visibility@rglCommand_line_manager@@QEAAXXZ")]
        //public static extern void Toggle_imgui_console_visibility(UIntPtr x);
        public override void BeginGameStart(Game game)
        {
            InformationManager.DisplayMessage(new InformationMessage("BeginGameStart"));
            base.BeginGameStart(game);
        }
        public override void OnMultiplayerGameStart(Game game, object starterObject)
        {
            InformationManager.DisplayMessage(new InformationMessage("OnMultiplayerGameStart"));
            base.OnMultiplayerGameStart(game, starterObject);
            //game.AddGameHandler<OfflineMultiplayerGameHandler>();
        }
        public override void OnGameInitializationFinished(Game game)
        {
            InformationManager.DisplayMessage(new InformationMessage("OnGameInitializationFinished"));
            base.OnGameInitializationFinished(game);
        }
        public override void OnMissionBehaviourInitialize(Mission mission)
        {
            InformationManager.DisplayMessage(new InformationMessage("OnMissionBehaviourInitialize"));
            string steamname = SteamFriends.GetPersonaName();
            InformationManager.DisplayMessage(new InformationMessage("OnMissionBehaviourInitialize" + steamname));
            string steamid = SteamUser.GetSteamID().ToString();
            InformationManager.DisplayMessage(new InformationMessage("OnMissionBehaviourInitialize" + steamid));
            base.OnMissionBehaviourInitialize(mission);
            //mission.AddMissionBehaviour(new MissionComponent());
        }
        protected override void OnSubModuleLoad()
        {
           
            InformationManager.DisplayMessage(new InformationMessage("OnSubModuleLoad"));
            Module.CurrentModule.AddMultiplayerGameMode(new CrpgBattleMissionBasedMultiplayerGamemode("ClassicBattle"));
            InformationManager.DisplayMessage(new InformationMessage("ClassicBattle"));

            //Module.CurrentModule.GetMultiplayerGameTypes().First(x => x.GameType == "ClassicBattle").Scenes.Add("mp_troop_test_10m");
            //Module.CurrentModule.GetMultiplayerGameTypes().First(x => x.GameType == "ClassicBattle").Scenes.Add("mp_troop_test_50m");
            //Module.CurrentModule.GetMultiplayerGameTypes().First(x => x.GameType == "ClassicBattle").Scenes.Add("mp_troop_test_90m");
            Module.CurrentModule.GetMultiplayerGameTypes().First(x => x.GameType == "ClassicBattle").Scenes.Add("mp_sergeant_map_007");
            Module.CurrentModule.GetMultiplayerGameTypes().First(x => x.GameType == "ClassicBattle").Scenes.Add("mp_sergeant_map_008");
            Module.CurrentModule.GetMultiplayerGameTypes().First(x => x.GameType == "ClassicBattle").Scenes.Add("mp_sergeant_map_009");
            Module.CurrentModule.GetMultiplayerGameTypes().First(x => x.GameType == "ClassicBattle").Scenes.Add("mp_sergeant_map_012");
            Module.CurrentModule.GetMultiplayerGameTypes().First(x => x.GameType == "ClassicBattle").Scenes.Add("mp_sergeant_map_vlandia_01");
            Module.CurrentModule.GetMultiplayerGameTypes().First(x => x.GameType == "ClassicBattle").Scenes.Add("mp_skirmish_spawn_test");
            Module.CurrentModule.GetMultiplayerGameTypes().First(x => x.GameType == "ClassicBattle").Scenes.Add("mp_skirmish_map_002f");
            Module.CurrentModule.GetMultiplayerGameTypes().First(x => x.GameType == "ClassicBattle").Scenes.Add("mp_skirmish_map_003_skinc");
            Module.CurrentModule.GetMultiplayerGameTypes().First(x => x.GameType == "ClassicBattle").Scenes.Add("mp_skirmish_map_007");
            Module.CurrentModule.GetMultiplayerGameTypes().First(x => x.GameType == "ClassicBattle").Scenes.Add("mp_skirmish_map_013");
            Module.CurrentModule.GetMultiplayerGameTypes().First(x => x.GameType == "ClassicBattle").Scenes.Add("mp_sergeant_map_013");

            base.OnSubModuleLoad();

            /*Module.CurrentModule.AddInitialStateOption(new InitialStateOption("Dump Items", new TextObject("Dump Items"), 9990, () =>
            {
                DumpItems();
                InformationManager.DisplayMessage(new InformationMessage("Exporting items to " + Path.GetFullPath(OutputPath)));
            }, false));*/
        }
    }
}
