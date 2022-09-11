using Crpg.Module.Battle;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

/// <summary>
/// Custom Warmup component so we can load the cRPG Spawn behavior as soon as Warmup ends
/// </summary>
internal class CrpgWarmupComponent : MultiplayerWarmupComponent
{
    private readonly CrpgConstants _constants;

    public CrpgWarmupComponent(CrpgConstants constants)
    {
        _constants = constants;
    }

    public override void OnRemoveBehavior()
    {
        base.OnRemoveBehavior();

        SpawnComponent spawnComponent = Mission.GetMissionBehavior<SpawnComponent>();
        MultiplayerRoundController multiplayerRoundController = Mission.GetMissionBehavior<MultiplayerRoundController>();
        spawnComponent.SetNewSpawnFrameBehavior(new BattleSpawnFrameBehavior());
        spawnComponent.SetNewSpawningBehavior(new CrpgBattleSpawningBehavior(_constants, multiplayerRoundController));
    }
}
