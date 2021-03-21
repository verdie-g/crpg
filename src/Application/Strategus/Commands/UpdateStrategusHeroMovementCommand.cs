using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Strategus.Models;
using Crpg.Domain.Entities.Strategus;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Strategus.Commands
{
    public class UpdateStrategusHeroMovementCommand : IMediatorRequest<StrategusHeroViewModel>
    {
        public int HeroId { get; set; }
        public StrategusHeroStatus Status { get; set; }
        public MultiPoint Waypoints { get; set; } = MultiPoint.Empty;
        public int TargetedHeroId { get; set; }
        public int TargetedSettlementId { get; set; }

        public class Validator : AbstractValidator<UpdateStrategusHeroMovementCommand>
        {
            public Validator()
            {
                RuleFor(m => m.Status).IsInEnum();
            }
        }

        internal class Handler : IMediatorRequestHandler<UpdateStrategusHeroMovementCommand, StrategusHeroViewModel>
        {
            private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpdateStrategusHeroMovementCommand>();

            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;
            private readonly IStrategusMap _strategusMap;

            public Handler(ICrpgDbContext db, IMapper mapper, IStrategusMap strategusMap)
            {
                _db = db;
                _mapper = mapper;
                _strategusMap = strategusMap;
            }

            public async Task<Result<StrategusHeroViewModel>> Handle(UpdateStrategusHeroMovementCommand req, CancellationToken cancellationToken)
            {
                var hero = await _db.StrategusHeroes
                    .Include(h => h.User)
                    .FirstOrDefaultAsync(h => h.Id == req.HeroId, cancellationToken);
                if (hero == null)
                {
                    return new Result<StrategusHeroViewModel>(CommonErrors.HeroNotFound(req.HeroId));
                }

                if (hero.Status == StrategusHeroStatus.InBattle)
                {
                    return new Result<StrategusHeroViewModel>(CommonErrors.HeroInBattle(req.HeroId));
                }

                // Reset movement.
                hero.Status = StrategusHeroStatus.Idle;
                hero.Waypoints = MultiPoint.Empty;
                hero.TargetedHeroId = null;
                hero.TargetedSettlementId = null;

                if (req.Status == StrategusHeroStatus.MovingToPoint)
                {
                    if (!req.Waypoints.IsEmpty)
                    {
                        hero.Status = req.Status;
                        hero.Waypoints = req.Waypoints;
                    }
                }
                else if (req.Status == StrategusHeroStatus.FollowingHero
                         || req.Status == StrategusHeroStatus.MovingToAttackHero)
                {
                    var targetHero = await _db.StrategusHeroes
                        .Include(h => h.User)
                        .FirstOrDefaultAsync(h => h.Id == req.TargetedHeroId, cancellationToken);
                    if (targetHero == null)
                    {
                        return new Result<StrategusHeroViewModel>(CommonErrors.UserNotFound(req.TargetedHeroId));
                    }

                    if (!hero.Position.IsWithinDistance(targetHero.Position, _strategusMap.ViewDistance))
                    {
                        return new Result<StrategusHeroViewModel>(CommonErrors.HeroNotInSight(req.TargetedHeroId));
                    }

                    hero.Status = req.Status;
                    hero.TargetedHero = targetHero;
                }
                else if (req.Status == StrategusHeroStatus.MovingToSettlement
                         || req.Status == StrategusHeroStatus.MovingToAttackSettlement)
                {
                    var targetSettlement = await _db.StrategusSettlements
                        .Include(s => s.Owner!.User)
                        .FirstOrDefaultAsync(s => s.Id == req.TargetedSettlementId, cancellationToken);
                    if (targetSettlement == null)
                    {
                        return new Result<StrategusHeroViewModel>(CommonErrors.SettlementNotFound(req.TargetedSettlementId));
                    }

                    hero.Status = req.Status;
                    hero.TargetedSettlement = targetSettlement;
                }

                await _db.SaveChangesAsync(cancellationToken);
                Logger.LogInformation("Hero '{0}' updated their movement on the map", req.HeroId);
                return new Result<StrategusHeroViewModel>(_mapper.Map<StrategusHeroViewModel>(hero));
            }
        }
    }
}
