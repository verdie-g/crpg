using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Characters.Models;
using Crpg.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Characters.Queries
{
    public class GetUserCharactersListQuery : IRequest<IList<CharacterViewModel>>
    {
        public int UserId { get; set; }

        public class Handler : IRequestHandler<GetUserCharactersListQuery, IList<CharacterViewModel>>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<IList<CharacterViewModel>> Handle(GetUserCharactersListQuery request, CancellationToken cancellationToken)
            {
                var characters = await _db.Characters
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
                    .Where(c => c.UserId == request.UserId)
                    .ToListAsync(cancellationToken);

                // can't use ProjectTo https://github.com/dotnet/efcore/issues/20729
                return _mapper.Map<IList<CharacterViewModel>>(characters);
            }
        }
    }
}
