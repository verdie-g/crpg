using Crpg.Module.Common;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.MissionRepresentatives;

namespace Crpg.Module.Duel;
internal class CrpgDuelSpawningBehavior : CrpgSpawningBehaviorBase
{
    private readonly MissionMultiplayerDuel _mission;

    public CrpgDuelSpawningBehavior(CrpgConstants constants, MissionMultiplayerDuel mission)
        : base(constants, null)
    {
        IsSpawningEnabled = true;
        _mission = mission;
    }

    public override void Initialize(SpawnComponent spawnComponent)
    {
        base.Initialize(spawnComponent);
        OnPeerSpawnedFromVisuals += OnPeerSpawned;
    }

    public override void Clear()
    {
        base.Clear();
        OnPeerSpawnedFromVisuals -= OnPeerSpawned;
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
            || component.HasSpawnedAgentVisuals
            || component.Team == null
            || component.Team == Mission.SpectatorTeam
            || component.Culture == null
            || !(component.Representative is DuelMissionRepresentative)
            || !component.SpawnTimer.Check(Mission.Current.CurrentTime)
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
        return Mission.Current.CurrentState == Mission.State.Continuing;
    }

    private void OnPeerSpawned(MissionPeer peer)
    {
        _ = peer.Representative;
    }
}
