using Crpg.Module.Common;
using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.PlayerServices;

namespace Crpg.Module.Modes.Dtv;

internal class CrpgDtvSpawningBehavior : CrpgSpawningBehaviorBase
{
    private const float DefendersSpawnWindowDuration = 15f;
    private readonly HashSet<PlayerId> _notifiedPlayersAboutSpawnRestriction;

    private MissionTimer? _defendersSpawnWindowTimer;

    public CrpgDtvSpawningBehavior(CrpgConstants constants)
        : base(constants)
    {
        _notifiedPlayersAboutSpawnRestriction = new HashSet<PlayerId>();
    }

    public override void OnTick(float dt)
    {
        if (!IsSpawningEnabled || !IsRoundInProgress())
        {
            return;
        }

        if (_defendersSpawnWindowTimer != null && _defendersSpawnWindowTimer.Check())
        {
            _defendersSpawnWindowTimer = null;
            RequestStopSpawnSession();
        }

        SpawnAgents();
    }

    public void RequestSpawnSessionForRoundStart(bool firstRound)
    {
        RequestStartSpawnSession();
        _notifiedPlayersAboutSpawnRestriction.Clear();
        _defendersSpawnWindowTimer = new MissionTimer(DefendersSpawnWindowDuration);

        if (firstRound)
        {
            SpawnViscount();
        }
    }

    public void RequestSpawnSessionForWaveStart(CrpgDtvWave wave, int defendersCount)
    {
        SpawnAttackers(wave, defendersCount);
    }

    public override void RequestStartSpawnSession()
    {
        base.RequestStartSpawnSession();

        // CrpgSpawningBehaviorBase resets LastSpawnTeam before each spawning session so it needs to be manually set
        // for players that didn't die and don't have to respawn otherwise they would be consider as spectator and
        // won't receive any rewards.
        foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
        {
            var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
            if (crpgPeer != null && networkPeer.ControlledAgent != null)
            {
                crpgPeer.LastSpawnTeam = Mission.DefenderTeam;
            }
        }
    }

    protected override bool IsRoundInProgress()
    {
        return Mission.CurrentState == Mission.State.Continuing;
    }

    protected override bool IsPlayerAllowedToSpawn(NetworkCommunicator networkPeer)
    {
        var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
        var missionPeer = networkPeer.GetComponent<MissionPeer>();
        if (crpgPeer?.User == null
            || missionPeer == null
            || missionPeer.HasSpawnedAgentVisuals)
        {
            return false;
        }

        var characterEquipment = CrpgCharacterBuilder.CreateCharacterEquipment(crpgPeer.User.Character.EquippedItems);
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
        missionPeer.SpawnCountThisRound += 1;
    }

    private void SpawnViscount()
    {
        var viscountAgent = SpawnBotAgent("crpg_dtv_viscount", Mission.DefenderTeam);
        var viscountSpawn = Mission.Scene.FindEntityWithTag("viscount");
        if (viscountSpawn != null)
        {
            viscountAgent.TeleportToPosition(viscountSpawn.GetGlobalFrame().origin);
        }

        // Prevent the viscount from moving.
        viscountAgent.SetTargetPosition(viscountAgent.Position.AsVec2);
    }

    private void SpawnAttackers(CrpgDtvWave wave, int defendersCount)
    {
        foreach (CrpgDtvGroup group in wave.Groups)
        {
            float botCount = group.ClassDivisionId.Contains("boss") ? group.Count : defendersCount * group.Count;
            Debug.Print($"Spawning {botCount} {group.ClassDivisionId}(s)");
            for (int i = 0; i < botCount; i++)
            {
                SpawnBotAgent(group.ClassDivisionId, Mission.AttackerTeam);
            }
        }
    }
}
