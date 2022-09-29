using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class CrpgSoundEvent : GameNetworkMessage
{
    public string SoundEvent { get; set; } = default!;

    protected override void OnWrite()
    {
        WriteStringToPacket(SoundEvent);
    }

    protected override bool OnRead()
    {
        bool bufferReadValid = true;
        SoundEvent = ReadStringFromPacket(ref bufferReadValid);
        return bufferReadValid;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.Messaging;
    }

    protected override string OnGetLogFormat()
    {
        return "cRPG sound event from server: " + SoundEvent;
    }
}
