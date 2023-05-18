using AutoMapper;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Characters.Queries;

public record GetLeaderboardQuery : IMediatorRequest<LeaderboardViewModel>
{
    internal class Handler : IMediatorRequestHandler<GetLeaderboardQuery, LeaderboardViewModel>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<LeaderboardViewModel>> Handle(GetLeaderboardQuery req, CancellationToken cancellationToken)
        {
            var leaderboard = await _db.Leaderboard.FirstOrDefaultAsync(cancellationToken);

            return new(_mapper.Map<LeaderboardViewModel>(leaderboard));
        }
    }
}
