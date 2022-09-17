using Crpg.Module.Api.Models;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class CrpgRewardUser : GameNetworkMessage
{
    private static readonly CompressionInfo.Integer Int32CompressionInfo = new(int.MinValue, int.MaxValue, true);

    public CrpgUserEffectiveReward Reward { get; set; } = default!;
    public int RepairCost { get; set; }

    protected override void OnWrite()
    {
        WriteIntToPacket(Reward.Gold, Int32CompressionInfo);
        WriteIntToPacket(Reward.Experience, Int32CompressionInfo);
        WriteBoolToPacket(Reward.LevelUp);
        WriteIntToPacket(RepairCost, Int32CompressionInfo);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        int gold = ReadIntFromPacket(Int32CompressionInfo, ref bufferReadValid);
        int experience = ReadIntFromPacket(Int32CompressionInfo, ref bufferReadValid);
        bool levelUp = ReadBoolFromPacket(ref bufferReadValid);
        Reward = new CrpgUserEffectiveReward { Gold = gold, Experience = experience, LevelUp = levelUp };
        RepairCost = ReadIntFromPacket(Int32CompressionInfo, ref bufferReadValid);
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.GameMode;
    }

    protected override string OnGetLogFormat()
    {
        return "cRPG Reward User";
    }
}
