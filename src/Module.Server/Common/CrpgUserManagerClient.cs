using Crpg.Module.Common;
using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.MissionRepresentatives;

namespace Crpg.Module.Common;

/// <summary>
/// Used to synchronize user data between client / server.
/// </summary>
internal class CrpgUserManagerClient : MissionNetwork
{
    private readonly List<CrpgPeer> _registeredCrpgPeerEventListener;
    private MissionNetworkComponent? _missionNetworkComponent;
    private CrpgPeer? _crpgPeer;

    public CrpgUserManagerClient()
        : base()
    {
        _registeredCrpgPeerEventListener = new();
    }

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
        _missionNetworkComponent = Mission.GetMissionBehavior<MissionNetworkComponent>();
        _missionNetworkComponent.OnMyClientSynchronized += OnMyClientSynchronized;
        _crpgPeer?.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
    }

    public override void OnRemoveBehavior()
    {
        base.OnRemoveBehavior();
        _missionNetworkComponent!.OnMyClientSynchronized -= OnMyClientSynchronized;

        foreach (CrpgPeer crpgPeer in _registeredCrpgPeerEventListener)
        {
            if (crpgPeer == null)
            {
                continue;
            }

            crpgPeer.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
        }

        _registeredCrpgPeerEventListener.Clear();
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        base.AddRemoveMessageHandlers(registerer);
        if (GameNetwork.IsClientOrReplay)
        {
            registerer.Register<CrpgAddPeerComponent>(HandleCrpgAddPeerComponent);
        }
    }

    private void OnMyClientSynchronized()
    {
        _crpgPeer = GameNetwork.MyPeer.GetComponent<CrpgPeer>();
        _crpgPeer.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
    }

    private void HandleCrpgAddPeerComponent(CrpgAddPeerComponent message)
    {
        NetworkCommunicator peer = message.Peer;
        uint componentId = message.ComponentId;
        if (peer?.GetComponent(componentId) != null)
        {
            peer.AddComponent(componentId);
            OnPeerComponentAdded(peer, componentId);
        }
    }

    private void OnPeerComponentAdded(NetworkCommunicator networkPeer, uint componentId)
    {
        var newComponent = networkPeer.GetComponent(componentId);
        if (newComponent != null && newComponent is CrpgPeer crpgPeer)
        {
            crpgPeer.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
            _registeredCrpgPeerEventListener.Add(crpgPeer);
        }
    }
}
