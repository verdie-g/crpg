using Crpg.Module.Api.Models.Users;

namespace Crpg.Module.Balancing;

/// <summary>
/// A clanGroup contain either player(s) of the same clan, or a single player that has no clan.
/// </summary>
internal class ClanGroup
{
    public ClanGroup(int? clanId)
    {
        ClanId = clanId;
    }

    public int? ClanId { get; }
    public List<CrpgUser> MemberList { get; } = new();
    public int Size => MemberList.Count;

    public void Add(CrpgUser user)
    {
        MemberList.Add(user);
    }

    public float RatingPsum(float p = MatchBalancingSystem.PowerParameter)
    {
        return RatingHelpers.ComputeTeamRatingPowerSum(MemberList, p);
    }

    public float RatingPMean(float p = MatchBalancingSystem.PowerParameter)
    {
        return RatingHelpers.ComputeTeamRatingPowerMean(MemberList, p);
    }
}
