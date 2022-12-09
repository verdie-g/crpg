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
    public List<CrpgUser> MemberList { get; } = new();
    public ClanGroup(int? clanId)
    {
        ClanId = clanId;
    }

    internal int Size()
    {
        return MemberList.Count();
    }

    internal void Add(CrpgUser user)
    {
        MemberList.Add(user);
    }

    internal float RatingPsum(float p = MatchBalancingSystem.PowerParameter)
    {
        return RatingHelpers.ComputeTeamRatingPowerSum(MemberList, p);
    }

    internal float RatingPMean(float p = MatchBalancingSystem.PowerParameter)
    {
        return RatingHelpers.ComputeTeamRatingPowerMean(MemberList, p);
    }
}
