using Crpg.Module.Common;
using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.PlayerServices;

namespace Crpg.Module.Battle;

internal class CrpgBattleSpawningBehavior : CrpgSpawningBehaviorBase
{
    private const float TotalSpawnDuration = 30f;
    private readonly MultiplayerRoundController? _roundController;
    private readonly HashSet<PlayerId> _notifiedPlayersAboutSpawnRestriction;
    private MissionTimer? _spawnTimer;
    private MissionTimer? _cavalrySpawnDelayTimer;
    private bool _botsSpawned;

    public CrpgBattleSpawningBehavior(CrpgConstants constants, MultiplayerRoundController? roundController)
        : base(constants)
    {
        _roundController = roundController;
        _notifiedPlayersAboutSpawnRestriction = new HashSet<PlayerId>();
    }

    public override void Initialize(SpawnComponent spawnComponent)
    {
        base.Initialize(spawnComponent);
        if (_roundController != null)
        {
            _roundController.OnPreparationEnded += RequestStartSpawnSession;
            _roundController.OnRoundEnding += RequestStopSpawnSession;
        }
    }

    public override void Clear()
    {
        base.Clear();
        if (_roundController != null)
        {
            _roundController.OnPreparationEnded -= RequestStartSpawnSession;
            _roundController.OnRoundEnding -= RequestStopSpawnSession;
        }
    }

    public override void OnTick(float dt)
    {
        if ((IsSpawningEnabled && IsRoundInProgress()) || _roundController == null)
        {
            SpawnAgents();
        }
    }

    public override void RequestStartSpawnSession()
    {
        base.RequestStartSpawnSession();
        _botsSpawned = false;
        _spawnTimer = new MissionTimer(TotalSpawnDuration); // Limit spawning for 30 seconds.
        _cavalrySpawnDelayTimer = new MissionTimer(GetCavalrySpawnDelay()); // Cav will spawn X seconds later.
        ResetSpawnTeams();
    }

    public override bool AllowEarlyAgentVisualsDespawning(MissionPeer missionPeer)
    {
        return false;
    }

    public bool SpawnDelayEnded()
    {
        return _cavalrySpawnDelayTimer != null && _cavalrySpawnDelayTimer!.Check();
    }

    protected override bool IsRoundInProgress()
    {
        return _roundController?.IsRoundInProgress ?? false;
    }

    protected override void SpawnAgents()
    {
        if (_roundController != null && _spawnTimer!.Check())
        {
            return;
        }

        if (!_botsSpawned || _roundController == null)
        {
            SpawnBotAgents();
            _botsSpawned = true;
        }

        SpawnPeerAgents();
    }

    protected override bool IsPlayerAllowedToSpawn(NetworkCommunicator networkPeer)
    {
        var missionPeer = networkPeer.GetComponent<MissionPeer>();
        var crpgPeer = networkPeer.GetComponent<CrpPeer>();
        if (!networkPeer.IsSynchronized
            || missionPeer == null
            || missionPeer.ControlledAgent != null
            || missionPeer.HasSpawnedAgentVisuals
            || missionPeer.Team == null
            || missionPeer.Team == Mission.SpectatorTeam
            || crpgPeer?.User == null
            || crpgPeer.SpawnTeamThisRound != null)
        {
            return false;
        }

        var characterEquipment = CreateCharacterEquipment(crpgPeer.User.Character.EquippedItems);
        if (!DoesEquipmentContainWeapon(characterEquipment)) // Disallow spawning without weapons.
        {
            if (_notifiedPlayersAboutSpawnRestriction.Add(networkPeer.VirtualPlayer.Id))
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new CrpgNotification
                {
                    Type = CrpgNotification.NotificationType.Notification,
                    Message = "You should have at least one weapon equipped to spawn!",
                    IsMessageTextId = false,
                    SoundEvent = string.Empty,
                });
                GameNetwork.EndModuleEventAsServer();
            }

            return false;
        }

        bool hasMount = characterEquipment[EquipmentIndex.Horse].Item != null;
        // Disallow spawning cavalry before the cav spawn delay ended.
        if (hasMount && (!_cavalrySpawnDelayTimer?.Check() ?? false))
        {
            if (_notifiedPlayersAboutSpawnRestriction.Add(networkPeer.VirtualPlayer.Id))
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new CrpgNotification
                {
                    Type = CrpgNotification.NotificationType.Notification,
                    Message = $"Cavalry will spawn in {_cavalrySpawnDelayTimer?.GetTimerDuration()} seconds!",
                    IsMessageTextId = false,
                    SoundEvent = string.Empty,
                });
                GameNetwork.EndModuleEventAsServer();
            }

            return false;
        }

        return true;
    }

    protected override void OnPeerSpawned(MissionPeer component)
    {
        base.OnPeerSpawned(component);

        if (_roundController != null)
        {
            component.SpawnCountThisRound += 1;
            component.GetNetworkPeer().GetComponent<CrpPeer>().SpawnTeamThisRound = component.Team;
        }
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
            if (!networkPeer.IsSynchronized || missionPeer == null || missionPeer.Team.Side == BattleSideEnum.None)
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
            var crpgPeer = networkPeer.GetComponent<CrpPeer>();
            if (crpgPeer != null)
            {
                crpgPeer.SpawnTeamThisRound = null;
            }
        }

        _notifiedPlayersAboutSpawnRestriction.Clear();
    }
}
