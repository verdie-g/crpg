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

            MissionState.OpenNew("MultiplayerSkirmish", new MissionInitializerRecord(scene), InitializeMissionBehaviours,
                true, true, false);
        }

        private IEnumerable<MissionBehaviour> InitializeMissionBehaviours(Mission mission)
        {
            // inspired by TaleWorlds.MountAndBlade.MultiplayerMissions
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
                MissionLobbyComponent.CreateBehaviour(),
                new MultiplayerRoundController(),
                new MissionMultiplayerFlagDomination(true),
                new MultiplayerWarmupComponent(),
                new MissionMultiplayerGameModeFlagDominationClient(),
                new MultiplayerTimerComponent(),
                new MultiplayerMissionAgentVisualSpawnComponent(),
                new SpawnComponent(new FlagDominationSpawnFrameBehaviour(), spawningBehaviour),
                new MissionLobbyEquipmentNetworkComponent(),
                new MultiplayerTeamSelectComponent(),
                new AgentVictoryLogic(),
                new AgentBattleAILogic(),
                new MissionBoundaryPlacer(),
                new MissionBoundaryCrossingHandler(),
                new MultiplayerPollComponent(),
                new MultiplayerAdminComponent(),
                new MultiplayerGameNotificationsComponent(),
                new MissionOptionsComponent(),
                new MissionScoreboardComponent("Skirmish")
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
                new AgentVictoryLogic(),
                new MissionBoundaryPlacer(),
                new MissionBoundaryCrossingHandler(),
                new MultiplayerPollComponent(),
                new MultiplayerGameNotificationsComponent(),
                new MissionOptionsComponent(),
                new MissionScoreboardComponent("Skirmish")
            };
        }
    }
}