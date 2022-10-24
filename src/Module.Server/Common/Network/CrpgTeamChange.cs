using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
internal sealed class CrpgTeamChange : GameNetworkMessage
{
    public bool AutoAssign
    {
        get;
        set;
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        AutoAssign = ReadBoolFromPacket(ref bufferReadValid);
        return bufferReadValid;
    }

    protected override void OnWrite()
    {
        WriteBoolToPacket(AutoAssign);
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.Mission;
    }

    protected override string OnGetLogFormat()
    {
        return "Changed crpg team";
    }
}
