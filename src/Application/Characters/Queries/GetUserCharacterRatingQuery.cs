using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Battles.Models;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Characters.Queries;

public record GetUserCharacterRatingQuery : IMediatorRequest<CharacterRatingViewModel>
{
    public int CharacterId { get; init; }
    public int UserId { get; init; }

    internal class Handler : IMediatorRequestHandler<GetUserCharacterRatingQuery, CharacterRatingViewModel>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<CharacterRatingViewModel>> Handle(GetUserCharacterRatingQuery req, CancellationToken cancellationToken)
        {
            var characterRatingViewModel = await _db.Characters
                .Where(c => c.Id == req.CharacterId && c.UserId == req.UserId)
                .Select(c => c.Rating)
                .ProjectTo<CharacterRatingViewModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);

            return characterRatingViewModel == null
                ? new(CommonErrors.CharacterNotFound(req.CharacterId, req.UserId))
                : new(characterRatingViewModel);
        }
    }
}
