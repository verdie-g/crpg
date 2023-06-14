using AutoMapper;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Parties.Commands;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Characters.Queries;

public record GetLeaderboardQuery : IMediatorRequest<IList<CharacterViewModel>>
{
    internal class Handler : IMediatorRequestHandler<GetLeaderboardQuery, IList<CharacterViewModel>>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<IList<CharacterViewModel>>> Handle(GetLeaderboardQuery req, CancellationToken cancellationToken)
        {
            var characters = await _db.Characters.ToArrayAsync();

            var topCharacters = characters
                .OrderByDescending(c => c.Rating.CompetitiveRating)
                .Take(50)
                .Select(c => _mapper.Map<CharacterViewModel>(c))
                .ToList();

            return new(topCharacters);
        }
    }
}
