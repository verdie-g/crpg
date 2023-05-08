using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mappings;
using Crpg.Application.Common.Services;
using Crpg.Application.Parties.Models;
using Crpg.Domain.Entities.Characters;
using Crpg.Domain.Entities.Parties;

namespace Crpg.Application.Characters.Models;

public record CharacterCompetitiveRatingViewModel : IMapFrom<CharacterRating>
{
    public float CompetitiveRating { get; init; }
    public void Mapping(Profile profile)
    {
        profile.CreateMap<Character, CharacterCompetitiveRatingViewModel>()
            .ForMember(u => u.CompetitiveRating, opt => opt.MapFrom(c => ComputeCompetitiveRating(c.Rating)));

    }
    private double ComputeCompetitiveRating(CharacterRating rating)
    {
        return 0.03 * Math.Pow(0.01 * rating.Value - 2 * rating.Deviation, 3.98);
    }
}
