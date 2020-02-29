using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Characters.Queries
{
    public class GetUserCharactersListQuery : IRequest<IReadOnlyList<CharacterViewModel>>
    {
        public int UserId { get; set; }

        public class Handler : IRequestHandler<GetUserCharactersListQuery, IReadOnlyList<CharacterViewModel>>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<IReadOnlyList<CharacterViewModel>> Handle(GetUserCharactersListQuery request, CancellationToken cancellationToken)
            {
                return await _db.Characters
                    .Where(c => c.UserId == request.UserId)
                    .ProjectTo<CharacterViewModel>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);
            }
        }
    }
}
