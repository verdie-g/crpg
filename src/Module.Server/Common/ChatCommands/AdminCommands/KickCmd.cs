using Crpg.Module.Common.GameHandler;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands.UserCommands;
internal class KickCmd : AdminCmd
{
    public KickCmd()
        : base()
    {
        Command = "kick";
        Pattern = new string[] { "p" }.ToList();
    }

    protected override void ExecuteSuccess(NetworkCommunicator fromPeer, string cmd, List<object> parameters)
    {
        CrpgChatBox crpgChat = GetChat();
        var targetPeer = (NetworkCommunicator)parameters[0];

        crpgChat.ServerSendMessageToPlayer(fromPeer, new TaleWorlds.Library.Color(1, 0, 0), "Kick: "+ targetPeer.UserName);
    }
}
