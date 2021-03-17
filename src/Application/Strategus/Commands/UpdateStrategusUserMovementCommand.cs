using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Application.Strategus.Models;
using Crpg.Domain.Entities.Strategus;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Strategus.Commands
{
    public class UpdateStrategusUserMovementCommand : IMediatorRequest<StrategusUserViewModel>
    {
        public int UserId { get; set; }
        public StrategusUserStatus Status { get; set; }
        public MultiPoint Waypoints { get; set; } = MultiPoint.Empty;
        public int TargetedUserId { get; set; }
        public int TargetedSettlementId { get; set; }

        public class Validator : AbstractValidator<UpdateStrategusUserMovementCommand>
        {
            public Validator()
            {
                RuleFor(m => m.Status).IsInEnum();
            }
        }

        internal class Handler : IMediatorRequestHandler<UpdateStrategusUserMovementCommand, StrategusUserViewModel>
        {
            private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpdateStrategusUserMovementCommand>();

            private readonly ICrpgDbContext _db;
            private readonly IMapper _mapper;
            private readonly IStrategusMap _strategusMap;

            public Handler(ICrpgDbContext db, IMapper mapper, IStrategusMap strategusMap)
            {
                _db = db;
                _mapper = mapper;
                _strategusMap = strategusMap;
            }

            public async Task<Result<StrategusUserViewModel>> Handle(UpdateStrategusUserMovementCommand req, CancellationToken cancellationToken)
            {
                var strategusUser = await _db.StrategusUsers
                    .Include(u => u.User)
                    .FirstOrDefaultAsync(u => u.UserId == req.UserId, cancellationToken);
                if (strategusUser == null)
                {
                    return new Result<StrategusUserViewModel>(CommonErrors.UserNotFound(req.UserId));
                }

                if (strategusUser.Status == StrategusUserStatus.InBattle)
                {
                    return new Result<StrategusUserViewModel>(CommonErrors.UserInBattle(req.UserId));
                }

                // Reset movement.
                strategusUser.Status = StrategusUserStatus.Idle;
                strategusUser.Waypoints = MultiPoint.Empty;
                strategusUser.TargetedUserId = null;
                strategusUser.TargetedSettlementId = null;

                if (req.Status == StrategusUserStatus.MovingToPoint)
                {
                    if (!req.Waypoints.IsEmpty)
                    {
                        strategusUser.Status = req.Status;
                        strategusUser.Waypoints = req.Waypoints;
                    }
                }
                else if (req.Status == StrategusUserStatus.FollowingUser
                         || req.Status == StrategusUserStatus.MovingToAttackUser)
                {
                    var targetUser = await _db.StrategusUsers
                        .Include(u => u.User)
                        .FirstOrDefaultAsync(u => u.UserId == req.TargetedUserId, cancellationToken);
                    if (targetUser == null)
                    {
                        return new Result<StrategusUserViewModel>(CommonErrors.UserNotFound(req.TargetedUserId));
                    }

                    if (!strategusUser.Position.IsWithinDistance(targetUser.Position, _strategusMap.ViewDistance))
                    {
                        return new Result<StrategusUserViewModel>(CommonErrors.UserNotInSight(req.TargetedUserId));
                    }

                    strategusUser.Status = req.Status;
                    strategusUser.TargetedUser = targetUser;
                }
                else if (req.Status == StrategusUserStatus.MovingToSettlement
                         || req.Status == StrategusUserStatus.MovingToAttackSettlement)
                {
                    var targetSettlement = await _db.StrategusSettlements
                        .Include(s => s.Owner!.User)
                        .FirstOrDefaultAsync(s => s.Id == req.TargetedSettlementId, cancellationToken);
                    if (targetSettlement == null)
                    {
                        return new Result<StrategusUserViewModel>(CommonErrors.SettlementNotFound(req.TargetedSettlementId));
                    }

                    strategusUser.Status = req.Status;
                    strategusUser.TargetedSettlement = targetSettlement;
                }

                await _db.SaveChangesAsync(cancellationToken);
                Logger.LogInformation("User '{0}' updated their movement on the map", req.UserId);
                return new Result<StrategusUserViewModel>(_mapper.Map<StrategusUserViewModel>(strategusUser));
            }
        }
    }
}
