using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crpg.BalancingAndRating.Balancing
{
    public class ClanGroup
    {
        public int Size()
        {
            return members.Count();
        }

        public Clan? Clan()
        {
            if (members.First().ClanMembership != null)
            {
                return members!.First().ClanMembership!.Clan;
            }
            else
            {
                return null;
            }
        }

        public int ClanID()
        {
            if (members.First().ClanMembership != null)
            {
                return members!.First().ClanMembership!.ClanId;
            }
            else
            {
                return 0;
            }
        }

        public void Add(User user)
        {
            members.Add(user);
        }

        public int EloPsum(int p)
        {
            var eloSystem = new Rating();
            return eloSystem.ComputeTeamEloPowerSum(members, p);
        }

        public int EloPMean(int p)
        {
            var eloSystem = new Rating();
            return eloSystem.ComputeTeamEloPowerMean(members, p);
        }

        public List<User> MemberList() { return members; }

        private List<User> members = new();
    }
}
