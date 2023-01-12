using Crpg.Module.Helpers;

namespace Crpg.Module.Balancing;

internal static class WeightHelpers
{
    internal static float ComputeTeamWeightPowerMean(List<WeightedCrpgUser> team, float p = MatchBalancer.PowerParameter)
    {
        List<float> weights = team.Select(u => u.Weight).ToList();
        return MathHelper.PowerMean(weights, p);
    }

    internal static float ClanGroupsPowerSum(List<ClanGroup> clanGroups)
    {
        return ComputeTeamWeightPowerSum(MatchBalancingHelpers.JoinClanGroupsIntoUsers(clanGroups));
    }

    internal static float ComputeTeamWeightedDifference(GameMatch gameMatch)
    {
        return ComputeTeamWeightPowerSum(gameMatch.TeamA, MatchBalancer.PowerParameter) - ComputeTeamWeightPowerSum(gameMatch.TeamB, MatchBalancer.PowerParameter);
    }

    internal static float ComputeTeamWeightPowerSum(List<WeightedCrpgUser> team, float p = MatchBalancer.PowerParameter)
    {
        return MathHelper.PowerSumBy(team, u => u.Weight, p);
    }
}
