using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class UpdateRoundSpawnCount : GameNetworkMessage
{
    public NetworkCommunicator Peer { get; set; } = default!;
    public int SpawnCount { get; set; }

    protected override void OnWrite()
    {
        WriteNetworkPeerReferenceToPacket(Peer);
        WriteIntToPacket(SpawnCount, CompressionBasic.DebugIntNonCompressionInfo);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        Peer = ReadNetworkPeerReferenceFromPacket(ref bufferReadValid);
        SpawnCount = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref bufferReadValid);
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.GameMode;
    }

    protected override string OnGetLogFormat()
    {
        return "Update spawn count";
    }
}
