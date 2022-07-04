using Crpg.Module.Api.Models.Users;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace Crpg.Module.Common.Network;

[DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
internal sealed class UpdateCrpgUser : GameNetworkMessage
{
    public CrpgUser User { get; set; } = default!;

    protected override void OnWrite()
    {
    }

    protected override bool OnRead()
    {
        return true;
    }

    protected override MultiplayerMessageFilter OnGetLogFilter()
    {
        return MultiplayerMessageFilter.GameMode;
    }

    protected override string OnGetLogFormat()
    {
        return "Update cRPG User";
    }
}
