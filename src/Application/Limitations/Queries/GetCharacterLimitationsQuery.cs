using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Limitations.Models;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Limitations.Queries;

public record GetCharacterLimitationsQuery : IMediatorRequest<CharacterLimitationsViewModel>
{
    public int CharacterId { get; init; }
    public int UserId { get; init; }

    internal class Handler : IMediatorRequestHandler<GetCharacterLimitationsQuery, CharacterLimitationsViewModel>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<CharacterLimitationsViewModel>> Handle(GetCharacterLimitationsQuery req, CancellationToken cancellationToken)
        {
            var character = await _db.Characters
                .AsNoTracking()
                .Include(c => c.Limitations)
                .FirstOrDefaultAsync(c => c.Id == req.CharacterId && c.UserId == req.UserId, cancellationToken);

            return character == null
                ? new(CommonErrors.CharacterNotFound(req.CharacterId, req.UserId))
                : new(_mapper.Map<CharacterLimitationsViewModel>(character.Limitations));
        }
    }
}
