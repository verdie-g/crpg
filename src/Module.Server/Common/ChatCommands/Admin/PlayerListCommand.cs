using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands.Admin;

internal class PlayerListCommand : AdminCommand
{
    public PlayerListCommand(ChatCommandsComponent chatComponent)
        : base(chatComponent)
    {
        Name = "pl";
        Description = $"'{ChatCommandsComponent.CommandPrefix}{Name} [NAMEFILTER] to list players.";
        Overloads = new CommandOverload[]
        {
            new(new[] { ChatCommandParameterType.String }, Execute),
            new(Array.Empty<ChatCommandParameterType>(), Execute),
        };
    }

    private void Execute(NetworkCommunicator fromPeer, object[] arguments)
    {
        string? filter = arguments.Length > 0 ? ((string)arguments[0]).ToLower() : null;
        ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorInfo, "- Players -");
        foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
        {
            if (filter != null && !networkPeer.UserName.ToLower().Contains(filter))
            {
                continue;
            }

            var crpgPeer = networkPeer.GetComponent<CrpPeer>();
            if (networkPeer.IsSynchronized && crpgPeer.User != null)
            {
                ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorWarning, $"{crpgPeer.User.Id} | '{networkPeer.UserName}'");
            }
        }
    }
}
