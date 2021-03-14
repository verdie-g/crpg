using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Strategus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Strategus.Commands
{
    public class UpdateStrategusUserPositionsCommand : IMediatorRequest
    {
        public TimeSpan DeltaTime { get; set; }

        internal class Handler : IMediatorRequestHandler<UpdateStrategusUserPositionsCommand>
        {
            private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpdateStrategusUserPositionsCommand>();

            private static readonly StrategusUserStatus[] MovementStatuses =
            {
                StrategusUserStatus.MovingToPoint,
                StrategusUserStatus.FollowingUser,
                StrategusUserStatus.MovingToSettlement,
                StrategusUserStatus.MovingToAttackUser,
                StrategusUserStatus.MovingToAttackSettlement,
            };

            private readonly ICrpgDbContext _db;
            private readonly IStrategusMap _strategusMap;

            public Handler(ICrpgDbContext db, IStrategusMap strategusMap)
            {
                _db = db;
                _strategusMap = strategusMap;
            }

            public async Task<Result> Handle(UpdateStrategusUserPositionsCommand req, CancellationToken cancellationToken)
            {
                var strategusUsers = await _db.StrategusUsers
                    .AsSplitQuery()
                    .Where(u => MovementStatuses.Contains(u.Status))
                    .Include(u => u.TargetedUser)
                    .Include(u => u.TargetedSettlement)
                    .ToArrayAsync(cancellationToken);

                foreach (var user in strategusUsers)
                {
                    switch (user.Status)
                    {
                        case StrategusUserStatus.MovingToPoint:
                            if (user.Waypoints.IsEmpty)
                            {
                                Logger.LogWarning("User '{userId}' was in status '{status}' without any points to go to",
                                    user.UserId, user.Status);
                                user.Status = StrategusUserStatus.Idle;
                                continue;
                            }

                            var targetPoint = (Point)user.Waypoints[0];
                            if (MoveUserTowardsPoint(user, targetPoint, req.DeltaTime, false))
                            {
                                // Skip 1 instead of 2 because for some reason MultiPoint.GetEnumerator returns an
                                // enumerator containing itself (https://github.com/NetTopologySuite/NetTopologySuite/issues/508).
                                user.Waypoints = new MultiPoint(user.Waypoints.Skip(2).Cast<Point>().ToArray());
                                if (user.Waypoints.Count == 0)
                                {
                                    user.Status = StrategusUserStatus.Idle;
                                }
                            }

                            break;

                        case StrategusUserStatus.FollowingUser:
                        case StrategusUserStatus.MovingToAttackUser:
                            if (user.TargetedUser == null)
                            {
                                Logger.LogWarning("User '{userId}' was in status '{status}' without target user",
                                    user.UserId, user.Status);
                                user.Status = StrategusUserStatus.Idle;
                                continue;
                            }

                            if (MoveUserTowardsPoint(user, user.TargetedUser.Position, req.DeltaTime, true)
                                && user.Status == StrategusUserStatus.MovingToAttackUser)
                            {
                                // TODO: attack user
                            }

                            break;
                        case StrategusUserStatus.MovingToSettlement:
                        case StrategusUserStatus.MovingToAttackSettlement:
                            if (user.TargetedSettlement == null)
                            {
                                Logger.LogWarning("User '{userId}' was in status '{status}' without target settlement",
                                    user.UserId, user.Status);
                                user.Status = StrategusUserStatus.Idle;
                                continue;
                            }

                            if (MoveUserTowardsPoint(user, user.TargetedSettlement.Position, req.DeltaTime, true))
                            {
                                if (user.Status == StrategusUserStatus.MovingToSettlement)
                                {
                                    user.Status = StrategusUserStatus.IdleInSettlement;
                                    user.Position = user.TargetedSettlement.Position;
                                }
                                else if (user.Status == StrategusUserStatus.MovingToAttackSettlement)
                                {
                                    // TODO: attack settlement.
                                }
                            }

                            break;
                    }
                }

                await _db.SaveChangesAsync(cancellationToken);
                return Result.NoErrors;
            }

            private bool MoveUserTowardsPoint(StrategusUser user, Point targetPoint, TimeSpan deltaTime, bool canInteractWithTarget)
            {
                double speed = ComputeUserSpeed(user);
                double distance = speed * deltaTime.TotalSeconds;
                user.Position = _strategusMap.MovePointTowards(user.Position, targetPoint, distance);
                return canInteractWithTarget
                    ? _strategusMap.ArePointsAtInteractionDistance(user.Position, targetPoint)
                    : _strategusMap.ArePointsEquivalent(user.Position, targetPoint);
            }

            private double ComputeUserSpeed(StrategusUser user)
            {
                const double terrainSpeedFactor = 1.0; // TODO: terrain penalty on speed (e.g. lower speed in forest).
                const double weightFactor = 1.0; // TODO: goods should slow down user
                const double horsesFactor = 1.0; // TODO: horse should speed user user
                const double terrainTroopInfluence = 1.0; // TODO: terrain influence on speed depending on the number of troops.

                double troopInfluence = Math.Min(0.99, Math.Pow(user.Troops, terrainTroopInfluence) / 100);
                return terrainSpeedFactor * weightFactor * horsesFactor * (1 - troopInfluence);
            }
        }
    }
}
