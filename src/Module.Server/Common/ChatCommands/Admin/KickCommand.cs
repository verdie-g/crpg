using Crpg.Module.Common.GameHandler;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;

namespace Crpg.Module.Common.ChatCommands.Admin;

internal class KickCommand : AdminCommand
{
    public KickCommand()
    {
        Name = "kick";
        Description = $"'{ChatCommandHandler.CommandPrefix}{Name} PLAYERID [REASON]' to kick a player.";
        Overloads = new CommandOverload[]
        {
            new(new[] { ChatCommandParameterType.PlayerId }, ExecuteKickByNetworkPeer),
            new(new[] { ChatCommandParameterType.PlayerId, ChatCommandParameterType.String }, ExecuteKickByNetworkPeer),
            new(new[] { ChatCommandParameterType.String, ChatCommandParameterType.String }, ExecuteKickByName),
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

        string? reason = null;
        if (arguments.Length > 1)
        {
            reason = (string)arguments[1];
        }

        arguments = new object[] { targetPeer!, reason! };
        ExecuteKickByNetworkPeer(fromPeer, cmd, arguments);
    }

    private async void ExecuteKickByNetworkPeer(NetworkCommunicator fromPeer, string cmd, object[] arguments)
    {
        CrpgChatBox crpgChat = GetChat();
        var targetPeer = (NetworkCommunicator)arguments[0];
        string? reason = null;
        if (arguments.Length > 1 && arguments[1] != null)
        {
            reason = (string)arguments[1];
        }

        crpgChat.ServerSendMessageToPlayer(fromPeer, ColorFatal, $"You have kicked {targetPeer.UserName}.");
        crpgChat.ServerSendMessageToPlayer(targetPeer, ColorFatal, $"You were kicked by {targetPeer.UserName}.{(reason != null ? $" Reason: {reason}" : string.Empty)}");
        crpgChat.ServerSendServerMessageToEveryone(ColorFatal, $"{targetPeer.UserName} was kicked by {fromPeer.UserName}.{(reason != null ? $" Reason: {reason}" : string.Empty)}");
        await Task.Delay(2500);
        if (!targetPeer.IsConnectionActive)
        {
            return;
        }

        var disconnectInfo = fromPeer.PlayerConnectionInfo.GetParameter<DisconnectInfo>("DisconnectInfo") ?? new DisconnectInfo();
        disconnectInfo.Type = DisconnectType.KickedByHost;
        targetPeer.PlayerConnectionInfo.AddParameter("DisconnectInfo", disconnectInfo);
        GameNetwork.AddNetworkPeerToDisconnectAsServer(targetPeer);
    }
}
