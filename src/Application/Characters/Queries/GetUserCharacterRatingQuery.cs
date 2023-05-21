using AutoMapper;
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
            var character = await _db.Characters
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == req.CharacterId && c.UserId == req.UserId, cancellationToken);
            return character == null
                ? new(CommonErrors.CharacterNotFound(req.CharacterId, req.UserId))
                : new(_mapper.Map<CharacterRatingViewModel>(character.Rating));
        }
    }
}
