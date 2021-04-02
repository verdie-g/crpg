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
    public record UpdateStrategusHeroStatusCommand : IMediatorRequest<StrategusHeroViewModel>
    {
        public int HeroId { get; set; }
        public StrategusHeroStatus Status { get; init; }
        public MultiPoint Waypoints { get; init; } = MultiPoint.Empty;
        public int TargetedHeroId { get; init; }
        public int TargetedSettlementId { get; init; }

        public class Validator : AbstractValidator<UpdateStrategusHeroStatusCommand>
        {
            public Validator()
            {
                RuleFor(m => m.Status).IsInEnum();
            }
        }

        internal class Handler : IMediatorRequestHandler<UpdateStrategusHeroStatusCommand, StrategusHeroViewModel>
        {
            private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpdateStrategusHeroStatusCommand>();

            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;
            private readonly IStrategusMap _strategusMap;

            public Handler(ICrpgDbContext db, IMapper mapper, IStrategusMap strategusMap)
            {
                _db = db;
                _mapper = mapper;
                _strategusMap = strategusMap;
            }

            public async Task<Result<StrategusHeroViewModel>> Handle(UpdateStrategusHeroStatusCommand req, CancellationToken cancellationToken)
            {
                var hero = await _db.StrategusHeroes
                    .Include(h => h.User)
                    .FirstOrDefaultAsync(h => h.Id == req.HeroId, cancellationToken);
                if (hero == null)
                {
                    return new(CommonErrors.HeroNotFound(req.HeroId));
                }

                if (hero.Status == StrategusHeroStatus.InBattle)
                {
                    return new(CommonErrors.HeroInBattle(req.HeroId));
                }

                if (req.Status == StrategusHeroStatus.RecruitingInSettlement)
                {
                    if (hero.Status != StrategusHeroStatus.IdleInSettlement)
                    {
                        return new(CommonErrors.HeroNotInASettlement(req.HeroId));
                    }

                    hero.Status = StrategusHeroStatus.RecruitingInSettlement;
                }
                else
                {
                    var result = await UpdateHeroMovement(hero, req, cancellationToken);
                    if (result.Errors != null)
                    {
                        return new(result.Errors);
                    }
                }

                await _db.SaveChangesAsync(cancellationToken);
                Logger.LogInformation("Hero '{0}' updated their movement on the map", req.HeroId);
                return new(_mapper.Map<StrategusHeroViewModel>(hero));
            }

            private async Task<Result> UpdateHeroMovement(StrategusHero hero, UpdateStrategusHeroStatusCommand req,
                CancellationToken cancellationToken)
            {
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
                        return new Result(CommonErrors.UserNotFound(req.TargetedHeroId));
                    }

                    if (!hero.Position.IsWithinDistance(targetHero.Position, _strategusMap.ViewDistance))
                    {
                        return new Result(CommonErrors.HeroNotInSight(req.TargetedHeroId));
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
                        return new Result(CommonErrors.SettlementNotFound(req.TargetedSettlementId));
                    }

                    hero.Status = req.Status;
                    hero.TargetedSettlement = targetSettlement;
                }

                return Result.NoErrors;
            }
        }
    }
}
