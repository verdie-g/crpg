using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;

namespace Crpg.Module.Common;

internal static class KickHelper
{
    public static void Kick(NetworkCommunicator networkPeer, DisconnectType disconnectType)
    {
        const string parameterName = "DisconnectInfo";
        var disconnectInfo = networkPeer.PlayerConnectionInfo.GetParameter<DisconnectInfo>(parameterName) ?? new DisconnectInfo();
        disconnectInfo.Type = disconnectType;
        networkPeer.PlayerConnectionInfo.AddParameter(parameterName, disconnectInfo);
        GameNetwork.AddNetworkPeerToDisconnectAsServer(networkPeer);
    }
}
