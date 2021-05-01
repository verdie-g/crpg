using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Strategus.Models;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Strategus;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Strategus.Commands
{
    public record UpdateStrategusSettlementCommand : IMediatorRequest<StrategusSettlementPublicViewModel>
    {
        public int HeroId { get; set; }
        public int SettlementId { get; set; }
        public int Troops { get; set; }

        public class Validator : AbstractValidator<UpdateStrategusSettlementCommand>
        {
            public Validator()
            {
                RuleFor(c => c.Troops).GreaterThanOrEqualTo(0);
            }
        }

        internal class Handler : IMediatorRequestHandler<UpdateStrategusSettlementCommand, StrategusSettlementPublicViewModel>
        {
            private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpdateStrategusSettlementCommand>();

            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;

            public Handler(ICrpgDbContext db, IMapper mapper)
            {
                _db = db;
                _mapper = mapper;
            }

            public async Task<Result<StrategusSettlementPublicViewModel>> Handle(UpdateStrategusSettlementCommand req, CancellationToken cancellationToken)
            {
                var hero = await _db.StrategusHeroes
                    .Include(h => h.TargetedSettlement)
                    .FirstOrDefaultAsync(h => h.Id == req.HeroId, cancellationToken);
                if (hero == null)
                {
                    return new(CommonErrors.HeroNotFound(req.HeroId));
                }

                if ((hero.Status != StrategusHeroStatus.IdleInSettlement
                     && hero.Status != StrategusHeroStatus.RecruitingInSettlement)
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
                return new(_mapper.Map<StrategusSettlementPublicViewModel>(hero.TargetedSettlement));
            }
        }
    }
}
