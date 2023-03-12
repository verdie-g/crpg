using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class CrpgNotificationId : GameNetworkMessage
{
    public CrpgNotificationType Type { get; set; }
    public string TextId { get; set; } = string.Empty;
    public string? TextVariation { get; set; }
    public string SoundEvent { get; set; } = string.Empty;

    protected override void OnWrite()
    {
        WriteIntToPacket((int)Type, CompressionBasic.DebugIntNonCompressionInfo);
        WriteStringToPacket(TextId);
        WriteStringToPacket(TextVariation);
        WriteStringToPacket(SoundEvent);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        Type = (CrpgNotificationType)ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref bufferReadValid);
        TextId = ReadStringFromPacket(ref bufferReadValid);
        TextVariation = ReadStringFromPacket(ref bufferReadValid);
        if (string.IsNullOrEmpty(TextVariation))
        {
            TextVariation = null;
        }

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
