using Crpg.Module.Common;
using Crpg.Module.Modes.Warmup;
using Crpg.Module.Rewards;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Source.Missions;

#if CRPG_SERVER
using Crpg.Module.Api;
using Crpg.Module.Common.ChatCommands;
#else
using Crpg.Module.GUI;
using Crpg.Module.GUI.HudExtension;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
#endif

namespace Crpg.Module.Modes.TeamDeathmatch;

[ViewCreatorModule] // Exposes methods with ViewMethod attribute.
internal class CrpgTeamDeathmatchGameMode : MissionBasedMultiplayerGameMode
{
    private const string GameName = "cRPGTeamDeathmatch";

    private static CrpgConstants _constants = default!; // Static so it's accessible from the views.

    public CrpgTeamDeathmatchGameMode(CrpgConstants constants)
        : base(GameName)
    {
        _constants = constants;
    }

#if CRPG_CLIENT
    [ViewMethod(GameName)]
    public static MissionView[] OpenCrpgTeamDeathmatch(Mission mission)
    {
        CrpgExperienceTable experienceTable = new(_constants);
        MissionMultiplayerGameModeBaseClient gameModeClient = mission.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();

        return new[]
        {
            ViewCreator.CreateMissionServerStatusUIHandler(),
            ViewCreator.CreateMissionAgentStatusUIHandler(mission),
            ViewCreator.CreateMissionMainAgentEquipmentController(mission), // Pick/drop items.
            ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
            ViewCreatorManager.CreateMissionView<CrpgMissionMultiplayerEscapeMenu>(isNetwork: false, null, "TeamDeathmatch", gameModeClient),
            ViewCreator.CreateMissionAgentLabelUIHandler(mission),
            ViewCreator.CreateMultiplayerTeamSelectUIHandler(),
            ViewCreator.CreateMissionScoreBoardUIHandler(mission, false),
            ViewCreator.CreateMultiplayerEndOfRoundUIHandler(),
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
        CrpgNotificationComponent notificationsComponent = new();
        CrpgScoreboardComponent scoreboardComponent = new(new TDMScoreboardData());

#if CRPG_SERVER
        ICrpgClient crpgClient = CrpgClient.Create();
        ChatBox chatBox = Game.Current.GetGameHandler<ChatBox>();
        CrpgWarmupComponent warmupComponent = new(_constants, notificationsComponent,
            () => (new TeamDeathmatchSpawnFrameBehavior(), new CrpgTeamDeathmatchSpawningBehavior(_constants)));
        CrpgRewardServer rewardServer = new(crpgClient, _constants, warmupComponent, enableTeamHitCompensations: false);
#else
        CrpgWarmupComponent warmupComponent = new(_constants, notificationsComponent, null);
#endif

        MissionState.OpenNew(GameName,
            new MissionInitializerRecord(scene) { SceneUpgradeLevel = 3, SceneLevels = string.Empty },
            _ => new MissionBehavior[]
            {
                MissionLobbyComponent.CreateBehavior(),
#if CRPG_CLIENT
                new CrpgUserManagerClient(), // Needs to be loaded before the Client mission part.
#endif
                warmupComponent,
                new CrpgTeamDeathmatchClient(),
                new MultiplayerTimerComponent(),
                new MultiplayerTeamSelectComponent(),
                new MissionHardBorderPlacer(),
                new MissionBoundaryPlacer(),
                new MissionBoundaryCrossingHandler(),
                new MultiplayerPollComponent(),
                new MultiplayerAdminComponent(),
                notificationsComponent,
                new MissionOptionsComponent(),
                scoreboardComponent,
                new MissionAgentPanicHandler(),
                new AgentHumanAILogic(),
                new EquipmentControllerLeaveLogic(),
                new MultiplayerPreloadHelper(),
                new WelcomeMessageBehavior(warmupComponent),

                // Shit that need to stay because BL code is extremely coupled to the visual spawning.
                new MultiplayerMissionAgentVisualSpawnComponent(),
                new MissionLobbyEquipmentNetworkComponent(),
#if CRPG_SERVER
                new CrpgTeamDeathmatchServer(scoreboardComponent, rewardServer),
                new SpawnComponent(new TeamDeathmatchSpawnFrameBehavior(), new CrpgTeamDeathmatchSpawningBehavior(_constants)),
                new CrpgUserManagerServer(crpgClient, _constants),
                new KickInactiveBehavior(inactiveTimeLimit: 30, warmupComponent),
                new MapPoolComponent(),
                new ChatCommandsComponent(chatBox, crpgClient),
                new CrpgActivityLogsBehavior(warmupComponent, chatBox, crpgClient),
                new PlayerStatsComponent(),
                new NotAllPlayersReadyComponent(),
                new DrowningBehavior(),
#else
                new MultiplayerAchievementComponent(),
                new MissionMatchHistoryComponent(),
                new MissionRecentPlayersComponent(),
                new CrpgRewardClient(),
#endif
            });
    }
}
