using Crpg.Module.Common.GameHandler;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands.UserCommands;
internal class PingCmd : ChatCommand
{
    public PingCmd()
        : base()
    {
        Command = "ping";
        PatternList = new Pattern[] { new Pattern(string.Empty, ExecuteSuccess) }.ToList();
    }

    private void ExecuteSuccess(NetworkCommunicator fromPeer, string cmd, List<object> parameters)
    {
        CrpgChatBox crpgChat = GetChat();
        crpgChat.ServerSendMessageToPlayer(fromPeer, CrpgChatBox.ColorSuccess, "Pong!");
    }
}
