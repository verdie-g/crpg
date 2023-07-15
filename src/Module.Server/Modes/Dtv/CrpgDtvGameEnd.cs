using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Modes.Dtv;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class CrpgDtvGameEnd : GameNetworkMessage
{
    public bool ViscountDead { get; set; }

    protected override void OnWrite()
    {
        WriteBoolToPacket(ViscountDead);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        ViscountDead = ReadBoolFromPacket(ref bufferReadValid);
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.GameMode;
    }

    protected override string OnGetLogFormat()
    {
        return "cRPG DTV Viscount Death Data";
    }
}
