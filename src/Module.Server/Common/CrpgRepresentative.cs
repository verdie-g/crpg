using Crpg.Module.Api.Models.Users;
using Crpg.Module.Common.Network;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

internal class CrpgRepresentative : MissionRepresentativeBase
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
