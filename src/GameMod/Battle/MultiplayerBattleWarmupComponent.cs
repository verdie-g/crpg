using System.Reflection;
using TaleWorlds.MountAndBlade;

namespace Crpg.GameMod.Battle
{
    /// <summary>
    /// <see cref="MultiplayerWarmupComponent"/> calls <see cref="SpawnComponent.SetSpawningBehaviorForCurrentGameType"/>
    /// on warm-up end to set the spawning behavior using hardcoded game type enum <see cref="MissionLobbyComponent.MultiplayerGameType"/>
    /// so <see cref="BattleSpawningBehaviour"/> is never used. This class inherits <see cref="MultiplayerWarmupComponent"/>
    /// and overrides the warm-up end logic to use <see cref="BattleSpawningBehaviour"/>.
    /// </summary>
    public class MultiplayerBattleWarmupComponent : MultiplayerWarmupComponent
    {
        // _warmupState is private in parent class so use reflection to get the value
        private static readonly FieldInfo WarmupStateField = typeof(MultiplayerBattleWarmupComponent).BaseType
            .GetField("_warmupState", BindingFlags.Instance | BindingFlags.NonPublic);

        // indicates if the spawn behaviour was set to avoid setting in several times when OnPreDisplayMissionTick
        // is called several times when _warmupState is Ended
        private bool _spawnBehaviourSet;

        public override void OnPreDisplayMissionTick(float dt)
        {
            base.OnPreDisplayMissionTick(dt);
            if (!GameNetwork.IsServer)
            {
                return;
            }

            if (!_spawnBehaviourSet && (WarmupStates)WarmupStateField.GetValue(this) == WarmupStates.Ended)
            {
                var spawnComponent = Mission.Current.GetMissionBehaviour<SpawnComponent>();
                spawnComponent.SetNewSpawnFrameBehaviour(new FlagDominationSpawnFrameBehaviour());
                spawnComponent.SetNewSpawningBehaviour(new BattleSpawningBehaviour());
                _spawnBehaviourSet = true;
            }

            // no need to reset _spawnBehaviourSet as after warmup end, the warmup component is removed forever
        }
    }
}
