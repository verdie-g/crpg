using Crpg.Module.Api.Models;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class CrpgDTVWaveEndMessage : GameNetworkMessage
{
    private static readonly CompressionInfo.Integer Int32CompressionInfo = new(int.MinValue, int.MaxValue, true);

    public CrpgDTVRoundData RoundData { get; set; } = default!;

    protected override void OnWrite()
    {
        WriteIntToPacket(RoundData.Wave, Int32CompressionInfo);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        int wave = ReadIntFromPacket(Int32CompressionInfo, ref bufferReadValid);
        RoundData = new CrpgDTVRoundData { Wave = wave };

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
