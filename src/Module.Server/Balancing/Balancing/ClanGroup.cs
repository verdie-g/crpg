using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crpg.Module.Api.Models.Users;

namespace Crpg.Module.Balancing;

internal class ClanGroup
{
    internal int Size()
    {
        return members.Count();
    }

    internal int? Clan
    {
        get
        {
            if (members.First().ClanMembership != null)
            {
                return members!.First().ClanMembership!.ClanId;
            }
            else
            {
                return null;
            }
        }
    }

    internal int ClanID
    {
        get
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
    }

    internal void Add(CrpgUser user)
    {
        members.Add(user);
    }

    internal float RatingPsum(float p = MatchBalancingSystem.PowerParameter)
    {
        return RatingHelpers.ComputeTeamRatingPowerSum(members, p);
    }

    internal float RatingPMean(float p = MatchBalancingSystem.PowerParameter)
    {
        return RatingHelpers.ComputeTeamRatingPowerMean(members, p);
    }

    internal List<CrpgUser> MemberList()
    {
        return members;
    }

    private List<CrpgUser> members = new();
}
