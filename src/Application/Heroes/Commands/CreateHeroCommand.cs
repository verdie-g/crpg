using AutoMapper;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Heroes.Models;
using Crpg.Domain.Entities;
using Crpg.Domain.Entities.Heroes;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Heroes.Commands;

public record CreateHeroCommand : IMediatorRequest<HeroViewModel>
{
    public int UserId { get; set; }
    public Region Region { get; init; }

    public class Validator : AbstractValidator<CreateHeroCommand>
    {
        public Validator()
        {
            RuleFor(su => su.Region).IsInEnum();
        }
    }

    internal class Handler : IMediatorRequestHandler<CreateHeroCommand, HeroViewModel>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<CreateHeroCommand>();

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

        public async Task<Result<HeroViewModel>> Handle(CreateHeroCommand req, CancellationToken cancellationToken)
        {
            var user = await _db.Users
                .Include(u => u.Hero)
                .FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);
            if (user == null)
            {
                return new(CommonErrors.UserNotFound(req.UserId));
            }

            if (user.Hero != null)
            {
                return new(CommonErrors.UserAlreadyRegisteredToStrategus(req.UserId));
            }

            user.Hero = new Hero
            {
                Region = req.Region,
                Gold = 0,
                Troops = _constants.StrategusMinHeroTroops,
                Position = _strategusMap.GetSpawnPosition(req.Region),
                Status = HeroStatus.Idle,
                Waypoints = MultiPoint.Empty,
                TargetedHeroId = null,
                TargetedSettlementId = null,
            };

            await _db.SaveChangesAsync(cancellationToken);
            Logger.LogInformation("User '{0}' registered to Strategus", req.UserId);
            return new(_mapper.Map<HeroViewModel>(user.Hero));
        }
    }
}
