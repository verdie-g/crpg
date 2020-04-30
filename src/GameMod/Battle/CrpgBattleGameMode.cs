using System;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Source.Missions;

namespace Crpg.GameMod.Battle
{
    internal class CrpgBattleGameMode : MissionBasedMultiplayerGameMode
    {
        public const string GameModeName = "cRPGBattle";

        public CrpgBattleGameMode() : base(GameModeName)
        {
        }

        public override void StartMultiplayerGame(string scene)
        {
            DebugUtils.Trace(scene);

            // inspired by TaleWorlds.MountAndBlade.MultiplayerMissions
            MissionState.OpenNew("MultiplayerSkirmish", new MissionInitializerRecord(scene), InitializeMissionBehaviours,
                true, true, false);
        }

        private IEnumerable<MissionBehaviour> InitializeMissionBehaviours(Mission mission)
        {
            return GameNetwork.IsServer
                ? InitializeMissionBehavioursServer()
                : InitializeMissionBehavioursClient();
        }

        private IEnumerable<MissionBehaviour> InitializeMissionBehavioursServer()
        {
            // FlagDominationSpawningBehaviour is internal, instantiate it using reflection
            // TODO: implement a cRPG spawning behaviour
            var mbAssembly = Assembly.GetAssembly(typeof(SpawningBehaviourBase));
            var spawningBehaviourType = mbAssembly.GetType(mbAssembly.GetName().Name + ".FlagDominationSpawningBehaviour");
            var spawningBehaviour = (SpawningBehaviourBase)Activator.CreateInstance(spawningBehaviourType);

            return new MissionBehaviour[]
            {
                MissionLobbyComponent.CreateBehaviour(), // ???
                new MultiplayerRoundController(), // starts/stops round, ends match
                new MissionMultiplayerFlagDomination(true), // flag + morale logic
                new MultiplayerWarmupComponent(), // warmup logic
                new MissionMultiplayerGameModeFlagDominationClient(),
                new MultiplayerTimerComponent(), // round timer
                new MultiplayerMissionAgentVisualSpawnComponent(), // expose method to spawn an agent
                new SpawnComponent(new FlagDominationSpawnFrameBehaviour(), spawningBehaviour),
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
                new MultiplayerWarmupComponent(),
                new MissionMultiplayerGameModeFlagDominationClient(),
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