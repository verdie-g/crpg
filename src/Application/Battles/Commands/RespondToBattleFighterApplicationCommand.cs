using AutoMapper;
using Crpg.Application.Battles.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Domain.Entities.Battles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Battles.Commands;

public record RespondToBattleFighterApplicationCommand : IMediatorRequest<BattleFighterApplicationViewModel>
{
    public int HeroId { get; init; }
    public int FighterApplicationId { get; init; }
    public bool Accept { get; init; }

    internal class Handler : IMediatorRequestHandler<RespondToBattleFighterApplicationCommand, BattleFighterApplicationViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<RespondToBattleFighterApplicationCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;

        public Handler(ICrpgDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<Result<BattleFighterApplicationViewModel>> Handle(RespondToBattleFighterApplicationCommand req,
            CancellationToken cancellationToken)
        {
            var hero = await _db.Heroes.FirstOrDefaultAsync(h => h.Id == req.HeroId, cancellationToken);
            if (hero == null)
            {
                return new(CommonErrors.HeroNotFound(req.HeroId));
            }

            var application = await _db.BattleFighterApplications
                .AsSplitQuery()
                .Include(a => a.Battle!).ThenInclude(b => b.Fighters.Where(f => f.HeroId == req.HeroId))
                .Include(a => a.Hero!).ThenInclude(h => h.User)
                .FirstOrDefaultAsync(a => a.Id == req.FighterApplicationId, cancellationToken);
            if (application == null)
            {
                return new(CommonErrors.ApplicationNotFound(req.FighterApplicationId));
            }

            var heroFighter = application.Battle!.Fighters.FirstOrDefault();
            if (heroFighter == null)
            {
                return new(CommonErrors.HeroNotAFighter(req.HeroId, application.BattleId));
            }

            if (!heroFighter.Commander)
            {
                return new(CommonErrors.FighterNotACommander(req.HeroId, application.BattleId));
            }

            if (heroFighter.Side != application.Side)
            {
                return new(CommonErrors.HeroesNotOnTheSameSide(heroFighter.Id, application.HeroId,
                    application.BattleId));
            }

            if (application.Battle.Phase != BattlePhase.Preparation)
            {
                return new(CommonErrors.BattleInvalidPhase(application.BattleId, application.Battle.Phase));
            }

            if (application.Status != BattleFighterApplicationStatus.Pending)
            {
                return new(CommonErrors.ApplicationClosed(application.Id));
            }

            if (req.Accept)
            {
                application.Status = BattleFighterApplicationStatus.Accepted;
                BattleFighter newFighter = new()
                {
                    Side = application.Side,
                    Commander = false,
                    MercenarySlots = 0,
                    Hero = application.Hero,
                    Battle = application.Battle,
                };
                _db.BattleFighters.Add(newFighter);

                // Delete all other applying hero pending applications for this battle.
                var otherApplications = await _db.BattleFighterApplications
                    .Where(a => a.Id != application.Id
                                && a.BattleId == application.BattleId
                                && a.HeroId == application.HeroId
                                && a.Status == BattleFighterApplicationStatus.Pending)
                    .ToArrayAsync(cancellationToken);
                _db.BattleFighterApplications.RemoveRange(otherApplications);
            }
            else
            {
                application.Status = BattleFighterApplicationStatus.Declined;
            }

            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation(
                "Hero '{0}' {1} application '{2}' from hero '{3}' to join battle '{4}' as a fighter",
                req.HeroId, req.Accept ? "accepted" : "declined", req.FighterApplicationId,
                application.HeroId, application.BattleId);
            return new(_mapper.Map<BattleFighterApplicationViewModel>(application));
        }
    }
}
