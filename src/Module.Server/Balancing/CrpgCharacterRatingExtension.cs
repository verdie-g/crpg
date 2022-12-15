using Crpg.Module.Api.Models.Characters;

namespace Crpg.Module.Balancing;

internal static class CrpgCharacterRatingExtension
{
    public static float GetWorkingRating(this CrpgCharacterRating rating)
    {
        return rating.Value - rating.Deviation;
    }
}
