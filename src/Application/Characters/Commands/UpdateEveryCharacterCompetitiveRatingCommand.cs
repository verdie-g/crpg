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

        public Handler(ICrpgDbContext db, ICompetitiveRatingModel competitiveRatingModel, Mapper mapper)
        {
            _db = db;
            _competitiveRatingModel = competitiveRatingModel;
            _mapper = mapper;
        }

        public async Task<Result> Handle(UpdateEveryCharacterCompetitiveRatingCommand req, CancellationToken cancellationToken)
        {
            var characters = await _db.Characters.ToListAsync();

            characters.ForEach(c => c.Rating.CompetitiveRating = _competitiveRatingModel.ComputeCompetitiveRating(_mapper.Map<CharacterRatingViewModel>(c.Rating)));
            await _db.SaveChangesAsync(cancellationToken);
            return Result.NoErrors;
        }
    }
}
