using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Characters.Queries
{
    public record GetUserCharactersQuery : IMediatorRequest<IList<CharacterViewModel>>
    {
        public int UserId { get; init; }

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
                    .Where(c => c.UserId == req.UserId)
                    .OrderByDescending(c => c.UpdatedAt)
                    .ProjectTo<CharacterViewModel>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                return new(characters);
            }
        }
    }
}
