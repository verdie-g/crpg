using Crpg.Module.Api;
using Crpg.Module.Api.Models;
using Crpg.Module.Api.Models.Clans;
using Crpg.Module.Api.Models.Restrictions;
using Crpg.Module.Api.Models.Users;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.PlayerServices;
using Platform = Crpg.Module.Api.Models.Users.Platform;

namespace Crpg.Module.Common;

internal class CrpgUserManagerServer : MissionNetwork
{
    /// <summary>
    /// Static variable used to persist user info such as reward multipliers between missions. A little hacky to my
    /// taste but as long as it works.
    /// </summary>
    private static readonly Dictionary<PlayerId, int> RewardMultiplierByPlayerId = new();

    private readonly ICrpgClient _crpgClient;
    private readonly Dictionary<int, Task<CrpgResult<CrpgClan>>> _clanTasks;

    public CrpgUserManagerServer(ICrpgClient crpgClient)
    {
        _crpgClient = crpgClient;
        _clanTasks = new Dictionary<int, Task<CrpgResult<CrpgClan>>>();
    }

    public override void OnPlayerDisconnectedFromServer(NetworkCommunicator networkPeer)
    {
        RewardMultiplierByPlayerId.Remove(networkPeer.VirtualPlayer.Id);
    }

    protected override void HandleEarlyNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
    {
        base.HandleEarlyNewClientAfterLoadingFinished(networkPeer);
        networkPeer.AddComponent<CrpgRepresentative>();
    }

    protected override void HandleNewClientAfterSynchronized(NetworkCommunicator networkPeer)
    {
        base.HandleNewClientAfterSynchronized(networkPeer);
        _ = SetCrpgComponentAsync(networkPeer);
    }

    protected override void OnEndMission()
    {
        RewardMultiplierByPlayerId.Clear();
        foreach (var networkPeer in GameNetwork.NetworkPeers)
        {
            var crpgRepresentative = networkPeer.GetComponent<CrpgRepresentative>();
            if (crpgRepresentative == null)
            {
                continue;
            }

            RewardMultiplierByPlayerId[networkPeer.VirtualPlayer.Id] = crpgRepresentative.RewardMultiplier;
        }
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
        CrpgClan? crpgClan = null;
        try
        {
            var userRes = await _crpgClient.GetUserAsync(platform, platformUserId, userName);
            crpgUser = userRes.Data!;

            if (crpgUser.ClanMembership != null)
            {
                int clanId = crpgUser.ClanMembership.ClanId;
                if (!_clanTasks.TryGetValue(clanId, out var clanTask) || clanTask.IsFaulted || clanTask.IsCanceled)
                {
                    clanTask = _crpgClient.GetClanAsync(clanId);
                    _clanTasks[clanId] = clanTask;
                }

                crpgClan = (await clanTask).Data;
            }
        }
        catch (Exception e)
        {
            Debug.Print($"Couldn't get user {userName} ({platform}#{platformUserId}): {e}");
            KickPeer(networkPeer, DisconnectType.KickedByHost);
            return;
        }

        if (crpgUser.Restrictions.FirstOrDefault(r => r.Type == CrpgRestrictionType.Join) != null)
        {
            Debug.Print($"Kick join restricted user {userName} ({platform}#{platformUserId})");
            KickPeer(networkPeer, DisconnectType.BannedByPoll);
            return;
        }

        if (crpgUser.Restrictions.FirstOrDefault(r => r.Type == CrpgRestrictionType.Chat) != null)
        {
            networkPeer.IsMuted = true;
        }

        var crpgRepresentative = networkPeer.GetComponent<CrpgRepresentative>();
        crpgRepresentative.User = crpgUser;
        crpgRepresentative.Clan = crpgClan;
        crpgRepresentative.RewardMultiplier =
            RewardMultiplierByPlayerId.TryGetValue(vp.Id, out int lastMissionMultiplier)
                ? lastMissionMultiplier
                : 1;
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
