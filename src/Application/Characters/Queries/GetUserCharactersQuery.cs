using System.Collections.Generic;
using System.Linq;
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
    public class GetUserCharactersQuery : IMediatorRequest<IList<CharacterViewModel>>
    {
        public int UserId { get; set; }

        internal class Handler : IMediatorRequestHandler<GetUserCharactersQuery, IList<CharacterViewModel>>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Result<IList<CharacterViewModel>>> Handle(GetUserCharactersQuery req, CancellationToken cancellationToken)
            {
                var characters = await _db.Characters
                    .AsNoTracking()
                    .Include(c => c.EquippedItems).ThenInclude(ei => ei.Item)
                    .Where(c => c.UserId == req.UserId)
                    .OrderByDescending(c => c.UpdatedAt)
                    .ToListAsync(cancellationToken);

                // can't use ProjectTo https://github.com/dotnet/efcore/issues/19726
                return new Result<IList<CharacterViewModel>>(_mapper.Map<IList<CharacterViewModel>>(characters));
            }
        }
    }
}
