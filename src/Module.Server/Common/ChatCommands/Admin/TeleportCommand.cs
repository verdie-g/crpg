using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands.Admin;

internal class TeleportCommand : AdminCommand
{
    public TeleportCommand(ChatCommandsComponent chatComponent)
        : base(chatComponent)
    {
        Name = "tp";
        Description = $"'{ChatCommandsComponent.CommandPrefix}{Name} PLAYERID_FROM PLAYER_ID_TO' to teleport ID1 to ID2.";
        Overloads = new CommandOverload[]
        {
            new(new[] { ChatCommandParameterType.PlayerId, ChatCommandParameterType.PlayerId }, ExecuteTeleportByNetworkPeer),
            new(new[] { ChatCommandParameterType.String, ChatCommandParameterType.String }, ExecuteTeleportByName),
        };
    }

    private void ExecuteTeleportByName(NetworkCommunicator fromPeer, object[] arguments)
    {
        string targetName1 = (string)arguments[0];
        string targetName2 = (string)arguments[1];
        if (!TryGetPlayerByName(fromPeer, targetName1, out var targetPeer1)
            || !TryGetPlayerByName(fromPeer, targetName2, out var targetPeer2))
        {
            return;
        }

        arguments = new object[] { targetPeer1!, targetPeer2! };
        ExecuteTeleportByNetworkPeer(fromPeer, arguments);
    }

    private void ExecuteTeleportByNetworkPeer(NetworkCommunicator fromPeer, object[] arguments)
    {
        var targetPeer1 = (NetworkCommunicator)arguments[0];
        var targetPeer2 = (NetworkCommunicator)arguments[1];

        Agent agent1 = targetPeer1.ControlledAgent;
        Agent agent2 = targetPeer2.ControlledAgent;
        if (agent1 == null || agent1.Health <= 0)
        {
            ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorWarning, targetPeer1.UserName + " is not alive.");
            return;
        }

        if (agent2 == null || agent2.Health <= 0)
        {
            ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorWarning, targetPeer2.UserName + " is not alive.");
            return;
        }

        if (agent1.MountAgent != null)
        {
            agent1.MountAgent.TeleportToPosition(agent2.Position);
        }
        else
        {
            agent1.TeleportToPosition(agent2.Position);
        }

        ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorSuccess, $"You teleported {targetPeer1.UserName} to {targetPeer2.UserName}.");
        ChatComponent.ServerSendMessageToPlayer(targetPeer2, ColorWarning, $"{fromPeer.UserName} teleported {targetPeer1.UserName} to you.");
        ChatComponent.ServerSendMessageToPlayer(targetPeer1, ColorWarning, $"You were teleported to {targetPeer2.UserName} by {fromPeer.UserName}.");
    }
}
