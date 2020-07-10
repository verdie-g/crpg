using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Exceptions;
using Crpg.Application.Common.Interfaces;
using Crpg.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
                    .AsNoTracking()
                    .Include(c => c.Items.HeadItem)
                    .Include(c => c.Items.CapeItem)
                    .Include(c => c.Items.BodyItem)
                    .Include(c => c.Items.HandItem)
                    .Include(c => c.Items.LegItem)
                    .Include(c => c.Items.HorseHarnessItem)
                    .Include(c => c.Items.HorseItem)
                    .Include(c => c.Items.Weapon1Item)
                    .Include(c => c.Items.Weapon2Item)
                    .Include(c => c.Items.Weapon3Item)
                    .Include(c => c.Items.Weapon4Item)
                    .FirstOrDefaultAsync(c => c.Id == request.CharacterId && c.UserId == request.UserId, cancellationToken);

                if (character == null)
                {
                    throw new NotFoundException(nameof(Character), request.CharacterId);
                }

                // can't use ProjectTo https://github.com/dotnet/efcore/issues/20729
                return _mapper.Map<CharacterViewModel>(character);
            }
        }
    }
}
