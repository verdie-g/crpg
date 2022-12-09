using System;
using System.Collections.Generic;
using System.Linq;
using Crpg.Module.Api.Models.Users;
using Crpg.Module.Helpers;

namespace Crpg.Module.Balancing;

/// <summary>
/// i don't know yet.
/// </summary>


internal class RatingHelpers
{

    internal static float ComputeTeamRatingPowerMean(List<CrpgUser> team, float p = MatchBalancingSystem.PowerParameter)
    {
        List<float> elos = team.Select(u => (float)u.Character.Rating.Value).ToList();
        return MathHelper.PowerMean(elos, p);
    }
    internal static float ClanGroupsPowerSum(List<ClanGroup> clanGroups)
    {
        return ComputeTeamRatingPowerSum(MatchBalancingHelpers.ConvertClanGroupsToCrpgUserList(clanGroups));
    }

    internal static float ComputeTeamRatingDifference(GameMatch gameMatch)
    {
        return ComputeTeamRatingPowerSum(gameMatch.TeamA, MatchBalancingSystem.PowerParameter) - ComputeTeamRatingPowerSum(gameMatch.TeamB, MatchBalancingSystem.PowerParameter);
    }



    internal static float ComputeTeamRatingPowerSum(List<CrpgUser> team, float p = MatchBalancingSystem.PowerParameter)
    {
        List<float> elos = team.Select(u => (float)u.Character.Rating.Value).ToList();
        return MathHelper.PowerSum(elos, p);
    }

}
