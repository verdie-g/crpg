using Crpg.Module.Common.Network;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;

namespace Crpg.Module.Common;

internal static class KickHelper
{
    public static void Kick(NetworkCommunicator networkPeer, DisconnectType disconnectType, string? messageId = null)
    {
        if (messageId == null)
        {
            DisconnectPeer(networkPeer, disconnectType);
        }
        else
        {
            _ = KickAsync(networkPeer, disconnectType, messageId);
        }
    }

    private static async Task KickAsync(NetworkCommunicator networkPeer, DisconnectType disconnectType, string messageId)
    {
        GameNetwork.BeginModuleEventAsServer(networkPeer);
        GameNetwork.WriteMessage(new CrpgNotificationId
        {
            Type = CrpgNotificationType.Announcement,
            TextId = "str_kick_reason",
            TextVariation = messageId,
            SoundEvent = string.Empty,
        });
        GameNetwork.EndModuleEventAsServer();

        await Task.Delay(TimeSpan.FromMilliseconds(250));
        DisconnectPeer(networkPeer, disconnectType);
    }

    private static void DisconnectPeer(NetworkCommunicator networkPeer, DisconnectType disconnectType)
    {
        if (!networkPeer.IsConnectionActive)
        {
            return;
        }

        const string parameterName = "DisconnectInfo";
        var disconnectInfo = networkPeer.PlayerConnectionInfo.GetParameter<DisconnectInfo>(parameterName) ?? new DisconnectInfo();
        disconnectInfo.Type = disconnectType;
        networkPeer.PlayerConnectionInfo.AddParameter(parameterName, disconnectInfo);
        GameNetwork.AddNetworkPeerToDisconnectAsServer(networkPeer);
    }
}
