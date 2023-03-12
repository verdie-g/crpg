using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class UpdateSharedConstant : GameNetworkMessage
{
    public int Id { get; set; }
    public float OldValue { get; set; }
    public float NewValue { get; set; }

    protected override void OnWrite()
    {
        WriteIntToPacket(Id, CompressionBasic.DebugIntNonCompressionInfo);
        WriteFloatToPacket(OldValue, CompressionInfo.Float.FullPrecision);
        WriteFloatToPacket(NewValue, CompressionInfo.Float.FullPrecision);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        Id = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref bufferReadValid);
        OldValue = ReadFloatFromPacket(CompressionInfo.Float.FullPrecision, ref bufferReadValid);
        NewValue = ReadFloatFromPacket(CompressionInfo.Float.FullPrecision, ref bufferReadValid);
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.GameMode;
    }

    protected override string OnGetLogFormat()
    {
        return "Update shared constant";
    }
}
