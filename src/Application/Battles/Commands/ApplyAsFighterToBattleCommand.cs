using AutoMapper;
using AutoMapper.QueryableExtensions;
using Crpg.Application.Battles.Models;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Parties.Commands;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Parties;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Battles.Commands;

public record ApplyAsFighterToBattleCommand : IMediatorRequest<BattleFighterApplicationViewModel>
{
    public int PartyId { get; init; }
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
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<BuySettlementItemCommand>();

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
            var party = await _db.Parties
                .FirstOrDefaultAsync(h => h.Id == req.PartyId, cancellationToken);
            if (party == null)
            {
                return new(CommonErrors.PartyNotFound(req.PartyId));
            }

            if (party.Status == PartyStatus.InBattle)
            {
                return new(CommonErrors.PartyInBattle(req.PartyId));
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

            if (!_strategusMap.ArePointsAtInteractionDistance(party.Position, battle.Position))
            {
                return new(CommonErrors.BattleTooFar(req.BattleId));
            }

            var existingPendingApplication = await _db.BattleFighterApplications
                .Include(a => a.Party)
                .Where(a => a.PartyId == req.PartyId && a.BattleId == req.BattleId
                                                   && a.Side == req.Side
                                                   && (a.Status == BattleFighterApplicationStatus.Pending
                                                       || a.Status == BattleFighterApplicationStatus.Accepted))
                .ProjectTo<BattleFighterApplicationViewModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(cancellationToken);
            if (existingPendingApplication != null)
            {
                return new(existingPendingApplication);
            }

            BattleFighterApplication application = new()
            {
                Side = req.Side,
                Status = BattleFighterApplicationStatus.Pending,
                Battle = battle,
                Party = party,
            };
            _db.BattleFighterApplications.Add(application);
            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("Party '{0}' applied as fighter to battle '{1}'", req.PartyId, req.BattleId);
            return new(_mapper.Map<BattleFighterApplicationViewModel>(application));
        }
    }
}
