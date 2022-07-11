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
    public int PartyId { get; init; }
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
            var party = await _db.Parties.FirstOrDefaultAsync(h => h.Id == req.PartyId, cancellationToken);
            if (party == null)
            {
                return new(CommonErrors.PartyNotFound(req.PartyId));
            }

            var application = await _db.BattleFighterApplications
                .AsSplitQuery()
                .Include(a => a.Battle!).ThenInclude(b => b.Fighters.Where(f => f.PartyId == req.PartyId))
                .Include(a => a.Party!).ThenInclude(h => h.User)
                .FirstOrDefaultAsync(a => a.Id == req.FighterApplicationId, cancellationToken);
            if (application == null)
            {
                return new(CommonErrors.ApplicationNotFound(req.FighterApplicationId));
            }

            var partyFighter = application.Battle!.Fighters.FirstOrDefault();
            if (partyFighter == null)
            {
                return new(CommonErrors.PartyNotAFighter(req.PartyId, application.BattleId));
            }

            if (!partyFighter.Commander)
            {
                return new(CommonErrors.FighterNotACommander(req.PartyId, application.BattleId));
            }

            if (partyFighter.Side != application.Side)
            {
                return new(CommonErrors.PartiesNotOnTheSameSide(partyFighter.Id, application.PartyId,
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
                    Party = application.Party,
                    Battle = application.Battle,
                };
                _db.BattleFighters.Add(newFighter);

                // Delete all other applying party pending applications for this battle.
                var otherApplications = await _db.BattleFighterApplications
                    .Where(a => a.Id != application.Id
                                && a.BattleId == application.BattleId
                                && a.PartyId == application.PartyId
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
                "Party '{0}' {1} application '{2}' from party '{3}' to join battle '{4}' as a fighter",
                req.PartyId, req.Accept ? "accepted" : "declined", req.FighterApplicationId,
                application.PartyId, application.BattleId);
            return new(_mapper.Map<BattleFighterApplicationViewModel>(application));
        }
    }
}
