using Crpg.Module.Common;
using Crpg.Module.Common.HotConstants;
using Crpg.Module.Modes.Skirmish;
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

namespace Crpg.Module.Modes.DTV;

[ViewCreatorModule] // Exposes methods with ViewMethod attribute.
internal class CrpgDTVGameMode : MissionBasedMultiplayerGameMode
{
    private const string GameName = "cRPGDTV";

    private static CrpgConstants _constants = default!; // Static so it's accessible from the views.

    private readonly bool _isSkirmish;

    public CrpgDTVGameMode(CrpgConstants constants, bool isSkirmish)
        : base(GameName)
    {
        _constants = constants;
        _isSkirmish = isSkirmish;
    }

#if CRPG_CLIENT
    // Used by MissionState.OpenNew that finds all methods having a ViewMethod attribute contained in class
    // having a ViewCreatorModule attribute.
    [ViewMethod(GameName)]
    private static MissionView[] OpenCrpgDTV(Mission mission)
    {
        CrpgExperienceTable experienceTable = new(_constants);
        MissionMultiplayerGameModeBaseClient gameModeClient = mission.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
        MissionView crpgEscapeMenu = ViewCreatorManager.CreateMissionView<CrpgMissionMultiplayerEscapeMenu>(isNetwork: false, null, "cRPG Defend the Virgin", gameModeClient);

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
        // Inherits the MultiplayerGameNotificationsComponent component.
        // used to send notifications (e.g. flag captured, round won) to peer
        CrpgNotificationComponent notificationsComponent = new();

#if CRPG_SERVER
        ICrpgClient crpgClient = CrpgClient.Create();
        ChatBox chatBox = Game.Current.GetGameHandler<ChatBox>();

        MultiplayerRoundController roundController = new(); // starts/stops round, ends match
        CrpgWarmupComponent warmupComponent = new(_constants, notificationsComponent, () =>
            (new FlagDominationSpawnFrameBehavior(), _isSkirmish
                ? new CrpgSkirmishSpawningBehavior(_constants, roundController)
                : new CrpgDTVSpawningBehavior(_constants, roundController)));
        CrpgTeamSelectComponent teamSelectComponent = new(warmupComponent, roundController);
        CrpgRewardServer rewardServer = new(crpgClient, _constants, warmupComponent, enableTeamHitCompensations: true);
#else
        CrpgWarmupComponent warmupComponent = new(_constants, notificationsComponent, null);
        CrpgTeamSelectComponent teamSelectComponent = new();
#endif
        CrpgDTVClient dtvClient = new(_isSkirmish);

        MissionState.OpenNew(
            Name,
            new MissionInitializerRecord(scene),
            _ =>
                new MissionBehavior[]
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
                    new CrpgScoreboardComponent(_isSkirmish ? new CrpgSkirmishScoreboardData() : new BattleScoreboardData()),
                    new MissionAgentPanicHandler(),
                    new EquipmentControllerLeaveLogic(),
                    new MultiplayerPreloadHelper(),
                    warmupComponent,
                    notificationsComponent,
                    new WelcomeMessageBehavior(warmupComponent),
#if CRPG_SERVER
                    roundController,
                    new CrpgDTVServer(dtvClient, _isSkirmish, rewardServer),
                    rewardServer,
                    // SpawnFrameBehaviour: where to spawn, SpawningBehaviour: when to spawn
                    new SpawnComponent(new BattleSpawnFrameBehavior(),
                        _isSkirmish ? new CrpgSkirmishSpawningBehavior(_constants, roundController) : new CrpgDTVSpawningBehavior(_constants, roundController)),
                    new AgentHumanAILogic(), // bot intelligence
                    new MultiplayerAdminComponent(), // admin UI to kick player or restart game
                    new CrpgUserManagerServer(crpgClient, _constants),
                    new KickInactiveBehavior(inactiveTimeLimit: 3600, warmupComponent),
                    new MapPoolComponent(),
                    new ChatCommandsComponent(chatBox, crpgClient),
                    new CrpgActivityLogsBehavior(warmupComponent, chatBox, crpgClient),
                    new PlayerStatsComponent(),
                    new NotAllPlayersReadyComponent(),
                    new DrowningBehavior(),
#else
                    new MultiplayerRoundComponent(),
                    new MultiplayerAchievementComponent(),
                    new MissionMatchHistoryComponent(),
                    new MissionRecentPlayersComponent(),
                    new CrpgRewardClient(),
                    new HotConstantsClient(),
#endif
                });
    }
}
