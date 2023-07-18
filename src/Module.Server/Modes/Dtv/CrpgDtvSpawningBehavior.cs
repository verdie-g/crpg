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
        float expectedWaveBotsCount = wave.Groups.Sum(g => ComputeScaledGroupCount(g, defendersCount));
        // Round up number if it's like 3.999
        expectedWaveBotsCount = expectedWaveBotsCount - Math.Truncate(expectedWaveBotsCount) > 0.99
            ? (float)Math.Truncate(expectedWaveBotsCount) + 1f
            : expectedWaveBotsCount;

        int actualWaveBotCount = 0;
        foreach (CrpgDtvGroup group in wave.Groups)
        {
            int groupBotCount = (int)ComputeScaledGroupCount(group, defendersCount);
            actualWaveBotCount += groupBotCount;
            Debug.Print($"Spawning {groupBotCount} {group.ClassDivisionId}(s)");
            for (int i = 0; i < groupBotCount; i++)
            {
                SpawnBotAgent(group.ClassDivisionId, Mission.AttackerTeam);
            }
        }

        // In case the wave is 0.5*A + 0.5*B, no bots will spawn if there is only one player. To cope with that,
        // extra bots are spawned.
        int extraBotsToSpawn = (int)expectedWaveBotsCount - actualWaveBotCount;
        var groupsWithoutBoss = wave.Groups.Where(g => g.Count != 0f).ToArray();
        for (int i = 0; i < extraBotsToSpawn; i += 1)
        {
            var group = groupsWithoutBoss[i % groupsWithoutBoss.Length];
            SpawnBotAgent(group.ClassDivisionId, Mission.AttackerTeam);
        }
    }

    private float ComputeScaledGroupCount(CrpgDtvGroup group, int defendersCount)
    {
        return group.Count == 0f ? 1f : defendersCount * group.Count;
    }
}
