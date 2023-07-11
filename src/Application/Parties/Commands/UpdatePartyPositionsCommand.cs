using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Battles;
using Crpg.Domain.Entities.Items;
using Crpg.Domain.Entities.Parties;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Parties.Commands;

public record UpdatePartyPositionsCommand : IMediatorRequest
{
    public TimeSpan DeltaTime { get; init; }

    internal class Handler : IMediatorRequestHandler<UpdatePartyPositionsCommand>
    {
        private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpdatePartyPositionsCommand>();

        private static readonly PartyStatus[] MovementStatuses =
        {
            PartyStatus.MovingToPoint,
            PartyStatus.FollowingParty,
            PartyStatus.MovingToSettlement,
            PartyStatus.MovingToAttackParty,
            PartyStatus.MovingToAttackSettlement,
        };

        private static readonly PartyStatus[] UnattackableStatuses =
        {
            PartyStatus.IdleInSettlement,
            PartyStatus.RecruitingInSettlement,
            PartyStatus.InBattle,
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

        public async Task<Result> Handle(UpdatePartyPositionsCommand req, CancellationToken cancellationToken)
        {
            var parties = await _db.Parties
                .AsSplitQuery()
                .Where(p => MovementStatuses.Contains(p.Status))
                .Include(p => p.TargetedParty).ThenInclude(p => p!.User)
                .Include(p => p.TargetedSettlement)
                // Load mounts items to compute movement speed.
                .Include(p => p.Items.Where(oi => oi.Item!.Type == ItemType.Mount)).ThenInclude(oi => oi.Item)
                .ToArrayAsync(cancellationToken);

            foreach (var party in parties)
            {
                switch (party.Status)
                {
                    case PartyStatus.MovingToPoint:
                        MoveToPoint(req.DeltaTime, party);
                        break;
                    case PartyStatus.FollowingParty:
                    case PartyStatus.MovingToAttackParty:
                        MoveToParty(req.DeltaTime, party);
                        break;
                    case PartyStatus.MovingToSettlement:
                    case PartyStatus.MovingToAttackSettlement:
                        await MoveToSettlement(req.DeltaTime, party, cancellationToken);
                        break;
                }
            }

            await _db.SaveChangesAsync(cancellationToken);
            return Result.NoErrors;
        }

        private void MoveToPoint(TimeSpan deltaTime, Party party)
        {
            if (party.Waypoints.IsEmpty)
            {
                Logger.LogWarning("Party '{partyId}' was in status '{status}' without any points to go to",
                    party.Id, party.Status);
                party.Status = PartyStatus.Idle;
                return;
            }

            var targetPoint = (Point)party.Waypoints[0];
            if (!MovePartyTowardsPoint(party, targetPoint, deltaTime, false))
            {
                return;
            }

            party.Waypoints = new MultiPoint(party.Waypoints.Skip(1).Cast<Point>().ToArray());
            if (party.Waypoints.Count == 0)
            {
                party.Status = PartyStatus.Idle;
            }
        }

        private void MoveToParty(TimeSpan deltaTime, Party party)
        {
            if (party.TargetedParty == null)
            {
                Logger.LogWarning("Party '{partyId}' was in status '{status}' without target party",
                    party.Id, party.Status);
                party.Status = PartyStatus.Idle;
                return;
            }

            if (!party.Position.IsWithinDistance(party.TargetedParty.Position, _strategusMap.ViewDistance))
            {
                // Followed party is not in sight anymore. Stop.
                party.Status = PartyStatus.Idle;
                party.TargetedParty = null;
                return;
            }

            if (party.Status == PartyStatus.FollowingParty)
            {
                // Set canInteractWithTarget to false to the party doesn't stop to interaction range
                // but stops on the target party itself.
                MovePartyTowardsPoint(party, party.TargetedParty.Position, deltaTime, false);
            }
            else if (party.Status == PartyStatus.MovingToAttackParty)
            {
                if (!MovePartyTowardsPoint(party, party.TargetedParty.Position, deltaTime, true))
                {
                    return;
                }

                if (UnattackableStatuses.Contains(party.TargetedParty.Status))
                {
                    return;
                }

                party.Status = PartyStatus.InBattle;
                party.TargetedParty.Status = PartyStatus.InBattle;
                Battle battle = new()
                {
                    Phase = BattlePhase.Preparation,
                    Region = party.TargetedParty.User!.Region!.Value, // Region of the defender.
                    Position = GetMidPoint(party.Position, party.TargetedParty.Position),
                    Fighters =
                    {
                        new BattleFighter
                        {
                            Party = party,
                            Side = BattleSide.Attacker,
                            Commander = true,
                        },
                        new BattleFighter
                        {
                            Party = party.TargetedParty,
                            Side = BattleSide.Defender,
                            Commander = true,
                        },
                    },
                };
                _db.Battles.Add(battle);
                Logger.LogInformation("Party '{0}' initiated a battle against party '{1}'",
                    party.Id, party.TargetedPartyId);
            }
        }

        private async Task MoveToSettlement(TimeSpan deltaTime, Party party, CancellationToken cancellationToken)
        {
            if (party.TargetedSettlement == null)
            {
                Logger.LogWarning("Party '{partyId}' was in status '{status}' without target settlement",
                    party.Id, party.Status);
                party.Status = PartyStatus.Idle;
                return;
            }

            if (!MovePartyTowardsPoint(party, party.TargetedSettlement.Position, deltaTime, true))
            {
                return;
            }

            if (party.Status == PartyStatus.MovingToSettlement)
            {
                party.Status = PartyStatus.IdleInSettlement;
                party.Position = party.TargetedSettlement.Position;
            }
            else if (party.Status == PartyStatus.MovingToAttackSettlement)
            {
                bool attackInProgress = await _db.Battles
                    .AnyAsync(b =>
                            b.Phase != BattlePhase.End
                            && b.Fighters.Any(f => f.SettlementId == party.TargetedSettlementId),
                        cancellationToken);
                if (attackInProgress)
                {
                    return;
                }

                party.Status = PartyStatus.InBattle;
                Battle battle = new()
                {
                    Phase = BattlePhase.Preparation,
                    Region = party.TargetedSettlement.Region, // Region of the defender.
                    Position = GetMidPoint(party.Position, party.TargetedSettlement.Position),
                    Fighters =
                    {
                        new BattleFighter
                        {
                            Party = party,
                            Side = BattleSide.Attacker,
                            Commander = true,
                        },
                        new BattleFighter
                        {
                            Settlement = party.TargetedSettlement,
                            Side = BattleSide.Defender,
                            Commander = true,
                        },
                    },
                };
                _db.Battles.Add(battle);
                Logger.LogInformation("Party '{0}' initiated a battle against settlement '{1}'",
                    party.Id, party.TargetedSettlementId);
            }
        }

        private bool MovePartyTowardsPoint(Party party, Point targetPoint, TimeSpan deltaTime, bool canInteractWithTarget)
        {
            double speed = _strategusSpeedModel.ComputePartySpeed(party);
            double distance = speed * deltaTime.TotalSeconds;
            party.Position = _strategusMap.MovePointTowards(party.Position, targetPoint, distance);
            return canInteractWithTarget
                ? _strategusMap.ArePointsAtInteractionDistance(party.Position, targetPoint)
                : _strategusMap.ArePointsEquivalent(party.Position, targetPoint);
        }

        private Point GetMidPoint(Point pointA, Point pointB)
        {
            return new((pointA.X + pointB.X) / 2, (pointA.Y + pointB.Y) / 2);
        }
    }
}
