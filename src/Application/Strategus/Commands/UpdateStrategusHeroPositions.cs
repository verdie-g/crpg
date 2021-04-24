using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Strategus;
using Crpg.Domain.Entities.Strategus.Battles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Strategus.Commands
{
    public record UpdateStrategusHeroPositionsCommand : IMediatorRequest
    {
        public TimeSpan DeltaTime { get; init; }

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

            private static readonly StrategusHeroStatus[] UnattackableStatuses =
            {
                StrategusHeroStatus.IdleInSettlement,
                StrategusHeroStatus.RecruitingInSettlement,
                StrategusHeroStatus.InBattle,
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

                foreach (var hero in strategusHeroes)
                {
                    switch (hero.Status)
                    {
                        case StrategusHeroStatus.MovingToPoint:
                            MoveToPoint(req.DeltaTime, hero);
                            break;
                        case StrategusHeroStatus.FollowingHero:
                        case StrategusHeroStatus.MovingToAttackHero:
                            MoveToHero(req.DeltaTime, hero);
                            break;
                        case StrategusHeroStatus.MovingToSettlement:
                        case StrategusHeroStatus.MovingToAttackSettlement:
                            await MoveToSettlement(req.DeltaTime, hero, cancellationToken);
                            break;
                    }
                }

                await _db.SaveChangesAsync(cancellationToken);
                return Result.NoErrors;
            }

            private void MoveToPoint(TimeSpan deltaTime, StrategusHero hero)
            {
                if (hero.Waypoints.IsEmpty)
                {
                    Logger.LogWarning("Hero '{heroId}' was in status '{status}' without any points to go to",
                        hero.Id, hero.Status);
                    hero.Status = StrategusHeroStatus.Idle;
                    return;
                }

                var targetPoint = (Point)hero.Waypoints[0];
                if (!MoveHeroTowardsPoint(hero, targetPoint, deltaTime, false))
                {
                    return;
                }

                // Skip 2 instead of 1 because for some reason MultiPoint.GetEnumerator returns an
                // enumerator containing itself (https://github.com/NetTopologySuite/NetTopologySuite/issues/508).
                hero.Waypoints = new MultiPoint(hero.Waypoints.Skip(2).Cast<Point>().ToArray());
                if (hero.Waypoints.Count == 0)
                {
                    hero.Status = StrategusHeroStatus.Idle;
                }
            }

            private void MoveToHero(TimeSpan deltaTime, StrategusHero hero)
            {
                if (hero.TargetedHero == null)
                {
                    Logger.LogWarning("Hero '{heroId}' was in status '{status}' without target hero",
                        hero.Id, hero.Status);
                    hero.Status = StrategusHeroStatus.Idle;
                    return;
                }

                if (!hero.Position.IsWithinDistance(hero.TargetedHero.Position, _strategusMap.ViewDistance))
                {
                    // Followed hero is not in sight anymore. Stop.
                    hero.Status = StrategusHeroStatus.Idle;
                    hero.TargetedHero = null;
                    return;
                }

                if (hero.Status == StrategusHeroStatus.FollowingHero)
                {
                    // Set canInteractWithTarget to false to the hero doesn't stop to interaction range
                    // but stops on the target hero itself.
                    MoveHeroTowardsPoint(hero, hero.TargetedHero.Position, deltaTime, false);
                }
                else if (hero.Status == StrategusHeroStatus.MovingToAttackHero)
                {
                    if (!MoveHeroTowardsPoint(hero, hero.TargetedHero.Position, deltaTime, true))
                    {
                        return;
                    }

                    if (UnattackableStatuses.Contains(hero.TargetedHero.Status))
                    {
                        return;
                    }

                    hero.Status = StrategusHeroStatus.InBattle;
                    hero.TargetedHero.Status = StrategusHeroStatus.InBattle;
                    StrategusBattle battle = new()
                    {
                        Phase = StrategusBattlePhase.Preparation,
                        Position = GetMidPoint(hero.Position, hero.TargetedHero.Position),
                        Fighters =
                        {
                            new StrategusBattleFighter
                            {
                                Hero = hero,
                                Side = StrategusBattleSide.Attacker,
                                MainFighter = true,
                            },
                            new StrategusBattleFighter
                            {
                                Hero = hero.TargetedHero,
                                Side = StrategusBattleSide.Defender,
                                MainFighter = true,
                            },
                        },
                    };
                    _db.StrategusBattles.Add(battle);
                    Logger.LogInformation("Hero '{0}' initiated a battle against hero '{1}'",
                        hero.Id, hero.TargetedHeroId);
                }
            }

            private async Task MoveToSettlement(TimeSpan deltaTime, StrategusHero hero, CancellationToken cancellationToken)
            {
                if (hero.TargetedSettlement == null)
                {
                    Logger.LogWarning("Hero '{heroId}' was in status '{status}' without target settlement",
                        hero.Id, hero.Status);
                    hero.Status = StrategusHeroStatus.Idle;
                    return;
                }

                if (!MoveHeroTowardsPoint(hero, hero.TargetedSettlement.Position, deltaTime, true))
                {
                    return;
                }

                if (hero.Status == StrategusHeroStatus.MovingToSettlement)
                {
                    hero.Status = StrategusHeroStatus.IdleInSettlement;
                    hero.Position = hero.TargetedSettlement.Position;
                }
                else if (hero.Status == StrategusHeroStatus.MovingToAttackSettlement)
                {
                    bool attackInProgress = await _db.StrategusBattles
                        .AnyAsync(b =>
                                b.AttackedSettlementId == hero.TargetedSettlementId
                                && b.Phase != StrategusBattlePhase.End,
                            cancellationToken);
                    if (attackInProgress)
                    {
                        return;
                    }

                    hero.Status = StrategusHeroStatus.InBattle;
                    var battle = new StrategusBattle
                    {
                        Phase = StrategusBattlePhase.Preparation,
                        Position = GetMidPoint(hero.Position, hero.TargetedSettlement.Position),
                        AttackedSettlement = hero.TargetedSettlement,
                        Fighters =
                        {
                            new StrategusBattleFighter
                            {
                                Hero = hero,
                                Side = StrategusBattleSide.Attacker,
                                MainFighter = true,
                            },
                        },
                    };
                    _db.StrategusBattles.Add(battle);
                    Logger.LogInformation("Hero '{0}' initiated a battle against settlement '{1}'",
                        hero.Id, hero.TargetedSettlementId);
                }
            }

            private bool MoveHeroTowardsPoint(StrategusHero hero, Point targetPoint, TimeSpan deltaTime, bool canInteractWithTarget)
            {
                double speed = ComputeHeroSpeed(hero);
                double distance = speed * deltaTime.TotalSeconds;
                hero.Position = _strategusMap.MovePointTowards(hero.Position, targetPoint, distance);
                return canInteractWithTarget
                    ? _strategusMap.ArePointsAtInteractionDistance(hero.Position, targetPoint)
                    : _strategusMap.ArePointsEquivalent(hero.Position, targetPoint);
            }

            private double ComputeHeroSpeed(StrategusHero hero)
            {
                const double terrainSpeedFactor = 1.0; // TODO: terrain penalty on speed (e.g. lower speed in forest).
                const double weightFactor = 1.0; // TODO: goods should slow down hero
                const double horsesFactor = 1.0; // TODO: horse should speed up hero
                const double terrainTroopInfluence = 1.0; // TODO: terrain influence on speed depending on the number of troops.

                double troopInfluence = Math.Min(0.99, Math.Pow(hero.Troops, terrainTroopInfluence) / 100);
                return terrainSpeedFactor * weightFactor * horsesFactor * (1 - troopInfluence);
            }

            private Point GetMidPoint(Point pointA, Point pointB)
            {
                return new((pointA.X + pointB.X) / 2, (pointA.Y + pointB.Y) / 2);
            }
        }
    }
}
