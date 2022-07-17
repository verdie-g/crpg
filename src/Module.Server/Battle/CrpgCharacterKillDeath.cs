using Crpg.Module.Common;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;

namespace Crpg.Module.Battle;

public class CrpgCharacterKillDeath : MissionLogic
{
    private readonly MultipleBattleResult _battleResult;

    public CrpgCharacterKillDeath(MultipleBattleResult battleResult)
    {
        _battleResult = battleResult;
    }

    protected override void OnEndMission()
    {
        var peers = GameNetwork.NetworkPeers.ToDictionary(p => p.VirtualPlayer.Id);
        foreach (var playerEntry in _battleResult.GetCurrentBattleResult().PlayerEntries)
        {
            if (!peers.TryGetValue(playerEntry.Key, out var peer))
            {
                continue;
            }

            var crpgRepresentative = peer.GetComponent<CrpgRepresentative>();
            if (crpgRepresentative?.User == null)
            {
                continue;
            }

            var stats = playerEntry.Value.PlayerStats;
        }
    }
}
