using System.Text;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Modes.Dtv;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class CrpgDtvViscountUnderAttackMessage : GameNetworkMessage
{
    public Agent? Attacker { get; set; }
    protected override void OnWrite()
    {
        WriteAgentReferenceToPacket(Attacker);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        Attacker = ReadAgentReferenceFromPacket(ref bufferReadValid);
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.GameMode;
    }

    protected override string OnGetLogFormat()
    {
        return "cRPG DTV Viscount Under Attack";
    }
}
