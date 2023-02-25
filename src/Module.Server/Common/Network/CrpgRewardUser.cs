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
    public List<string> SoldItemIds { get; set; } = new();

    protected override void OnWrite()
    {
        WriteIntToPacket(Reward.Gold, Int32CompressionInfo);
        WriteIntToPacket(Reward.Experience, Int32CompressionInfo);
        WriteBoolToPacket(Reward.LevelUp);
        WriteBoolToPacket(Valour);
        WriteIntToPacket(RepairCost, Int32CompressionInfo);
        WriteIntToPacket(SoldItemIds.Count, Int32CompressionInfo);
        foreach (string soldItem in SoldItemIds)
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
        int soldItemIdsCount = ReadIntFromPacket(Int32CompressionInfo, ref bufferReadValid);
        for (int i = 0; i < soldItemIdsCount; i += 1)
        {
            SoldItemIds.Add(ReadStringFromPacket(ref bufferReadValid));
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
