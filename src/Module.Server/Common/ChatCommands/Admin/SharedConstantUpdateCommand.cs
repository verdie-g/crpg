using Crpg.Module.Common.Network;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands.Admin;

/// <summary>Command to update a <see cref="SharedConstant"/>.</summary>
internal class SharedConstantUpdateCommand : AdminCommand
{
    public SharedConstantUpdateCommand(ChatCommandsComponent chatComponent)
        : base(chatComponent)
    {
        Name = "const";
        Description = $"'{ChatCommandsComponent.CommandPrefix}{Name} id value' to change a constant.";
        Overloads = new CommandOverload[]
        {
            new(new[] { ChatCommandParameterType.Int32, ChatCommandParameterType.Float32 }, Execute),
        };
    }

    private void Execute(NetworkCommunicator fromPeer, object[] arguments)
    {
        int sharedConstantId = (int)arguments[0];
        float newValue = (float)arguments[1];

        if (!SharedConstant.TryUpdate(sharedConstantId, newValue, out float oldValue))
        {
            ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorWarning, $"Not constant was found with id '{sharedConstantId}'.");
            return;
        }

        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new UpdateSharedConstant
        {
            Id = sharedConstantId,
            OldValue = oldValue,
            NewValue = newValue,
        });
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
    }
}
