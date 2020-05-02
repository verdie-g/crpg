using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crpg.GameMod.Battle
{
    /// <summary>
    /// Component that spawn peers and bots agent.
    /// </summary>
    public class BattleSpawningBehaviour : SpawningBehaviourBase
    {
        private MissionTimer? _spawnSessionTimer;
        private MultiplayerRoundController? _roundController;

        /// <summary>
        /// To check if a peer has already spawned <see cref="MissionPeer.HasSpawnedAgentVisuals"/> is checked but
        /// a bot is not a <see cref="MissionPeer"/> so use a boolean just for that.
        /// TODO: find if a bot can be a MissionPeer
        /// </summary>
        private bool _botsSpawned;

        public override void Initialize(SpawnComponent spawnComponent)
        {
            base.Initialize(spawnComponent);
            _roundController = Mission.GetMissionBehaviour<MultiplayerRoundController>();
            _roundController.OnRoundStarted += RequestStartSpawnSession;
            _roundController.OnRoundEnding += RequestStopSpawnSession;
        }

        public override void Clear()
        {
            base.Clear();
            _roundController!.OnRoundStarted -= RequestStartSpawnSession;
            _roundController!.OnRoundEnding -= RequestStopSpawnSession;
        }

        public override void OnTick(float dt)
        {
            if (IsSpawningEnabled)
            {
                if (_spawnSessionTimer!.Check()) // end of round preparation time
                {
                    IsSpawningEnabled = false;
                }

                SpawnAgents();
            }

            base.OnTick(dt);
        }

        public override void RequestStartSpawnSession()
        {
            base.RequestStartSpawnSession();
            _spawnSessionTimer = new MissionTimer(MultiplayerOptions.OptionType.RoundPreparationTimeLimit.GetIntValue());
            _botsSpawned = false;
        }

        // no idea what is about
        public override bool AllowEarlyAgentVisualsDespawning(MissionPeer missionPeer) => _roundController!.IsRoundInProgress;

        protected override void SpawnAgents()
        {
            SpawnBotsAgent();
            SpawnPeersAgent();
        }

        protected override bool IsRoundInProgress() => _roundController!.IsRoundInProgress;

        private void SpawnBotsAgent()
        {
            int botsTeam1 = MultiplayerOptions.OptionType.NumberOfBotsTeam1.GetIntValue();
            int botsTeam2 = MultiplayerOptions.OptionType.NumberOfBotsTeam2.GetIntValue();
            if (_botsSpawned || (botsTeam1 == 0 && botsTeam2 == 0))
            {
                return;
            }

            var objectManager = MBObjectManager.Instance;
            var cultureTeam1 = objectManager.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue());
            var cultureTeam2 = objectManager.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue());

            Mission.Current.AllowAiTicking = false;
            foreach (Team team in Mission.Teams)
            {
                if (Mission.AttackerTeam != team && Mission.DefenderTeam != team)
                {
                    continue;
                }

                int bots = Mission.AttackerTeam == team ? botsTeam1 : botsTeam2;
                for (int botIndex = 0; botIndex < bots; botIndex += 1)
                {
                    MultiplayerClassDivisions.MPHeroClass heroClass = MultiplayerClassDivisions.GetMPHeroClasses().First();
                    BasicCharacterObject heroCharacter = heroClass.HeroCharacter;

                    AgentBuildData agentBuildData = new AgentBuildData(heroCharacter);
                    agentBuildData.Equipment(heroClass.HeroCharacter.Equipment);
                    agentBuildData.TroopOrigin(new BasicBattleAgentOrigin(heroCharacter));
                    agentBuildData.EquipmentSeed(MissionLobbyComponent.GetRandomFaceSeedForCharacter(heroCharacter));
                    agentBuildData.Team(team);
                    agentBuildData.VisualsIndex(0);
                    agentBuildData.InitialFrame(SpawnComponent.GetSpawnFrame(team,
                        heroClass.HeroCharacter.Equipment[EquipmentIndex.Horse].Item != null, true));
                    agentBuildData.SpawnOnInitialPoint(true);
                    agentBuildData.IsFemale(heroCharacter.IsFemale);
                    agentBuildData.BodyProperties(BodyProperties.GetRandomBodyProperties(agentBuildData.AgentIsFemale,
                        heroCharacter.GetBodyPropertiesMin(), heroCharacter.GetBodyPropertiesMax(),
                        (int)agentBuildData.AgentOverridenSpawnEquipment.HairCoverType, agentBuildData.AgentEquipmentSeed,
                        heroCharacter.HairTags, heroCharacter.BeardTags, heroCharacter.TattooTags));
                    agentBuildData.ClothingColor1(team == Mission.AttackerTeam ? cultureTeam1.Color : cultureTeam2.Color);
                    agentBuildData.ClothingColor2(team == Mission.AttackerTeam ? cultureTeam1.Color2 : cultureTeam2.Color2);

                    Agent agent = Mission.SpawnAgent(agentBuildData);
                    agent.SetWatchState(AgentAIStateFlagComponent.WatchState.Alarmed);
                    agent.WieldInitialWeapons();
                }
            }

            _botsSpawned = true;
        }

        private void SpawnPeersAgent()
        {
            var objectManager = MBObjectManager.Instance;
            var cultureTeam1 = objectManager.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue());
            var cultureTeam2 = objectManager.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue());

            foreach (MissionPeer peer in VirtualPlayer.Peers<MissionPeer>())
            {
                NetworkCommunicator networkPeer = peer.GetNetworkPeer();
                if (!networkPeer.IsSynchronized || peer.Team == null || peer.Team == Mission.SpectatorTeam
                    || peer.Team.Side == BattleSideEnum.None || peer.ControlledAgent != null || peer.HasSpawnedAgentVisuals)
                {
                    continue;
                }

                // for now everyone will have the same hero class
                MultiplayerClassDivisions.MPHeroClass heroClass = MultiplayerClassDivisions.GetMPHeroClasses().First();
                BasicCharacterObject heroCharacter = heroClass.HeroCharacter;
                var equipment = heroCharacter.Equipment.Clone();
                var bodyProperties = GetBodyProperties(peer, peer.Team == Mission.AttackerTeam ? cultureTeam1 : cultureTeam2);

                var agentBuildData = new AgentBuildData(heroCharacter);
                agentBuildData.MissionPeer(peer);
                agentBuildData.Equipment(equipment);
                agentBuildData.Team(peer.Team);
                agentBuildData.VisualsIndex(0);
                agentBuildData.SpawnOnInitialPoint(true);
                agentBuildData.IsFemale(peer.Peer.IsFemale);
                agentBuildData.BodyProperties(bodyProperties);
                agentBuildData.ClothingColor1(peer.Team == Mission.AttackerTeam ? cultureTeam1.Color : cultureTeam2.Color);
                agentBuildData.ClothingColor2(peer.Team == Mission.AttackerTeam ? cultureTeam1.Color2 : cultureTeam2.Color2);
                agentBuildData.InitialFrame(SpawnComponent.GetSpawnFrame(peer.Team,
                    equipment[EquipmentIndex.Horse].Item != null, true));

                if (GameMode.ShouldSpawnVisualsForServer(networkPeer))
                {
                    AgentVisualSpawnComponent.SpawnAgentVisualsForPeer(peer, agentBuildData, peer.SelectedTroopIndex);
                }

                GameMode.HandleAgentVisualSpawning(networkPeer, agentBuildData);
            }
        }
    }
}