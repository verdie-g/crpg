using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Characters.Queries
{
    public record GetUserCharacterQuery : IMediatorRequest<CharacterViewModel>
    {
        public int CharacterId { get; init; }
        public int UserId { get; init; }

        internal class Handler : IMediatorRequestHandler<GetUserCharacterQuery, CharacterViewModel>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Result<CharacterViewModel>> Handle(GetUserCharacterQuery req, CancellationToken cancellationToken)
            {
                var character = await _db.Characters
                    .AsNoTracking()
                    .Include(c => c.EquippedItems).ThenInclude(ei => ei.Item)
                    .FirstOrDefaultAsync(c => c.Id == req.CharacterId && c.UserId == req.UserId, cancellationToken);

                return character == null
                    ? new(CommonErrors.CharacterNotFound(req.CharacterId, req.UserId))
                    : new(_mapper.Map<CharacterViewModel>(character)); // can't use ProjectTo https://github.com/dotnet/efcore/issues/19726
            }
        }
    }
}
