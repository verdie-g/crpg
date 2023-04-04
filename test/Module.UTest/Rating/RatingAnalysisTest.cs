using System.Runtime.CompilerServices;
using Crpg.Module.Api.Models.Clans;
using Crpg.Module.Api.Models.Items;
using Crpg.Module.Api.Models.Users;
using Crpg.Module.Balancing;
using Crpg.Module.Helpers;
using Crpg.Module.Rating;
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

        var ratingAnalysis = new CrpgRatingAnalysis(@"A:\maxilog.txt");
        Debug.Print("penaltyfactor,prediction");
        for (int i = 1; i < 1000; i++)
        {
            BattleSideEnum ClangroupPenalizedTeamRaterPrediction(RoundResultData result)
            {
                return TeamRaterPrediction(result, players => FullRater(players, ratingPower: i / 100f));
            }

            float successPercentage = ratingAnalysis.AccuratePredictionPercentage(ClangroupPenalizedTeamRaterPrediction);
            Debug.Print($"{i / 100f},{successPercentage * 100}");
        }
    }

    [Test]
    public void HowOftenDoAttackerWin()
    {

        var ratingAnalysis = new CrpgRatingAnalysis(@"A:\maxilogtxt");
        Debug.Print("penaltyfactor,prediction");
        for (int i = 0; i < 1000; i++)
        {
            float successPercentage = ratingAnalysis.AccuratePredictionPercentage(AlwaysAttackerWin);
            Debug.Print($"{i / 1000f},{successPercentage * 100}");
        }
    }

    private BattleSideEnum TeamRaterPrediction(RoundResultData result, Func<List<RoundPlayerData>, float> teamRater)
    {
        return teamRater(result.Attackers) > teamRater(result.Defenders) ? BattleSideEnum.Attacker : BattleSideEnum.Defender;
    }

    private BattleSideEnum AlwaysAttackerWin(RoundResultData result)
    {
        return BattleSideEnum.Attacker;
    }

    private float ClanGroupPenalizedTeamRater(List<RoundPlayerData> playerList, float penaltyFactor = 0.05f)
    {

        float rating = 0;
        var clanGroups = SplitUsersIntoClanGroups(playerList);
        foreach (var clanGroup in clanGroups)
        {
            rating += clanGroup.Sum(p => p.Weight) * (1 + penaltyFactor * clanGroup.Count) / (1 + 0.028f * clanGroup.Count);
        }

        return rating;
    }
    private float FullRater(List<RoundPlayerData> playerList, float penaltyFactor = 0.048f, float priceDivider = 500000f, float maxPrice = 56000f, float ratingPower = 3f)
    {
        float ComputeWeight(RoundPlayerData user)
        {
            float ratingWeight = ComputeRatingWeight(user);
            float itemsWeight = ComputeEquippedItemsWeight(user.EquipmentCost);
            float levelWeight = ComputeLevelWeight(user.Level);

            return ratingWeight * itemsWeight * levelWeight;
        }

        float ComputeRatingWeight(RoundPlayerData user)
        {
            var rating = user.Rating;
            // https://www.desmos.com/calculator/snynzhhoay
            return 6E-8f * (float)Math.Pow(rating - 2 * 50, ratingPower);
        }

        float ComputeEquippedItemsWeight(float equipmentcost)
        {
            float itemsPrice = equipmentcost;
            return 1f + itemsPrice / priceDivider;
        }

        float ComputeLevelWeight(int level)
        {
            // Ideally the rating should be elastic enough to change when the character
            // retires but that's not the case so for now let's use the level to compute
            // the weight.
            return 1f + level / 30f;
        }

        float weight = 0;
        var clanGroups = SplitUsersIntoClanGroups(playerList);
        foreach (var clanGroup in clanGroups)
        {
            weight += clanGroup.Sum(p => ComputeWeight(p)) * (1 + penaltyFactor * clanGroup.Count) / (1 + 0.028f * clanGroup.Count);
        }

        return weight;
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
