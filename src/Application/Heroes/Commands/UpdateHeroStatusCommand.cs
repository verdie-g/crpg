using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Heroes.Models;
using Crpg.Domain.Entities.Heroes;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Heroes.Commands;

public record UpdateHeroStatusCommand : IMediatorRequest<HeroViewModel>
{
    public int HeroId { get; set; }
    public HeroStatus Status { get; init; }
    public MultiPoint Waypoints { get; init; } = MultiPoint.Empty;
    public int TargetedHeroId { get; init; }
    public int TargetedSettlementId { get; init; }

    public class Validator : AbstractValidator<UpdateHeroStatusCommand>
    {
        public Validator()
        {
            RuleFor(m => m.Status).IsInEnum();
        }
    }

    internal class Handler : IMediatorRequestHandler<UpdateHeroStatusCommand, HeroViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpdateHeroStatusCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly IStrategusMap _strategusMap;

        public Handler(ICrpgDbContext db, IMapper mapper, IStrategusMap strategusMap)
        {
            _db = db;
            _mapper = mapper;
            _strategusMap = strategusMap;
        }

        public async Task<Result<HeroViewModel>> Handle(UpdateHeroStatusCommand req, CancellationToken cancellationToken)
        {
            var hero = await _db.Heroes
                .Include(h => h.User)
                .Include(h => h.TargetedSettlement)
                .FirstOrDefaultAsync(h => h.Id == req.HeroId, cancellationToken);
            if (hero == null)
            {
                return new(CommonErrors.HeroNotFound(req.HeroId));
            }

            if (hero.Status == HeroStatus.InBattle)
            {
                return new(CommonErrors.HeroInBattle(req.HeroId));
            }

            if (req.Status == HeroStatus.IdleInSettlement || req.Status == HeroStatus.RecruitingInSettlement)
            {
                var result = StartStopRecruiting(req.Status == HeroStatus.RecruitingInSettlement, hero);
                if (result.Errors != null)
                {
                    return new(result.Errors);
                }
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
            Logger.LogInformation("Hero '{0}' updated their status", req.HeroId);
            return new(_mapper.Map<HeroViewModel>(hero));
        }

        private Result StartStopRecruiting(bool start, Hero hero)
        {
            if (start)
            {
                if (hero.Status != HeroStatus.IdleInSettlement)
                {
                    return new(CommonErrors.HeroNotInASettlement(hero.Id));
                }
            }
            else
            {
                if (hero.Status != HeroStatus.RecruitingInSettlement)
                {
                    return new(CommonErrors.HeroNotInASettlement(hero.Id));
                }
            }

            hero.Status = start ? HeroStatus.RecruitingInSettlement : HeroStatus.IdleInSettlement;
            return Result.NoErrors;
        }

        private async Task<Result> UpdateHeroMovement(Hero hero, UpdateHeroStatusCommand req,
            CancellationToken cancellationToken)
        {
            // Reset movement.
            hero.Status = HeroStatus.Idle;
            hero.Waypoints = MultiPoint.Empty;
            hero.TargetedHeroId = null;
            hero.TargetedSettlementId = null;

            if (req.Status == HeroStatus.MovingToPoint)
            {
                if (!req.Waypoints.IsEmpty)
                {
                    hero.Status = req.Status;
                    hero.Waypoints = req.Waypoints;
                }
            }
            else if (req.Status == HeroStatus.FollowingHero
                     || req.Status == HeroStatus.MovingToAttackHero)
            {
                var targetHero = await _db.Heroes
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
                // Need to be set manually because it was set to null above and it can confuse EF Core.
                hero.TargetedHeroId = targetHero.Id;
                hero.TargetedHero = targetHero;
            }
            else if (req.Status == HeroStatus.MovingToSettlement
                     || req.Status == HeroStatus.MovingToAttackSettlement)
            {
                var targetSettlement = await _db.Settlements
                    .Include(s => s.Owner!.User)
                    .FirstOrDefaultAsync(s => s.Id == req.TargetedSettlementId, cancellationToken);
                if (targetSettlement == null)
                {
                    return new Result(CommonErrors.SettlementNotFound(req.TargetedSettlementId));
                }

                hero.Status = req.Status;
                // Need to be set manually because it was set to null above and it can confuse EF Core.
                hero.TargetedSettlementId = targetSettlement.Id;
                hero.TargetedSettlement = targetSettlement;
            }

            return Result.NoErrors;
        }
    }
}
