using Crpg.Module.Api.Models.Clans;
using Crpg.Module.Api.Models.Users;
using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

internal class CrpgPeer : PeerComponent
{
    private CrpgUser? _user;
    private int _rewardMultiplier;

    public CrpgClan? Clan { get; set; }

    /// <summary>The team the user has spawn in. Used to give the correct reward multiplier even after changing team.</summary>
    public Team? SpawnTeamThisRound { get; set; }

    public CrpgUser? User
    {
        get => _user;
        set
        {
            _user = value ?? throw new ArgumentNullException();
            SynchronizeToEveryone(); // Synchronize the property with the client.
        }
    }

    public void SynchronizeToPlayer(VirtualPlayer targetPeer)
    {
        if (_user == null || !GameNetwork.IsServerOrRecorder)
        {
            return;
        }

        GameNetwork.BeginModuleEventAsServer(targetPeer);
        GameNetwork.WriteMessage(new UpdateCrpgUser { Peer = Peer, User = _user });
        GameNetwork.EndModuleEventAsServer();
    }

    public void SynchronizeToEveryone()
    {
        if (_user == null || !GameNetwork.IsServerOrRecorder)
        {
            return;
        }

        GameNetwork.BeginBroadcastModuleEvent();
        GameNetwork.WriteMessage(new UpdateCrpgUser { Peer = Peer, User = _user });
        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
    }

    public int RewardMultiplier
    {
        get => _rewardMultiplier;
        set
        {
            _rewardMultiplier = value;
            if (GameNetwork.IsServerOrRecorder)
            {
                GameNetwork.BeginModuleEventAsServer(Peer);
                GameNetwork.WriteMessage(new UpdateRewardMultiplier { RewardMultiplier = _rewardMultiplier });
                GameNetwork.EndModuleEventAsServer();
            }
        }
    }
}
