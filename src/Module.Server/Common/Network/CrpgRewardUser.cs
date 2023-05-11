using Crpg.Module.Api.Models;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class CrpgRewardUser : GameNetworkMessage
{
    private static readonly CompressionInfo.Integer Int32CompressionInfo = new(int.MinValue, int.MaxValue, true);

    public CrpgUserEffectiveReward Reward { get; set; } = default!;
    public bool Valour { get; set; }
    public int RepairCost { get; set; }
    public int TotalCompensation { get; set; }
    public List<string> BrokeItemIds { get; set; } = new();

    protected override void OnWrite()
    {
        WriteIntToPacket(Reward.Gold, Int32CompressionInfo);
        WriteIntToPacket(Reward.Experience, Int32CompressionInfo);
        WriteBoolToPacket(Reward.LevelUp);
        WriteBoolToPacket(Valour);
        WriteIntToPacket(RepairCost, Int32CompressionInfo);
        WriteIntToPacket(TotalCompensation, Int32CompressionInfo);
        WriteIntToPacket(BrokeItemIds.Count, Int32CompressionInfo);
        foreach (string soldItem in BrokeItemIds)
        {
            WriteStringToPacket(soldItem);
        }
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        int gold = ReadIntFromPacket(Int32CompressionInfo, ref bufferReadValid);
        int experience = ReadIntFromPacket(Int32CompressionInfo, ref bufferReadValid);
        bool levelUp = ReadBoolFromPacket(ref bufferReadValid);
        Reward = new CrpgUserEffectiveReward { Gold = gold, Experience = experience, LevelUp = levelUp };
        Valour = ReadBoolFromPacket(ref bufferReadValid);
        RepairCost = ReadIntFromPacket(Int32CompressionInfo, ref bufferReadValid);
        TotalCompensation = ReadIntFromPacket(Int32CompressionInfo, ref bufferReadValid);
        int brokeItemIds = ReadIntFromPacket(Int32CompressionInfo, ref bufferReadValid);
        for (int i = 0; i < brokeItemIds; i += 1)
        {
            BrokeItemIds.Add(ReadStringFromPacket(ref bufferReadValid));
        }

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
