using Crpg.Module.Api.Models.Users;
using Crpg.Module.Common.Network;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

internal class CrpgRepresentative : MissionRepresentativeBase
{
    private CrpgUser? _user;

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

    public int RewardMultiplier { get; set; }

    public void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
    {
        if (GameNetwork.IsClientOrReplay)
        {
            GameNetwork.NetworkMessageHandlerRegisterer registerer = new(mode);
            registerer.Register<UpdateCrpgUser>(HandleUpdateCrpgUser);
        }
    }

    private void HandleUpdateCrpgUser(UpdateCrpgUser message)
    {
    }
}
