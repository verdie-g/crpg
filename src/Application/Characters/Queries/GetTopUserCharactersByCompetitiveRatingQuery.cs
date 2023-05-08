using AutoMapper;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Crpg.Application.Characters.Queries;

public record GetTopUserCharactersByCompetitiveRatingQuery : IMediatorRequest<IList<CharacterCompetitiveRatingViewModel>>
{
    public int Top { get; init; } = 50;

    internal class Handler : IMediatorRequestHandler<GetTopUserCharactersByCompetitiveRatingQuery, IList<CharacterCompetitiveRatingViewModel>>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<IList<CharacterCompetitiveRatingViewModel>>> Handle(GetTopUserCharactersByCompetitiveRatingQuery req, CancellationToken cancellationToken)
        {
            var characters = await _db.Characters
                .Include(c => c.Rating)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var characterViewModels = _mapper.Map<IList<CharacterCompetitiveRatingViewModel>>(characters);

            var topRankedCharacterViewModels = characterViewModels
                .OrderByDescending(c => c.CompetitiveRating)
                .Take(req.Top)
                .ToList();

            return new(topRankedCharacterViewModels);
        }
    }
}
