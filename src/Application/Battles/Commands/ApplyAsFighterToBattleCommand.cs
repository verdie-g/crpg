using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Battles.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Heroes.Commands;
using Crpg.Application.Strategus.Models;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Heroes;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Battles.Commands
{
    public record ApplyAsFighterToBattleCommand : IMediatorRequest<BattleFighterApplicationViewModel>
    {
        public int HeroId { get; init; }
        public int BattleId { get; init; }
        public BattleSide Side { get; init; }

        public class Validator : AbstractValidator<ApplyAsFighterToBattleCommand>
        {
            public Validator()
            {
                RuleFor(a => a.Side).IsInEnum();
            }
        }

        internal class Handler : IMediatorRequestHandler<ApplyAsFighterToBattleCommand, BattleFighterApplicationViewModel>
        {
            private static readonly ILogger Logger = LoggerFactory.CreateLogger<BuyItemCommand>();

            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;
            private readonly IStrategusMap _strategusMap;

            public Handler(ICrpgDbContext db, IMapper mapper, IStrategusMap strategusMap)
            {
                _db = db;
                _mapper = mapper;
                _strategusMap = strategusMap;
            }

            public async Task<Result<BattleFighterApplicationViewModel>> Handle(
                ApplyAsFighterToBattleCommand req,
                CancellationToken cancellationToken)
            {
                var hero = await _db.Heroes
                    .FirstOrDefaultAsync(h => h.Id == req.HeroId, cancellationToken);
                if (hero == null)
                {
                    return new(CommonErrors.HeroNotFound(req.HeroId));
                }

                if (hero.Status == HeroStatus.InBattle)
                {
                    return new(CommonErrors.HeroInBattle(req.HeroId));
                }

                var battle = await _db.Battles
                    .Include(b => b.Fighters)
                    .FirstOrDefaultAsync(b => b.Id == req.BattleId, cancellationToken);
                if (battle == null)
                {
                    return new(CommonErrors.BattleNotFound(req.BattleId));
                }

                if (battle.Phase != BattlePhase.Preparation)
                {
                    return new(CommonErrors.BattleInvalidPhase(req.BattleId, battle.Phase));
                }

                if (!_strategusMap.ArePointsAtInteractionDistance(hero.Position, battle.Position))
                {
                    return new(CommonErrors.BattleTooFar(req.BattleId));
                }

                var existingPendingApplication = await _db.BattleFighterApplications
                    .Include(a => a.Hero)
                    .Where(a => a.HeroId == req.HeroId && a.BattleId == req.BattleId
                                                       && a.Side == req.Side
                                                       && (a.Status == BattleFighterApplicationStatus.Pending
                                                       || a.Status == BattleFighterApplicationStatus.Accepted))
                    .ProjectTo<BattleFighterApplicationViewModel>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(cancellationToken);
                if (existingPendingApplication != null)
                {
                    return new(existingPendingApplication);
                }

                FighterApplication application = new()
                {
                    Side = req.Side,
                    Status = BattleFighterApplicationStatus.Pending,
                    Battle = battle,
                    Hero = hero,
                };
                _db.BattleFighterApplications.Add(application);
                await _db.SaveChangesAsync(cancellationToken);
                Logger.LogInformation("Hero '{0}' applied as fighter to battle '{1}'", req.HeroId, req.BattleId);
                return new(_mapper.Map<BattleFighterApplicationViewModel>(application));
            }
        }
    }
}
