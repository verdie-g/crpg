using Crpg.Module.Api.Models.Clans;
using Crpg.Module.Api.Models.Users;
using Crpg.Module.Common.Network;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

internal class CrpgRepresentative : PeerComponent
{
    private CrpgUser? _user;
    private int _rewardMultiplier;

    public CrpgUser? User
    {
        get => _user;
        set
        {
            _user = value ?? throw new ArgumentNullException();
            if (GameNetwork.IsServerOrRecorder) // Synchronize the property with the client.
            {
                GameNetwork.BeginModuleEventAsServer(Peer);
                GameNetwork.WriteMessage(new UpdateCrpgUser { User = _user });
                GameNetwork.EndModuleEventAsServer();
            }
        }
    }

    public CrpgClan? Clan { get; set; }

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

    /// <summary>The team the user has spawn in. Used to give the correct reward multiplier even after changing team.</summary>
    public Team? SpawnTeamThisRound { get; set; }

    public void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
    {
        if (GameNetwork.IsClientOrReplay)
        {
            GameNetwork.NetworkMessageHandlerRegisterer registerer = new(mode);
            registerer.Register<UpdateCrpgUser>(HandleUpdateCrpgUser);
            registerer.Register<UpdateRewardMultiplier>(HandleUpdateRewardMultiplier);
        }
    }

    private void HandleUpdateCrpgUser(UpdateCrpgUser message)
    {
        User = message.User;
    }

    private void HandleUpdateRewardMultiplier(UpdateRewardMultiplier message)
    {
        RewardMultiplier = message.RewardMultiplier;
    }
}
