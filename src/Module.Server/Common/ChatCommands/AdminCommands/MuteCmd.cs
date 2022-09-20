using Crpg.Module.Common.GameHandler;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;

namespace Crpg.Module.Common.ChatCommands.UserCommands;
internal class MuteCmd : AdminCmd
{
    public MuteCmd()
        : base()
    {
        Command = "mute";
        Description = $"'{ChatCommandHandler.CommandPrefix}{Command} PLAYERID' to kick a player.";
        PatternList = new Pattern[] { new Pattern("p", ExecuteMuteByNetworkPeer), new Pattern("s", ExecuteMuteByName) }.ToList();
    }

    protected override void ExecuteFailed(NetworkCommunicator fromPeer)
    {
        CrpgChatBox crpgChat = GetChat();
        crpgChat.ServerSendMessageToPlayer(fromPeer, ChatCommandHandler.ColorInfo, $"Wrong usage. Type {Description}");
    }

    private void ExecuteMuteByNetworkPeer(NetworkCommunicator fromPeer, string cmd, List<object> parameters)
    {
        CrpgChatBox crpgChat = GetChat();
        var targetPeer = (NetworkCommunicator)parameters[0];
        crpgChat.ServerSendMessageToPlayer(fromPeer, ChatCommandHandler.ColorFatal, $"You {(targetPeer.IsMuted ? "unmuted" : "muted")}: {targetPeer.UserName}");
        targetPeer.IsMuted = !targetPeer.IsMuted;
        // Call mute for backend
    }

    private void ExecuteMuteByName(NetworkCommunicator fromPeer, string cmd, List<object> parameters)
    {
        string targetName = (string)parameters[0];
        var (success, targetPeer) = GetPlayerByName(fromPeer, targetName);
        if (!success || targetPeer == null)
        {
            return;
        }

        parameters = new List<object> { targetPeer };
        ExecuteMuteByNetworkPeer(fromPeer, cmd, parameters);
    }
}
