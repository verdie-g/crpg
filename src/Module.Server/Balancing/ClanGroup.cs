using Crpg.Module.Common;
using Crpg.Module.Helpers;

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
        float clanGroupSizePenalty = CrpgServerConfiguration.ClanGroupSizePenalty;
        return MathHelper.PowerSumBy(Members, u => u.Weight, p) * (1 + Size * clanGroupSizePenalty);
    }

    public float WeightMean()
    {
        return Weight() / Size;
    }
}
