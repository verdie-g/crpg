using Crpg.Module.Api.Models;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class RewardCrpgUser : GameNetworkMessage
{
    private static readonly CompressionInfo.Integer Int32CompressionInfo = new(int.MinValue, int.MaxValue, true);

    public CrpgUserEffectiveReward Reward { get; set; } = default!;

    protected override void OnWrite()
    {
        WriteIntToPacket(Reward.Gold, Int32CompressionInfo);
        WriteIntToPacket(Reward.Experience, Int32CompressionInfo);
        WriteBoolToPacket(Reward.LevelUp);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        int gold = ReadIntFromPacket(Int32CompressionInfo, ref bufferReadValid);
        int experience = ReadIntFromPacket(Int32CompressionInfo, ref bufferReadValid);
        bool levelUp = ReadBoolFromPacket(ref bufferReadValid);
        Reward = new CrpgUserEffectiveReward { Gold = gold, Experience = experience, LevelUp = levelUp };
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.GameMode;
    }

    protected override string OnGetLogFormat()
    {
        return "Reward cRPG User";
    }
}
