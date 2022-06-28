using Crpg.Module.Common;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Source.Missions;

#if CRPG_SERVER
using Crpg.Module.Api;
#else
using TaleWorlds.MountAndBlade.LegacyGUI.Missions;
using TaleWorlds.MountAndBlade.View.Missions;
#endif

namespace Crpg.Module.Battle;

[ViewCreatorModule] // Exposes methods with ViewMethod attribute.
internal class CrpgBattleGameMode : MissionBasedMultiplayerGameMode
{
    private const string GameName = "cRPG";

    private readonly CrpgConstants _constants;

    public CrpgBattleGameMode(CrpgConstants constants)
        : base(GameName)
    {
        _constants = constants;
    }

#if CRPG_CLIENT
    // Used by MissionState.OpenNew that finds all methods having a ViewMethod attribute contained in class
    // having a ViewCreatorModule attribute.
    [ViewMethod(GameName)]
    public static MissionView[] OpenCrpgBattle(Mission mission) =>
        new[]
        {
            // ViewCreator.CreateLobbyEquipmentUIHandler(), // UI to choose loadout.
            ViewCreator.CreateMultiplayerFactionBanVoteUIHandler(),
            ViewCreator.CreateMissionAgentStatusUIHandler(mission),
            // ViewCreator.CreateMissionMainAgentEquipmentController(mission),
            ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
            ViewCreator.CreateMissionMultiplayerEscapeMenu("Battle"),
            // ViewCreator.CreateMultiplayerMissionOrderUIHandler(mission),
            ViewCreator.CreateMissionAgentLabelUIHandler(mission),
            // ViewCreator.CreateOrderTroopPlacerView(mission),
            ViewCreator.CreateMultiplayerTeamSelectUIHandler(),
            ViewCreator.CreateMissionScoreBoardUIHandler(mission, false),
            ViewCreator.CreateMultiplayerEndOfRoundUIHandler(),
            ViewCreator.CreateMultiplayerEndOfBattleUIHandler(),
            ViewCreator.CreatePollProgressUIHandler(),
            // new MissionItemContourControllerView(),
            // new MissionAgentContourControllerView(),
            ViewCreator.CreateMissionKillNotificationUIHandler(),
            ViewCreator.CreateMultiplayerMissionHUDExtensionUIHandler(),
            ViewCreator.CreateMultiplayerMissionDeathCardUIHandler(),
            // ViewCreator.CreateMissionFlagMarkerUIHandler(),
            ViewCreator.CreateOptionsUIHandler(),
            ViewCreator.CreateMissionMainAgentEquipDropView(mission),
            ViewCreator.CreateMissionBoundaryCrossingView(),
            new MissionBoundaryWallView(),
            new SpectatorCameraView(),
        };
#endif

    public override void StartMultiplayerGame(string scene)
    {
#if CRPG_SERVER
        CrpgHttpClient crpgClient = new();
        MultiplayerRoundController roundController = new(); // starts/stops round, ends match
#endif

        MissionState.OpenNew(
            Name,
            new MissionInitializerRecord(scene),
            missionController =>
                new MissionBehavior[]
                {
                    MissionLobbyComponent.CreateBehavior(),
                    // new MultiplayerWarmupComponent(),
                    new CrpgBattleMissionMultiplayerClient(), // new MissionMultiplayerGameModeFlagDominationClient(),
                    new MultiplayerTimerComponent(), // round timer
                    new NoMultiplayerMissionAgentVisualSpawnComponent(), // expose method to spawn an agent
                    // new ConsoleMatchStartEndHandler(),
                    new NoMissionLobbyEquipmentNetworkComponent(), // new MissionLobbyEquipmentNetworkComponent(), // logic to change troop or perks
                    new MultiplayerTeamSelectComponent(), // logic to change team, autoselect
                    new MissionHardBorderPlacer(),
                    new MissionBoundaryPlacer(), // set walkable boundaries
                    new AgentVictoryLogic(), // AI cheering when winning round
                    new MissionBoundaryCrossingHandler(), // kills agent out of mission boundaries
                    new MultiplayerPollComponent(), // poll logic to kick player, ban player, change game
                    // used to send notifications (e.g. flag captured, round won) to peer. same logic can be used to send gold + xp gains
                    new MultiplayerGameNotificationsComponent(),
                    new MissionOptionsComponent(),
                    new MissionScoreboardComponent(new BattleScoreboardData()), // score board
                    new EquipmentControllerLeaveLogic(),
                    new MultiplayerPreloadHelper(),
#if CRPG_SERVER
                    roundController,
                    new CrpgBattleMissionMultiplayer(crpgClient), // new MissionMultiplayerFlagDomination(MissionLobbyComponent.MultiplayerGameType.Battle),
                    // SpawnFrameBehaviour: where to spawn, SpawningBehaviour: when to spawn
                    new SpawnComponent(new BattleSpawnFrameBehavior(), new CrpgBattleSpawningBehavior(_constants, roundController)),
                    new AgentHumanAILogic(), // bot intelligence
                    new MultiplayerAdminComponent(), // admin UI to kick player or restart game
                    new CrpgUserManager(crpgClient),
#else
                    new MultiplayerRoundComponent(),
                    new MissionMatchHistoryComponent(),
#endif
                });
    }
}
