using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class WelcomeMessage : GameNetworkMessage
{
    public NetworkCommunicator[] Peers { get; set; } = Array.Empty<NetworkCommunicator>();

    protected override void OnWrite()
    {
        WriteIntToPacket(Peers.Length, CompressionBasic.DebugIntNonCompressionInfo);
        foreach (var peer in Peers)
        {
            WriteNetworkPeerReferenceToPacket(peer);
        }
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        int peersLength = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref bufferReadValid);
        Peers = new NetworkCommunicator[peersLength];
        for (int i = 0; i < Peers.Length; i += 1)
        {
            Peers[i] = ReadNetworkPeerReferenceFromPacket(ref bufferReadValid);
        }

        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.GameMode;
    }

    protected override string OnGetLogFormat()
    {
        return "Welcome message";
    }
}
