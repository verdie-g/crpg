using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Parties;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Parties.Commands;

public record UpdateLeaderboardCommand : IMediatorRequest
{
    public TimeSpan DeltaTime { get; init; }

    internal class Handler : IMediatorRequestHandler<UpdateLeaderboardCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpdateLeaderboardCommand>();
        private readonly ICrpgDbContext _db;
        private readonly ICompetitiveRatingModel _competitiveRatingModel;

        public Handler(ICrpgDbContext db, ICompetitiveRatingModel competitiveRatingModel)
        {
            _db = db;
            _competitiveRatingModel = competitiveRatingModel;
        }

        public async Task<Result> Handle(UpdateLeaderboardCommand req, CancellationToken cancellationToken)
        {
            var leaderboard = await _db.Leaderboard.FirstOrDefaultAsync();
            var characters = await _db.Characters.ToListAsync();

            var topCharacters = characters
                .OrderByDescending(c => _competitiveRatingModel.ComputeCompetitiveRating(
                    new CharacterRatingViewModel
                    {
                        Value = c.Rating.Value,
                        Deviation = c.Rating.Deviation,
                        Volatility = c.Rating.Volatility
                    }))
                .Take(50)
                .ToList();
            if (leaderboard == null)
            {
                leaderboard = new();
            }

            leaderboard.LeaderboardList = topCharacters;
            leaderboard.LeaderboardLastUpdatedDate = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            return Result.NoErrors;
        }
    }
}
