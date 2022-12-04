using System;
using System.Collections.Generic;
using System.Linq;

namespace Crpg.Module.Balancing;

/// <summary>
/// i don't know yet.
/// </summary>


public class RatingHelpers
{

    public static float ComputeTeamRatingPowerMean(List<User> team, float p = MatchBalancingSystem.PowerParameter)
    {
        List<float> elos = team.Select(u => (float)u.Rating).ToList();
        return MathHelper.PowerMean(elos, p);
    }
    public static float ClanGroupsPowerSum(List<ClanGroup> clanGroups)
    {
        return ComputeTeamRatingPowerSum(MatchBalancingHelpers.ConvertClanGroupsToUserList(clanGroups));
    }

    public static float ComputeTeamRatingDifference(GameMatch gameMatch)
    {
        return ComputeTeamRatingPowerSum(gameMatch.TeamA, MatchBalancingSystem.PowerParameter) - ComputeTeamRatingPowerSum(gameMatch.TeamB, MatchBalancingSystem.PowerParameter);
    }



    public static float ComputeTeamRatingPowerSum(List<User> team, float p = MatchBalancingSystem.PowerParameter)
    {
        List<float> elos = team.Select(u => (float)u.Rating).ToList();
        return MathHelper.PowerSum(elos, p);
    }

}
