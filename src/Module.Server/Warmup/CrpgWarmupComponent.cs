using Crpg.Module.Battle;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.Warmup;

/// <summary>
/// Custom warmup component so we can load the <see cref="CrpgBattleSpawningBehavior"/> as soon as warmup ends.
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
        if (!GameNetwork.IsServer)
        {
            return;
        }

        SpawnComponent spawnComponent = Mission.GetMissionBehavior<SpawnComponent>();
        MultiplayerRoundController multiplayerRoundController = Mission.GetMissionBehavior<MultiplayerRoundController>();
        spawnComponent.SetNewSpawnFrameBehavior(new BattleSpawnFrameBehavior());
        spawnComponent.SetNewSpawningBehavior(new CrpgBattleSpawningBehavior(_constants, multiplayerRoundController));
    }
}
