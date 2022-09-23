using Crpg.Module.Common;
using Crpg.Module.Common.Warmup;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Source.Missions;

#if CRPG_SERVER
using Crpg.Module.Api;
#else
using Crpg.Module.GUI;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;
#endif

namespace Crpg.Module.Battle;

[ViewCreatorModule] // Exposes methods with ViewMethod attribute.
internal class CrpgBattleGameMode : MissionBasedMultiplayerGameMode
{
    public const string GameName = "cRPGBattle";

    private static CrpgConstants _constants = default!; // Static so it's accessible from the views.

    public CrpgBattleGameMode(CrpgConstants constants)
        : base(GameName)
    {
        _constants = constants;
    }

#if CRPG_CLIENT
    // Used by MissionState.OpenNew that finds all methods having a ViewMethod attribute contained in class
    // having a ViewCreatorModule attribute.
    [ViewMethod(GameName)]
    public static MissionView[] OpenCrpgBattle(Mission mission)
    {
        CrpgExperienceTable experienceTable = new(_constants);
        return new[]
        {
            ViewCreator.CreateMultiplayerFactionBanVoteUIHandler(),
            ViewCreator.CreateMissionAgentStatusUIHandler(mission),
            ViewCreator.CreateMissionMainAgentEquipmentController(mission), // Pick/drop items.
            ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
            ViewCreator.CreateMissionMultiplayerEscapeMenu("Battle"),
            ViewCreator.CreateMissionAgentLabelUIHandler(mission),
            ViewCreator.CreateMultiplayerTeamSelectUIHandler(),
            ViewCreator.CreateMissionScoreBoardUIHandler(mission, false),
            ViewCreator.CreateMultiplayerEndOfRoundUIHandler(),
            ViewCreator.CreateMultiplayerEndOfBattleUIHandler(),
            ViewCreator.CreatePollProgressUIHandler(),
            new MissionItemContourControllerView(), // Draw contour of item on the ground when pressing ALT.
            new MissionAgentContourControllerView(),
            ViewCreator.CreateMissionFlagMarkerUIHandler(), // Draw flags but also player names when pressing ALT.
            ViewCreator.CreateMissionKillNotificationUIHandler(),
            ViewCreator.CreateMultiplayerMissionHUDExtensionUIHandler(),
            ViewCreator.CreateMultiplayerMissionDeathCardUIHandler(),
            ViewCreator.CreateOptionsUIHandler(),
            ViewCreator.CreateMissionMainAgentEquipDropView(mission),
            ViewCreator.CreateMissionBoundaryCrossingView(),
            new MissionBoundaryWallView(),
            new SpectatorCameraView(),
            new CrpgAgentHud(experienceTable),
        };
    }
#endif

    public override void StartMultiplayerGame(string scene)
    {
#if CRPG_SERVER
        CrpgHttpClient crpgClient = new();
        MultiplayerRoundController roundController = new(); // starts/stops round, ends match
#endif
        MultiplayerGameNotificationsComponent notificationsComponent = new(); // used to send notifications (e.g. flag captured, round won) to peer
        CrpgWarmupComponent warmupComponent = new(_constants);

        MissionState.OpenNew(
            Name,
            new MissionInitializerRecord(scene),
            missionController =>
                new MissionBehavior[]
                {
                    MissionLobbyComponent.CreateBehavior(),
                    new CrpgBattleMissionMultiplayerClient(),
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
#if CRPG_SERVER
                    roundController,
                    new CrpgBattleMissionMultiplayer(crpgClient, _constants),
                    notificationsComponent,
                    // SpawnFrameBehaviour: where to spawn, SpawningBehaviour: when to spawn
                    new SpawnComponent(new BattleSpawnFrameBehavior(), new CrpgBattleSpawningBehavior(_constants, roundController)),
                    new AgentHumanAILogic(), // bot intelligence
                    new MultiplayerAdminComponent(), // admin UI to kick player or restart game
                    new CrpgUserManager(crpgClient),
                    new KickInactiveBehavior(warmupComponent, notificationsComponent),
#else
                    new MultiplayerRoundComponent(),
                    new MissionMatchHistoryComponent(),
#endif
                });
    }
}
