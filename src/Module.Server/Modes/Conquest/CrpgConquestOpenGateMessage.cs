using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Modes.Conquest;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class CrpgConquestOpenGateMessage : GameNetworkMessage
{
    public string CrpgUserName { get; set; } = string.Empty;

    protected override void OnWrite()
    {
        WriteStringToPacket(CrpgUserName);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        CrpgUserName = ReadStringFromPacket(ref bufferReadValid);
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.GameMode;
    }

    protected override string OnGetLogFormat()
    {
        return "Conquest Gate Opened";
    }
}
