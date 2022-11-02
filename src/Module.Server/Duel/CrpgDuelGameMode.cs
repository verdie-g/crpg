using Crpg.Module.Common;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Source.Missions;

#if CRPG_SERVER
using Crpg.Module.Api;
using Crpg.Module.Common.ChatCommands;
#else
using Crpg.Module.GUI;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
#endif

namespace Crpg.Module.Duel;

[ViewCreatorModule] // Exposes methods with ViewMethod attribute.
internal class CrpgDuelGameMode : MissionBasedMultiplayerGameMode
{
    public const string GameName = "cRPGDuel";

    private static CrpgConstants _constants = default!; // Static so it's accessible from the views.

    public CrpgDuelGameMode(CrpgConstants constants)
        : base(GameName)
    {
        _constants = constants;
    }

#if CRPG_CLIENT
    // Used by MissionState.OpenNew that finds all methods having a ViewMethod attribute contained in class
    // having a ViewCreatorModule attribute.
    [ViewMethod(GameName)]
    public static MissionView[] OpenCrpgDuel(Mission mission)
    {
        CrpgExperienceTable experienceTable = new(_constants);
        MissionMultiplayerGameModeBaseClient gameModeClient = mission.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
        MissionView crpgEscapeMenu = ViewCreatorManager.CreateMissionView<CrpgMissionMultiplayerEscapeMenu>(isNetwork: false, null, "Duel", gameModeClient);
        MissionView altKeyMarkerView = ViewCreatorManager.CreateMissionView<CrpgMultiplayerMarkerUIHandler>(false, null, gameModeClient);
        return new[]
        {
            ViewCreator.CreateMissionServerStatusUIHandler(),
            ViewCreator.CreateMissionMultiplayerPreloadView(mission),
            ViewCreator.CreateMissionKillNotificationUIHandler(),
            ViewCreator.CreateMissionAgentStatusUIHandler(mission),
            ViewCreator.CreateMissionMainAgentEquipmentController(mission),
            ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
            crpgEscapeMenu,
            ViewCreator.CreateMultiplayerEndOfBattleUIHandler(),
            ViewCreator.CreateMissionScoreBoardUIHandler(mission, true),
            ViewCreator.CreateLobbyEquipmentUIHandler(),
            ViewCreator.CreateMissionMultiplayerDuelUI(),
            ViewCreator.CreatePollProgressUIHandler(),
            ViewCreator.CreateOptionsUIHandler(),
            ViewCreator.CreateMissionMainAgentEquipDropView(mission),
            ViewCreator.CreateMissionBoundaryCrossingView(),
            new MissionBoundaryWallView(),
            new MissionItemContourControllerView(),
            new MissionAgentContourControllerView(),
            new CrpgAgentHud(experienceTable),
            altKeyMarkerView,
        };
    }
#endif

    public override void StartMultiplayerGame(string scene)
    {
#if CRPG_SERVER
        CrpgHttpClient crpgClient = new();
        ChatBox chatBox = Game.Current.GetGameHandler<ChatBox>();
        CrpgDuelMissionMultiplayer duelMission = new(crpgClient);
#endif
        CrpgDuelMissionMultiplayerClient duelClient = new();
        MissionState.OpenNew(
            Name,
            new MissionInitializerRecord(scene),
            missionController =>
                new MissionBehavior[]
                {
                    MissionLobbyComponent.CreateBehavior(),
#if CRPG_CLIENT
                    new CrpgUserManagerClient(), // Needs to be loaded before the Client mission part.
#endif
                    duelClient,
                    new MultiplayerTimerComponent(), // round timer
                    new CrpgNotificationComponent(), // Inherits the MultiplayerGameNotificationsComponent component.
                    new MultiplayerMissionAgentVisualSpawnComponent(), // expose method to spawn an agent
                    new ConsoleMatchStartEndHandler(),
                    new MissionLobbyEquipmentNetworkComponent(), // logic to change troop or perks
                    new NoTeamSelectComponent(), // logic to change team, autoselect
                    new MissionHardBorderPlacer(),
                    new MissionBoundaryPlacer(), // set walkable boundaries
                    new MissionBoundaryCrossingHandler(), // kills agent out of mission boundaries
                    new MultiplayerPollComponent(), // poll logic to kick player, ban player, change game
                    new MissionOptionsComponent(),
                    new MissionScoreboardComponent(new DuelScoreboardData()), // score board
                    new MultiplayerPreloadHelper(),
#if CRPG_SERVER
                    duelMission,
                    // SpawnFrameBehaviour: where to spawn, SpawningBehaviour: when to spawn
                    new SpawnComponent(new DuelSpawnFrameBehavior(), new CrpgDuelSpawningBehavior(_constants, duelMission)),
                    new MultiplayerAdminComponent(), // admin UI to kick player or restart game
                    new MissionAgentPanicHandler(),
                    new AgentHumanAILogic(), // bot intelligence
                    new EquipmentControllerLeaveLogic(),
                    new CrpgUserManagerServer(crpgClient),
                    new ChatCommandsComponent(chatBox, crpgClient),
#else
                    new MultiplayerAchievementComponent(),
                    new MissionMatchHistoryComponent(),
                    new MissionRecentPlayersComponent(),
#endif
                });
    }
}
