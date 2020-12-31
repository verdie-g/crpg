using System;
using Crpg.GameMod.Api.Models.Users;
using TaleWorlds.MountAndBlade;

namespace Crpg.GameMod.Common
{
    internal class CrpgUserAccessor : MissionBehaviour
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

        public override MissionBehaviourType BehaviourType => MissionBehaviourType.Other;
    }
}
