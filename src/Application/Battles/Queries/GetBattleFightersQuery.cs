using AutoMapper;
using Crpg.Application.Battles.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Battles;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Battles.Queries;

public record GetBattleFightersQuery : IMediatorRequest<IList<BattleFighterViewModel>>
{
    public int BattleId { get; init; }

    internal class Handler : IMediatorRequestHandler<GetBattleFightersQuery, IList<BattleFighterViewModel>>
    {
        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<IList<BattleFighterViewModel>>> Handle(GetBattleFightersQuery req, CancellationToken cancellationToken)
        {
            var battle = await _db.Battles
                .AsSplitQuery()
                .Include(b => b.Fighters).ThenInclude(f => f.Hero!).ThenInclude(h => h.User)
                .Include(b => b.Fighters).ThenInclude(f => f.Settlement)
                .FirstOrDefaultAsync(b => b.Id == req.BattleId, cancellationToken);
            if (battle == null)
            {
                return new(CommonErrors.BattleNotFound(req.BattleId));
            }

            // Battles in preparation shouldn't be visible to anyone but only to heroes in sight on the map.
            if (battle.Phase == BattlePhase.Preparation)
            {
                return new(CommonErrors.BattleInvalidPhase(req.BattleId, battle.Phase));
            }

            return new(_mapper.Map<IList<BattleFighterViewModel>>(battle.Fighters));
        }
    }
}
