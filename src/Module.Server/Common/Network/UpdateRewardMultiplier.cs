using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class UpdateRewardMultiplier : GameNetworkMessage
{
    private static readonly CompressionInfo.Integer RewardMultiplierCompressionInfo = new(1, 5, true);

    public int RewardMultiplier { get; set; }

    protected override void OnWrite()
    {
        WriteIntToPacket(RewardMultiplier, RewardMultiplierCompressionInfo);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        RewardMultiplier = ReadIntFromPacket(RewardMultiplierCompressionInfo, ref bufferReadValid);
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.GameMode;
    }

    protected override string OnGetLogFormat()
    {
        return "Update reward multiplier";
    }
}
