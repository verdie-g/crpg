using Crpg.Module.Api;
using Crpg.Module.Api.Models.Users;
using Crpg.Module.Battle;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;
using Platform = Crpg.Module.Api.Models.Users.Platform;

namespace Crpg.Module.Common;

internal class CrpgUserManager : MissionNetwork
{
    private readonly ICrpgClient _crpgClient;

    public CrpgUserManager(ICrpgClient crpgClient)
    {
        _crpgClient = crpgClient;
    }

    protected override void HandleNewClientAfterSynchronized(NetworkCommunicator networkPeer)
    {
        base.HandleNewClientAfterSynchronized(networkPeer);
        _ = SetCrpgComponentAsync(networkPeer);
    }

    private async Task SetCrpgComponentAsync(NetworkCommunicator networkPeer)
    {
        VirtualPlayer vp = networkPeer.VirtualPlayer;
        if (!Enum.TryParse(vp.Id.ProvidedType.ToString(), out Platform platform) || platform != Platform.Steam)
        {
            Debug.Print($"Kick player {vp.UserName} playing on {vp.Id.ProvidedType}");
            KickPeer(networkPeer, DisconnectType.KickedByHost);
        }

        string platformUserId = vp.Id.Id2.ToString();
        string userName = vp.UserName;

        CrpgUser crpgUser;
        try
        {
            var res = await _crpgClient.GetUserAsync(platform, platformUserId, userName);
            crpgUser = res.Data!;
        }
        catch (Exception e)
        {
            Debug.Print($"Couldn't get user {userName} ({platform}#{platformUserId}): {e}");
            KickPeer(networkPeer, DisconnectType.KickedByHost);
            return;
        }

        if (crpgUser.Ban != null)
        {
            Debug.Print($"Kick banned user {userName} ({platform}#{platformUserId})");
            KickPeer(networkPeer, DisconnectType.BannedByPoll);
            return;
        }

        var crpgPeerComponent = networkPeer.AddComponent<CrpgPeer>();
        crpgPeerComponent.User = crpgUser;
        crpgPeerComponent.RewardMultiplier = 0;
    }

    private void KickPeer(NetworkCommunicator networkPeer, DisconnectType disconnectType)
    {
        const string parameterName = "DisconnectInfo";
        var disconnectInfo = networkPeer.PlayerConnectionInfo.GetParameter<DisconnectInfo>(parameterName) ?? new DisconnectInfo();
        disconnectInfo.Type = disconnectType;
        networkPeer.PlayerConnectionInfo.AddParameter(parameterName, disconnectInfo);
        GameNetwork.AddNetworkPeerToDisconnectAsServer(networkPeer);
    }
}
