using Crpg.Module.Common;
using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.PlayerServices;

namespace Crpg.Module.Siege;

internal class CrpgSiegeSpawningBehavior : CrpgSpawningBehaviorBase
{
    private readonly HashSet<PlayerId> _notifiedPlayersAboutSpawnRestriction;

    public CrpgSiegeSpawningBehavior(CrpgConstants constants)
        : base(constants)
    {
        _notifiedPlayersAboutSpawnRestriction = new HashSet<PlayerId>();
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

        return true;
    }
}
