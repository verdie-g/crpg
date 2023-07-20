using System.Text;
using Crpg.Module.Common.Network;
using TaleWorlds.Library;
using TaleWorlds.Localization;
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

        StringBuilder names = new();
        foreach (var peer in message.Peers)
        {
            names.Append(peer.VirtualPlayer.UserName);
            names.Append(", ");
        }

        names.Length -= ", ".Length;

        TextObject textObject = new("{=ck7dhCeM}Welcome to {NAMES} who just joined cRPG!",
            new Dictionary<string, object>
            {
                ["NAMES"] = names,
                ["IS_PLURAL"] = message.Peers.Length > 1,
            });
        InformationManager.DisplayMessage(new InformationMessage(textObject.ToString()));
    }
}
