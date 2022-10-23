using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands.Admin;

internal class PlayerListCommand : AdminCommand
{
    public PlayerListCommand(ChatCommandsComponent chatComponent)
        : base(chatComponent)
    {
        Name = "pl";
        Overloads = new CommandOverload[]
        {
            new(Array.Empty<ChatCommandParameterType>(), ExecuteSuccess),
        };
    }

    private void ExecuteSuccess(NetworkCommunicator fromPeer, object[] arguments)
    {
        ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorInfo, "- Players -");
        foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
        {
            var crpgRepresentative = networkPeer.GetComponent<CrpgRepresentative>();
            if (networkPeer.IsSynchronized && crpgRepresentative.User != null)
            {
                ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorWarning, $"{crpgRepresentative.User.Id} | '{networkPeer.UserName}'");
            }
        }
    }
}
