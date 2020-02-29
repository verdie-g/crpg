using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Common.Interfaces;
using Crpg.Domain.Entities;

namespace Crpg.Application.Characters.Queries
{
    public class GetUserCharacterQuery : IRequest<CharacterViewModel>
    {
        public int CharacterId { get; set; }
        public int UserId { get; set; }

        public class Handler : IRequestHandler<GetUserCharacterQuery, CharacterViewModel>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<CharacterViewModel> Handle(GetUserCharacterQuery request, CancellationToken cancellationToken)
            {
                var character = await _db.Characters
                    .FirstOrDefaultAsync(c => c.Id == request.CharacterId && c.UserId == request.UserId, cancellationToken);

                if (character == null)
                {
                    throw new NotFoundException(nameof(Character), request.CharacterId);
                }

                return _mapper.Map<CharacterViewModel>(character);
            }
        }
    }
}
