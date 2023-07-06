using Crpg.Module.Common.Network;
using Crpg.Module.Modes.Conquest;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Modes.Conquest;

internal class CrpgConquestOpenGateClient : MissionNetwork
{
    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        registerer.Register<CrpgConquestOpenGateMessage>(HandleBroadCastOpenGateUser);
    }

    private void HandleBroadCastOpenGateUser(CrpgConquestOpenGateMessage message)
    {
        if (!message.CrpgUserName.Equals(string.Empty))
        {
            InformationManager.DisplayMessage(new InformationMessage($"Defender {message.CrpgUserName} has opened the gate.", new Color(0.74f, 0.28f, 0.01f)));
        }
    }
}
