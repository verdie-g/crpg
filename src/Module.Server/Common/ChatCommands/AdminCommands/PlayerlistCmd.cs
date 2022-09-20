using Crpg.Module.Common.GameHandler;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands.UserCommands;
internal class PlayerlistCmd : AdminCmd
{
    public PlayerlistCmd()
        : base()
    {
        Command = "pl";
        Pattern = new string[] { string.Empty }.ToList();
    }

    protected override void ExecuteSuccess(NetworkCommunicator fromPeer, string cmd, List<object> parameters)
    {
        CrpgChatBox crpgChat = GetChat();
        crpgChat.ServerSendMessageToPlayer(fromPeer, new TaleWorlds.Library.Color(.8f, .8f, 0), "- Playerlist -");
        foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
        {
            if (networkPeer.IsSynchronized)
            {
                crpgChat.ServerSendMessageToPlayer(fromPeer, new TaleWorlds.Library.Color(1, 1, 0), $"{networkPeer.UserName} - {networkPeer.Index}");
            }
        }
    }
}
