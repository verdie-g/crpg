using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crpg.Module.Balancing
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

        public int RatingPsum(int p)
        {
            return RatingHelpers.ComputeTeamRatingPowerSum(members, p);
        }

        public int RatingPMean(int p=1)
        {
            return RatingHelpers.ComputeTeamRatingPowerMean(members, p);
        }

        public List<User> MemberList() { return members; }

        private List<User> members = new();
    }
    public class ClanGroupCompare : IComparer<ClanGroup>
    {
        // Compares by Rating
        public int Compare(ClanGroup x, ClanGroup y)
        {
            if (x.RatingPMean().CompareTo(y.RatingPMean()) != 0)
            {
                return x.RatingPMean().CompareTo(y.RatingPMean());
            }

            else
            {
                return 0;
            }
        }
    }
}
