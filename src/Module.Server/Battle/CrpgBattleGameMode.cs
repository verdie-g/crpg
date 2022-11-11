using Crpg.Module.Common;
using Crpg.Module.Common.Warmup;
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

namespace Crpg.Module.Battle;

[ViewCreatorModule] // Exposes methods with ViewMethod attribute.
internal class CrpgBattleGameMode : MissionBasedMultiplayerGameMode
{
    private const string BattleGameName = "cRPGBattle";
    private const string SkirmishGameName = "cRPGSkirmish";

    private static CrpgConstants _constants = default!; // Static so it's accessible from the views.

    private readonly bool _isSkirmish;

    public CrpgBattleGameMode(CrpgConstants constants, bool isSkirmish)
        : base(isSkirmish ? SkirmishGameName : BattleGameName)
    {
        _constants = constants;
        _isSkirmish = isSkirmish;
    }

#if CRPG_CLIENT
    // Used by MissionState.OpenNew that finds all methods having a ViewMethod attribute contained in class
    // having a ViewCreatorModule attribute.
    [ViewMethod(BattleGameName)]
    public static MissionView[] OpenCrpgBattle(Mission mission) => OpenCrpgBattleOrSkirmish(mission);
    [ViewMethod(SkirmishGameName)]
    public static MissionView[] OpenCrpgSkirmish(Mission mission) => OpenCrpgBattleOrSkirmish(mission);

    [ViewMethod("")] // All static instances in ViewCreatorModule classes are expected to have a ViewMethod attribute.
    private static MissionView[] OpenCrpgBattleOrSkirmish(Mission mission)
    {
        CrpgExperienceTable experienceTable = new(_constants);
        MissionMultiplayerGameModeBaseClient gameModeClient = mission.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
        MissionView crpgEscapeMenu = ViewCreatorManager.CreateMissionView<CrpgMissionMultiplayerEscapeMenu>(isNetwork: false, null, "Battle", gameModeClient);

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
            ViewCreator.CreateMultiplayerMissionHUDExtensionUIHandler(),
            ViewCreator.CreateMultiplayerMissionDeathCardUIHandler(),
            ViewCreator.CreateOptionsUIHandler(),
            ViewCreator.CreateMissionMainAgentEquipDropView(mission),
            ViewCreator.CreateMissionBoundaryCrossingView(),
            new MissionBoundaryWallView(),
            new SpectatorCameraView(),
            new CrpgAgentHud(experienceTable),
            // Draw flags but also player names when pressing ALT. (Native: CreateMissionFlagMarkerUIHandler)
            ViewCreatorManager.CreateMissionView<CrpgMultiplayerMarkerUIHandler>(isNetwork: false, null, gameModeClient),
        };
    }
#endif

    public override void StartMultiplayerGame(string scene)
    {
        // Inherits the MultiplayerGameNotificationsComponent component.
        // used to send notifications (e.g. flag captured, round won) to peer
        CrpgNotificationComponent notificationsComponent = new();

#if CRPG_SERVER
        CrpgHttpClient crpgClient = new();
        ChatBox chatBox = Game.Current.GetGameHandler<ChatBox>();

        MultiplayerRoundController roundController = new(); // starts/stops round, ends match
        CrpgWarmupComponent warmupComponent = new(_constants, notificationsComponent, () =>
            (new FlagDominationSpawnFrameBehavior(), _isSkirmish
                ? new CrpgSkirmishSpawningBehavior(_constants, roundController)
                : new CrpgBattleSpawningBehavior(_constants, roundController)));
#else
        CrpgWarmupComponent warmupComponent = new(_constants, notificationsComponent, null);
#endif
        CrpgFlagDominationMissionMultiplayerClient flagDominationClient = new(_isSkirmish);

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
                    flagDominationClient,
                    new MultiplayerTimerComponent(), // round timer
                    new MultiplayerMissionAgentVisualSpawnComponent(), // expose method to spawn an agent
                    new MissionLobbyEquipmentNetworkComponent(), // logic to change troop or perks
                    new NoTeamSelectComponent(), // logic to change team, autoselect
                    new MissionHardBorderPlacer(),
                    new MissionBoundaryPlacer(), // set walkable boundaries
                    new AgentVictoryLogic(), // AI cheering when winning round
                    new MissionBoundaryCrossingHandler(), // kills agent out of mission boundaries
                    new MultiplayerPollComponent(), // poll logic to kick player, ban player, change game
                    new MissionOptionsComponent(),
                    new MissionScoreboardComponent(new BattleScoreboardData()), // score board
                    new MissionAgentPanicHandler(),
                    new EquipmentControllerLeaveLogic(),
                    new MultiplayerPreloadHelper(),
                    warmupComponent,
                    notificationsComponent,
#if CRPG_SERVER
                    roundController,
                    new CrpgFlagDominationMissionMultiplayer(flagDominationClient, _isSkirmish),
                    new CrpgRewardServer(crpgClient, _constants, warmupComponent, roundController),
                    // SpawnFrameBehaviour: where to spawn, SpawningBehaviour: when to spawn
                    new SpawnComponent(new BattleSpawnFrameBehavior(),
                        _isSkirmish ? new CrpgSkirmishSpawningBehavior(_constants, roundController) : new CrpgBattleSpawningBehavior(_constants, roundController)),
                    new AgentHumanAILogic(), // bot intelligence
                    new MultiplayerAdminComponent(), // admin UI to kick player or restart game
                    new CrpgUserManagerServer(crpgClient),
                    new KickInactiveBehavior(warmupComponent, notificationsComponent),
                    new MapVoteComponent(),
                    new ChatCommandsComponent(chatBox, crpgClient),
#else
                    new MultiplayerRoundComponent(),
                    new MissionMatchHistoryComponent(),
                    new CrpgRewardClient(),
#endif
                });
    }
}
