using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands.Admin;

internal class PlayerListCommand : AdminCommand
{
    public PlayerListCommand()
    {
        Name = "pl";
        Overloads = new CommandOverload[]
        {
            new(Array.Empty<ChatCommandParameterType>(), ExecuteSuccess),
        };
    }

    private void ExecuteSuccess(NetworkCommunicator fromPeer, string cmd, object[] arguments)
    {
        PrintPlayerList(fromPeer, GameNetwork.NetworkPeers.ToList());
    }
}
