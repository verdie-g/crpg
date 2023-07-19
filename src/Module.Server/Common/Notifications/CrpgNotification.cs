using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Notifications;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class CrpgNotification : GameNetworkMessage
{
    public CrpgNotificationType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public string SoundEvent { get; set; } = string.Empty;

    protected override void OnWrite()
    {
        WriteIntToPacket((int)Type, CompressionBasic.DebugIntNonCompressionInfo);
        WriteStringToPacket(Message);
        WriteStringToPacket(SoundEvent);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        Type = (CrpgNotificationType)ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref bufferReadValid);
        Message = ReadStringFromPacket(ref bufferReadValid);
        SoundEvent = ReadStringFromPacket(ref bufferReadValid);
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.Messaging;
    }

    protected override string OnGetLogFormat()
    {
        return "cRPG Notification message from server";
    }
}
