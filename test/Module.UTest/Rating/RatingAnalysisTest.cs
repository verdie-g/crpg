using System.Runtime.CompilerServices;
using Crpg.Module.Api.Models.Clans;
using Crpg.Module.Api.Models.Users;
using Crpg.Module.Balancing;
using Crpg.Module.Helpers;
using NUnit.Framework;
using TaleWorlds.Core;
using TaleWorlds.Library;
using static Crpg.Module.UTest.Rating.CrpgRatingAnalysis;

namespace Crpg.Module.UTest.Rating;

public class RatingAnalysisTest
{
  
    [Test]
    public void TestDifferentRating()
    {
        var ratingAnalysis = new CrpgRatingAnalysis(@"A:\\log.txt");
        BattleSideEnum ClangroupPenalizedTeamRaterPrediction(RoundResultData result)
        {
            return TeamRaterPrediction(result, players => ClanGroupPenalizedTeamRater(players));
        }

        float successPercentage = ratingAnalysis.AccuratePredictionPercentage(ClangroupPenalizedTeamRaterPrediction);
        Debug.Print($"{successPercentage}");

    }

    private BattleSideEnum TeamRaterPrediction(RoundResultData result, Func<List<RoundPlayerData>,float> teamRater)
    {
        return teamRater(result.Attackers) > teamRater(result.Defenders) ? BattleSideEnum.Attacker : BattleSideEnum.Defender;
    }

    private float ClanGroupPenalizedTeamRater(List<RoundPlayerData> playerList, float penaltyFactor = 0.02f)
    {
        float rating = 0;
        var clanGroups = SplitUsersIntoClanGroups(playerList);
        foreach (var clanGroup in clanGroups)
        {
            rating = clanGroup.Sum(p => p.Rating) * (1 + penaltyFactor * clanGroup.Count);
        }
        return rating;
    }

    private static List<List<RoundPlayerData>> SplitUsersIntoClanGroups(List<RoundPlayerData> users)
    {
        Dictionary<string, List<RoundPlayerData>> clanGroupsByClanId = new();
        List<List<RoundPlayerData>> clanGroups = new();

        foreach (RoundPlayerData player in users.OrderByDescending(u => u.ClanTag))
        {
            List<RoundPlayerData> clanGroup;
            if (player.ClanTag == null)
            {
                clanGroup = new();
                clanGroups.Add(clanGroup);
            }
            else if (!clanGroupsByClanId.TryGetValue(player.ClanTag, out clanGroup!))
            {
                clanGroup = new();
                clanGroups.Add(clanGroup);
                clanGroupsByClanId[player.ClanTag] = clanGroup;
            }

            clanGroup.Add(player);
        }

        return clanGroups;
    }

}
