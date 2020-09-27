using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.GameMod.DefendTheVirgin
{
    internal class WaveSpawnLogic : MissionLogic
    {
        private readonly WaveController _waveController;
        private readonly IList<Wave> _waves;

        public WaveSpawnLogic(WaveController waveController, IList<Wave> waves)
        {
            _waveController = waveController;
            _waves = waves;
            _waveController.OnWaveStarted += SpawnAgents;
        }

        protected override void OnEndMission()
        {
            _waveController.OnWaveStarted -= SpawnAgents;
        }

        private void SpawnAgents(int waveNb)
        {
            BasicCharacterObject character = Game.Current.ObjectManager.GetObject<BasicCharacterObject>("battanian_picked_warrior");
            // TODO: add virgin character

            // Set player character. Without this line we would just spectate bots fighting without being able to play
            Game.Current.PlayerTroop = character;

            Mission.SpawnTroop(new BasicBattleAgentOrigin(character), true, false, false, false, true, 0, 0, false, true);

            Wave wave = _waves[waveNb - 1];
            foreach (var group in wave.Groups)
            {
                var troop = Game.Current.ObjectManager.GetObject<BasicCharacterObject>(group.CharacterId);
                var agentOrigin = new BasicBattleAgentOrigin(troop);
                Formation formation = Mission.GetAgentTeam(agentOrigin, false).GetFormation(FormationClass.Infantry);
                formation.BeginSpawn(group.Count, false);
                Mission.Current.SpawnFormation(formation, group.Count, false, false, false);
                for (int i = 0; i < group.Count; i += 1)
                {
                    Mission.Current.SpawnTroop(agentOrigin, false, true, false, false, true, group.Count, i, true, true);
                }

                formation.EndSpawn();
            }
        }
    }
}
