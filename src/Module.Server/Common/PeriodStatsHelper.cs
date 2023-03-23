using TaleWorlds.MountAndBlade;
using TaleWorlds.PlayerServices;

namespace Crpg.Module.Common;

internal class PeriodStatsHelper
{
    private readonly Dictionary<PlayerId, PeriodStats> _lastPeriodStatsSum;

    public PeriodStatsHelper()
    {
        _lastPeriodStatsSum = new Dictionary<PlayerId, PeriodStats>();
    }

    public Dictionary<PlayerId, PeriodStats> ComputePeriodStats()
    {
        Dictionary<PlayerId, PeriodStats> periodStats = new();
        foreach (var networkPeer in GameNetwork.NetworkPeersIncludingDisconnectedPeers)
        {
            var missionPeer = networkPeer.GetComponent<MissionPeer>();
            if (missionPeer == null)
            {
                continue;
            }

            PeriodStats newPeriodStatsSum = new()
            {
                Score = missionPeer.Score,
                Kills = missionPeer.KillCount,
                Deaths = missionPeer.DeathCount,
                Assists = missionPeer.AssistCount,
                PlayTime = DateTime.Now - missionPeer.JoinTime,
            };

            PeriodStats currentPeriodStats;
#pragma warning disable IDE0045
            if (_lastPeriodStatsSum.TryGetValue(networkPeer.VirtualPlayer.Id, out var lastPeriodStatsSum))
#pragma warning restore IDE0045
            {
                currentPeriodStats = new PeriodStats
                {
                    Score = newPeriodStatsSum.Score - lastPeriodStatsSum.Score,
                    Kills = newPeriodStatsSum.Kills - lastPeriodStatsSum.Kills,
                    Deaths = newPeriodStatsSum.Deaths - lastPeriodStatsSum.Deaths,
                    Assists = newPeriodStatsSum.Assists - lastPeriodStatsSum.Assists,
                    PlayTime = newPeriodStatsSum.PlayTime - lastPeriodStatsSum.PlayTime,
                };
            }
            else
            {
                currentPeriodStats = new PeriodStats
                {
                    Score = newPeriodStatsSum.Score,
                    Kills = newPeriodStatsSum.Kills,
                    Deaths = newPeriodStatsSum.Deaths,
                    Assists = newPeriodStatsSum.Assists,
                    PlayTime = newPeriodStatsSum.PlayTime,
                };
            }

            _lastPeriodStatsSum[networkPeer.VirtualPlayer.Id] = newPeriodStatsSum;
            periodStats[networkPeer.VirtualPlayer.Id] = currentPeriodStats;
        }

        return periodStats;
    }
}
