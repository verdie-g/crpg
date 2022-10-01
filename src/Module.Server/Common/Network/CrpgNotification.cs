using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class CrpgNotification : GameNetworkMessage
{
    public enum NotificationType
    {
        Notification,
        Announcement,
        Sound,
        End,
    }

    private static readonly CompressionInfo.Integer NotificationCompressionInfo = new(0, (int)NotificationType.End - 1, true);


    public string Message { get; set; } = default!;
    public NotificationType Type { get; set; } = default!;

    public bool IsMessageTextId { get; set; }
    public string SoundEvent { get; set; } = default!;

    protected override void OnWrite()
    {
        WriteStringToPacket(Message);
        WriteIntToPacket((int)Type, NotificationCompressionInfo);
        WriteBoolToPacket(IsMessageTextId);
        WriteStringToPacket(SoundEvent);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        Message = ReadStringFromPacket(ref bufferReadValid);
        Type = (NotificationType)ReadIntFromPacket(NotificationCompressionInfo, ref bufferReadValid);
        IsMessageTextId = ReadBoolFromPacket(ref bufferReadValid);
        SoundEvent = ReadStringFromPacket(ref bufferReadValid);
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.Messaging;
    }

    protected override string OnGetLogFormat()
    {
        return $"cRPG Notification message from server. Type: {Type}. Msg: {Message}";
    }
}
