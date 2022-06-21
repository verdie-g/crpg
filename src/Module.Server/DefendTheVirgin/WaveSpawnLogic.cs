using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.DefendTheVirgin;

internal class WaveSpawnLogic : MissionLogic
{
    private readonly WaveController _waveController;
    private readonly WaveGroup[][] _waves;
    private readonly BasicCharacterObject _mainCharacter;

    public WaveSpawnLogic(WaveController waveController, WaveGroup[][] waves, BasicCharacterObject mainCharacter)
    {
        _waveController = waveController;
        _waves = waves;
        _mainCharacter = mainCharacter;
        _waveController.OnWaveStarted += SpawnAgents;
    }

    protected override void OnEndMission()
    {
        _waveController.OnWaveStarted -= SpawnAgents;
    }

    private void SpawnAgents(int waveNb)
    {
        // Set player character. Without this line we would just spectate bots fighting without being able to play
        Game.Current.PlayerTroop = _mainCharacter;

        // Initialize spawning positions. Without this line everyone spawn on the same point.
        Mission.MakeDefaultDeploymentPlans();

        Mission.SpawnTroop(new BasicBattleAgentOrigin(_mainCharacter), true, false,
            !_mainCharacter.Equipment.Horse.IsEmpty, false, true, 0, 0, false, true, false, null, null);

        var virgin = Game.Current.ObjectManager.GetObject<BasicCharacterObject>("townswoman_empire");
        // virgin.Name = new TextObject("The Virgin");
        Mission.SpawnTroop(new BasicBattleAgentOrigin(virgin), true, false, false, false, true, 0, 0, false, true,
            false, null, null);

        WaveGroup[] wave = _waves[waveNb - 1];
        foreach (var group in wave)
        {
            var troop = Game.Current.ObjectManager.GetObject<BasicCharacterObject>(group.Id);
            BasicBattleAgentOrigin agentOrigin = new(troop);
            Formation formation = Mission.GetAgentTeam(agentOrigin, false).GetFormation(FormationClass.Infantry);
            formation.BeginSpawn(group.Count, false);
            Mission.Current.SpawnFormation(formation);
            for (int i = 0; i < group.Count; i += 1)
            {
                Mission.Current.SpawnTroop(agentOrigin, false, true, !troop.Equipment.Horse.IsEmpty, false, true,
                    group.Count, i, true, true, false, null, null);
            }

            formation.EndSpawn();
        }
    }
}
