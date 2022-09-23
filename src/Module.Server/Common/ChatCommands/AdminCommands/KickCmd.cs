using Crpg.Module.Common.GameHandler;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;

namespace Crpg.Module.Common.ChatCommands.UserCommands;
internal class KickCmd : AdminCmd
{
    public KickCmd()
        : base()
    {
        Command = "kick";
        Description = $"'{ChatCommandHandler.CommandPrefix}{Command} PLAYERID' to kick a player.";
        PatternList = new Pattern[]
        {
            new Pattern(new ParameterType[] { ParameterType.PlayerId }.ToList(), ExecuteKickByNetworkPeer),
            new Pattern(new ParameterType[] { ParameterType.String }.ToList(), ExecuteKickByName),
        }.ToList();
    }

    protected override void ExecuteFailed(NetworkCommunicator fromPeer)
    {
        CrpgChatBox crpgChat = GetChat();
        crpgChat.ServerSendMessageToPlayer(fromPeer, ChatCommandHandler.ColorInfo, $"Wrong usage. Type {Description}");
    }

    private void ExecuteKickByNetworkPeer(NetworkCommunicator fromPeer, string cmd, List<object> parameters)
    {
        CrpgChatBox crpgChat = GetChat();
        var targetPeer = (NetworkCommunicator)parameters[0];
        crpgChat.ServerSendMessageToPlayer(fromPeer, ChatCommandHandler.ColorFatal, $"You have kicked {targetPeer.UserName}.");
        var disconnectInfo = fromPeer.PlayerConnectionInfo.GetParameter<DisconnectInfo>("DisconnectInfo") ?? new DisconnectInfo();
        disconnectInfo.Type = DisconnectType.KickedByHost;
        targetPeer.PlayerConnectionInfo.AddParameter("DisconnectInfo", disconnectInfo);
        GameNetwork.AddNetworkPeerToDisconnectAsServer(targetPeer);
    }

    private void ExecuteKickByName(NetworkCommunicator fromPeer, string cmd, List<object> parameters)
    {
        string targetName = (string)parameters[0];
        var (success, targetPeer) = GetPlayerByName(fromPeer, targetName);
        if (!success || targetPeer == null)
        {
            return;
        }

        parameters = new List<object> { targetPeer };
        ExecuteKickByNetworkPeer(fromPeer, cmd, parameters);
    }
}
