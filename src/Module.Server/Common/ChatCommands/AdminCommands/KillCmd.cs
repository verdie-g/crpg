using Crpg.Module.Common.GameHandler;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands.UserCommands;
internal class KillCmd : AdminCmd
{
    public KillCmd()
        : base()
    {
        Command = "kill";
        Description = $"'{ChatCommandHandler.CommandPrefix}{Command} PLAYERID' to kill a player.";
        PatternList = new Pattern[]
        {
            new Pattern(new ParameterType[] { ParameterType.PlayerId }.ToList(), ExecuteKillByNetworkPeer),
            new Pattern(new ParameterType[] { ParameterType.String }.ToList(), ExecuteKillByName),
        }.ToList();
    }

    protected override void ExecuteFailed(NetworkCommunicator fromPeer)
    {
        CrpgChatBox crpgChat = GetChat();
        crpgChat.ServerSendMessageToPlayer(fromPeer, ChatCommandHandler.ColorInfo, $"Wrong usage. Type {Description}");
    }

    private void ExecuteKillByNetworkPeer(NetworkCommunicator fromPeer, string cmd, List<object> parameters)
    {
        CrpgChatBox crpgChat = GetChat();
        var targetPeer = (NetworkCommunicator)parameters[0];

        Agent agent = targetPeer.ControlledAgent;
        if (agent == null || agent.Health <= 0)
        {
            crpgChat.ServerSendMessageToPlayer(fromPeer, ChatCommandHandler.ColorWarning, $"{targetPeer.UserName} is not alive.");
            return;
        }

        Blow b = new(agent.Index);
        b.DamageType = DamageTypes.Invalid;
        b.BaseMagnitude = 10000f;
        b.Position = agent.Position;
        b.DamagedPercentage = 1f;
        agent.Die(b, Agent.KillInfo.Gravity);
        crpgChat.ServerSendMessageToPlayer(fromPeer, ChatCommandHandler.ColorSuccess, $"You have killed {targetPeer.UserName}.");
        crpgChat.ServerSendMessageToPlayer(targetPeer, ChatCommandHandler.ColorFatal, $"You were killed by {fromPeer.UserName}.");
    }

    private void ExecuteKillByName(NetworkCommunicator fromPeer, string cmd, List<object> parameters)
    {
        string targetName = (string)parameters[0];
        var (success, targetPeer) = GetPlayerByName(fromPeer, targetName);
        if (!success || targetPeer == null)
        {
            return;
        }

        parameters = new List<object> { targetPeer };
        ExecuteKillByNetworkPeer(fromPeer, cmd, parameters);
    }
}
