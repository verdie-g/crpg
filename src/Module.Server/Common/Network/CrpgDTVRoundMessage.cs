using Crpg.Module.Api.Models;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class CrpgDTVRoundMessage : GameNetworkMessage
{
    private static readonly CompressionInfo.Integer Int32CompressionInfo = new(int.MinValue, int.MaxValue, true);

    public CrpgDTVRoundData RoundData { get; set; } = default!;
    public bool Valour { get; set; }
    public int RepairCost { get; set; }
    public int Compensation { get; set; }
    public List<string> BrokeItemIds { get; set; } = new();

    protected override void OnWrite()
    {
        WriteIntToPacket(RoundData.Wave, Int32CompressionInfo);
        WriteIntToPacket(RoundData.Round, Int32CompressionInfo);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        int wave = ReadIntFromPacket(Int32CompressionInfo, ref bufferReadValid);
        int round = ReadIntFromPacket(Int32CompressionInfo, ref bufferReadValid);
        RoundData = new CrpgDTVRoundData { Round = round, Wave = wave };

        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.GameMode;
    }

    protected override string OnGetLogFormat()
    {
        return "cRPG DTV Round Data";
    }
}
