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
    public List<WeightedCrpgUser> Members { get; } = new();
    public int Size => Members.Count;

    public void Add(WeightedCrpgUser user)
    {
        Members.Add(user);
    }

    public float WeightPsum(float p = MatchBalancer.PowerParameter)
    {
        return WeightHelpers.ComputeTeamWeightPowerSum(Members, p);
    }

    public float WeightPMean(float p = MatchBalancer.PowerParameter)
    {
        return WeightHelpers.ComputeTeamWeightPowerMean(Members, p);
    }
}
