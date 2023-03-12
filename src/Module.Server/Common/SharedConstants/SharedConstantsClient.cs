using Crpg.Module.Common.Network;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

internal class SharedConstantsClient : MissionNetwork
{
    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        registerer.Register<UpdateSharedConstant>(HandleUpdateSharedConstant);
    }

    private void HandleUpdateSharedConstant(UpdateSharedConstant message)
    {
        SharedConstant.TryUpdate(message.Id, message.NewValue, out _);
        InformationManager.DisplayMessage(new InformationMessage($"Changed constant with id '{message.Id}' from {message.OldValue} to {message.NewValue}"));
    }
}
