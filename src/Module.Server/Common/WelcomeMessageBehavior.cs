using System.Text;
using Crpg.Module.Common.Network;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

internal class WelcomeMessageBehavior : MissionNetwork
{
    private readonly MultiplayerWarmupComponent _warmupComponent;

    public WelcomeMessageBehavior(MultiplayerWarmupComponent warmupComponent)
    {
        _warmupComponent = warmupComponent;
    }

    public override void OnBehaviorInitialize()
    {
        _warmupComponent.OnWarmupEnded += OnWarmupEnded;
    }

    public override void OnRemoveBehavior()
    {
        _warmupComponent.OnWarmupEnded -= OnWarmupEnded;
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        if (GameNetwork.IsServer)
        {
            return;
        }

        registerer.Register<WelcomeMessage>(HandleWelcomeMessage);
    }

    private void OnWarmupEnded()
    {
        if (!GameNetwork.IsServer)
        {
            return;
        }

        var newPeers = new List<NetworkCommunicator>();
        foreach (var networkPeer in GameNetwork.NetworkPeers)
        {
            var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
            if (crpgPeer?.User == null)
            {
                continue;
            }

            if (crpgPeer.User.CreatedAt.AddHours(1) < DateTime.UtcNow)
            {
                continue;
            }

            newPeers.Add(networkPeer);
        }

        if (newPeers.Count == 0)
        {
            return;
        }

        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new WelcomeMessage { Peers = newPeers.ToArray() });
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
    }

    private void HandleWelcomeMessage(WelcomeMessage message)
    {
        if (message.Peers.Length == 0)
        {
            return;
        }

        StringBuilder sb = new("Welcome to ");
        foreach (var peer in message.Peers)
        {
            sb.Append(peer.VirtualPlayer.UserName);
            sb.Append(", ");
        }

        sb.Length -= ", ".Length;
        sb.Append(" that just joined cRPG!");
        InformationManager.DisplayMessage(new InformationMessage(sb.ToString()));
    }
}
