using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
internal sealed class XboxIdMessage : GameNetworkMessage
{
    public string XboxId { get; set; } = string.Empty;

    protected override void OnWrite()
    {
        WriteStringToPacket(XboxId);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        XboxId = ReadStringFromPacket(ref bufferReadValid);
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.Messaging;
    }

    protected override string OnGetLogFormat()
    {
        return "Xbox id from client";
    }
}
