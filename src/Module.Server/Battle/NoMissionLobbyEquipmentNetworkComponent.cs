using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Battle;

public class NoMissionLobbyEquipmentNetworkComponent : MissionLobbyEquipmentNetworkComponent
{
    public override void OnBehaviorInitialize()
    {
    }

    public override void OnRemoveBehavior()
    {
    }

    public override void OnMissionTick(float dt)
    {
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
    }

    protected override void HandleLateNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
    {
    }
}
