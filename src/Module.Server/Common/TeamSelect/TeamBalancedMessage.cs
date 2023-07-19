using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.TeamSelect;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class TeamBalancedMessage : GameNetworkMessage
{
    public int DefendersMovedToAttackers { get; set; }
    public int AttackersMovedToDefenders { get; set; }
    public int DefendersJoined { get; set; }
    public int AttackersJoined { get; set; }

    protected override void OnWrite()
    {
        WriteIntToPacket(DefendersMovedToAttackers, CompressionBasic.DebugIntNonCompressionInfo);
        WriteIntToPacket(AttackersMovedToDefenders, CompressionBasic.DebugIntNonCompressionInfo);
        WriteIntToPacket(DefendersJoined, CompressionBasic.DebugIntNonCompressionInfo);
        WriteIntToPacket(AttackersJoined, CompressionBasic.DebugIntNonCompressionInfo);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        DefendersMovedToAttackers = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref bufferReadValid);
        AttackersMovedToDefenders = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref bufferReadValid);
        DefendersJoined = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref bufferReadValid);
        AttackersJoined = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref bufferReadValid);
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.Messaging;
    }

    protected override string OnGetLogFormat()
    {
        return nameof(TeamBalancedMessage);
    }
}
