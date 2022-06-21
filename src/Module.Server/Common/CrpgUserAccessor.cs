using Crpg.Module.Api.Models.Users;
using TaleWorlds.MountAndBlade;

namespace Crpg.Module.Common;

internal class CrpgUserAccessor : MissionBehavior
{
    private CrpgUser _user;

    public CrpgUserAccessor(CrpgUser user) => _user = user;

    public event Action<CrpgUser> OnUserUpdate = _ => { };

    public CrpgUser User
    {
        get => _user;
        set
        {
            _user = value;
            OnUserUpdate(_user);
        }
    }

    public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;
}
