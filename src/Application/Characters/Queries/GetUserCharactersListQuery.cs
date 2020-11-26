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
    public class GetUserCharactersListQuery : IMediatorRequest<IList<CharacterViewModel>>
    {
        public int UserId { get; set; }

        public class Handler : IMediatorRequestHandler<GetUserCharactersListQuery, IList<CharacterViewModel>>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Result<IList<CharacterViewModel>>> Handle(GetUserCharactersListQuery req, CancellationToken cancellationToken)
            {
                var characters = await _db.Characters
                    .AsNoTracking()
                    .Include(c => c.Items.HeadItem)
                    .Include(c => c.Items.ShoulderItem)
                    .Include(c => c.Items.BodyItem)
                    .Include(c => c.Items.HandItem)
                    .Include(c => c.Items.LegItem)
                    .Include(c => c.Items.MountHarnessItem)
                    .Include(c => c.Items.MountItem)
                    .Include(c => c.Items.Weapon1Item)
                    .Include(c => c.Items.Weapon2Item)
                    .Include(c => c.Items.Weapon3Item)
                    .Include(c => c.Items.Weapon4Item)
                    .Where(c => c.UserId == req.UserId)
                    .ToListAsync(cancellationToken);

                // can't use ProjectTo https://github.com/dotnet/efcore/issues/19726
                return new Result<IList<CharacterViewModel>>(_mapper.Map<IList<CharacterViewModel>>(characters));
            }
        }
    }
}
