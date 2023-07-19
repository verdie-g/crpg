using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Rewards;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class CrpgRewardHappyHour : GameNetworkMessage
{
    public bool Started { get; set; }

    protected override void OnWrite()
    {
        WriteBoolToPacket(Started);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        Started = ReadBoolFromPacket(ref bufferReadValid);
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.GameMode;
    }

    protected override string OnGetLogFormat()
    {
        return nameof(CrpgRewardHappyHour);
    }
}
