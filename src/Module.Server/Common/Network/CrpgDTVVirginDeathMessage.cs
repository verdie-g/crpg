using Crpg.Module.Api.Models;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class CrpgDTVVirginDeathMessage : GameNetworkMessage
{
    public CrpgDTVRoundData RoundData { get; set; } = default!;

    protected override void OnWrite()
    {
        WriteBoolToPacket(RoundData.IsVirginDead);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        bool isVirginDead = ReadBoolFromPacket(ref bufferReadValid);
        RoundData = new CrpgDTVRoundData { IsVirginDead = isVirginDead };

        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.GameMode;
    }

    protected override string OnGetLogFormat()
    {
        return "cRPG DTV Virgin Death Data";
    }
}
