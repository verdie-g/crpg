using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Characters.Commands;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
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
    public class CreateStrategusUserCommand : IMediatorRequest<StrategusUserViewModel>
    {
        public int UserId { get; set; }
        public Region Region { get; set; }

        public class Validator : AbstractValidator<CreateStrategusUserCommand>
        {
            public Validator()
            {
                RuleFor(su => su.Region).IsInEnum();
            }
        }

        internal class Handler : IMediatorRequestHandler<CreateStrategusUserCommand, StrategusUserViewModel>
        {
            private static readonly ILogger Logger = LoggerFactory.CreateLogger<CreateStrategusUserCommand>();

            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;
            private readonly Constants _constants;

            public Handler(ICrpgDbContext db, IMapper mapper, Constants constants)
            {
                _db = db;
                _mapper = mapper;
                _constants = constants;
            }

            public async Task<Result<StrategusUserViewModel>> Handle(CreateStrategusUserCommand req, CancellationToken cancellationToken)
            {
                var user = await _db.Users
                    .Include(u => u.StrategusUser)
                    .FirstOrDefaultAsync(u => u.Id == req.UserId, cancellationToken);
                if (user == null)
                {
                    return new Result<StrategusUserViewModel>(CommonErrors.UserNotFound(req.UserId));
                }

                if (user.StrategusUser != null)
                {
                    return new Result<StrategusUserViewModel>(CommonErrors.UserAlreadyRegisteredToStrategus(req.UserId));
                }

                user.StrategusUser = new StrategusUser
                {
                    Region = req.Region,
                    Silver = 0,
                    Troops = 0,
                    Position = StartPositionFromRegion(req.Region),
                    Moves = new MultiPoint(Array.Empty<Point>()),
                };

                await _db.SaveChangesAsync(cancellationToken);
                Logger.LogInformation("User '{0}' registered to Strategus", req.UserId);
                return new Result<StrategusUserViewModel>(_mapper.Map<StrategusUserViewModel>(user.StrategusUser));
            }

            private Point StartPositionFromRegion(Region region)
            {
                return region switch
                {
                    Region.Europe => new Point(_constants.StrategusMapWidth / 2.0, _constants.StrategusMapHeight / 2.0),
                    Region.NorthAmerica => new Point(_constants.StrategusMapWidth + _constants.StrategusMapWidth / 2.0, _constants.StrategusMapHeight / 2.0),
                    Region.Asia => new Point(2 * _constants.StrategusMapWidth + _constants.StrategusMapWidth / 2.0, _constants.StrategusMapHeight / 2.0),
                    _ => throw new ArgumentOutOfRangeException(nameof(region), region, null),
                };
            }
        }
    }
}
