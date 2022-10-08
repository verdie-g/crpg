using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands.Admin;

internal class KillCommand : AdminCommand
{
    public KillCommand(ChatCommandsComponent chatComponent)
        : base(chatComponent)
    {
        Name = "kill";
        Description = $"'{ChatCommandsComponent.CommandPrefix}{Name} PLAYERID' to kill a player.";
        Overloads = new CommandOverload[]
        {
            new(new[] { ChatCommandParameterType.PlayerId }, ExecuteKillByNetworkPeer),
            new(new[] { ChatCommandParameterType.String }, ExecuteKillByName),
        };
    }

    protected override void ExecuteFailed(NetworkCommunicator fromPeer)
    {
        ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorInfo, $"Wrong usage. Type {Description}");
    }

    private void ExecuteKillByName(NetworkCommunicator fromPeer, string cmd, object[] arguments)
    {
        string targetName = (string)arguments[0];
        if (!TryGetPlayerByName(fromPeer, targetName, out var targetPeer))
        {
            return;
        }

        arguments = new object[] { targetPeer! };
        ExecuteKillByNetworkPeer(fromPeer, cmd, arguments);
    }

    private void ExecuteKillByNetworkPeer(NetworkCommunicator fromPeer, string cmd, object[] arguments)
    {
        var targetPeer = (NetworkCommunicator)arguments[0];

        Agent agent = targetPeer.ControlledAgent;
        if (agent == null || agent.Health <= 0)
        {
            ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorWarning, $"{targetPeer.UserName} is not alive.");
            return;
        }

        Blow blow = new(agent.Index)
        {
            DamageType = DamageTypes.Invalid,
            BaseMagnitude = 10000f,
            Position = agent.Position,
            DamagedPercentage = 1f,
        };
        agent.Die(blow, Agent.KillInfo.Gravity);
        ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorSuccess, $"You have killed {targetPeer.UserName}.");
        ChatComponent.ServerSendMessageToPlayer(targetPeer, ColorFatal, $"You were killed by {fromPeer.UserName}.");
    }
}
