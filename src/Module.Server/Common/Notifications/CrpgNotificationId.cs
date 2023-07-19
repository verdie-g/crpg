using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Notifications;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class CrpgNotificationId : GameNetworkMessage
{
    public CrpgNotificationType Type { get; set; }
    public string TextId { get; set; } = string.Empty;
    public string? TextVariation { get; set; }
    public string SoundEvent { get; set; } = string.Empty;
    public Dictionary<string, string> Variables { get; set; } = new();

    protected override void OnWrite()
    {
        WriteIntToPacket((int)Type, CompressionBasic.DebugIntNonCompressionInfo);
        WriteStringToPacket(TextId);
        WriteStringToPacket(TextVariation);
        WriteStringToPacket(SoundEvent);
        WriteIntToPacket(Variables.Count, CompressionBasic.DebugIntNonCompressionInfo);
        foreach (var v in Variables)
        {
            WriteStringToPacket(v.Key);
            WriteStringToPacket(v.Value);
        }
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
        int variablesCount = ReadIntFromPacket(CompressionBasic.DebugIntNonCompressionInfo, ref bufferReadValid);
        for (int i = 0; i < variablesCount; i += 1)
        {
            string key = ReadStringFromPacket(ref bufferReadValid);
            string val = ReadStringFromPacket(ref bufferReadValid);
            Variables[key] = val;
        }

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
