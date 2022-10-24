using Crpg.Module.Api.Models.Users;
using Crpg.Module.Battle;
using Crpg.Module.Common.Network;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

/// <summary>
/// Disables team selection. Auto select is done in <see cref="CrpgBattleMissionMultiplayer.HandleLateNewClientAfterSynchronized"/>.
/// </summary>
internal class NoTeamSelectComponent : MultiplayerTeamSelectComponent
{
    public override void OnBehaviorInitialize()
    {
        base.OnBehaviorInitialize();
        typeof(MultiplayerTeamSelectComponent).GetProperty("TeamSelectionEnabled")!.SetValue(this, false);
    }

    public void RequestTeamChange(bool autoAssign = false)
    {
        foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
        {
            networkPeer.GetComponent<MissionPeer>()?.ClearAllVisuals();
        }

        GameNetwork.BeginModuleEventAsClient();
        GameNetwork.WriteMessage(new CrpgTeamChange { AutoAssign = autoAssign });
        GameNetwork.EndModuleEventAsClient();
    }

    protected override void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
    {
        base.AddRemoveMessageHandlers(registerer);
        if (GameNetwork.IsServer)
        {
            registerer.Register<CrpgTeamChange>(HandleClientEventCrpgTeamChange);
        }
    }

    private bool HandleClientEventCrpgTeamChange(NetworkCommunicator peer, CrpgTeamChange message)
    {
        CrpgRepresentative representative = peer.GetComponent<CrpgRepresentative>();
        if (representative == null)
        {
            return true;
        }

        // Admins can also join spectators which is basically the case when it is not AutoAssign.
        if (TeamSelectionEnabled || (representative.User?.Role is CrpgUserRole.Moderator or CrpgUserRole.Admin))
        {
            if (message.AutoAssign)
            {
                AutoAssignTeam(peer);
            }
            else
            {
                ChangeTeamServer(peer, Mission.SpectatorTeam);
            }
        }

        return true;
    }
}
