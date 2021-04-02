using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Strategus.Models;
using Crpg.Domain.Entities.Strategus;
using Microsoft.EntityFrameworkCore;

namespace Crpg.Application.Strategus.Queries
{
    public record GetStrategusUpdateQuery : IMediatorRequest<StrategusUpdate>
    {
        public int HeroId { get; init; }

        internal class Handler : IMediatorRequestHandler<GetStrategusUpdateQuery, StrategusUpdate>
        {
            private static readonly StrategusHeroStatus[] VisibleStatuses =
            {
                StrategusHeroStatus.Idle,
                StrategusHeroStatus.MovingToPoint,
                StrategusHeroStatus.FollowingHero,
                StrategusHeroStatus.MovingToSettlement,
                StrategusHeroStatus.MovingToAttackHero,
                StrategusHeroStatus.MovingToAttackSettlement,
                StrategusHeroStatus.InBattle,
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
                var hero = await _db.StrategusHeroes
                    .Include(h => h.User)
                    .FirstOrDefaultAsync(h => h.Id == req.HeroId, cancellationToken);
                if (hero == null)
                {
                    return new(CommonErrors.HeroNotFound(req.HeroId));
                }

                var visibleHeroes = await _db.StrategusHeroes
                    .Where(h => h.Id != hero.Id
                                && h.Position.IsWithinDistance(hero.Position, _strategusMap.ViewDistance)
                                && VisibleStatuses.Contains(h.Status))
                    .ProjectTo<StrategusHeroVisibleViewModel>(_mapper.ConfigurationProvider)
                    .ToArrayAsync(cancellationToken);

                var visibleSettlements = await _db.StrategusSettlements
                    .Where(s => s.Position.IsWithinDistance(hero.Position, _strategusMap.ViewDistance))
                    .ProjectTo<StrategusSettlementPublicViewModel>(_mapper.ConfigurationProvider)
                    .ToArrayAsync(cancellationToken);

                return new(new StrategusUpdate
                {
                    Hero = _mapper.Map<StrategusHeroViewModel>(hero),
                    VisibleHeroes = visibleHeroes,
                    VisibleSettlements = visibleSettlements,
                });
            }
        }
    }
}
