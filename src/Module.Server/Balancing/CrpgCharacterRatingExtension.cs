using Crpg.Module.Api.Models.Characters;

namespace Crpg.Module.Balancing;

internal static class CrpgCharacterRatingExtension
{
    public static float GetWorkingRating(this CrpgCharacterRating rating)
    {
        return 0.0025f * (float)Math.Pow(rating.Value - 2* rating.Deviation, 1.5f);
    }
}
