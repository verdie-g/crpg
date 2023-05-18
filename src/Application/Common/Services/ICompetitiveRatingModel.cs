using Crpg.Application.Characters.Models;
using Crpg.Domain.Entities.Parties;

namespace Crpg.Application.Common.Services;

/// <summary>
/// Service to compute the speed of a <see cref="Party"/>.
/// </summary>
internal interface ICompetitiveRatingModel
{
    /// <summary>Compute the Party Speed.</summary>
    CharacterCompetitiveRatingViewModel ComputeCompetitiveRating(CharacterRatingViewModel ratingViewModel);
}

internal class CompetitiveRatingModel : ICompetitiveRatingModel
{
    /// <inheritdoc />
    public CharacterCompetitiveRatingViewModel ComputeCompetitiveRating(CharacterRatingViewModel rating)
    {
        CharacterCompetitiveRatingViewModel competitiveRating = new()
        {
            CompetitiveRating = 0.03 * Math.Pow((0.01 * rating.Value) - (2 * rating.Deviation), 3.98),
        };
        return competitiveRating;
    }
}
