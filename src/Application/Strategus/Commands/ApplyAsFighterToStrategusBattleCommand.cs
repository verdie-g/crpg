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
using Crpg.Domain.Entities.Strategus.Battles;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Strategus.Commands
{
    public record ApplyAsFighterToStrategusBattleCommand : IMediatorRequest<StrategusBattleFighterApplicationViewModel>
    {
        public int HeroId { get; init; }
        public int BattleId { get; init; }
        public StrategusBattleSide Side { get; init; }

        public class Validator : AbstractValidator<ApplyAsFighterToStrategusBattleCommand>
        {
            public Validator()
            {
                RuleFor(a => a.Side).IsInEnum();
            }
        }

        internal class Handler : IMediatorRequestHandler<ApplyAsFighterToStrategusBattleCommand, StrategusBattleFighterApplicationViewModel>
        {
            private static readonly ILogger Logger = LoggerFactory.CreateLogger<BuyStrategusItemCommand>();

            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;
            private readonly IStrategusMap _strategusMap;

            public Handler(ICrpgDbContext db, IMapper mapper, IStrategusMap strategusMap)
            {
                _db = db;
                _mapper = mapper;
                _strategusMap = strategusMap;
            }

            public async Task<Result<StrategusBattleFighterApplicationViewModel>> Handle(
                ApplyAsFighterToStrategusBattleCommand req,
                CancellationToken cancellationToken)
            {
                var hero = await _db.StrategusHeroes
                    .FirstOrDefaultAsync(h => h.Id == req.HeroId, cancellationToken);
                if (hero == null)
                {
                    return new(CommonErrors.HeroNotFound(req.HeroId));
                }

                if (hero.Status == StrategusHeroStatus.InBattle)
                {
                    return new(CommonErrors.HeroInBattle(req.HeroId));
                }

                var battle = await _db.StrategusBattles
                    .Include(b => b.Fighters)
                    .FirstOrDefaultAsync(b => b.Id == req.BattleId, cancellationToken);
                if (battle == null)
                {
                    return new(CommonErrors.BattleNotFound(req.BattleId));
                }

                if (battle.Phase != StrategusBattlePhase.Preparation)
                {
                    return new(CommonErrors.BattleInvalidPhase(req.BattleId, battle.Phase));
                }

                if (!_strategusMap.ArePointsAtInteractionDistance(hero.Position, battle.Position))
                {
                    return new(CommonErrors.BattleTooFar(req.BattleId));
                }

                var existingPendingApplication = await _db.StrategusBattleFighterApplications
                    .Include(a => a.Hero)
                    .Where(a => a.HeroId == req.HeroId && a.BattleId == req.BattleId
                                                       && a.Side == req.Side
                                                       && (a.Status == StrategusBattleFighterApplicationStatus.Pending
                                                       || a.Status == StrategusBattleFighterApplicationStatus.Accepted))
                    .ProjectTo<StrategusBattleFighterApplicationViewModel>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(cancellationToken);
                if (existingPendingApplication != null)
                {
                    return new(existingPendingApplication);
                }

                StrategusBattleFighterApplication application = new()
                {
                    Side = req.Side,
                    Status = StrategusBattleFighterApplicationStatus.Pending,
                    Battle = battle,
                    Hero = hero,
                };
                _db.StrategusBattleFighterApplications.Add(application);
                await _db.SaveChangesAsync(cancellationToken);
                Logger.LogInformation("Hero '{0}' applied as fighter to battle '{1}'", req.HeroId, req.BattleId);
                return new(_mapper.Map<StrategusBattleFighterApplicationViewModel>(application));
            }
        }
    }
}
