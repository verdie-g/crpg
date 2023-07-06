using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Modes.Conquest;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class CrpgConquestOpenGateMessage : GameNetworkMessage
{
    public NetworkCommunicator Peer { get; set; } = default!;

    protected override void OnWrite()
    {
        WriteNetworkPeerReferenceToPacket(Peer);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        Peer = ReadNetworkPeerReferenceFromPacket(ref bufferReadValid);
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.GameMode;
    }

    protected override string OnGetLogFormat()
    {
        return "Conquest Gate Opened";
    }
}
