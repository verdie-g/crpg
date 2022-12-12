using Crpg.Module.Api.Models.Users;
using Crpg.Module.Helpers;

namespace Crpg.Module.Balancing;

internal static class RatingHelpers
{
    internal static float ComputeTeamRatingPowerMean(List<CrpgUser> team, float p = MatchBalancingSystem.PowerParameter)
    {
        List<float> ratings = team.Select(u => u.Character.Rating.Value).ToList();
        return MathHelper.PowerMean(ratings, p);
    }

    internal static float ClanGroupsPowerSum(List<ClanGroup> clanGroups)
    {
        return ComputeTeamRatingPowerSum(MatchBalancingHelpers.JoinClanGroupsIntoUsers(clanGroups));
    }

    /// <summary>
    /// Compute a signed Team Rating Difference.
    /// </summary>
    /// <param name="gameMatch">A GameMatch.</param>
    /// <returns>Returns Team A rating - Team B rating.</returns>
    internal static float ComputeTeamRatingDifference(GameMatch gameMatch)
    {
        return ComputeTeamRatingPowerSum(gameMatch.TeamA, MatchBalancingSystem.PowerParameter) - ComputeTeamRatingPowerSum(gameMatch.TeamB, MatchBalancingSystem.PowerParameter);
    }

    internal static float ComputeTeamRatingPowerSum(List<CrpgUser> team, float p = MatchBalancingSystem.PowerParameter)
    {
        return MathHelper.PowerSumBy(team, u => u.Character.Rating.Value, p);
    }
}
