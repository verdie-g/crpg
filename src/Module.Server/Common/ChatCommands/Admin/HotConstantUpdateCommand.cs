using Crpg.Module.Common.HotConstants;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.ChatCommands.Admin;

/// <summary>Command to update a <see cref="HotConstant"/>.</summary>
internal class HotConstantUpdateCommand : AdminCommand
{
    public HotConstantUpdateCommand(ChatCommandsComponent chatComponent)
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
        int hotConstantId = (int)arguments[0];
        float newValue = (float)arguments[1];

        if (!HotConstant.TryUpdate(hotConstantId, newValue, out float oldValue))
        {
            ChatComponent.ServerSendMessageToPlayer(fromPeer, ColorWarning, $"Not constant was found with id '{hotConstantId}'.");
            return;
        }

        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new UpdateHotConstant
        {
            Id = hotConstantId,
            OldValue = oldValue,
            NewValue = newValue,
        });
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
    }
}
