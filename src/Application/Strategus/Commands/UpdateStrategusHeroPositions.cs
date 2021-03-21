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
    public class UpdateStrategusHeroPositionsCommand : IMediatorRequest
    {
        public TimeSpan DeltaTime { get; set; }

        internal class Handler : IMediatorRequestHandler<UpdateStrategusHeroPositionsCommand>
        {
            private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpdateStrategusHeroPositionsCommand>();

            private static readonly StrategusHeroStatus[] MovementStatuses =
            {
                StrategusHeroStatus.MovingToPoint,
                StrategusHeroStatus.FollowingHero,
                StrategusHeroStatus.MovingToSettlement,
                StrategusHeroStatus.MovingToAttackHero,
                StrategusHeroStatus.MovingToAttackSettlement,
            };

            private readonly ICrpgDbContext _db;
            private readonly IStrategusMap _strategusMap;

            public Handler(ICrpgDbContext db, IStrategusMap strategusMap)
            {
                _db = db;
                _strategusMap = strategusMap;
            }

            public async Task<Result> Handle(UpdateStrategusHeroPositionsCommand req, CancellationToken cancellationToken)
            {
                var strategusHeroes = await _db.StrategusHeroes
                    .AsSplitQuery()
                    .Where(h => MovementStatuses.Contains(h.Status))
                    .Include(h => h.TargetedHero)
                    .Include(h => h.TargetedSettlement)
                    .ToArrayAsync(cancellationToken);

                foreach (var user in strategusHeroes)
                {
                    switch (user.Status)
                    {
                        case StrategusHeroStatus.MovingToPoint:
                            if (user.Waypoints.IsEmpty)
                            {
                                Logger.LogWarning("User '{userId}' was in status '{status}' without any points to go to",
                                    user.Id, user.Status);
                                user.Status = StrategusHeroStatus.Idle;
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
                                    user.Status = StrategusHeroStatus.Idle;
                                }
                            }

                            break;

                        case StrategusHeroStatus.FollowingHero:
                        case StrategusHeroStatus.MovingToAttackHero:
                            if (user.TargetedHero == null)
                            {
                                Logger.LogWarning("User '{userId}' was in status '{status}' without target user",
                                    user.Id, user.Status);
                                user.Status = StrategusHeroStatus.Idle;
                                continue;
                            }

                            if (!user.Position.IsWithinDistance(user.TargetedHero.Position, _strategusMap.ViewDistance))
                            {
                                // Followed user is not in sight anymore. Stop.
                                user.Status = StrategusHeroStatus.Idle;
                                user.TargetedHero = null;
                                continue;
                            }

                            if (user.Status == StrategusHeroStatus.MovingToAttackHero)
                            {
                                if (MoveUserTowardsPoint(user, user.TargetedHero.Position, req.DeltaTime, true))
                                {
                                    // TODO: attack user
                                }
                            }
                            else if (user.Status == StrategusHeroStatus.FollowingHero)
                            {
                                // Set canInteractWithTarget to false to the user doesn't stop to interaction range
                                // but stops on the target user itself.
                                MoveUserTowardsPoint(user, user.TargetedHero.Position, req.DeltaTime, false);
                            }

                            break;
                        case StrategusHeroStatus.MovingToSettlement:
                        case StrategusHeroStatus.MovingToAttackSettlement:
                            if (user.TargetedSettlement == null)
                            {
                                Logger.LogWarning("User '{userId}' was in status '{status}' without target settlement",
                                    user.Id, user.Status);
                                user.Status = StrategusHeroStatus.Idle;
                                continue;
                            }

                            if (MoveUserTowardsPoint(user, user.TargetedSettlement.Position, req.DeltaTime, true))
                            {
                                if (user.Status == StrategusHeroStatus.MovingToSettlement)
                                {
                                    user.Status = StrategusHeroStatus.IdleInSettlement;
                                    user.Position = user.TargetedSettlement.Position;
                                }
                                else if (user.Status == StrategusHeroStatus.MovingToAttackSettlement)
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

            private bool MoveUserTowardsPoint(StrategusHero user, Point targetPoint, TimeSpan deltaTime, bool canInteractWithTarget)
            {
                double speed = ComputeUserSpeed(user);
                double distance = speed * deltaTime.TotalSeconds;
                user.Position = _strategusMap.MovePointTowards(user.Position, targetPoint, distance);
                return canInteractWithTarget
                    ? _strategusMap.ArePointsAtInteractionDistance(user.Position, targetPoint)
                    : _strategusMap.ArePointsEquivalent(user.Position, targetPoint);
            }

            private double ComputeUserSpeed(StrategusHero user)
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
