using Crpg.Module.Common;
using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.PlayerServices;

namespace Crpg.Module.Siege;

internal class CrpgSiegeSpawningBehavior : CrpgSpawningBehaviorBase
{
    private readonly Dictionary<PlayerId, MissionTime> _lastSpawnRestrictionNotifications;

    public CrpgSiegeSpawningBehavior(CrpgConstants constants)
        : base(constants)
    {
        _lastSpawnRestrictionNotifications = new Dictionary<PlayerId, MissionTime>();
    }

    public override void OnTick(float dt)
    {
        if (IsSpawningEnabled && _spawnCheckTimer.Check(Mission.CurrentTime))
        {
            SpawnAgents();
            SpawnBotAgents();
        }

        base.OnTick(dt);
    }

    public override bool AllowEarlyAgentVisualsDespawning(MissionPeer missionPeer)
    {
        return false;
    }

    public override int GetMaximumReSpawnPeriodForPeer(MissionPeer peer)
    {
        if (GameMode.WarmupComponent is { IsInWarmup: true })
        {
            return 3;
        }

        if (peer.Team != null)
        {
            if (peer.Team.Side == BattleSideEnum.Attacker)
            {
                return MultiplayerOptions.OptionType.RespawnPeriodTeam1.GetIntValue();
            }

            if (peer.Team.Side == BattleSideEnum.Defender)
            {
                return MultiplayerOptions.OptionType.RespawnPeriodTeam2.GetIntValue();
            }
        }

        return -1;
    }

    protected override bool IsRoundInProgress()
    {
        return Mission.CurrentState == Mission.State.Continuing;
    }

    protected override void SpawnAgents()
    {
        SpawnPeerAgents();
    }

    protected override bool IsPlayerAllowedToSpawn(NetworkCommunicator networkPeer)
    {
        var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
        var missionPeer = networkPeer.GetComponent<MissionPeer>();
        if (crpgPeer?.User == null
            || crpgPeer.SpawnTeamThisRound != null
            || missionPeer == null
            || missionPeer.HasSpawnedAgentVisuals
            || !missionPeer.SpawnTimer.Check(Mission.CurrentTime))
        {
            return false;
        }

        var characterEquipment = CreateCharacterEquipment(crpgPeer.User.Character.EquippedItems);
        if (!DoesEquipmentContainWeapon(characterEquipment)) // Disallow spawning without weapons.
        {
            if (!_lastSpawnRestrictionNotifications.TryGetValue(networkPeer.VirtualPlayer.Id, out var lastNotification))
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new CrpgNotification
                {
                    Type = CrpgNotification.NotificationType.Announcement,
                    Message = "You should have at least one weapon equipped to spawn! Equip a weapon and reconnect to the server.",
                    IsMessageTextId = false,
                    SoundEvent = string.Empty,
                });
                GameNetwork.EndModuleEventAsServer();

                _lastSpawnRestrictionNotifications[networkPeer.VirtualPlayer.Id] = MissionTime.Now;
            }
            else if (lastNotification + MissionTime.Seconds(3) < MissionTime.Now)
            {
                const string parameterName = "DisconnectInfo";
                var disconnectInfo = networkPeer.PlayerConnectionInfo.GetParameter<DisconnectInfo>(parameterName) ?? new DisconnectInfo();
                disconnectInfo.Type = DisconnectType.KickedByHost;
                networkPeer.PlayerConnectionInfo.AddParameter(parameterName, disconnectInfo);
                GameNetwork.AddNetworkPeerToDisconnectAsServer(networkPeer);
            }

            return false;
        }

        return true;
    }
}
