using System.Globalization;
using System.Text;
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
    private readonly CrpgConstants _constants;
    private readonly Dictionary<int, Task<CrpgResult<CrpgClan>>> _clanTasks;

    public CrpgUserManagerServer(ICrpgClient crpgClient, CrpgConstants constants)
    {
        _crpgClient = crpgClient;
        _constants = constants;
        _clanTasks = new Dictionary<int, Task<CrpgResult<CrpgClan>>>();
    }

    public override void OnPlayerDisconnectedFromServer(NetworkCommunicator networkPeer)
    {
        RewardMultiplierByPlayerId.Remove(networkPeer.VirtualPlayer.Id);
    }

    protected override void HandleEarlyNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
    {
        base.HandleEarlyNewClientAfterLoadingFinished(networkPeer);
        networkPeer.AddComponent<CrpgPeer>();
        SendExistingCrpgPeers(networkPeer); // Add CrpgPeer component for all other players for new client.
    }

    protected override void HandleNewClientAfterSynchronized(NetworkCommunicator networkPeer)
    {
        base.HandleNewClientAfterSynchronized(networkPeer);
        if (KickEmptyNames(networkPeer) || KickWeirdBodyProperties(networkPeer))
        {
            return;
        }

        _ = SetCrpgComponentAsync(networkPeer);
    }

    protected override void OnEndMission()
    {
        RewardMultiplierByPlayerId.Clear();
        foreach (var networkPeer in GameNetwork.NetworkPeers)
        {
            var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
            if (crpgPeer == null)
            {
                continue;
            }

            RewardMultiplierByPlayerId[networkPeer.VirtualPlayer.Id] = crpgPeer.RewardMultiplier;
        }
    }

    private bool KickWeirdBodyProperties(NetworkCommunicator networkPeer)
    {
        var vp = networkPeer.VirtualPlayer;
        var bodyProperties = vp.BodyProperties;
        ulong height = (bodyProperties.KeyPart8 >> 19) & 0x3F;
        if (height >= 15 && height <= 47) // Min/max height of the armory.
        {
            return false;
        }

        Debug.Print($"Kick player {vp.UserName} with a height of {height}");
        KickHelper.Kick(networkPeer, DisconnectType.KickedByHost, "bad_player_height");
        return true;
    }

    private bool KickEmptyNames(NetworkCommunicator networkPeer)
    {
        var vp = networkPeer.VirtualPlayer;
        if (!string.IsNullOrWhiteSpace(vp.UserName))
        {
            return false;
        }

        Debug.Print($"Kick player with an empty name \"{vp.UserName}\"");
        KickHelper.Kick(networkPeer, DisconnectType.KickedByHost, "empty_name");
        return true;
    }

    /// <summary>
    /// Used to synchronize existing CrpgPeers to the new client.
    /// </summary>
    private void SendExistingCrpgPeers(NetworkCommunicator newPlayerNetworkPeer)
    {
        foreach (NetworkCommunicator networkPeers in GameNetwork.NetworkPeers)
        {
            CrpgPeer crpgPeer = networkPeers.GetComponent<CrpgPeer>();
            if (!networkPeers.IsConnectionActive
                || !networkPeers.IsSynchronized
                || crpgPeer?.User == null
                || newPlayerNetworkPeer == networkPeers)
            {
                continue;
            }

            // Update all CrpgPeers to current values.
            crpgPeer.SynchronizeToPlayer(newPlayerNetworkPeer.VirtualPlayer);
        }
    }

    private async Task SetCrpgComponentAsync(NetworkCommunicator networkPeer)
    {
        VirtualPlayer vp = networkPeer.VirtualPlayer;
        if (!Enum.TryParse(vp.Id.ProvidedType.ToString(), out Platform platform)
            || (platform != Platform.Steam && platform != Platform.Epic))
        {
            Debug.Print($"Kick player {vp.UserName} playing on {vp.Id.ProvidedType}");
            KickHelper.Kick(networkPeer, DisconnectType.KickedByHost, "unsupported_platform");
        }

        string platformUserId = PlayerIdToPlatformUserId(vp.Id, platform);
        string userName = vp.UserName;

        CrpgUser crpgUser;
        CrpgClan? crpgClan = null;
        try
        {
            var userRes = CrpgFeatureFlags.IsEnabled(CrpgFeatureFlags.FeatureTournament)
                ? await _crpgClient.GetTournamentUserAsync(platform, platformUserId)
                : await _crpgClient.GetUserAsync(platform, platformUserId);
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
                if (crpgClan != null)
                {
                    crpgClan.BannerKey = SanitizeBannerKey(crpgClan.BannerKey);
                    var missionPeer = vp.GetComponent<MissionPeer>();
                    if (!string.IsNullOrEmpty(crpgClan.BannerKey) && missionPeer != null)
                    {
                        vp.BannerCode = crpgClan.BannerKey;
                        GameNetwork.BeginBroadcastModuleEvent();
                        GameNetwork.WriteMessage(new NetworkMessages.FromServer.CreateBanner(networkPeer, crpgClan.BannerKey));
                        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.ExcludeTargetPlayer, networkPeer);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.Print($"Couldn't get user {userName} ({platform}#{platformUserId}): {e}");
            KickHelper.Kick(networkPeer, DisconnectType.ServerNotResponding, "unreachable_server");
            return;
        }

        if (crpgUser.Restrictions.FirstOrDefault(r => r.Type == CrpgRestrictionType.Join) != null)
        {
            Debug.Print($"Kick join restricted user {userName} ({platform}#{platformUserId})");
            KickHelper.Kick(networkPeer, DisconnectType.BannedByPoll, "banned");
            return;
        }

        if (crpgUser.Restrictions.FirstOrDefault(r => r.Type == CrpgRestrictionType.Chat) != null)
        {
            networkPeer.IsMuted = true;
        }

        var crpgPeer = networkPeer.GetComponent<CrpgPeer>();
        crpgPeer.User = crpgUser;
        crpgPeer.Clan = crpgClan;
        crpgPeer.RewardMultiplier =
            RewardMultiplierByPlayerId.TryGetValue(vp.Id, out int lastMissionMultiplier)
                ? lastMissionMultiplier
                : 1;
    }

    private string PlayerIdToPlatformUserId(PlayerId playerId, Platform platform)
    {
        switch (platform)
        {
            case Platform.Steam:
                return playerId.Id2.ToString(CultureInfo.InvariantCulture);
            case Platform.Epic:
                byte[] guidBytes = new ArraySegment<byte>(playerId.ToByteArray(), offset: 16, count: 16).ToArray();
                return new Guid(guidBytes).ToString("N");
            case Platform.Gog:
            default:
                throw new ArgumentOutOfRangeException(nameof(platform), platform, null);
        }
    }

    private string SanitizeBannerKey(string? bannerKey)
    {
        if (bannerKey == null)
        {
            return string.Empty;
        }

        if (bannerKey.Length > _constants.ClanBannerKeyMaxLength)
        {
            return string.Empty;
        }

        string[] array = bannerKey.Split('.');

        StringBuilder fixedBannerCode = new();
        // The maximum size of the banner is Banner.BannerFullSize. But apparently negative values do not cause crashes. Anyway added some checks with tolerance to parse the banner.
        const int maxX = 2 * Banner.BannerFullSize;
        const int minX = -2 * Banner.BannerFullSize;
        const int maxY = maxX;
        const int minY = minX;

        /*
         * Format values seperated by dots (.)
         * Icons / Colors found inside of the banner_icons.xml
         * --------
         * iconId
         * colorId
         * colorId2
         * sizeX
         * sizeY
         * posX  (total canvas size is Banner.BannerFullSize but being out of these doesn't seem to cause any issues)
         * posY
         * stroke (0 or 1)
         * mirror (0 or 1)
         * rotation (0-359)
         */
        for (int i = 0; i + 10 <= array.Length; i += 10)
        {
            if (!int.TryParse(array[i], out int iconId))
            {
                return string.Empty;
            }

            if (!CheckBannerIconList(iconId))
            {
                return string.Empty;
            }

            if (!int.TryParse(array[i + 1], out int colorId1)
                || !BannerManager.Instance.ReadOnlyColorPalette.ContainsKey(colorId1)
                || !int.TryParse(array[i + 2], out int colorId2)
                || !BannerManager.Instance.ReadOnlyColorPalette.ContainsKey(colorId2)
                || !int.TryParse(array[i + 3], out int sizeX)
                || !int.TryParse(array[i + 4], out int sizeY)
                || !int.TryParse(array[i + 5], out int posX)
                || posX > maxX
                || posX < minX
                || !int.TryParse(array[i + 6], out int posY)
                || posY > maxY
                || posY < minY
                || !int.TryParse(array[i + 7], out int drawStroke)
                || drawStroke > 1
                || drawStroke < 0
                || !int.TryParse(array[i + 8], out int mirror)
                || mirror > 1
                || mirror < 0)
            {
                return string.Empty;
            }

            if (!int.TryParse(array[i + 9], out int rotation))
            {
                return string.Empty;
            }

            rotation %= 360;
            if (rotation < 0)
            {
                rotation += 360;
            }

            fixedBannerCode.Append(iconId);
            fixedBannerCode.Append(".");
            fixedBannerCode.Append(colorId1);
            fixedBannerCode.Append(".");
            fixedBannerCode.Append(colorId2);
            fixedBannerCode.Append(".");
            fixedBannerCode.Append(sizeX);
            fixedBannerCode.Append(".");
            fixedBannerCode.Append(sizeY);
            fixedBannerCode.Append(".");
            fixedBannerCode.Append(posX);
            fixedBannerCode.Append(".");
            fixedBannerCode.Append(posY);
            fixedBannerCode.Append(".");
            fixedBannerCode.Append(drawStroke);
            fixedBannerCode.Append(".");
            fixedBannerCode.Append(mirror);
            fixedBannerCode.Append(".");
            fixedBannerCode.Append(rotation);
            fixedBannerCode.Append(".");
        }

        if (fixedBannerCode.Length == 0)
        {
            return string.Empty;
        }

        fixedBannerCode.Length -= ".".Length;

        return fixedBannerCode.ToString();
    }

    private bool CheckBannerIconList(int id)
    {
        foreach (BannerIconGroup bannerIconGroup in BannerManager.Instance.BannerIconGroups)
        {
            if (bannerIconGroup.AllBackgrounds.ContainsKey(id) || bannerIconGroup.AllIcons.ContainsKey(id))
            {
                return true;
            }
        }

        return false;
    }
}
