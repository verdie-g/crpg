using Crpg.Module.Common;
using Crpg.Module.Common.Network;
using TaleWorlds.MountAndBlade;
using TaleWorlds.PlayerServices;

namespace Crpg.Module.Modes.Skirmish;

internal class CrpgSkirmishSpawningBehavior : CrpgSpawningBehaviorBase
{
    public const int MaxSpawns = 2;

    private readonly MultiplayerRoundController _roundController;
    private readonly HashSet<PlayerId> _notifiedPlayersAboutSpawnRestriction;
    private bool _botsSpawned;

    public CrpgSkirmishSpawningBehavior(CrpgConstants constants, MultiplayerRoundController roundController)
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
    }

    public override void RequestStartSpawnSession()
    {
        base.RequestStartSpawnSession();
        _botsSpawned = false;
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
        if (!_botsSpawned)
        {
            SpawnBotAgents();
            _botsSpawned = true;
        }

        SpawnPeerAgents();
    }

    protected override bool IsPlayerAllowedToSpawn(NetworkCommunicator networkPeer)
    {
        var crpgPeer = networkPeer.GetComponent<CrpgPeer>()!;
        var missionPeer = networkPeer.GetComponent<MissionPeer>();
        if (missionPeer == null || missionPeer.SpawnCountThisRound >= MaxSpawns)
        {
            return false;
        }

        var characterEquipment = CreateCharacterEquipment(crpgPeer.User!.Character.EquippedItems);
        if (!DoesEquipmentContainWeapon(characterEquipment)) // Disallow spawning without weapons.
        {
            if (_notifiedPlayersAboutSpawnRestriction.Add(networkPeer.VirtualPlayer.Id))
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new CrpgNotificationId
                {
                    Type = CrpgNotificationType.Announcement,
                    TextId = "str_kick_reason",
                    TextVariation = "no_weapon",
                    SoundEvent = string.Empty,
                });
                GameNetwork.EndModuleEventAsServer();
            }

            return false;
        }

        return true;
    }

    protected override void OnPeerSpawned(MissionPeer missionPeer)
    {
        base.OnPeerSpawned(missionPeer);
        UpdateSpawnCount(missionPeer, missionPeer.SpawnCountThisRound + 1);
        var crpgPeer = missionPeer.GetComponent<CrpgPeer>();
        crpgPeer.SpawnTeamThisRound ??= missionPeer.Team;
    }

    private void ResetSpawnTeams()
    {
        foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
        {
            var missionPeer = networkPeer.GetComponent<MissionPeer>();
            if (missionPeer != null)
            {
                UpdateSpawnCount(missionPeer, 0);
            }

            var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
            if (crpgPeer != null)
            {
                crpgPeer.SpawnTeamThisRound = null;
            }
        }

        _notifiedPlayersAboutSpawnRestriction.Clear();
    }

    private void UpdateSpawnCount(MissionPeer missionPeer, int spawnCount)
    {
        missionPeer.SpawnCountThisRound = spawnCount;

        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new UpdateRoundSpawnCount { Peer = missionPeer.GetNetworkPeer(), SpawnCount = spawnCount });
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
    }
}
