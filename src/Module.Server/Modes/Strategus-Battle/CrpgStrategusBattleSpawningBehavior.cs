using Crpg.Module.Common;
using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.PlayerServices;

namespace Crpg.Module.Modes.StrategusBattle;

internal class CrpgStrategusBattleSpawningBehavior : CrpgSpawningBehaviorBase
{
    private const float CyclicSpawnDuration = 30f; // This needs to depends on how many players are on the side
    private const float SpawnDuration = 30f; // this needs to make sure dead player has time to spawn without being too long
    private readonly MultiplayerRoundController _roundController;
    private readonly HashSet<PlayerId> _notifiedPlayersAboutSpawnRestriction;
    private MissionTimer? _cyclicSpawnTimer;
    private MissionTimer? _spawnTimer;
    private MissionTimer? _cavalrySpawnDelayTimer;
    private bool _botsSpawned;

    public CrpgStrategusBattleSpawningBehavior(CrpgConstants constants, MultiplayerRoundController roundController)
        : base(constants)
    {
        _roundController = roundController;
        _notifiedPlayersAboutSpawnRestriction = new HashSet<PlayerId>();
    }

    public override void Initialize(SpawnComponent spawnComponent)
    {
        base.Initialize(spawnComponent);
        _roundController.OnPreparationEnded += RequestStartSpawnSession;
        _roundController.OnRoundEnding += RequestStopSpawnSession;
    }

    public override void Clear()
    {
        base.Clear();
        _roundController.OnPreparationEnded -= RequestStartSpawnSession;
        _roundController.OnRoundEnding -= RequestStopSpawnSession;
    }

    public override void OnTick(float dt)
    {
        if (IsSpawningEnabled && IsRoundInProgress())
        {
            SpawnAgents();
        }
        // Update the spawn timer.
        if (_cyclicSpawnTimer != null)
        {
            // Check if the timer has reached 0.
            if (_cyclicSpawnTimer.Check(true))
            {
                if (_spawnTimer != null)
                {
                    _spawnTimer.Check(true);
                }
            }
        }
    }

    public override void RequestStartSpawnSession()
    {
        base.RequestStartSpawnSession();
        _botsSpawned = false;
        _cyclicSpawnTimer = new MissionTimer(CyclicSpawnDuration); // Spawning in waves the same way Wolfenstein enemy territory does (or full invasion)
        _spawnTimer = new MissionTimer(SpawnDuration);

        ResetSpawnTeams();
    }

    public override bool AllowEarlyAgentVisualsDespawning(MissionPeer missionPeer)
    {
        return false;
    }

    protected override bool IsRoundInProgress()
    {
        return _roundController.IsRoundInProgress;
    }

    protected override void SpawnAgents()
    {
        if (_spawnTimer!.Check())
        {
            return;
        }

        if (!_botsSpawned)
        {
            SpawnBotAgents();
            _botsSpawned = true;
        }

        SpawnPeerAgents();
    }

    protected override bool IsPlayerAllowedToSpawn(NetworkCommunicator networkPeer)
    {
        var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
        var missionPeer = networkPeer.GetComponent<MissionPeer>();
        if (crpgPeer?.User == null
            || crpgPeer.SpawnTeamThisRound != null
            || missionPeer == null
            || missionPeer.HasSpawnedAgentVisuals)
        {
            return false;
        }

        var characterEquipment = CreateCharacterEquipment(crpgPeer.User.Character.EquippedItems);
        return true;
    }

    protected override void OnPeerSpawned(MissionPeer component)
    {
        base.OnPeerSpawned(component);
        component.SpawnCountThisRound += 1;
        var crpgPeer = component.GetComponent<CrpgPeer>();
        crpgPeer.SpawnTeamThisRound = component.Team;
    }

    /// <summary>
    /// Cav spawn delay values
    /// 10 => 7sec
    /// 30 => 9sec
    /// 60 => 13sec
    /// 90 => 17sec
    /// 120 => 22sec
    /// 150 => 26sec
    /// 165+ => 28sec.
    /// </summary>
    private int GetCavalrySpawnDelay()
    {
        int currentPlayers = Math.Max(GetCurrentPlayerCount(), 1);
        return Math.Min(28, 5 + currentPlayers / 7);
    }

    private int GetCurrentPlayerCount()
    {
        int counter = 0;
        foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
        {
            var missionPeer = networkPeer.GetComponent<MissionPeer>();
            if (!networkPeer.IsSynchronized
                || missionPeer == null
                || missionPeer.Team == null
                || missionPeer.Team.Side == BattleSideEnum.None)
            {
                continue;
            }

            counter++;
        }

        return counter;
    }

    private void ResetSpawnTeams()
    {
        foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
        {
            var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
            if (crpgPeer != null)
            {
                crpgPeer.SpawnTeamThisRound = null;
            }
        }

        _notifiedPlayersAboutSpawnRestriction.Clear();
    }
}
