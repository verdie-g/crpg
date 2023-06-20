using AutoMapper;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Parties.Commands;

public record UpdateEveryCharacterCompetitiveRatingCommand : IMediatorRequest
{
    internal class Handler : IMediatorRequestHandler<UpdateEveryCharacterCompetitiveRatingCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpdateEveryCharacterCompetitiveRatingCommand>();

        private readonly ICrpgDbContext _db;
        private readonly ICompetitiveRatingModel _competitiveRatingModel;
        private readonly Mapper _mapper;

        public Handler(ICrpgDbContext db, Mapper mapper, ICompetitiveRatingModel competitiveRatingModel)
        {
            _db = db;
            _mapper = mapper;
            _competitiveRatingModel = competitiveRatingModel;
        }

        public async Task<Result> Handle(UpdateEveryCharacterCompetitiveRatingCommand req, CancellationToken cancellationToken)
        {
            var characters = await _db.Characters.ToArrayAsync();

            foreach (var character in characters)
            {
                character.Rating.CompetitiveValue = _competitiveRatingModel.ComputeCompetitiveRating(_mapper.Map<CharacterRatingViewModel>(character.Rating));
            }

            await _db.SaveChangesAsync(cancellationToken);
            return Result.NoErrors;
        }
    }
}
