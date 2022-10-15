using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands.User;

internal class PingCommand : ChatCommand
{
    public PingCommand(ChatCommandsComponent chatComponent)
        : base(chatComponent)
    {
        Name = "ping";
        Overloads = new CommandOverload[]
        {
            new(Array.Empty<ChatCommandParameterType>(), ExecuteSuccess),
        };
    }

    private void ExecuteSuccess(NetworkCommunicator fromPeer, string cmd, object[] arguments)
    {
        ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorSuccess, "Pong!");
    }
}
