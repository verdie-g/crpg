using Crpg.Module.Api.Models.Characters;
using Crpg.Module.Api.Models.Clans;
using Crpg.Module.Api.Models.Users;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class CrpgAddPeerComponent : GameNetworkMessage
{
    public NetworkCommunicator Peer { get; set; } = default!;

    public uint ComponentId { get; set; }

    protected override void OnWrite()
    {
        WriteNetworkPeerReferenceToPacket(Peer);
        WriteUintToPacket(ComponentId, CompressionBasic.PeerComponentCompressionInfo);
    }

    protected override bool OnRead()
    {
        bool result = true;
        Peer = ReadNetworkPeerReferenceFromPacket(ref result);
        ComponentId = ReadUintFromPacket(CompressionBasic.PeerComponentCompressionInfo, ref result);
        return result;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.GameMode;
    }

    protected override string OnGetLogFormat()
    {
        return "Add Peer Component cRPG";
    }
}
