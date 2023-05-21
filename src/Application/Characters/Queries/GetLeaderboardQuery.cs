using AutoMapper;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Parties.Commands;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Characters.Queries;

public record GetLeaderboardQuery : IMediatorRequest<List<CharacterViewModel>>
{
    internal class Handler : IMediatorRequestHandler<GetLeaderboardQuery, List<CharacterViewModel>>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<List<CharacterViewModel>>> Handle(GetLeaderboardQuery req, CancellationToken cancellationToken)
        {
            var characters = await _db.Characters.ToListAsync();

            var topCharacters = characters
                .OrderByDescending(c => c.Rating.CompetitiveRating)
                .Take(50)
                .Select(c => _mapper.Map<CharacterViewModel>(c))
                .ToList();


            return new(topCharacters);
        }
    }
}
