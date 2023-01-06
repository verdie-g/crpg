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
    public List<CrpgUser> Members { get; } = new();
    public int Size => Members.Count;

    public void Add(CrpgUser user)
    {
        Members.Add(user);
    }

    public float RatingPsum(float p = MatchBalancer.PowerParameter)
    {
        return RatingHelpers.ComputeTeamRatingPowerSum(Members, p);
    }

    public float RatingPMean(float p = MatchBalancer.PowerParameter)
    {
        return RatingHelpers.ComputeTeamRatingPowerMean(Members, p);
    }
}
