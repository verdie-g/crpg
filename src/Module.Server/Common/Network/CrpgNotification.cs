using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class CrpgNotification : GameNetworkMessage
{
    public string Message { get; set; } = default!;
    public bool IsMessageTextId { get; set; }
    public string SoundEvent { get; set; } = default!;

    protected override void OnWrite()
    {
        WriteStringToPacket(Message);
        WriteBoolToPacket(IsMessageTextId);
        WriteStringToPacket(SoundEvent);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        Message = ReadStringFromPacket(ref bufferReadValid);
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
        return "cRPG Notification message from server: " + Message;
    }
}
