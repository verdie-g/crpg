using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Heroes;
using Crpg.Domain.Entities.Items;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Heroes.Commands
{
    public record UpdateHeroPositionsCommand : IMediatorRequest
    {
        public TimeSpan DeltaTime { get; init; }

        internal class Handler : IMediatorRequestHandler<UpdateHeroPositionsCommand>
        {
            private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpdateHeroPositionsCommand>();

            private static readonly HeroStatus[] MovementStatuses =
            {
                HeroStatus.MovingToPoint,
                HeroStatus.FollowingHero,
                HeroStatus.MovingToSettlement,
                HeroStatus.MovingToAttackHero,
                HeroStatus.MovingToAttackSettlement,
            };

            private static readonly HeroStatus[] UnattackableStatuses =
            {
                HeroStatus.IdleInSettlement,
                HeroStatus.RecruitingInSettlement,
                HeroStatus.InBattle,
            };

            private readonly ICrpgDbContext _db;
            private readonly IStrategusMap _strategusMap;
            private readonly IStrategusSpeedModel _strategusSpeedModel;

            public Handler(ICrpgDbContext db, IStrategusMap strategusMap, IStrategusSpeedModel strategusSpeedModel)
            {
                _db = db;
                _strategusMap = strategusMap;
                _strategusSpeedModel = strategusSpeedModel;
            }

            public async Task<Result> Handle(UpdateHeroPositionsCommand req, CancellationToken cancellationToken)
            {
                var heroes = await _db.Heroes
                    .AsSplitQuery()
                    .Where(h => MovementStatuses.Contains(h.Status))
                    .Include(h => h.TargetedHero)
                    .Include(h => h.TargetedSettlement)
                    // Load mounts items to compute movement speed.
                    .Include(h => h.Items!.Where(oi => oi.Item!.Type == ItemType.Mount)).ThenInclude(oi => oi.Item)
                    .ToArrayAsync(cancellationToken);

                foreach (var hero in heroes)
                {
                    switch (hero.Status)
                    {
                        case HeroStatus.MovingToPoint:
                            MoveToPoint(req.DeltaTime, hero);
                            break;
                        case HeroStatus.FollowingHero:
                        case HeroStatus.MovingToAttackHero:
                            MoveToHero(req.DeltaTime, hero);
                            break;
                        case HeroStatus.MovingToSettlement:
                        case HeroStatus.MovingToAttackSettlement:
                            await MoveToSettlement(req.DeltaTime, hero, cancellationToken);
                            break;
                    }
                }

                await _db.SaveChangesAsync(cancellationToken);
                return Result.NoErrors;
            }

            private void MoveToPoint(TimeSpan deltaTime, Hero hero)
            {
                if (hero.Waypoints.IsEmpty)
                {
                    Logger.LogWarning("Hero '{heroId}' was in status '{status}' without any points to go to",
                        hero.Id, hero.Status);
                    hero.Status = HeroStatus.Idle;
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
                    hero.Status = HeroStatus.Idle;
                }
            }

            private void MoveToHero(TimeSpan deltaTime, Hero hero)
            {
                if (hero.TargetedHero == null)
                {
                    Logger.LogWarning("Hero '{heroId}' was in status '{status}' without target hero",
                        hero.Id, hero.Status);
                    hero.Status = HeroStatus.Idle;
                    return;
                }

                if (!hero.Position.IsWithinDistance(hero.TargetedHero.Position, _strategusMap.ViewDistance))
                {
                    // Followed hero is not in sight anymore. Stop.
                    hero.Status = HeroStatus.Idle;
                    hero.TargetedHero = null;
                    return;
                }

                if (hero.Status == HeroStatus.FollowingHero)
                {
                    // Set canInteractWithTarget to false to the hero doesn't stop to interaction range
                    // but stops on the target hero itself.
                    MoveHeroTowardsPoint(hero, hero.TargetedHero.Position, deltaTime, false);
                }
                else if (hero.Status == HeroStatus.MovingToAttackHero)
                {
                    if (!MoveHeroTowardsPoint(hero, hero.TargetedHero.Position, deltaTime, true))
                    {
                        return;
                    }

                    if (UnattackableStatuses.Contains(hero.TargetedHero.Status))
                    {
                        return;
                    }

                    hero.Status = HeroStatus.InBattle;
                    hero.TargetedHero.Status = HeroStatus.InBattle;
                    Battle battle = new()
                    {
                        Phase = BattlePhase.Preparation,
                        Region = hero.TargetedHero.Region, // Region of the defender.
                        Position = GetMidPoint(hero.Position, hero.TargetedHero.Position),
                        Fighters =
                        {
                            new BattleFighter
                            {
                                Hero = hero,
                                Side = BattleSide.Attacker,
                                Commander = true,
                            },
                            new BattleFighter
                            {
                                Hero = hero.TargetedHero,
                                Side = BattleSide.Defender,
                                Commander = true,
                            },
                        },
                    };
                    _db.Battles.Add(battle);
                    Logger.LogInformation("Hero '{0}' initiated a battle against hero '{1}'",
                        hero.Id, hero.TargetedHeroId);
                }
            }

            private async Task MoveToSettlement(TimeSpan deltaTime, Hero hero, CancellationToken cancellationToken)
            {
                if (hero.TargetedSettlement == null)
                {
                    Logger.LogWarning("Hero '{heroId}' was in status '{status}' without target settlement",
                        hero.Id, hero.Status);
                    hero.Status = HeroStatus.Idle;
                    return;
                }

                if (!MoveHeroTowardsPoint(hero, hero.TargetedSettlement.Position, deltaTime, true))
                {
                    return;
                }

                if (hero.Status == HeroStatus.MovingToSettlement)
                {
                    hero.Status = HeroStatus.IdleInSettlement;
                    hero.Position = hero.TargetedSettlement.Position;
                }
                else if (hero.Status == HeroStatus.MovingToAttackSettlement)
                {
                    bool attackInProgress = await _db.Battles
                        .AnyAsync(b =>
                                b.Phase != BattlePhase.End
                                && b.Fighters.Any(f => f.SettlementId == hero.TargetedSettlementId),
                            cancellationToken);
                    if (attackInProgress)
                    {
                        return;
                    }

                    hero.Status = HeroStatus.InBattle;
                    var battle = new Battle
                    {
                        Phase = BattlePhase.Preparation,
                        Region = hero.TargetedSettlement.Region, // Region of the defender.
                        Position = GetMidPoint(hero.Position, hero.TargetedSettlement.Position),
                        Fighters =
                        {
                            new BattleFighter
                            {
                                Hero = hero,
                                Side = BattleSide.Attacker,
                                Commander = true,
                            },
                            new BattleFighter
                            {
                                Settlement = hero.TargetedSettlement,
                                Side = BattleSide.Defender,
                                Commander = true,
                            },
                        },
                    };
                    _db.Battles.Add(battle);
                    Logger.LogInformation("Hero '{0}' initiated a battle against settlement '{1}'",
                        hero.Id, hero.TargetedSettlementId);
                }
            }

            private bool MoveHeroTowardsPoint(Hero hero, Point targetPoint, TimeSpan deltaTime, bool canInteractWithTarget)
            {
                double speed = _strategusSpeedModel.ComputeHeroSpeed(hero);
                double distance = speed * deltaTime.TotalSeconds;
                hero.Position = _strategusMap.MovePointTowards(hero.Position, targetPoint, distance);
                return canInteractWithTarget
                    ? _strategusMap.ArePointsAtInteractionDistance(hero.Position, targetPoint)
                    : _strategusMap.ArePointsEquivalent(hero.Position, targetPoint);
            }

            private Point GetMidPoint(Point pointA, Point pointB)
            {
                return new((pointA.X + pointB.X) / 2, (pointA.Y + pointB.Y) / 2);
            }
        }
    }
}
