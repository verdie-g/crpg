using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.HarmonyPatches;

// From https://github.com/HornsGuy/BannerlordServerPatches/blob/master/ServerPatches/Patches/PatchMissionLobbyComponent.cs
internal class SendPeerInformationsToPeerPatch
{
    public static bool Prefix(MissionLobbyComponent __instance, NetworkCommunicator peer)
    {
        foreach (NetworkCommunicator disconnectedPeer in GameNetwork.NetworkPeersIncludingDisconnectedPeers)
        {
            if (disconnectedPeer == null)
            {
                continue;
            }

            bool flag = disconnectedPeer.VirtualPlayer != MBNetwork.VirtualPlayers[disconnectedPeer.VirtualPlayer.Index];
            if (!flag && !disconnectedPeer.IsSynchronized && !disconnectedPeer.JustReconnecting)
            {
                continue;
            }

            if (peer == null)
            {
                continue;
            }

            MissionPeer component = disconnectedPeer.GetComponent<MissionPeer>();
            if (component == null)
            {
                continue;
            }

            GameNetwork.BeginModuleEventAsServer(peer);
            GameNetwork.WriteMessage(new KillDeathCountChange(component.GetNetworkPeer(), null, component.KillCount,
                component.AssistCount, component.DeathCount, component.Score));
            GameNetwork.EndModuleEventAsServer();
            if (component.BotsUnderControlAlive != 0 || component.BotsUnderControlTotal != 0)
            {
                GameNetwork.BeginModuleEventAsServer(peer);
                GameNetwork.WriteMessage(new BotsControlledChange(component.GetNetworkPeer(),
                    component.BotsUnderControlAlive, component.BotsUnderControlTotal));
                GameNetwork.EndModuleEventAsServer();
            }
        }

        return false;
    }
}
