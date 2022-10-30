using Crpg.Module.Common;
using Crpg.Module.Helpers;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.MissionRepresentatives;

namespace Crpg.Module.Duel;
internal class CrpgDuelSpawningBehavior : CrpgSpawningBehaviorBase
{
    public CrpgDuelSpawningBehavior(CrpgConstants constants)
        : base(constants, null)
    {
        IsSpawningEnabled = true;
    }

    public override void Initialize(SpawnComponent spawnComponent)
    {
        base.Initialize(spawnComponent);
    }

    public override void Clear()
    {
        base.Clear();
    }

    public override void OnTick(float dt)
    {
        if (IsSpawningEnabled && _spawnCheckTimer.Check(Mission.CurrentTime))
        {
            SpawnAgents();
        }

        base.OnTick(dt);
    }

    public override bool AllowEarlyAgentVisualsDespawning(MissionPeer missionPeer)
    {
        return true;
    }

    protected override bool IsPlayerAllowedToSpawn(NetworkCommunicator networkPeer)
    {
        MissionPeer component = networkPeer.GetComponent<MissionPeer>();
        CrpgRepresentative crpgRepresentative = networkPeer.GetComponent<CrpgRepresentative>();
        if (!networkPeer.IsSynchronized
            || component.ControlledAgent != null
            || component.Team == null
            || component.Team == Mission.SpectatorTeam
            || component.Culture == null
            || component.Representative is not DuelMissionRepresentative
            || !component.SpawnTimer.Check(Mission.CurrentTime)
            || crpgRepresentative?.User == null
            || crpgRepresentative.SpawnTeamThisRound != null)
        {
            return false;
        }

        return true;
    }

    protected override void SpawnAgents()
    {
        SpawnPeerAgents();
    }

    protected override bool IsRoundInProgress()
    {
        return Mission.CurrentState == Mission.State.Continuing;
    }

    protected override void OnPeerSpawned(MissionPeer peer)
    {
        _ = peer.Representative;
    }
}
