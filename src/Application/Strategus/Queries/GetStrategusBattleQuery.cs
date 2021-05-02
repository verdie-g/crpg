using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Strategus.Models;
using Crpg.Domain.Entities.Strategus.Battles;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Strategus.Queries
{
    public record GetStrategusBattleQuery : IMediatorRequest<StrategusBattleViewModel>
    {
        public int BattleId { get; init; }

        internal class Handler : IMediatorRequestHandler<GetStrategusBattleQuery, StrategusBattleViewModel>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Result<StrategusBattleViewModel>> Handle(GetStrategusBattleQuery req, CancellationToken cancellationToken)
            {
                var battle = await _db.StrategusBattles
                    .ProjectTo<StrategusBattleViewModel>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(b => b.Id == req.BattleId, cancellationToken);
                if (battle == null)
                {
                    return new(CommonErrors.BattleNotFound(req.BattleId));
                }

                // Battles in preparation shouldn't be visible to anyone but only to heroes in sight on the map.
                if (battle.Phase == StrategusBattlePhase.Preparation)
                {
                    return new(CommonErrors.BattleInvalidPhase(req.BattleId, battle.Phase));
                }

                return new(battle);
            }
        }
    }
}
