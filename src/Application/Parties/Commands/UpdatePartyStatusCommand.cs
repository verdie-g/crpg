using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Parties.Models;
using Crpg.Domain.Entities.Parties;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Parties.Commands;

public record UpdatePartyStatusCommand : IMediatorRequest<PartyViewModel>
{
    public int PartyId { get; set; }
    public PartyStatus Status { get; init; }
    public MultiPoint Waypoints { get; init; } = MultiPoint.Empty;
    public int TargetedPartyId { get; init; }
    public int TargetedSettlementId { get; init; }

    public class Validator : AbstractValidator<UpdatePartyStatusCommand>
    {
        public Validator()
        {
            RuleFor(m => m.Status).IsInEnum();
        }
    }

    internal class Handler : IMediatorRequestHandler<UpdatePartyStatusCommand, PartyViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpdatePartyStatusCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly IStrategusMap _strategusMap;

        public Handler(ICrpgDbContext db, IMapper mapper, IStrategusMap strategusMap)
        {
            _db = db;
            _mapper = mapper;
            _strategusMap = strategusMap;
        }

        public async Task<Result<PartyViewModel>> Handle(UpdatePartyStatusCommand req, CancellationToken cancellationToken)
        {
            var party = await _db.Parties
                .Include(h => h.User)
                .Include(h => h.TargetedSettlement)
                .FirstOrDefaultAsync(h => h.Id == req.PartyId, cancellationToken);
            if (party == null)
            {
                return new(CommonErrors.PartyNotFound(req.PartyId));
            }

            if (party.Status == PartyStatus.InBattle)
            {
                return new(CommonErrors.PartyInBattle(req.PartyId));
            }

            if (req.Status == PartyStatus.IdleInSettlement || req.Status == PartyStatus.RecruitingInSettlement)
            {
                var result = StartStopRecruiting(req.Status == PartyStatus.RecruitingInSettlement, party);
                if (result.Errors != null)
                {
                    return new(result.Errors);
                }
            }
            else
            {
                var result = await UpdatePartyMovement(party, req, cancellationToken);
                if (result.Errors != null)
                {
                    return new(result.Errors);
                }
            }

            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("Party '{0}' updated their status", req.PartyId);
            return new(_mapper.Map<PartyViewModel>(party));
        }

        private Result StartStopRecruiting(bool start, Party party)
        {
            if (start)
            {
                if (party.Status != PartyStatus.IdleInSettlement)
                {
                    return new(CommonErrors.PartyNotInASettlement(party.Id));
                }
            }
            else
            {
                if (party.Status != PartyStatus.RecruitingInSettlement)
                {
                    return new(CommonErrors.PartyNotInASettlement(party.Id));
                }
            }

            party.Status = start ? PartyStatus.RecruitingInSettlement : PartyStatus.IdleInSettlement;
            return Result.NoErrors;
        }

        private async Task<Result> UpdatePartyMovement(Party party, UpdatePartyStatusCommand req,
            CancellationToken cancellationToken)
        {
            // Reset movement.
            party.Status = PartyStatus.Idle;
            party.Waypoints = MultiPoint.Empty;
            party.TargetedPartyId = null;
            party.TargetedSettlementId = null;

            if (req.Status == PartyStatus.MovingToPoint)
            {
                if (!req.Waypoints.IsEmpty)
                {
                    party.Status = req.Status;
                    party.Waypoints = req.Waypoints;
                }
            }
            else if (req.Status == PartyStatus.FollowingParty
                     || req.Status == PartyStatus.MovingToAttackParty)
            {
                var targetParty = await _db.Parties
                    .Include(h => h.User)
                    .FirstOrDefaultAsync(h => h.Id == req.TargetedPartyId, cancellationToken);
                if (targetParty == null)
                {
                    return new Result(CommonErrors.UserNotFound(req.TargetedPartyId));
                }

                if (!party.Position.IsWithinDistance(targetParty.Position, _strategusMap.ViewDistance))
                {
                    return new Result(CommonErrors.PartyNotInSight(req.TargetedPartyId));
                }

                party.Status = req.Status;
                // Need to be set manually because it was set to null above and it can confuse EF Core.
                party.TargetedPartyId = targetParty.Id;
                party.TargetedParty = targetParty;
            }
            else if (req.Status == PartyStatus.MovingToSettlement
                     || req.Status == PartyStatus.MovingToAttackSettlement)
            {
                var targetSettlement = await _db.Settlements
                    .Include(s => s.Owner!.User)
                    .FirstOrDefaultAsync(s => s.Id == req.TargetedSettlementId, cancellationToken);
                if (targetSettlement == null)
                {
                    return new Result(CommonErrors.SettlementNotFound(req.TargetedSettlementId));
                }

                party.Status = req.Status;
                // Need to be set manually because it was set to null above and it can confuse EF Core.
                party.TargetedSettlementId = targetSettlement.Id;
                party.TargetedSettlement = targetSettlement;
            }

            return Result.NoErrors;
        }
    }
}
