using System.Net;
using System.Net.Sockets;
using System.Text;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

/// <summary>
/// Emit a metric with the number of playing users. Since it's the only metric emitted, to avoid a dependency to an
/// external library, the Datadog datagram is crafted and sent to the DogStatsD port.
/// </summary>
internal class PlayerStatsComponent : MissionBehavior
{
    private static readonly EndPoint DogStatsDEndpoint = new DnsEndPoint("localhost", 8125);
    private static readonly string? Service = Environment.GetEnvironmentVariable("CRPG_SERVICE");
    private static readonly string? Instance = Environment.GetEnvironmentVariable("CRPG_INSTANCE");

    private MissionTimer? _updateTimer;
    private Socket? _socket;

    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

    public override void OnBehaviorInitialize()
    {
        if (Service == null || Instance == null)
        {
            return;
        }

        _updateTimer = new MissionTimer(60);
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _socket.Connect(DogStatsDEndpoint);
    }

    public override void OnRemoveBehavior()
    {
        _socket?.Dispose();
    }

    public override void OnMissionTick(float dt)
    {
        if (_updateTimer == null || !_updateTimer.Check(reset: true))
        {
            return;
        }

        int players = GameNetwork.NetworkPeers.Count(x => x.IsSynchronized);

        // https://docs.datadoghq.com/developers/dogstatsd/datagram_shell?tab=metrics
        string datagramStr = $"crpg.users.playing.count:{players}|g|#service:{Service},instance:{Instance}";
        byte[] datagram = Encoding.ASCII.GetBytes(datagramStr);
        _socket!.Send(datagram);
    }
}
