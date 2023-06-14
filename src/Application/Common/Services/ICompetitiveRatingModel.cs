using Crpg.Application.Characters.Models;
using Crpg.Domain.Entities.Parties;

namespace Crpg.Application.Common.Services;

/// <summary>
/// Service to compute the Competitive Rating <see cref="CharacterRatingViewModel"/>.
/// </summary>
internal interface ICompetitiveRatingModel
{
    /// <summary>Compute the Competitive Rating.</summary>
    float ComputeCompetitiveRating(CharacterRatingViewModel ratingViewModel);
}

internal class CompetitiveRatingModel : ICompetitiveRatingModel
{
    /// <inheritdoc />
    public float ComputeCompetitiveRating(CharacterRatingViewModel rating)
    {
        return (float)(0.03f * Math.Pow((0.01 * rating.Value) - (2 * rating.Deviation), 3.98));
    }
}
