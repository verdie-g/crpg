using Crpg.Module.Common.GameHandler;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;

namespace Crpg.Module.Common.ChatCommands.Admin;

internal class KickCommand : AdminCommand
{
    public KickCommand()
    {
        Name = "kick";
        Description = $"'{ChatCommandHandler.CommandPrefix}{Name} PLAYERID' to kick a player.";
        Overloads = new CommandOverload[]
        {
            new(new[] { ChatCommandParameterType.PlayerId }, ExecuteKickByNetworkPeer),
            new(new[] { ChatCommandParameterType.String }, ExecuteKickByName),
        };
    }

    protected override void ExecuteFailed(NetworkCommunicator fromPeer)
    {
        CrpgChatBox crpgChat = GetChat();
        crpgChat.ServerSendMessageToPlayer(fromPeer, ColorInfo, $"Wrong usage. Type {Description}");
    }

    private void ExecuteKickByName(NetworkCommunicator fromPeer, string cmd, object[] arguments)
    {
        string targetName = (string)arguments[0];
        if (!TryGetPlayerByName(fromPeer, targetName, out var targetPeer))
        {
            return;
        }

        arguments = new object[] { targetPeer! };
        ExecuteKickByNetworkPeer(fromPeer, cmd, arguments);
    }

    private void ExecuteKickByNetworkPeer(NetworkCommunicator fromPeer, string cmd, object[] arguments)
    {
        CrpgChatBox crpgChat = GetChat();
        var targetPeer = (NetworkCommunicator)arguments[0];
        crpgChat.ServerSendMessageToPlayer(fromPeer, ColorFatal, $"You have kicked {targetPeer.UserName}.");
        var disconnectInfo = fromPeer.PlayerConnectionInfo.GetParameter<DisconnectInfo>("DisconnectInfo") ?? new DisconnectInfo();
        disconnectInfo.Type = DisconnectType.KickedByHost;
        targetPeer.PlayerConnectionInfo.AddParameter("DisconnectInfo", disconnectInfo);
        GameNetwork.AddNetworkPeerToDisconnectAsServer(targetPeer);
    }
}
