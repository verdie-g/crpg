using Crpg.Module.Helpers;

namespace Crpg.Module.Balancing;

internal static class WeightHelpers
{
    internal static float ClanGroupsWeightSum(List<ClanGroup> clanGroups)
    {
        return clanGroups.Sum(c => c.Weight());
    }

    internal static float ComputeTeamWeightedDifference(GameMatch gameMatch)
    {
        return ComputeTeamWeight(gameMatch.TeamA, MatchBalancer.PowerParameter) - ComputeTeamWeight(gameMatch.TeamB, MatchBalancer.PowerParameter);
    }

    internal static float ComputeTeamWeight(List<WeightedCrpgUser> team, float p = MatchBalancer.PowerParameter)
    {
        var clangroupTeam = MatchBalancingHelpers.SplitUsersIntoClanGroups(team);
        return clangroupTeam.Sum(c => c.Weight());
    }
    internal static float ComputeTeamAbsWeight(List<WeightedCrpgUser> team, float p = MatchBalancer.PowerParameter)
    {
        var clangroupTeam = MatchBalancingHelpers.SplitUsersIntoClanGroups(team);
        return clangroupTeam.Sum(c => Math.Abs(c.Weight()));
    }
}
