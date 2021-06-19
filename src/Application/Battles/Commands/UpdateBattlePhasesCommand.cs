using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Battles;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Battles.Commands
{
    public record UpdateBattlePhasesCommand : IMediatorRequest
    {
        public TimeSpan DeltaTime { get; init; }

        internal class Handler : IMediatorRequestHandler<UpdateBattlePhasesCommand>
        {
            private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpdateBattlePhasesCommand>();

            private readonly ICrpgDbContext _db;
            private readonly IBattleMercenaryDistributionModel _battleMercenaryDistributionModel;
            private readonly IBattleScheduler _battleScheduler;
            private readonly IDateTimeOffset _dateTimeOffset;
            private readonly TimeSpan _battleInitiationDuration;
            private readonly TimeSpan _battleHiringDuration;

            public Handler(ICrpgDbContext db, IBattleMercenaryDistributionModel battleMercenaryDistributionModel,
                IBattleScheduler battleScheduler, IDateTimeOffset dateTimeOffset, Constants constants)
            {
                _db = db;
                _battleMercenaryDistributionModel = battleMercenaryDistributionModel;
                _battleScheduler = battleScheduler;
                _dateTimeOffset = dateTimeOffset;
                _battleInitiationDuration = TimeSpan.FromHours(constants.StrategusBattleInitiationDurationHours);
                _battleHiringDuration = TimeSpan.FromHours(constants.StrategusBattleHiringDurationHours);
            }

            public async Task<Result> Handle(UpdateBattlePhasesCommand req, CancellationToken cancellationToken)
            {
                var battles = _db.Battles
                    .AsSplitQuery()
                    .Include(b => b.Fighters).ThenInclude(f => f.Hero)
                    .Include(b => b.Fighters).ThenInclude(f => f.Settlement)
                    .Where(b =>
                        (b.Phase == BattlePhase.Preparation && b.CreatedAt + _battleInitiationDuration < _dateTimeOffset.Now)
                        || (b.Phase == BattlePhase.Hiring && b.CreatedAt + _battleInitiationDuration + _battleHiringDuration < _dateTimeOffset.Now)
                        || (b.Phase == BattlePhase.Scheduled && b.ScheduledFor < _dateTimeOffset.Now))
                    .AsAsyncEnumerable();

                await foreach (var battle in battles.WithCancellation(cancellationToken))
                {
                    BattlePhase oldPhase = battle.Phase;
                    switch (battle.Phase)
                    {
                        case BattlePhase.Preparation:
                            int battleSlots = 100; // TODO: make it depend on the number of troops.
                            _battleMercenaryDistributionModel.DistributeMercenaries(battle.Fighters, battleSlots);
                            battle.Phase = BattlePhase.Hiring;
                            break;
                        case BattlePhase.Hiring:
                            await _battleScheduler.ScheduleBattle(battle);
                            battle.Phase = BattlePhase.Scheduled;
                            break;
                        case BattlePhase.Scheduled:
                            battle.Phase = BattlePhase.Live;
                            break;
                    }

                    Logger.LogInformation("Battle '{0}' switches from phase '{1}' to phase '{2}'",
                        battle.Id, oldPhase, battle.Phase);
                }

                await _db.SaveChangesAsync(cancellationToken);
                return Result.NoErrors;
            }
        }
    }
}
