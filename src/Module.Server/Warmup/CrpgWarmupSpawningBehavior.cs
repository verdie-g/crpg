using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.Warmup;

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

    protected override bool IsPlayerAllowedToSpawn(NetworkCommunicator networkPeer)
    {
        var missionPeer = networkPeer.GetComponent<MissionPeer>();
        var crpgPeer = networkPeer.GetComponent<CrpPeer>();
        return networkPeer.IsSynchronized
               && missionPeer != null
               && missionPeer.ControlledAgent == null
               && !missionPeer.HasSpawnedAgentVisuals
               && missionPeer.Team != null
               && missionPeer.Team != Mission.SpectatorTeam
               && crpgPeer?.User != null;
    }
}
