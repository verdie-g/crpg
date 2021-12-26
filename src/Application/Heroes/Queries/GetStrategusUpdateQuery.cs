using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Battles.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Heroes.Models;
using Crpg.Application.Settlements.Models;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Heroes;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Heroes.Queries;

public record GetStrategusUpdateQuery : IMediatorRequest<StrategusUpdate>
{
    public int HeroId { get; init; }

    internal class Handler : IMediatorRequestHandler<GetStrategusUpdateQuery, StrategusUpdate>
    {
        private static readonly HeroStatus[] VisibleStatuses =
        {
            HeroStatus.Idle,
            HeroStatus.MovingToPoint,
            HeroStatus.FollowingHero,
            HeroStatus.MovingToSettlement,
            HeroStatus.MovingToAttackHero,
            HeroStatus.MovingToAttackSettlement,
        };

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly IStrategusMap _strategusMap;

        public Handler(ICrpgDbContext db, IMapper mapper, IStrategusMap strategusMap)
        {
            _db = db;
            _mapper = mapper;
            _strategusMap = strategusMap;
        }

        public async Task<Result<StrategusUpdate>> Handle(GetStrategusUpdateQuery req, CancellationToken cancellationToken)
        {
            var hero = await _db.Heroes
                .Include(h => h.User)
                .Include(h => h.TargetedHero!.User)
                .Include(h => h.TargetedSettlement)
                .FirstOrDefaultAsync(h => h.Id == req.HeroId, cancellationToken);
            if (hero == null)
            {
                return new(CommonErrors.HeroNotFound(req.HeroId));
            }

            var visibleHeroes = await _db.Heroes
                .Where(h => h.Id != hero.Id
                            && h.Position.IsWithinDistance(hero.Position, _strategusMap.ViewDistance)
                            && VisibleStatuses.Contains(h.Status))
                .ProjectTo<HeroVisibleViewModel>(_mapper.ConfigurationProvider)
                .ToArrayAsync(cancellationToken);

            var visibleSettlements = await _db.Settlements
                .Where(s => s.Position.IsWithinDistance(hero.Position, _strategusMap.ViewDistance))
                .ProjectTo<SettlementPublicViewModel>(_mapper.ConfigurationProvider)
                .ToArrayAsync(cancellationToken);

            var visibleBattles = await _db.Battles
                .Where(b => b.Position.IsWithinDistance(hero.Position, _strategusMap.ViewDistance)
                            && b.Phase != BattlePhase.End)
                .ProjectTo<BattleViewModel>(_mapper.ConfigurationProvider)
                .ToArrayAsync(cancellationToken);

            return new(new StrategusUpdate
            {
                Hero = _mapper.Map<HeroViewModel>(hero),
                VisibleHeroes = visibleHeroes,
                VisibleSettlements = visibleSettlements,
                VisibleBattles = visibleBattles,
            });
        }
    }
}
