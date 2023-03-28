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
        SpawnAgents();
    }

    public override bool AllowEarlyAgentVisualsDespawning(MissionPeer missionPeer)
    {
        return false;
    }

    protected override bool IsRoundInProgress()
    {
        return false;
    }

    protected override void SpawnAgents()
    {
        SpawnBotAgents();
        SpawnPeerAgents();
    }
}
