using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Domain.Entities;
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
                var characters = await _db.Characters
                    .Where(c => c.UserId == request.UserId)
                    .ToListAsync(cancellationToken);

                // can't use ProjectTo https://github.com/dotnet/efcore/issues/20729
                return _mapper.Map<IReadOnlyList<CharacterViewModel>>(characters);
            }
        }
    }
}
