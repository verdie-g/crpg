using AutoMapper;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Characters.Queries;

public record GetUserCharacterCompetitiveRatingQuery : IMediatorRequest<CharacterCompetitiveRatingViewModel>
{
    public int CharacterId { get; init; }
    public int UserId { get; init; }

    internal class Handler : IMediatorRequestHandler<GetUserCharacterCompetitiveRatingQuery, CharacterCompetitiveRatingViewModel>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly ICompetitiveRatingModel _ratingmodel;

        public Handler(ICrpgDbContext db, IMapper mapper, ICompetitiveRatingModel ratingmodel)
        {
            _db = db;
            _mapper = mapper;
            _ratingmodel = ratingmodel;
        }

        public async Task<Result<CharacterCompetitiveRatingViewModel>> Handle(GetUserCharacterCompetitiveRatingQuery req, CancellationToken cancellationToken)
        {
            var character = await _db.Characters
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == req.CharacterId && c.UserId == req.UserId, cancellationToken);

            return character == null
                ? new(CommonErrors.CharacterNotFound(req.CharacterId, req.UserId))
                : new(_ratingmodel.ComputeCompetitiveRating(_mapper.Map<CharacterRatingViewModel>(character)));
        }
    }
}
