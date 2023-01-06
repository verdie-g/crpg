using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class CrpgServerMessage : GameNetworkMessage
{
    private static readonly CompressionInfo.Float FloatCompressionInfo = new(0.0f, 1.1f, 7);
    public string Message { get; set; } = default!;

    public float Red { get; set; }

    public float Green { get; set; }

    public float Blue { get; set; }

    public float Alpha { get; set; }

    public bool IsMessageTextId { get; set; }

    protected override void OnWrite()
    {
        WriteStringToPacket(Message);
        WriteFloatToPacket(Red, FloatCompressionInfo);
        WriteFloatToPacket(Green, FloatCompressionInfo);
        WriteFloatToPacket(Blue, FloatCompressionInfo);
        WriteFloatToPacket(Alpha, FloatCompressionInfo);
        WriteBoolToPacket(IsMessageTextId);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        Message = ReadStringFromPacket(ref bufferReadValid);
        Red = ReadFloatFromPacket(FloatCompressionInfo, ref bufferReadValid);
        Green = ReadFloatFromPacket(FloatCompressionInfo, ref bufferReadValid);
        Blue = ReadFloatFromPacket(FloatCompressionInfo, ref bufferReadValid);
        Alpha = ReadFloatFromPacket(FloatCompressionInfo, ref bufferReadValid);
        IsMessageTextId = ReadBoolFromPacket(ref bufferReadValid);
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.Messaging;
    }

    protected override string OnGetLogFormat()
    {
        return "Crpg Message from server: " + Message;
    }
}
