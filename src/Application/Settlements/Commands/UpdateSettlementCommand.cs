using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Settlements.Models;
using Crpg.Domain.Entities.Heroes;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Settlements.Commands;

public record UpdateSettlementCommand : IMediatorRequest<SettlementPublicViewModel>
{
    public int HeroId { get; init; }
    public int SettlementId { get; init; }
    public int Troops { get; init; }

    public class Validator : AbstractValidator<UpdateSettlementCommand>
    {
        public Validator()
        {
            RuleFor(c => c.Troops).GreaterThanOrEqualTo(0);
        }
    }

    internal class Handler : IMediatorRequestHandler<UpdateSettlementCommand, SettlementPublicViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpdateSettlementCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<SettlementPublicViewModel>> Handle(UpdateSettlementCommand req, CancellationToken cancellationToken)
        {
            var hero = await _db.Heroes
                .Include(h => h.TargetedSettlement)
                .FirstOrDefaultAsync(h => h.Id == req.HeroId, cancellationToken);
            if (hero == null)
            {
                return new(CommonErrors.HeroNotFound(req.HeroId));
            }

            if ((hero.Status != HeroStatus.IdleInSettlement
                 && hero.Status != HeroStatus.RecruitingInSettlement)
                || hero.TargetedSettlementId != req.SettlementId)
            {
                return new(CommonErrors.HeroNotInASettlement(hero.Id));
            }

            int troopsDelta = req.Troops - hero.TargetedSettlement!.Troops;
            if (troopsDelta >= 0) // Hero troops -> settlement troops.
            {
                if (hero.Troops < troopsDelta)
                {
                    return new(CommonErrors.HeroNotEnoughTroops(hero.Id));
                }
            }
            else // Settlement troops -> hero troops.
            {
                if (hero.TargetedSettlement!.OwnerId != hero.Id)
                {
                    return new(CommonErrors.HeroNotSettlementOwner(hero.Id, hero.TargetedSettlementId!.Value));
                }
            }

            hero.TargetedSettlement.Troops += troopsDelta;
            hero.Troops -= troopsDelta;

            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("Hero '{0}' {1} settlement '{2}'", req.HeroId,
                troopsDelta >= 0 ? "gave troops to" : "took troops from", hero.TargetedSettlementId);
            return new(_mapper.Map<SettlementPublicViewModel>(hero.TargetedSettlement));
        }
    }
}
