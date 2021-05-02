using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Strategus.Models;
using Crpg.Domain.Entities.Strategus.Battles;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Strategus.Queries
{
    public record GetStrategusBattleFightersQuery : IMediatorRequest<IList<StrategusBattleFighterViewModel>>
    {
        public int BattleId { get; init; }

        internal class Handler : IMediatorRequestHandler<GetStrategusBattleFightersQuery, IList<StrategusBattleFighterViewModel>>
        {
            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Result<IList<StrategusBattleFighterViewModel>>> Handle(GetStrategusBattleFightersQuery req, CancellationToken cancellationToken)
            {
                var battle = await _db.StrategusBattles
                    .AsSplitQuery()
                    .Include(b => b.Fighters).ThenInclude(f => f.Hero!).ThenInclude(h => h.User)
                    .Include(b => b.Fighters).ThenInclude(f => f.Settlement)
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

                return new(_mapper.Map<IList<StrategusBattleFighterViewModel>>(battle.Fighters));
            }
        }
    }
}
