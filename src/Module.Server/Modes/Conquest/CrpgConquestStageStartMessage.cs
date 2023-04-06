using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Modes.Conquest;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class CrpgConquestStageStartMessage : GameNetworkMessage
{
    public int StageIndex { get; set; }
    public int StageStartTime { get; set; }
    public int StageDuration { get; set; }

    protected override void OnWrite()
    {
        WriteIntToPacket(StageIndex, CompressionBasic.DebugIntNonCompressionInfo);
        WriteIntToPacket(StageStartTime, CompressionBasic.DebugIntNonCompressionInfo);
        WriteIntToPacket(StageDuration, CompressionBasic.DebugIntNonCompressionInfo);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        StageIndex = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref bufferReadValid);
        StageStartTime = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref bufferReadValid);
        StageDuration = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref bufferReadValid);
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.GameMode;
    }

    protected override string OnGetLogFormat()
    {
        return "Conquest Stage Start";
    }
}
