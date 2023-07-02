using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Characters.Commands;

public record UpdateEveryCharacterCompetitiveRatingCommand : IMediatorRequest
{
    internal class Handler : IMediatorRequestHandler<UpdateEveryCharacterCompetitiveRatingCommand>
    {
        private readonly ICrpgDbContext _db;
        private readonly ICompetitiveRatingModel _competitiveRatingModel;

        public Handler(ICrpgDbContext db, ICompetitiveRatingModel competitiveRatingModel)
        {
            _db = db;
            _competitiveRatingModel = competitiveRatingModel;
        }

        public async Task<Result> Handle(UpdateEveryCharacterCompetitiveRatingCommand req, CancellationToken cancellationToken)
        {
            var characters = await _db.Characters.ToArrayAsync(cancellationToken: cancellationToken);

            foreach (var character in characters)
            {
                character.Rating.CompetitiveValue = _competitiveRatingModel.ComputeCompetitiveRating(character.Rating);
                // Trick to avoid UpdatedAt to be updated.
                character.UpdatedAt = character.UpdatedAt;
            }

            await _db.SaveChangesAsync(cancellationToken);
            return Result.NoErrors;
        }
    }
}
