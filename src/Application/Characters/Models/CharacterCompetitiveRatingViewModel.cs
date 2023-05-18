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
    public double CompetitiveRating { get; init; }
}
