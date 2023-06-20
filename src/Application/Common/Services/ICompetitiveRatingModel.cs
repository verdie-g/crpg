using Crpg.Domain.Entities.Characters;

namespace Crpg.Application.Common.Services;

/// <summary>
/// Service to compute the Competitive Rating <see cref="CharacterRating"/>.
/// </summary>
internal interface ICompetitiveRatingModel
{
    /// <summary>Compute the Competitive Rating.</summary>
    float ComputeCompetitiveRating(CharacterRating rating);
}

internal class CompetitiveRatingModel : ICompetitiveRatingModel
{
    /// <inheritdoc />
    public float ComputeCompetitiveRating(CharacterRating rating)
    {
        return (float)(0.03f * Math.Pow((0.01 * rating.Value) - (2 * rating.Deviation), 3.98));
    }
}
