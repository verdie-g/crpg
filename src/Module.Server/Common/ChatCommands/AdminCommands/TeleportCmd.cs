using Crpg.Module.Common.GameHandler;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands.UserCommands;
internal class TeleportCmd : AdminCmd
{
    public TeleportCmd()
        : base()
    {
        Command = "tp";
        Description = $"'{ChatCommandHandler.CommandPrefix}{Command} PLAYERID_FROM PLAYER_ID_TO' to teleport ID1 to ID2.";
        PatternList = new Pattern[] { new Pattern("pp", ExecuteTeleportByNetworkPeer), new Pattern("ss", ExecuteTeleportByName) }.ToList();
    }

    protected override void ExecuteFailed(NetworkCommunicator fromPeer)
    {
        CrpgChatBox crpgChat = GetChat();
        crpgChat.ServerSendMessageToPlayer(fromPeer, ChatCommandHandler.ColorInfo, $"Wrong usage. Type {Description}");
    }

    private void ExecuteTeleportByNetworkPeer(NetworkCommunicator fromPeer, string cmd, List<object> parameters)
    {
        CrpgChatBox crpgChat = GetChat();
        var targetPeer1 = (NetworkCommunicator)parameters[0];
        var targetPeer2 = (NetworkCommunicator)parameters[1];

        Agent agent1 = targetPeer1.ControlledAgent;
        Agent agent2 = targetPeer2.ControlledAgent;
        if (agent1 == null || agent1.Health <= 0)
        {
            crpgChat.ServerSendMessageToPlayer(fromPeer, ChatCommandHandler.ColorWarning, targetPeer1.UserName + " is not alive.");
            return;
        }

        if (agent2 == null || agent2.Health <= 0)
        {
            crpgChat.ServerSendMessageToPlayer(fromPeer, ChatCommandHandler.ColorWarning, targetPeer2.UserName + " is not alive.");
            return;
        }

        if (agent1.MountAgent != null)
        {
            agent1.MountAgent.TeleportToPosition(agent2.Position);
        }
        else
        {
            agent1.TeleportToPosition(agent2.Position);
        }

        crpgChat.ServerSendMessageToPlayer(fromPeer, ChatCommandHandler.ColorSuccess, $"You teleported {targetPeer1.UserName} to {targetPeer2.UserName}.");
        crpgChat.ServerSendMessageToPlayer(targetPeer2, ChatCommandHandler.ColorWarning, $"{fromPeer.UserName} teleported {targetPeer1.UserName} to you.");
        crpgChat.ServerSendMessageToPlayer(targetPeer1, ChatCommandHandler.ColorWarning, $"You were teleported to {targetPeer2.UserName} by {fromPeer.UserName}.");
    }

    private void ExecuteTeleportByName(NetworkCommunicator fromPeer, string cmd, List<object> parameters)
    {
        string targetName1 = (string)parameters[0];
        string targetName2 = (string)parameters[1];
        var (success1, targetPeer1) = GetPlayerByName(fromPeer, targetName1);
        if (!success1 || targetPeer1 == null)
        {
            return;
        }

        var (success2, targetPeer2) = GetPlayerByName(fromPeer, targetName2);
        if (!success2 || targetPeer2 == null)
        {
            return;
        }
        parameters = new List<object> { targetName1, targetName2 };
        ExecuteTeleportByNetworkPeer(fromPeer, cmd, parameters);
    }
}
