using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Crpg.Application.Common;
using Crpg.Application.Common.Interfaces;
using Crpg.Application.Common.Mediator;
using Crpg.Application.Common.Results;
using Crpg.Application.Common.Services;
using Crpg.Domain.Entities.Strategus;
using Crpg.Sdk.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LoggerFactory = Crpg.Logging.LoggerFactory;

namespace Crpg.Application.Strategus.Commands
{
    public record UpdateStrategusBattlePhasesCommand : IMediatorRequest
    {
        public TimeSpan DeltaTime { get; init; }

        internal class Handler : IMediatorRequestHandler<UpdateStrategusBattlePhasesCommand>
        {
            private static readonly ILogger Logger = LoggerFactory.CreateLogger<UpdateStrategusBattlePhasesCommand>();

            private readonly ICrpgDbContext _db;
            private readonly IStrategusBattleScheduler _battleScheduler;
            private readonly IDateTimeOffset _dateTimeOffset;
            private readonly TimeSpan _battleInitiationDuration;
            private readonly TimeSpan _battleHiringDuration;

            public Handler(ICrpgDbContext db, IStrategusBattleScheduler battleScheduler, IDateTimeOffset dateTimeOffset,
                Constants constants)
            {
                _db = db;
                _battleScheduler = battleScheduler;
                _dateTimeOffset = dateTimeOffset;
                _battleInitiationDuration = TimeSpan.FromHours(constants.StrategusBattleInitiationDurationHours);
                _battleHiringDuration = TimeSpan.FromHours(constants.StrategusBattleHiringDurationHours);
            }

            public async Task<Result> Handle(UpdateStrategusBattlePhasesCommand req, CancellationToken cancellationToken)
            {
                var battles = _db.StrategusBattles
                    .AsSplitQuery()
                    .Include(b => b.AttackedSettlement)
                    .Include(b => b.Fighters).ThenInclude(f => f.Hero)
                    .Where(b =>
                        (b.Phase == StrategusBattlePhase.Preparation && b.CreatedAt + _battleInitiationDuration < _dateTimeOffset.Now)
                        || (b.Phase == StrategusBattlePhase.Hiring && b.CreatedAt + _battleInitiationDuration + _battleHiringDuration < _dateTimeOffset.Now))
                    .AsAsyncEnumerable();

                await foreach (var battle in battles.WithCancellation(cancellationToken))
                {
                    switch (battle.Phase)
                    {
                        case StrategusBattlePhase.Preparation:
                            battle.Phase = StrategusBattlePhase.Hiring;
                            break;
                        case StrategusBattlePhase.Hiring:
                            await _battleScheduler.ScheduleBattle(battle);
                            battle.Phase = StrategusBattlePhase.Battle;
                            break;
                    }
                }

                await _db.SaveChangesAsync(cancellationToken);
                return Result.NoErrors;
            }
        }
    }
}
