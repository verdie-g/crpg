using Crpg.Module.Common.Network;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;

namespace Crpg.Module.Common.ChatCommands.Admin;

internal class KickCommand : AdminCommand
{
    public KickCommand(ChatCommandsComponent chatComponent)
        : base(chatComponent)
    {
        Name = "kick";
        Description = $"'{ChatCommandsComponent.CommandPrefix}{Name} PLAYERID [REASON]' to kick a player.";
        Overloads = new CommandOverload[]
        {
            new(new[] { ChatCommandParameterType.PlayerId, ChatCommandParameterType.String }, ExecuteKickByNetworkPeer),
            new(new[] { ChatCommandParameterType.PlayerId }, ExecuteKickByNetworkPeer),
            new(new[] { ChatCommandParameterType.String, ChatCommandParameterType.String }, ExecuteKickByName),
            new(new[] { ChatCommandParameterType.String }, ExecuteKickByName),
        };
    }

    private void ExecuteKickByName(NetworkCommunicator fromPeer, object[] arguments)
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
        ExecuteKickByNetworkPeer(fromPeer, arguments);
    }

    private async void ExecuteKickByNetworkPeer(NetworkCommunicator fromPeer, object[] arguments)
    {
        var targetPeer = (NetworkCommunicator)arguments[0];
        string? reason = null;
        if (arguments.Length > 1 && arguments[1] != null)
        {
            reason = (string)arguments[1];
        }

        ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorFatal, $"You have kicked {targetPeer.UserName}.");
        ChatComponent.ServerSendServerMessageToEveryone(ColorFatal, $"{targetPeer.UserName} was kicked by {fromPeer.UserName}.{(reason != null ? $" Reason: {reason}" : string.Empty)}");

        GameNetwork.BeginModuleEventAsServer(targetPeer);
        GameNetwork.WriteMessage(new CrpgNotification
        {
            Type = CrpgNotification.NotificationType.Announcement,
            Message = $"Kicked by {targetPeer.UserName}.{(reason != null ? $" Reason: {reason}" : string.Empty)}",
        });
        GameNetwork.EndModuleEventAsServer();

        await Task.Delay(250);
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
