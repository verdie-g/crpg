using Crpg.Module.Common;
using Crpg.Module.Common.TeamSelect;
using Crpg.Module.Modes.Warmup;
using Crpg.Module.Notifications;
using Crpg.Module.Rewards;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Source.Missions;

#if CRPG_SERVER
using Crpg.Module.Api;
using Crpg.Module.Common.ChatCommands;
#else
using Crpg.Module.Common.HotConstants;
using Crpg.Module.GUI;
using Crpg.Module.GUI.HudExtension;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
#endif

namespace Crpg.Module.Modes.Dtv;

[ViewCreatorModule] // Exposes methods with ViewMethod attribute.
internal class CrpgDtvGameMode : MissionBasedMultiplayerGameMode
{
    private const string GameName = "cRPGDTV";

    private static CrpgConstants _constants = default!; // Static so it's accessible from the views.

    public CrpgDtvGameMode(CrpgConstants constants)
        : base(GameName)
    {
        _constants = constants;
    }

#if CRPG_CLIENT
    // Used by MissionState.OpenNew that finds all methods having a ViewMethod attribute contained in class
    // having a ViewCreatorModule attribute.
    [ViewMethod(GameName)]
    public static MissionView[] OpenCrpgDtv(Mission mission)
    {
        CrpgExperienceTable experienceTable = new(_constants);
        MissionMultiplayerGameModeBaseClient gameModeClient = mission.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
        MissionView crpgEscapeMenu = ViewCreatorManager.CreateMissionView<CrpgMissionMultiplayerEscapeMenu>(isNetwork: false, null, "cRPGDTV", gameModeClient);

        return new[]
        {
            ViewCreator.CreateMultiplayerFactionBanVoteUIHandler(),
            ViewCreator.CreateMissionAgentStatusUIHandler(mission),
            ViewCreator.CreateMissionMainAgentEquipmentController(mission), // Pick/drop items.
            ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
            crpgEscapeMenu,
            ViewCreator.CreateMissionAgentLabelUIHandler(mission),
            ViewCreator.CreateMultiplayerTeamSelectUIHandler(),
            ViewCreator.CreateMissionScoreBoardUIHandler(mission, false),
            ViewCreator.CreateMultiplayerEndOfBattleUIHandler(),
            ViewCreator.CreatePollProgressUIHandler(),
            new MissionItemContourControllerView(), // Draw contour of item on the ground when pressing ALT.
            new MissionAgentContourControllerView(),
            ViewCreator.CreateMissionKillNotificationUIHandler(),
            new CrpgHudExtensionHandler(),
            ViewCreator.CreateMultiplayerMissionDeathCardUIHandler(),
            ViewCreator.CreateOptionsUIHandler(),
            ViewCreator.CreateMissionMainAgentEquipDropView(mission),
            ViewCreator.CreateMissionBoundaryCrossingView(),
            new MissionBoundaryWallView(),
            new SpectatorCameraView(),
            new CrpgAgentHud(experienceTable),
            // Draw flags but also player names when pressing ALT. (Native: CreateMissionFlagMarkerUIHandler)
            ViewCreatorManager.CreateMissionView<CrpgMarkerUiHandler>(isNetwork: false, null, gameModeClient),
        };
    }
#endif

    public override void StartMultiplayerGame(string scene)
    {
        // Inherits the MultiplayerGameNotificationsComponent component.
        // used to send notifications (e.g. flag captured, round won) to peer
        CrpgNotificationComponent notificationsComponent = new();

#if CRPG_SERVER
        ICrpgClient crpgClient = CrpgClient.Create();
        ChatBox chatBox = Game.Current.GetGameHandler<ChatBox>();

        CrpgWarmupComponent warmupComponent = new(_constants, notificationsComponent, () =>
            (new FlagDominationSpawnFrameBehavior(),
            new CrpgDtvSpawningBehavior(_constants)));
        CrpgTeamSelectServerComponent teamSelectComponent = new(warmupComponent, null);
        CrpgRewardServer rewardServer = new(crpgClient, _constants, warmupComponent, enableTeamHitCompensations: false, enableRating: false);
        CrpgDtvSpawningBehavior spawnBehaviour = new(_constants);
#else
        CrpgWarmupComponent warmupComponent = new(_constants, notificationsComponent, null);
        CrpgTeamSelectClientComponent teamSelectComponent = new();
#endif
        CrpgDtvClient dtvClient = new();
        MissionState.OpenNew(
            Name,
            new MissionInitializerRecord(scene),
            _ => new MissionBehavior[]
            {
                MissionLobbyComponent.CreateBehavior(),
#if CRPG_CLIENT
                new CrpgUserManagerClient(), // Needs to be loaded before the Client mission part.
#endif
                dtvClient,
                new MultiplayerTimerComponent(), // round timer
                new MultiplayerMissionAgentVisualSpawnComponent(), // expose method to spawn an agent
                new MissionLobbyEquipmentNetworkComponent(), // logic to change troop or perks
                teamSelectComponent,
                new MissionHardBorderPlacer(),
                new MissionBoundaryPlacer(), // set walkable boundaries
                new AgentVictoryLogic(), // AI cheering when winning round
                new MissionBoundaryCrossingHandler(), // kills agent out of mission boundaries
                new MultiplayerPollComponent(), // poll logic to kick player, ban player, change game
                new MissionOptionsComponent(),
                new CrpgScoreboardComponent(new BattleScoreboardData()),
                new MissionAgentPanicHandler(),
                new EquipmentControllerLeaveLogic(),
                new MultiplayerPreloadHelper(),
                warmupComponent,
                notificationsComponent,
                new WelcomeMessageBehavior(warmupComponent),
#if CRPG_SERVER
                new CrpgDtvServer(rewardServer),
                rewardServer,
                // SpawnFrameBehaviour: where to spawn, SpawningBehaviour: when to spawn
                new SpawnComponent(new BattleSpawnFrameBehavior(), spawnBehaviour),
                new AgentHumanAILogic(), // bot intelligence
                new MultiplayerAdminComponent(), // admin UI to kick player or restart game
                new CrpgUserManagerServer(crpgClient, _constants),
                new KickInactiveBehavior(inactiveTimeLimit: 120, warmupComponent),
                new MapPoolComponent(),
                new ChatCommandsComponent(chatBox, crpgClient),
                new CrpgActivityLogsBehavior(warmupComponent, chatBox, crpgClient),
                new ServerMetricsBehavior(),
                new NotAllPlayersReadyComponent(),
                new DrowningBehavior(),
#else
                new MultiplayerAchievementComponent(),
                new MissionMatchHistoryComponent(),
                new MissionRecentPlayersComponent(),
                new CrpgRewardClient(),
                new HotConstantsClient(),
#endif
            });
    }
}
