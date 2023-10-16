using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.MountAndBlade.Objects;

namespace Crpg.Module.Modes.Battle;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class CrpgBattleSpawnFlagMessage : GameNetworkMessage
{
    private static readonly CompressionInfo.Integer FlagCapturePointCharCompressionInfo = new(65, 5);
    public int FlagChar { get; set; } = default!;

    protected override void OnWrite()
    {
        WriteIntToPacket(FlagChar, FlagCapturePointCharCompressionInfo);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        FlagChar = GameNetworkMessage.ReadIntFromPacket(FlagCapturePointCharCompressionInfo, ref bufferReadValid);
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.GameMode;
    }

    protected override string OnGetLogFormat()
    {
        return "Random Flag Spawned";
    }
}
