using Crpg.Application.Common.Mappings;
using Crpg.Domain.Entities.Characters;

namespace Crpg.Application.Characters.Models;

public record CharacterRatingViewModel : IMapFrom<CharacterRating>
{
    public float Value { get; init; }
    public float Deviation { get; init; }
    public float Volatility { get; init; }
    public float CompetitiveValue { get; init; }
}
