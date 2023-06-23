using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Characters.Queries;

public record GetLeaderboardQuery : IMediatorRequest<IList<CharacterPublicViewModel>>
{
    public Region? Region { get; set; }
    internal class Handler : IMediatorRequestHandler<GetLeaderboardQuery, IList<CharacterPublicViewModel>>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<IList<CharacterPublicViewModel>>> Handle(GetLeaderboardQuery req, CancellationToken cancellationToken)
        {
            var topRatedCharacters = await _db.Characters
                .OrderByDescending(c => c.Rating.CompetitiveValue)
                .Where(c => c.User!.Region == req.Region)
                .Take(50)
                .ProjectTo<CharacterPublicViewModel>(_mapper.ConfigurationProvider)
                .AsSplitQuery()
                .ToArrayAsync();

            return new(topRatedCharacters);
        }
    }
}
