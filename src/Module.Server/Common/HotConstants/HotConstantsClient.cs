using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common.HotConstants;

internal class HotConstantsClient : MissionNetwork
{
    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        registerer.Register<UpdateHotConstant>(HandleUpdateHotConstant);
    }

    private void HandleUpdateHotConstant(UpdateHotConstant message)
    {
        HotConstant.TryUpdate(message.Id, message.NewValue, out _);
        InformationManager.DisplayMessage(new InformationMessage($"Changed constant with id '{message.Id}' from {message.OldValue} to {message.NewValue}"));
    }
}
