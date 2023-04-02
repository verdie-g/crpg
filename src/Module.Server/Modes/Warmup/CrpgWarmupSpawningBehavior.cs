using Crpg.Module.Common;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Modes.Warmup;

internal class CrpgWarmupSpawningBehavior : CrpgSpawningBehaviorBase
{
    public CrpgWarmupSpawningBehavior(CrpgConstants constants)
        : base(constants)
    {
    }

    public override void OnTick(float dt)
    {
        SpawnBotAgents();
        SpawnAgents();
    }

    protected override bool IsRoundInProgress()
    {
        return false;
    }
}
