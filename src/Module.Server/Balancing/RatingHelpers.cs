using Crpg.Module.Api.Models.Users;
using Crpg.Module.Helpers;

namespace Crpg.Module.Balancing;

internal static class RatingHelpers
{
    internal static float ComputeTeamRatingPowerMean(List<CrpgUser> team, float p = MatchBalancer.PowerParameter)
    {
        List<float> ratings = team.Select(u => u.Character.Rating.GetWorkingRating()).ToList();
        return MathHelper.PowerMean(ratings, p);
    }

    /// <summary>
    /// Compute a signed Team Rating Difference.
    /// </summary>
    /// <param name="gameMatch">A GameMatch.</param>
    /// <returns>Returns Team A rating - Team B rating.</returns>
    internal static float ComputeTeamRatingDifference(GameMatch gameMatch)
    {
        return ComputeTeamRatingPowerSum(gameMatch.TeamA, MatchBalancer.PowerParameter) - ComputeTeamRatingPowerSum(gameMatch.TeamB, MatchBalancer.PowerParameter);
    }

    internal static float ComputeTeamRatingPowerSum(List<CrpgUser> team, float p = MatchBalancer.PowerParameter)
    {
        return MathHelper.PowerSumBy(team, u => u.Character.Rating.GetWorkingRating(), p);
    }

    internal static float ComputeTeamGlickoRatingPowerSum(List<CrpgUser> team, float p = MatchBalancer.PowerParameter)
    {
        return MathHelper.PowerSumBy(team, u => u.Character.Rating.GetRegularWorkingRating(), p);
    }
}
