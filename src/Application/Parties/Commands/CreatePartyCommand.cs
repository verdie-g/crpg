using AutoMapper;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Parties.Models;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Parties;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Parties.Commands;

public record CreatePartyCommand : IMediatorRequest<PartyViewModel>
{
    public int UserId { get; set; }
    public Region Region { get; init; }

    public class Validator : AbstractValidator<CreatePartyCommand>
    {
        public Validator()
        {
            RuleFor(su => su.Region).IsInEnum();
        }
    }

    internal class Handler : IMediatorRequestHandler<CreatePartyCommand, PartyViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<CreatePartyCommand>();

        private readonly ICrpgDbContext _db;
        private readonly IMapper _mapper;
        private readonly IStrategusMap _strategusMap;
        private readonly Constants _constants;

        public Handler(ICrpgDbContext db, IMapper mapper, IStrategusMap strategusMap, Constants constants)
        {
            _db = db;
            _mapper = mapper;
            _strategusMap = strategusMap;
            _constants = constants;
        }

        public async Task<Result<PartyViewModel>> Handle(CreatePartyCommand req, CancellationToken cancellationToken)
        {
            var user = await _db.Users
                .Include(u => u.Party)
                .FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);
            if (user == null)
            {
                return new(CommonErrors.UserNotFound(req.UserId));
            }

            if (user.Party != null)
            {
                return new(CommonErrors.UserAlreadyRegisteredToStrategus(req.UserId));
            }

            user.Party = new Party
            {
                Region = req.Region,
                Gold = 0,
                Troops = _constants.StrategusMinPartyTroops,
                Position = _strategusMap.GetSpawnPosition(req.Region),
                Status = PartyStatus.Idle,
                Waypoints = MultiPoint.Empty,
                TargetedPartyId = null,
                TargetedSettlementId = null,
            };

            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("User '{0}' registered to Strategus", req.UserId);
            return new(_mapper.Map<PartyViewModel>(user.Party));
        }
    }
}
