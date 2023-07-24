using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Modes.Dtv;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class CrpgDtvSetTimerMessage : GameNetworkMessage
{
    public int StartTime { get; set; }
    public int Duration { get; set; }

    protected override void OnWrite()
    {
        WriteIntToPacket(StartTime, CompressionBasic.DebugIntNonCompressionInfo);
        WriteIntToPacket(Duration, CompressionBasic.DebugIntNonCompressionInfo);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        StartTime = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref bufferReadValid);
        Duration = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref bufferReadValid);
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.GameMode;
    }

    protected override string OnGetLogFormat()
    {
        return nameof(CrpgDtvSetTimerMessage);
    }
}
