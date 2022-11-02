﻿using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

/// <summary>
/// Used to synchronize user data between client / server.
/// </summary>
internal class CrpgUserManagerClient : MissionNetwork
{
    private MissionNetworkComponent? _missionNetworkComponent;
    private CrpgPeer? _crpgPeer;

    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
        _missionNetworkComponent = Mission.GetMissionBehavior<MissionNetworkComponent>();
        _missionNetworkComponent.OnMyClientSynchronized += OnMyClientSynchronized;
        _crpgPeer?.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
    }

    /*
    public override void OnPlayerConnectedToServer(NetworkCommunicator networkPeer)
    {
        if (networkPeer == null && networkPeer != GameNetwork.MyPeer)
        {
            return;
        }

        CrpgPeer crpgPeer = networkPeer.GetComponent<CrpgPeer>();
        if (crpgPeer == null)
        {
            InformationManager.DisplayMessage(new InformationMessage("CrpgPeer added for " + networkPeer.UserName));
            networkPeer.AddComponent<CrpgPeer>();
        }
    }

    public override void OnPlayerDisconnectedFromServer(NetworkCommunicator networkPeer)
    {
        if (networkPeer == null && networkPeer != GameNetwork.MyPeer)
        {
            return;
        }

        CrpgPeer crpgPeer = networkPeer.GetComponent<CrpgPeer>();
        if (crpgPeer != null)
        {
            InformationManager.DisplayMessage(new InformationMessage("CrpgPeer removed for " + networkPeer.UserName));
            networkPeer.RemoveComponent<CrpgPeer>();
        }
    }
    */

    public override void OnRemoveBehavior()
    {
        base.OnRemoveBehavior();
        _missionNetworkComponent!.OnMyClientSynchronized -= OnMyClientSynchronized;
    }

    private void OnMyClientSynchronized()
    {
        _crpgPeer = GameNetwork.MyPeer.GetComponent<CrpgPeer>();
        _crpgPeer.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
    }
}
