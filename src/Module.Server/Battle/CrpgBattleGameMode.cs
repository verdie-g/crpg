using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Source.Missions;

#if CRPG_CLIENT
using TaleWorlds.MountAndBlade.LegacyGUI.Missions;
using TaleWorlds.MountAndBlade.View.Missions;
#endif

namespace Crpg.Module.Battle;

[ViewCreatorModule] // Exposes methods with ViewMethod attribute.
internal class CrpgBattleGameMode : MissionBasedMultiplayerGameMode
{
    private const string GameName = "cRPG";

    public CrpgBattleGameMode()
        : base(GameName)
    {
    }

#if CRPG_CLIENT
    // Used by MissionState.OpenNew that finds all methods having a ViewMethod attribute contained in class
    // having a ViewCreatorModule attribute.
    [ViewMethod(GameName)]
    public static MissionView[] OpenBattle(Mission mission) =>
        new[]
        {
            ViewCreator.CreateLobbyEquipmentUIHandler(), ViewCreator.CreateMultiplayerFactionBanVoteUIHandler(),
            ViewCreator.CreateMissionAgentStatusUIHandler(mission),
            ViewCreator.CreateMissionMainAgentEquipmentController(mission),
            ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission),
            ViewCreator.CreateMissionMultiplayerEscapeMenu("Battle"),
            ViewCreator.CreateMultiplayerMissionOrderUIHandler(mission),
            ViewCreator.CreateMissionAgentLabelUIHandler(mission),
            ViewCreator.CreateOrderTroopPlacerView(mission),
            ViewCreator.CreateMultiplayerTeamSelectUIHandler(),
            ViewCreator.CreateMissionScoreBoardUIHandler(mission, false),
            ViewCreator.CreateMultiplayerEndOfRoundUIHandler(),
            ViewCreator.CreateMultiplayerEndOfBattleUIHandler(),
            ViewCreator.CreatePollProgressUIHandler(),
            new MissionItemContourControllerView(),
            new MissionAgentContourControllerView(),
            ViewCreator.CreateMissionKillNotificationUIHandler(),
            ViewCreator.CreateMultiplayerMissionHUDExtensionUIHandler(),
            ViewCreator.CreateMultiplayerMissionDeathCardUIHandler(),
            ViewCreator.CreateMissionFlagMarkerUIHandler(),
            ViewCreator.CreateOptionsUIHandler(),
            ViewCreator.CreateMissionMainAgentEquipDropView(mission),
            ViewCreator.CreateMissionBoundaryCrossingView(),
            new MissionBoundaryWallView(),
            new SpectatorCameraView(),
        };
#endif

    public override void StartMultiplayerGame(string scene)
    {
        MissionState.OpenNew(
            Name,
            new MissionInitializerRecord(scene),
            missionController =>
#if CRPG_SERVER
                new MissionBehavior[]
                {
                    MissionLobbyComponent.CreateBehavior(),
                    new MultiplayerRoundController(), // starts/stops round, ends match
                    new MissionMultiplayerFlagDomination(MissionLobbyComponent.MultiplayerGameType.Battle),
                    new MultiplayerWarmupComponent(),
                    new MissionMultiplayerGameModeFlagDominationClient(),
                    new MultiplayerTimerComponent(), // round timer
                    new MultiplayerMissionAgentVisualSpawnComponent(), // expose method to spawn an agent
                    new ConsoleMatchStartEndHandler(),
                    // SpawnFrameBehaviour: where to spawn, SpawningBehaviour: when to spawn
                    new SpawnComponent(new FlagDominationSpawnFrameBehavior(), new FlagDominationSpawningBehavior()),
                    new MissionLobbyEquipmentNetworkComponent(), // logic to change troop or perks
                    new MultiplayerTeamSelectComponent(), // logic to change team, autoselect
                    new MissionHardBorderPlacer(),
                    new MissionBoundaryPlacer(), // set walkable boundaries
                    new AgentVictoryLogic(), // AI cheering when winning round
                    new AgentHumanAILogic(), // bot intelligence
                    new MissionBoundaryCrossingHandler(), // kills agent out of mission boundaries
                    new MultiplayerPollComponent(), // poll logic to kick player, ban player, change game
                    new MultiplayerAdminComponent(), // admin UI to kick player or restart game
                    // used to send notifications (e.g. flag captured, round won) to peer. same logic can be used to send gold + xp gains
                    new MultiplayerGameNotificationsComponent(),
                    new MissionOptionsComponent(),
                    new MissionScoreboardComponent(new BattleScoreboardData()), // score board
                    new EquipmentControllerLeaveLogic(),
                    new MultiplayerPreloadHelper(),
                });
#else
                new MissionBehavior[]
                {
                    MissionLobbyComponent.CreateBehavior(),
                    new MultiplayerRoundComponent(),
                    new MultiplayerWarmupComponent(),
                    new MissionMultiplayerGameModeFlagDominationClient(),
                    new MultiplayerTimerComponent(),
                    new MultiplayerMissionAgentVisualSpawnComponent(),
                    new ConsoleMatchStartEndHandler(),
                    new MissionLobbyEquipmentNetworkComponent(),
                    new MultiplayerTeamSelectComponent(),
                    new MissionHardBorderPlacer(),
                    new MissionBoundaryPlacer(),
                    new AgentVictoryLogic(),
                    new MissionBoundaryCrossingHandler(),
                    new MultiplayerPollComponent(),
                    new MultiplayerGameNotificationsComponent(),
                    new MissionOptionsComponent(),
                    new MissionScoreboardComponent(new BattleScoreboardData()),
                    new MissionMatchHistoryComponent(),
                    new EquipmentControllerLeaveLogic(),
                    new MultiplayerPreloadHelper(),
                });
#endif
    }
}
