using Crpg.Module.Common;
using Crpg.Module.Common.Network;
using Crpg.Module.Duel.Network;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.MissionRepresentatives;

namespace Crpg.Module.Duel;

internal class CrpgDuelMissionMultiplayerClient : MissionMultiplayerGameModeDuelClient
{
    public override bool IsGameModeUsingAllowCultureChange => false;
    public override bool IsGameModeUsingAllowTroopChange => false;
    private readonly List<CrpgPeer> registeredCrpgPeerEventListener;

    public CrpgDuelMissionMultiplayerClient()
        : base()
    {
        registeredCrpgPeerEventListener = new();
    }

    public override bool CanRequestCultureChange()
    {
        return false;
    }

    public override bool CanRequestTroopChange()
    {
        return false;
    }

    public override void OnRemoveBehavior()
    {
        base.OnRemoveBehavior();
        foreach (CrpgPeer crpgPeer in registeredCrpgPeerEventListener)
        {
            if (crpgPeer == null)
            {
                continue;
            }

            crpgPeer.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
        }

        registeredCrpgPeerEventListener.Clear();
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        base.AddRemoveMessageHandlers(registerer);
        if (GameNetwork.IsClientOrReplay)
        {
            registerer.Register<CrpgUpdateDuelArenaType>(HandleCrpgDuelArenaType);
            registerer.Register<CrpgAddPeerComponent>(HandleCrpgAddPeerComponent);
        }
    }

    private void HandleCrpgAddPeerComponent(CrpgAddPeerComponent message)
    {
        NetworkCommunicator peer = message.Peer;
        uint componentId = message.ComponentId;
        if (peer.GetComponent(componentId) == null)
        {
            peer.AddComponent(componentId);
            OnPeerComponentAdded(peer, componentId);
        }
    }

    private void OnPeerComponentAdded(NetworkCommunicator networkPeer, uint componentId)
    {
        var newComponent = networkPeer.GetComponent(componentId);
        if (newComponent is CrpgPeer crpgPeer)
        {
            crpgPeer.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
            registeredCrpgPeerEventListener.Add(crpgPeer);
        }
    }

    private void HandleCrpgDuelArenaType(CrpgUpdateDuelArenaType message)
    {
        if (GameNetwork.MyPeer == null)
        {
            return;
        }

        MissionPeer myMissionPeer = GameNetwork.MyPeer.GetComponent<MissionPeer>();
        if (myMissionPeer == null)
        {
            return;
        }

        Action<TroopType> onMyPreferredZoneChanged = ((DuelMissionRepresentative)myMissionPeer.Representative).OnMyPreferredZoneChanged;
        onMyPreferredZoneChanged?.Invoke(message.PlayerTroopType);
    }
}
