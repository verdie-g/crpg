using Crpg.Module.Api.Models.Users;
using Crpg.Module.Helpers;
using TaleWorlds.MountAndBlade;

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

    public float Weight(float p = MatchBalancer.PowerParameter)
    {
        return MathHelper.PowerSumBy(Members, u => u.Weight, p) * (1 + Size / 50f);
    }

    public float WeightPMean(float p = MatchBalancer.PowerParameter)
    {
        return Weight() / Size;
    }
}
