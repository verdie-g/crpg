using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crpg.Module.Api.Models.Users;

namespace Crpg.Module.Balancing;

/// <summary>
/// A clanGroup contain either player(s) of the same clan, or a single player that has no clan.
/// </summary>
internal class ClanGroup
{
    public int? ClanId { get; }
    public ClanGroup(int? clanId)
    {
        ClanId = clanId;
    }

    internal int Size()
    {
        return members.Count();
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
