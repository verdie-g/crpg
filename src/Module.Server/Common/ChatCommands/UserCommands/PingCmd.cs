using Crpg.Module.Common.GameHandler;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands.UserCommands;
internal class PingCmd : ChatCommand
{
    public PingCmd()
        : base()
    {
        Command = "ping";
        Pattern = new string[] { string.Empty }.ToList();
    }

    protected override void ExecuteSuccess(NetworkCommunicator fromPeer, string cmd, List<dynamic> parameters)
    {
        CrpgChatBox crpgChat = GetChat();
        Console.WriteLine("Pong!");
        crpgChat.ServerSendMessageToPlayer(fromPeer, new TaleWorlds.Library.Color(1, 0, 0), "Pong!");
    }
}
