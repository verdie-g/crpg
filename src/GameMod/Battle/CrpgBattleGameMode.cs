using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.LegacyGUI.Missions;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.MountAndBlade.View.Missions;

namespace Crpg.GameMod.Battle
{
    /// <summary>
    /// cRPG Battle entry point.
    /// </summary>
    [ViewCreatorModule] // exposes methods with ViewMethod attribute
    internal class CrpgBattleGameMode : MissionBasedMultiplayerGameMode
    {
        public const string GameModeName = "cRPGBattle";

        // used by MissionState.OpenNew that finds all methods having a ViewMethod attribute contained in class
        // having a ViewCreatorModule attribute
        [ViewMethod(GameModeName)]
        private static MissionView[] OpenCrpgBattle(Mission mission)
        {
            var missionViewList = new List<MissionView>
            {
                ViewCreator.CreateLobbyEquipmentUIHandler(),
                ViewCreator.CreateMultiplayerFactionBanVoteUIHandler(),
                ViewCreator.CreateLobbyUIHandler(),
                ViewCreator.CreateMissionKillNotificationUIHandler(),
                ViewCreator.CreateMissionAgentStatusUIHandler(mission),
                ViewCreator.CreateMissionMultiplayerPreloadView(mission),
                ViewCreator.CreateMissionMainAgentEquipmentController(mission),
                ViewCreator.CreateMissionMultiplayerEscapeMenu("Skirmish"),
                ViewCreator.CreateMissionAgentLabelUIHandler(mission),
                ViewCreator.CreateMultiplayerTeamSelectUIHandler(),
                ViewCreator.CreateMissionScoreBoardUIHandler(mission, false),
                ViewCreator.CreateMultiplayerEndOfRoundUIHandler(),
                ViewCreator.CreatePollProgressUIHandler(),
                new MissionItemContourControllerView(),
                new MissionAgentContourControllerView(),
                ViewCreator.CreateMultiplayerMissionHUDExtensionUIHandler(),
                ViewCreator.CreateMultiplayerMissionDeathCardUIHandler(),
                ViewCreator.CreateOptionsUIHandler(),
                ViewCreator.CreateMissionMainAgentGamepadEquipDropView(mission),
            };

            if (!GameNetwork.IsClient)
            {
                missionViewList.Add(ViewCreator.CreateMultiplayerAdminPanelUIHandler());
            }

            missionViewList.Add(ViewCreator.CreateMissionBoundaryCrossingView());
            missionViewList.Add(new MissionBoundaryWallView());
            missionViewList.Add(new SpectatorCameraView());
            return missionViewList.ToArray();
        }

        public CrpgBattleGameMode() : base(GameModeName)
        {
        }

        public override void StartMultiplayerGame(string scene)
        {
            // hardcore options as they're not modifiable in the UI
            MultiplayerOptions.OptionType.NumberOfBotsTeam1.SetValue(10);
            MultiplayerOptions.OptionType.NumberOfBotsTeam2.SetValue(10);
            MultiplayerOptions.OptionType.WarmupTimeLimit.SetValue(1); // warm-up will last at most 1 minute
            MultiplayerOptions.OptionType.MaxNumberOfPlayers.SetValue(1); // shortens the warm-up to 30 seconds

            // inspired by TaleWorlds.MountAndBlade.MultiplayerMissions

            // the first parameter, missionName, is used to retrieve the MissionsViews
            // registered in TaleWorlds.MountAndBlade.View.Missions.MultiplayerMissionViews
            MissionState.OpenNew(GameModeName, new MissionInitializerRecord(scene), InitializeMissionBehaviours);
        }

        private IEnumerable<MissionBehaviour> InitializeMissionBehaviours(Mission mission)
        {
            return GameNetwork.IsServer
                ? InitializeMissionBehavioursServer()
                : InitializeMissionBehavioursClient();
        }

        private IEnumerable<MissionBehaviour> InitializeMissionBehavioursServer()
        {
            return new MissionBehaviour[]
            {
                MissionLobbyComponent.CreateBehaviour(), // ???
                new MultiplayerRoundController(), // starts/stops round, ends match
                new MultiplayerBattleWarmupComponent(), // warmup logic
                new MissionMultiplayerBattle(),
                new MissionMultiplayerBattleClient(),
                new MultiplayerTimerComponent(), // round timer
                new MultiplayerMissionAgentVisualSpawnComponent(), // expose method to spawn an agent
                // SpawnFrameBehaviour: where to spawn, SpawningBehaviour: when to spawn
                new SpawnComponent(new FlagDominationSpawnFrameBehaviour(), new BattleSpawningBehaviour()),
                new MissionLobbyEquipmentNetworkComponent(), // logic to change troop or perks
                new MultiplayerTeamSelectComponent(), // logic to change team, autoselect
                new AgentVictoryLogic(), // AI cheering when winning round
                new AgentBattleAILogic(), // bot intelligence
                new MissionBoundaryPlacer(), // set walkable boundaries
                new MissionBoundaryCrossingHandler(), // kills agent out of mission boundaries
                new MultiplayerPollComponent(), // poll logic to kick player, ban player, change game
                new MultiplayerAdminComponent(), // admin UI to kick player or restart game
                // used to send notifications (e.g. flag captured, round won) to peer. same logic can be used to send gold + xp gains
                new MultiplayerGameNotificationsComponent(),
                new MissionOptionsComponent(), // ???
                // score board, parameter is used to get the right IScoreboardData implementation for the game mode
                new MissionScoreboardComponent("Skirmish"),
            };
        }

        private IEnumerable<MissionBehaviour> InitializeMissionBehavioursClient()
        {
            return new MissionBehaviour[]
            {
                MissionLobbyComponent.CreateBehaviour(),
                new MultiplayerRoundComponent(),
                new MultiplayerBattleWarmupComponent(),
                new MissionMultiplayerBattleClient(),
                new MultiplayerTimerComponent(),
                new MultiplayerMissionAgentVisualSpawnComponent(),
                new MissionLobbyEquipmentNetworkComponent(),
                new MultiplayerTeamSelectComponent(),
                new AgentVictoryLogic(), // AI cheering when winning round
                new MissionBoundaryPlacer(),
                new MissionBoundaryCrossingHandler(),
                new MultiplayerPollComponent(),
                new MultiplayerGameNotificationsComponent(),
                new MissionOptionsComponent(),
                new MissionScoreboardComponent("Skirmish"),
            };
        }
    }
}
