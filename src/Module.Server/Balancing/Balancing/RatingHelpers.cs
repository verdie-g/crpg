using System;
using System.Collections.Generic;
using System.Linq;

namespace Crpg.Module.Balancing;

/// <summary>
/// i don't know yet.
/// </summary>


public class RatingHelpers
{

    public static int ComputeTeamRatingPowerMean(List<User> team, int p)
    {
        List<int> elos = team.Select(u => (int) u.Rating).ToList();
        return MathHelper.PowerMean(elos, p);
    }

    public static int ComputeTeamRatingDifference(GameMatch gameMatch)
    {
        return ComputeTeamRatingPowerSum(gameMatch.TeamA, 1) - ComputeTeamRatingPowerSum(gameMatch.TeamB, 1);
    }



    public static int ComputeTeamRatingPowerSum(List<User> team, int p)
    {
        List<int> elos = (List<int>)team.Select(u => u.Rating);
        return MathHelper.PowerSum(elos, p);
    }

}
