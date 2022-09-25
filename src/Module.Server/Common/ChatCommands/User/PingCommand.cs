using Crpg.Module.Common.GameHandler;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands.User;

internal class PingCommand : ChatCommand
{
    public PingCommand()
    {
        Name = "ping";
        Overloads = new CommandOverload[]
        {
            new(Array.Empty<ChatCommandParameterType>(), ExecuteSuccess),
        };
    }

    private void ExecuteSuccess(NetworkCommunicator fromPeer, string cmd, object[] arguments)
    {
        CrpgChatBox crpgChat = GetChat();
        crpgChat.ServerSendMessageToPlayer(fromPeer, ColorSuccess, "Pong!");
    }
}
