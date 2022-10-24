using Crpg.Module.Common.ChatCommands.Admin;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands.User;

internal class SuicideCommand : AdminCommand
{
    public SuicideCommand(ChatCommandsComponent chatComponent)
        : base(chatComponent)
    {
        Name = "suicide";
        Description = $"'{ChatCommandsComponent.CommandPrefix}{Name}' to kill yourself.";
        Overloads = new CommandOverload[]
        {
            new(Array.Empty<ChatCommandParameterType>(), Execute),
        };
    }

    private void Execute(NetworkCommunicator fromPeer, object[] arguments)
    {
        Agent agent = fromPeer.ControlledAgent;
        if (agent == null || agent.Health <= 0)
        {
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
    }
}
